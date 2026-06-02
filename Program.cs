/* Código para configurar y ejecutar una aplicación web utilizando ASP.NET Core. 
   El código establece servicios, maneja solicitudes de WebSocket y configura la aplicación para servir páginas Razor.*/

using SopaDeLetras.Web.Services;
using System.Net.WebSockets;

var builder = WebApplication.CreateBuilder(args); // Crea un nuevo constructor de aplicación web con los argumentos proporcionados
builder.Services.AddRazorPages(); // Agrega servicios para Razor Pages, lo que permite utilizar páginas Razor en la aplicación
builder.Services.AddSingleton<PalabraService>(); // Agrega el servicio PalabraService como un singleton para que esté disponible en toda la aplicación
builder.Services.AddSingleton<TableroService>(); // Agrega el servicio TableroService como un singleton para que esté disponible en toda la aplicación
builder.Services.AddSingleton<GameSocketHandler>(); // Agrega el servicio GameSocketHandler como un singleton para que esté disponible en toda la aplicación
builder.Services.AddDistributedMemoryCache(); // Agrega un servicio de caché en memoria distribuida para almacenar datos de sesión u otros datos temporales
builder.Services.AddSession(o => o.IdleTimeout = TimeSpan.FromMinutes(30)); // Agrega servicios de sesión con un tiempo de espera de 30 minutos, lo que permite mantener la sesión activa durante ese período de inactividad

var app = builder.Build(); // Construye la aplicación web utilizando la configuración y los servicios definidos en el constructor
app.UseStaticFiles(); // Habilita el uso de archivos estáticos, lo que permite servir archivos como CSS, JavaScript e imágenes desde la carpeta wwwroot u otras ubicaciones configuradas
app.UseSession(); // Habilita el uso de sesiones en la aplicación, lo que permite almacenar y recuperar datos específicos de cada sesión de usuario
app.UseWebSockets(); // Habilita el soporte para WebSockets, lo que permite manejar conexiones WebSocket para comunicación en tiempo real entre el cliente y el servidor

app.Map("/ws/{sesionId}", async context => // Mapea la ruta "/ws/{sesionId}" para manejar solicitudes de WebSocket, donde {sesionId} es un parámetro que representa el ID de la sesión de juego
{
    if (context.WebSockets.IsWebSocketRequest) // Verifica si la solicitud es una solicitud de WebSocket
    {
        var sesionId = context.Request.RouteValues["sesionId"]?.ToString() ?? ""; // Obtiene el ID de la sesión de juego desde los valores de la ruta
        var ws = await context.WebSockets.AcceptWebSocketAsync(); // Acepta la conexión WebSocket y obtiene el objeto WebSocket para comunicarse con el cliente
        var handler = context.RequestServices.GetRequiredService<GameSocketHandler>(); // Obtiene una instancia del servicio GameSocketHandler para manejar la conexión WebSocket
        await handler.ManejarConexion(ws, sesionId, context.RequestAborted); // Llama al método ManejarConexion del GameSocketHandler para manejar la conexión WebSocket, pasando el socket, el ID de la sesión y el token de cancelación
    }
    else
    {
        context.Response.StatusCode = 400; // Si la solicitud no es una solicitud de WebSocket, responde con un código de estado 400 (Bad Request)
    }
});

app.MapRazorPages();
app.Run();
