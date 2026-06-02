//Servicio para generar un tablero de sopa de letras con palabras ubicadas de manera concurrente.

using SopaDeLetras.Web.Models;
using System.Collections.Concurrent;

namespace SopaDeLetras.Web.Services

{
    public class TableroService
    {
        private static readonly (int df, int dc)[] Direcciones = // Arreglo de tuplas que representan las posibles direcciones para colocar las palabras en el tablero (horizontal, vertical, diagonal, antidiagonal y sus inversas)
        {
            (0, 1), // → Horizontal H
            (1, 0), // ↓ Vertical V
            (1, 1), // ↘ Diagonal D
            (0, -1), // ← Horizontal inversa H-
            (-1, 0), // ↑ Vertical inversa V-
            (-1, -1), // ↖ Diagonal inversa D-
            (1, -1), // ↙ Antidiagonal D2
            (-1, 1) // ↗ Antidiagonal inversa D3
        };

        private readonly ConcurrentDictionary<string, SesionJuego> _sesiones = new(); // Diccionario concurrente para almacenar las sesiones de juego, permitiendo el acceso seguro desde múltiples hilos

        public Tablero GenerarTablero(List<string> palabras, int tamanio) // Método público para generar un tablero de sopa de letras sin especificar una sesión, simplemente llama al método privado con sesionId como null
        {
            return GenerarTablero(palabras, tamanio, sesionId: null); // Llama al método privado para generar el tablero, pasando null como sesionId ya que no se está utilizando una sesión específica en este caso
        }

        public Tablero GenerarTablero(List<string> palabras, int tamanio, string? sesionId) // Método para generar un tablero de sopa de letras con una lista de palabras y un tamaño específico
        {
            var tablero = new Tablero(tamanio); // Crea un nuevo tablero con el tamaño especificado
            var lockObj = new object(); // Objeto de bloqueo para sincronizar el acceso al tablero
            var threads = new List<Thread>(); // Lista para almacenar los hilos que se crearán para colocar las palabras en el tablero
            var errores = new List<Exception>(); // Lista para almacenar cualquier excepción que ocurra durante la colocación de las palabras
            foreach (var texto in palabras) // Itera sobre cada palabra en la lista de palabras
            {
                var palabra = new Palabra { Texto = texto }; // Crea un nuevo objeto Palabra con el texto de la palabra actual
                tablero.Palabras.Add(palabra); // Agrega la palabra al tablero
                var hilo = new Thread(() =>
                {
                    try
                    {
                        UbicarPalabra(tablero, palabra, lockObj); // Intenta ubicar la palabra en el tablero utilizando el método UbicarPalabra
                    }
                    catch (Exception ex)
                    {
                        lock (errores) { errores.Add(ex); }
                    } // Si ocurre una excepción, se agrega a la lista de errores de manera segura utilizando un bloqueo
                });
                hilo.Name = $"Hilo-{texto}"; // Asigna un nombre al hilo para facilitar la identificación en caso de errores
                threads.Add(hilo); // Agrega el hilo a la lista de hilos
            }
            threads.ForEach(t => t.Start()); // Inicia todos los hilos para colocar las palabras en el tablero
            threads.ForEach(t => t.Join()); // Espera a que todos los hilos terminen su ejecución antes de continuar
            if (errores.Any())
            {
                throw new AggregateException("Ocurrieron errores al ubicar las palabras en el tablero.", errores); // Si hubo errores, se lanza una excepción agregada con todos los errores ocurridos
            }
            RellenarEspacios(tablero); // Rellena los espacios vacíos del tablero con letras aleatorias
            if (!string.IsNullOrEmpty(sesionId))
            {
                var sesion = new SesionJuego { Id = sesionId, Tablero = tablero }; // Crea una nueva sesión de juego con el ID proporcionado y el tablero generado
                _sesiones[sesionId] = sesion; // Almacena la sesión en el diccionario concurrente utilizando el ID como clave
            }
            return tablero; // Devuelve el tablero generado
        }

