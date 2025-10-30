using WhatsAppBot.Models;

namespace WhatsAppBot.Services.Interfaces
{
    public interface IConversacionService
    {
        Task<EstadoConversacion> ObtenerOIniciarAsync(string numero);
        Task ActualizarEstadoAsync(string numero, string nuevoEstado, string? datosTemporales = null);
    }
}
