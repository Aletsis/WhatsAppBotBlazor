using WhatsAppBot.Models;

namespace WhatsAppBot.Services.Interfaces
{
    public interface IHistoryMessageService
    {
        Task GuardarMensajeEnHistorial(MensajeWhatsApp mensaje);
        Task<List<MensajeWhatsApp>> ObtenerHistorialPorTelefono(string telefono);
    }
}