        public bool MarcarPalabraEncontrada(string sesionId, string textoPalabra) // Método para marcar una palabra como encontrada en una sesión de juego específica
        {
            if(!_sesiones.TryGetValue(sesionId, out var sesion)) // Intenta obtener la sesión de juego del diccionario utilizando el ID proporcionado, si no se encuentra, retorna false
            {
                return false;
            }
            var palabra = sesion.Tablero.Palabras.FirstOrDefault(p => p.Texto.Equals(textoPalabra, StringComparison.OrdinalIgnoreCase)); // Busca la palabra en el tablero de la sesión utilizando una comparación de texto sin distinguir mayúsculas y minúsculas
            if (palabra == null) return false; // Si la palabra no se encuentra en el tablero, retorna false
            palabra.Encontrada = true; // Marca la palabra como encontrada
            if (sesion.Tablero.Palabras.All(p => p.Encontrada)) // Verifica si todas las palabras del tablero han sido encontradas
            {
                sesion.Terminado = true; // Si todas las palabras han sido encontradas, marca la sesión como terminada
            }
            return true; // Retorna true para indicar que la palabra fue marcada como encontrada correctamente
        }

        public SesionJuego? ObtenerSesion(string sesionId) // Método para obtener una sesión de juego por su ID, retorna null si no se encuentra la sesión
        {
            _sesiones.TryGetValue(sesionId, out var sesion); // Intenta obtener la sesión del diccionario utilizando el ID proporcionado
            return sesion; // Retorna la sesión encontrada o null si no se encuentra
        }

        public void LimpiarSesionesExpiradas(TimeSpan tiempoMaximo)
        {
            var expiradas = _sesiones.Where(kv => DateTime.Now - kv.Value.Inicio > tiempoMaximo).Select(kv => kv.Key).ToList(); // Obtiene una lista de IDs de sesiones que han expirado según el tiempo máximo permitido
            foreach (var id in expiradas) // Itera sobre cada ID de sesión expirada
            {
                _sesiones.TryRemove(id, out _); // Elimina la sesión del diccionario concurrente utilizando el ID
            }
        }

        private void UbicarPalabra(Tablero tablero, Palabra palabra, object lockObj) // Método para ubicar una palabra en el tablero de manera segura utilizando un bloqueo
        {
            var rnd = new Random(); // Crea una instancia de Random para generar números aleatorios
            int intentos = 0; // Contador de intentos para ubicar la palabra
            const int maxIntentos = 200; // Número máximo de intentos para ubicar la palabra antes de considerar que no se puede ubicar
            while (intentos < maxIntentos) // Bucle que intenta ubicar la palabra hasta alcanzar el número máximo de intentos
            {
                int fila = rnd.Next(tablero.Tamanio); // Genera una fila aleatoria dentro del tamaño del tablero
                int col = rnd.Next(tablero.Tamanio); // Genera una columna aleatoria dentro del tamaño del tablero
                var (df, dc) = Direcciones[rnd.Next(Direcciones.Length)]; // Selecciona una dirección aleatoria de las posibles direcciones
                if (CabeEnTablero(tablero, palabra.Texto, fila, col, df, dc)) // Verifica si es posible ubicar la palabra en la posición y dirección seleccionadas
                {
                    lock (lockObj) // Bloquea el acceso al tablero para evitar conflictos con otros hilos
                    {
                        if (NoCausaConflicto(tablero, palabra.Texto, fila, col, df, dc)) // Verifica si ubicar la palabra en la posición y dirección seleccionadas no causaría conflictos con otras palabras ya ubicadas
                        {
                            EscribirPalabra(tablero, palabra, fila, col, df, dc); // Escribe la palabra en el tablero en la posición y dirección seleccionadas
                            return;
                        }
                    }
                }
                intentos++;
            }
            lock (lockObj)
            {
                UbicarForzado(tablero, palabra); // Si no se pudo ubicar la palabra después de varios intentos, se intenta ubicarla de manera forzada (sin importar conflictos)
            }
        }

