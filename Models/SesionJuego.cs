/*Modelo de datos para representar una sesión de juego en la aplicación de sopa de letras. 
  Esta clase contiene información sobre el tablero de juego, la fecha de inicio, 
  si la sesión ha terminado y el número de palabras encontradas.*/

namespace SopaDeLetras.Web.Models

{
    public class SesionJuego
    {
        public string Id { get; set; } = Guid.NewGuid().ToString(); // Identificador único de la sesión
        public Tablero Tablero { get; set; } = null!; // El tablero de la sopa de letras asociado a esta sesión
        public DateTime Inicio { get; set; } = DateTime.Now; // La fecha y hora de inicio de la sesión
        public bool Terminado { get; set; } = false; // Indica si la sesión ha terminado o no
        public int PalabrasEncontradas => Tablero.Palabras.Count(p => p.Encontrada) ; // El número de palabras encontradas en esta sesión
    }
}
