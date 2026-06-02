using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SopaDeLetras.Web.Services;

namespace SopaDeLetras.Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly PalabraService _palabraService;
        public List<string> Categorias { get; private set; } = new(); // Propiedad para almacenar la lista de categorías disponibles, inicializada como una lista vacía para evitar null reference exceptions al acceder a ella
        public IndexModel(PalabraService palabraService)
        {
            _palabraService = palabraService;
        }
        public void OnGet()
        {
            Categorias = _palabraService.ObtenerCategorias(); // Carga la lista de categorías disponibles utilizando el servicio de palabras y asigna el resultado a la propiedad Categorias para que pueda ser accedida desde la vista Razor
        }
    }
}
