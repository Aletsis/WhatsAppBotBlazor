using Microsoft.EntityFrameworkCore;
using WhatsAppBot.Data;
using WhatsAppBot.Models;
using WhatsAppBot.Services.Interfaces;

namespace WhatsAppBot.Services
{
    public class ConversacionService : IConversacionService
    {
        private readonly WhatsAppDbContext _context;

        public ConversacionService(WhatsAppDbContext context)
        {
            _context = context;
        }

        public async Task<EstadoConversacion> ObtenerOIniciarAsync(string numero)
        {
            var conv = await _context.EstadosConversacion
                .FirstOrDefaultAsync(c => c.Telefono == numero);

            if (conv == null)
            {
                conv = new EstadoConversacion
                {
                    Telefono = numero,
                    EstadoActual = "Inicio",
                    UltimaActualizacion = DateTime.Now,
                };
                _context.EstadosConversacion.Add(conv);
                await _context.SaveChangesAsync();
            }

            return conv;
        }

        public async Task ActualizarEstadoAsync(string numero, string nuevoEstado, string? datosTemporales = null)
        {
            var conv = await _context.EstadosConversacion
                .FirstOrDefaultAsync(c => c.Telefono == numero);

            if (conv != null)
            {
                conv.EstadoActual = nuevoEstado;
                conv.UltimaActualizacion = DateTime.Now;
                _context.EstadosConversacion.Update(conv);
                await _context.SaveChangesAsync();
            }
        }
    }
}

