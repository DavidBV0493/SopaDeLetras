//Código para la página Razor "Juego", que maneja la lógica de generación del tablero de juego y la sesión de juego.

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SopaDeLetras.Web.Models;
using SopaDeLetras.Web.Services;

namespace SopaDeLetras.Web.Pages

{
    public class JuegoModel : PageModel
    {
        private readonly TableroService _tableroService; // Servicio para generar el tablero de juego
        private readonly PalabraService _palabraService; // Servicio para cargar palabras según la categoría seleccionada
        public Tablero Tablero { get; private set; } = null!; // Propiedad para almacenar el tablero de juego generado
        public SesionJuego Sesion { get; private set; } = null!; // Propiedad para almacenar la sesión de juego actual
        [BindProperty(SupportsGet = true)] // Propiedad para recibir el tamaño del tablero desde la consulta GET
        public int Size { get; set; } = 15; // Tamaño predeterminado del tablero
        [BindProperty(SupportsGet = true)] // Propiedad para recibir la categoría de palabras desde la consulta GET
        public string Categoria { get; set; } = "Tecnologia"; // Categoría predeterminada de palabras

        public JuegoModel(TableroService ts, PalabraService ps) // Constructor que recibe los servicios necesarios para generar el tablero y cargar las palabras
        {
            _tableroService = ts; // Asigna el servicio de tablero al campo privado
            _palabraService = ps; // Asigna el servicio de palabras al campo privado
        }

        public void OnGet() // Método que se ejecuta cuando se accede a la página mediante una solicitud GET
        {
            var palabras = _palabraService.CargarPalabras(Categoria, cantidad: 20); // Carga un conjunto de palabras según la categoría seleccionada, con una cantidad específica
            var palabrasFiltradas = palabras.Where(p => p.Length <= Size).Take(8).ToList(); // Filtra las palabras para incluir solo aquellas que se ajusten al tamaño del tablero, limitando la cantidad a 20
            if (palabrasFiltradas.Count == 0)
            {
                TempData["Error"] = $"No hay palabras disponibles para un tablero de {Size}×{Size} en la categoría '{Categoria}'."; // Si no hay palabras disponibles para el tamaño del tablero, se muestra un mensaje de error al usuario utilizando TempData
                RedirectToPage("/Index"); // Redirige al usuario a la página de inicio si no hay palabras disponibles para el tamaño del tablero
                return; // Termina la ejecución del método para evitar continuar con la generación del tablero si no hay palabras disponibles
            }
            Tablero = _tableroService.GenerarTablero(palabrasFiltradas, Size); // Genera el tablero de juego utilizando el servicio de tablero, pasando las palabras cargadas y el tamaño del tablero
            Sesion = new SesionJuego { Tablero = Tablero }; // Crea una nueva sesión de juego y asigna el tablero generado a la propiedad Tablero de la sesión
            ViewData["SesionId"] = Sesion.Id; // Almacena el ID de la sesión en ViewData para que pueda ser accedido desde la vista Razor, permitiendo que la vista tenga acceso a esta información para futuras interacciones con el juego
        }
    }
}
