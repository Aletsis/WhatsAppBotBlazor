using WhatsAppBot.Models;
using WhatsAppBot.Services.Interfaces;
using WhatsAppBot.Data.Repositories.Interfaces;

namespace WhatsAppBot.Services
{
    public class ConversacionService : IConversacionService
    {
        private readonly IUnitOfWork _uow;

        public ConversacionService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<EstadoConversacion> ObtenerOIniciarAsync(string numero)
        {
            var conv = await _uow.EstadosConversacion.GetByPhoneAsync(numero);

            if (conv == null)
            {
                conv = new EstadoConversacion
                {
                    Telefono = numero,
                    EstadoActual = "Inicio",
                    UltimaActualizacion = DateTime.Now,
                };

                await _uow.EstadosConversacion.AddAsync(conv);
                await _uow.CompleteAsync();
            }

            return conv;
        }

        public async Task ActualizarEstadoAsync(string numero, string nuevoEstado, string? datosTemporales = null)
        {
            var conv = await _uow.EstadosConversacion.GetByPhoneAsync(numero);
            if (conv != null)
            {
                conv.EstadoActual = nuevoEstado;
                conv.UltimaActualizacion = DateTime.Now;
                await _uow.EstadosConversacion.UpdateAsync(conv);
                await _uow.CompleteAsync();
            }
        }
    }
}

