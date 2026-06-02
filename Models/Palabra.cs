//Modelo de una palabra en la sopa de letras, con su texto, posición inicial, dirección y estado de encontrada o no.

namespace SopaDeLetras.Web.Models

{
    public class Palabra
    {
        public string Texto { get; set; } = string.Empty; // El texto de la palabra
        public int FilaInicio { get; set; } // La fila donde comienza la palabra
        public int ColInicio { get; set; } // La columna donde comienza la palabra
        public string Direccion { get; set; } = string.Empty; // La dirección de la palabra (horizontal, vertical, diagonal, antidiagonal)
        public bool Encontrada { get; set; } = false; // Indica si la palabra ha sido encontrada o no
    }
}
