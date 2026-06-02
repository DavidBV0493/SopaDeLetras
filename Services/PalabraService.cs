//Servicio para cargar palabras desde un archivo JSON y obtener categorías disponibles.

using SopaDeLetras.Web.Models;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SopaDeLetras.Web.Services

{
    internal class CategoriasRoot // Clase que representa la estructura raíz del archivo JSON, que contiene una lista de categorías
    {
        [JsonPropertyName("categorias")] // Propiedad que mapea el array de categorías en el JSON
        public List<CategoriaJson> Categorias { get; set; } = new(); // Inicializa la lista de categorías para evitar null reference exceptions al acceder a ella
    }

    internal class CategoriaJson // Clase que representa la estructura de cada categoría en el archivo JSON, con su nombre y lista de palabras asociadas
    {
        [JsonPropertyName("nombre")] // Propiedad que mapea el nombre de la categoría en el JSON
        public string Nombre { get; set; } = string.Empty; // Inicializa el nombre de la categoría con una cadena vacía para evitar null reference exceptions al acceder a ella
        [JsonPropertyName("palabras")] // Propiedad que mapea el array de palabras de la categoría en el JSON
        public List<string> Palabras { get; set; } = new(); // Inicializa la lista de palabras para evitar null reference exceptions al acceder a ella
    }

    public class PalabraService
    {
        private readonly List<CategoriaJson> _categorias; // Lista de categorías cargadas desde el archivo JSON, utilizada para almacenar las categorías y sus palabras en memoria
        //private readonly string _rutaArchivo; // Ruta al archivo JSON que contiene las palabras y categorías

        public PalabraService(IWebHostEnvironment env) // Constructor que recibe el entorno de la aplicación para construir la ruta al archivo JSON de palabras
        {
            var rutaArchivo = Path.Combine(env.ContentRootPath, "Data", "palabras.json"); // Construye la ruta al archivo JSON combinando el directorio raíz de la aplicación con la carpeta "Data" y el nombre del archivo "palabras.json"
            var json = File.ReadAllText(rutaArchivo, Encoding.UTF8).Trim('\uFEFF'); // Lee el contenido del archivo JSON y elimina el BOM (Byte Order Mark) si está presente al inicio del archivo, lo que puede causar problemas al parsear el JSON
            var root = JsonSerializer.Deserialize<CategoriasRoot>(json) ??
                throw new InvalidOperationException("El archivo palabras.json no tiene el formato esperado."); // Deserializa el JSON en un objeto de tipo CategoriasRoot, lanzando una excepción si el formato del JSON no es el esperado
            _categorias = root.Categorias; // Asigna la lista de categorías deserializada a la variable de instancia para su uso posterior en el servicio
        }

        public List<string> CargarPalabras(string categoria = "Tecnología", int cantidad = 8) // Método para cargar palabras de una categoría específica desde el archivo JSON, con una cantidad limitada
        {
            var cat = _categorias.FirstOrDefault(c => c.Nombre.Equals(categoria, StringComparison.OrdinalIgnoreCase)); // Busca la categoría solicitada en la lista de categorías cargadas, ignorando mayúsculas y minúsculas
            if (cat == null) return new List<string>(); // Si no se encuentra la categoría solicitada, retorna una lista vacía
            return cat.Palabras.OrderBy(_ => Random.Shared.Next()).Take(cantidad).ToList(); // Mezcla las palabras de la categoría de forma aleatoria y toma la cantidad solicitada, retornando la lista resultante
            //var json = File.ReadAllText(_rutaArchivo, Encoding.UTF8); // Lee el contenido del archivo JSON
            //json = json.TrimStart('\uFEFF'); // Elimina el BOM (Byte Order Mark) si está presente al inicio del archivo JSON, lo que puede causar problemas al parsear el JSON
            //using var doc = JsonDocument.Parse(json); // Parsea el JSON para acceder a sus elementos
            //var categorias = doc.RootElement.GetProperty("categorias"); // Obtiene el array de categorías del JSON
            //foreach (var cat in categorias.EnumerateArray()) // Itera sobre cada categoría en el array
            //{
            //    if (cat.GetProperty("nombre").GetString() == categoria) // Verifica si el nombre de la categoría coincide con la solicitada
            //    {
            //        var todas = cat.GetProperty("palabras").EnumerateArray().Select(p => p.GetString()!).ToList(); // Obtiene todas las palabras de la categoría seleccionada
            //        return todas.OrderBy(_ => Random.Shared.Next()).Take(cantidad).ToList(); // Mezcla las palabras de forma aleatoria y toma la cantidad solicitada
            //    }
            //}
            //return new List<string>(); // Retorna una lista vacía si no se encuentra la categoría solicitada
        }

        public List<string> ObtenerCategorias() // Método para obtener la lista de categorías disponibles desde el archivo JSON
        {
            return _categorias.Select(c => c.Nombre).ToList(); // Retorna una lista con los nombres de las categorías disponibles en la lista de categorías cargada en memoria
            //var json = File.ReadAllText(_rutaArchivo); // Lee el contenido del archivo JSON
            //using var doc = JsonDocument.Parse(json); // Parsea el JSON para acceder a sus elementos
            //return doc.RootElement.GetProperty("categorias").EnumerateArray().Select(c => c.GetProperty("nombre").GetString()!).ToList(); // Retorna una lista con los nombres de las categorías disponibles en el JSON
        }
    }
}
