using WhatsAppBot.Data;
using WhatsAppBot.Models;
using WhatsAppBot.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace WhatsAppBot.Services
{
    public class HistoryMessageService : IHistoryMessageService
    {
        private readonly WhatsAppDbContext _context;

        public HistoryMessageService(WhatsAppDbContext context)
        {
            _context = context;
        }

        public async Task GuardarMensajeEnHistorial(MensajeWhatsApp mensaje)
        {
            _context.MensajesWhatsApp.Add(mensaje);
            await _context.SaveChangesAsync();
        }

        public async Task<List<MensajeWhatsApp>> ObtenerHistorialPorTelefono(string telefono)
        {
            return await _context.MensajesWhatsApp
                .Where(m => m.Telefono == telefono)
                .ToListAsync();
        }
    }
}
