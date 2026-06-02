//Servicio para manejar conexiones WebSocket en el juego de sopa de letras, permitiendo la comunicación en tiempo real entre el cliente y el servidor para enviar actualizaciones y recibir mensajes del cliente.

using System.Net.WebSockets;
using System.Text;
using SopaDeLetras.Web.Services;

namespace SopaDeLetras.Web.Services

{
    public class GameSocketHandler
    {
        private readonly TableroService _tableroService;

        public GameSocketHandler(TableroService tableroService) // Constructor que recibe una instancia del servicio de tablero para poder interactuar con el estado del juego
        {
            _tableroService = tableroService; // Asigna la instancia del servicio de tablero a la variable de instancia para su uso posterior en el manejo de conexiones WebSocket
        }

        public async Task ManejarConexion(WebSocket socket, string sesionId, CancellationToken ct) // Método para manejar la conexión de un cliente a través de WebSocket, recibiendo el socket, el ID de la sesión de juego y un token de cancelación
        {
            var buffer = new byte[256]; // Buffer para recibir mensajes del cliente
            var inicio = DateTime.Now; // Marca el inicio de la conexión
            var sendLock = new SemaphoreSlim(1, 1); // Semáforo para asegurar que solo un mensaje se envíe a la vez a través del socket, evitando condiciones de carrera
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1)); // Timer periódico para enviar actualizaciones al cliente cada segundo
            var timerTask = Task.Run(async () =>
            {
                while (await timer.WaitForNextTickAsync(ct)) // Mientras el timer esté activo, espera el siguiente tick para enviar una actualización
                {
                    if (socket.State != WebSocketState.Open) break; // Si el socket no está abierto, sale del bucle
                    var segundos = (int)(DateTime.Now - inicio).TotalSeconds; // Calcula el tiempo transcurrido desde el inicio de la conexión en segundos
                    var msg = Encoding.UTF8.GetBytes($"TIEMPO:{segundos}|SESION:{sesionId}"); // Crea un mensaje con el tiempo conectado y el ID de la sesión
                    await sendLock.WaitAsync(ct); // Espera a adquirir el semáforo para enviar el mensaje, asegurando que no se envíen mensajes simultáneamente
                    try
                    {
                        await socket.SendAsync(msg, WebSocketMessageType.Text, true, ct); // Envía el mensaje al cliente a través del socket
                    }
                    finally
                    {
                        sendLock.Release();
                    }
                }
            }, ct);
            while (socket.State == WebSocketState.Open && !ct.IsCancellationRequested) // Mientras el socket esté abierto y no se haya solicitado la cancelación
            {
                WebSocketReceiveResult result;
                try
                {
                    result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), ct); // Espera a recibir un mensaje del cliente
                }
                catch (OperationCanceledException)
                {
                    break; // Si se cancela la operación de recepción, sale del bucle
                }
                if (result.MessageType == WebSocketMessageType.Close) // Si el mensaje recibido es una solicitud de cierre, sale del bucle
                {
                    break;
                }
                var mensaje = Encoding.UTF8.GetString(buffer, 0, result.Count); // Convierte el mensaje recibido a una cadena de texto
                if (mensaje.StartsWith("ENCONTRADA:"))
                {
                    var partes = mensaje.Split(':'); // Divide el mensaje en partes utilizando ":" como separador
                    if (partes.Length < 2 || string.IsNullOrWhiteSpace(partes[1]))
                    {
                        var errorMsg = Encoding.UTF8.GetBytes("ERROR:FORMATO_INVALIDO"); // Crea un mensaje de error indicando que el formato del mensaje es inválido
                        await EnviarSeguro(socket, errorMsg, sendLock, ct); // Envía el mensaje de error al cliente de manera segura utilizando el semáforo para evitar condiciones de carrera
                        continue;
                    }
                    var textoPalabra = partes[1].Trim().ToUpperInvariant(); // Extrae la palabra enviada por el cliente, eliminando espacios y convirtiéndola a mayúsculas para una comparación uniforme
                    bool actualizado = _tableroService.MarcarPalabraEncontrada(sesionId, textoPalabra); // Marca la palabra como encontrada en el servicio de tablero, actualizando el estado del juego
                    var rspuesta = actualizado ? Encoding.UTF8.GetBytes($"OK: {textoPalabra}") : Encoding.UTF8.GetBytes($"ERROR: PALABRA_NO_ENCONTRADA: {textoPalabra}"); // Crea un mensaje de respuesta indicando si la palabra fue encontrada o no
                    await EnviarSeguro(socket, rspuesta, sendLock, ct); // Envía la respuesta al cliente de manera segura utilizando el semáforo para evitar condiciones de carrera
                }
            }
            try { await timerTask; } catch (OperationCanceledException) { } // Intenta esperar a que la tarea del timer termine, manejando la posible cancelación de la operación
            if (socket.State == WebSocketState.Open || socket.State == WebSocketState.CloseReceived) // Si el socket aún está abierto o ha recibido una solicitud de cierre, cierra la conexión de manera normal
            {
                try
                {
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Conexión cerrada por el servidor", CancellationToken.None); // Cierra la conexión del socket de manera normal con un mensaje indicando que la conexión fue cerrada por el servidor
                }
                catch (WebSocketException)
                {
                }
            }
        }
        private static async Task EnviarSeguro(WebSocket socket, byte[] mensaje, SemaphoreSlim sendLock, CancellationToken ct) // Método auxiliar para enviar mensajes de manera segura a través del socket utilizando un semáforo para evitar condiciones de carrera
        {
            if (socket.State != WebSocketState.Open) return; // Si el socket no está abierto, no intenta enviar el mensaje
            await sendLock.WaitAsync(ct); // Espera a adquirir el semáforo para enviar el mensaje, asegurando que no se envíen mensajes simultáneamente
            try
            {
                await socket.SendAsync(mensaje, WebSocketMessageType.Text, true, ct); // Envía el mensaje al cliente a través del socket

            }
            finally
            {
                sendLock.Release(); // Libera el semáforo después de enviar el mensaje para permitir que otros mensajes puedan ser enviados
            }
        }
    }
}
