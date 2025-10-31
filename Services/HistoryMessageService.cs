using System.Collections.Generic;
using System.Threading.Tasks;
using WhatsAppBot.Models;
using WhatsAppBot.Services.Interfaces;
using WhatsAppBot.Data.Repositories.Interfaces;

namespace WhatsAppBot.Services
{
    public class HistoryMessageService : IHistoryMessageService
    {
        private readonly IUnitOfWork _uow;

        public HistoryMessageService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task GuardarMensajeEnHistorial(MensajeWhatsApp mensaje)
        {
            await _uow.Mensajes.AddAsync(mensaje);
            await _uow.CompleteAsync();
        }

        public async Task<List<MensajeWhatsApp>> ObtenerHistorialPorTelefono(string telefono)
        {
            var list = (await _uow.Mensajes.GetByPhoneAsync(telefono));
            return new List<MensajeWhatsApp>(list);
        }
    }
}