        private bool CabeEnTablero(Tablero t, string texto, int f, int c, int df, int dc) // Método para verificar si una palabra cabe en el tablero en una posición y dirección específicas
        {
            int fFin = f + df * (texto.Length - 1); // Calcula la fila final de la palabra según la dirección y longitud
            int cFin = c + dc * (texto.Length - 1); // Calcula la columna final de la palabra según la dirección y longitud
            return fFin >= 0 && fFin < t.Tamanio && cFin >= 0 && cFin < t.Tamanio; // Retorna true si la palabra cabe dentro de los límites del tablero, false en caso contrario
        }

        private bool NoCausaConflicto(Tablero t, string texto, int f, int c, int df, int dc) // Método para verificar si ubicar una palabra en una posición y dirección específicas no causaría conflictos con otras palabras ya ubicadas
        {
            for (int i = 0; i < texto.Length; i++) // Itera sobre cada letra de la palabra
            {
                char celda = t.Celdas[f + i * df, c + i * dc]; // Obtiene la letra actual del tablero en la posición correspondiente a la letra de la palabra
                if (celda != '\0' && celda != texto[i]) // Si la letra del tablero no es un espacio vacío y no coincide con la letra de la palabra, hay un conflicto
                {
                    return false;
                }
            }
            return true;
        }

        private void EscribirPalabra(Tablero t, Palabra p, int f, int c, int df, int dc) // Método para escribir una palabra en el tablero en una posición y dirección específicas
        {
            string[] dirs = { "H", "V", "D", "H-", "V-", "D-", "D2", "D3" }; // Arreglo de símbolos que representan las direcciones para mostrar en la palabra ubicada
            for (int i = 0; i < p.Texto.Length; i++) // Itera sobre cada letra de la palabra
            {
                t.Celdas[f + i * df, c + i * dc] = p.Texto[i]; // Escribe la letra de la palabra en el tablero en la posición correspondiente
            }
            p.FilaInicio = f; // Establece la fila de inicio de la palabra
            p.ColInicio = c; // Establece la columna de inicio de la palabra
            p.Direccion = $"{df},{dc}"; // Establece la dirección de la palabra utilizando las coordenadas de dirección
        }

        private void UbicarForzado(Tablero t, Palabra p) // Método para ubicar una palabra de manera forzada en el tablero, sin importar conflictos
        {
            for (int f = 0; f < t.Tamanio; f++) // Itera sobre cada fila del tablero
            {
                for (int c = 0; c < t.Tamanio - p.Texto.Length; c++) // Itera sobre cada columna del tablero
                {
                    if (NoCausaConflicto(t, p.Texto, f, c, 0, 1)) // Verifica si ubicar la palabra horizontalmente no causaría conflictos
                    {
                        EscribirPalabra(t, p, f, c, 0, 1); // Escribe la palabra horizontalmente en el tablero
                        return;
                    }
                }
            }
            throw new InvalidOperationException(
                $"No fue posible ubicar la palabra '{p.Texto}' en el tablero de tamaño {t.Tamanio}x{t.Tamanio}. " +
                $"Considera aumentar el tamaño del tablero o reducir el número de palabras.");
        }

        private void RellenarEspacios(Tablero t) // Método para rellenar los espacios vacíos del tablero con letras aleatorias
        {
            const string letras = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"; // Cadena de letras mayúsculas para rellenar los espacios vacíos
            for (int f = 0; f < t.Tamanio; f++) // Itera sobre cada fila del tablero
            {
                for (int c = 0; c < t.Tamanio; c++) // Itera sobre cada columna del tablero
                {
                    if (t.Celdas[f, c] == '\0') // Si la celda está vacía (representada por el carácter nulo)
                    {
                        t.Celdas[f, c] = letras[Random.Shared.Next(letras.Length)]; // Rellena la celda con una letra aleatoria de la cadena de letras
                    }
                }
            }
        }
    }
}
