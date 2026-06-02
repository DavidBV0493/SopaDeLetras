//Modelo que representa el tablero de la sopa de letras, con su tamaño, las celdas y las palabras que contiene.

namespace SopaDeLetras.Web.Models

{
    public class Tablero
    {
        public int Tamanio { get; set; } // El tamaño del tablero (n x n)
        public char[,] Celdas { get; set; } // Las celdas del tablero, representadas como un arreglo bidimensional de caracteres
        public List<Palabra> Palabras { get; set; } = new(); // La lista de palabras que contiene el tablero
        public DateTime FechaCreacion { get; set; } = DateTime.Now; // La fecha de creación del tablero

        public Tablero(int tamanio) // Constructor que inicializa el tablero con un tamaño específico y crea el arreglo de celdas
        {
            Tamanio = tamanio;
            Celdas = new char[tamanio, tamanio];
        }
    }
}
