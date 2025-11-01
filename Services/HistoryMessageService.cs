using System.Collections.Generic;
using System.Threading.Tasks;
using WhatsAppBot.Models;
using WhatsAppBot.Services.Interfaces;
using WhatsAppBot.Data.Repositories.Interfaces;
using WhatsAppBot.Data.DTOs;

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
            var list = await _uow.Mensajes.GetByPhoneAsync(telefono);
            return new List<MensajeWhatsApp>(list);
        }
        public async Task<List<ConversationHistory>> GetConversationsByPhoneAsync(
        string phone, DateTime? startDate = null, DateTime? endDate = null)
        {
            var messages = await _uow.Mensajes.GetByPhoneAsync(phone);

            // Filtrar por fechas si se proporcionan
            if (startDate.HasValue)
                messages = messages.Where(m => m.Fecha >= startDate.Value);
            if (endDate.HasValue)
                messages = messages.Where(m => m.Fecha <= endDate.Value);

            return messages.GroupBy(m => m.Telefono)
                        .Select(g => new ConversationHistory
                        {
                            PhoneNumber = g.Key,
                            LastMessageDate = g.Max(m => m.Fecha),
                            MessageCount = g.Count()
                        })
                        .ToList();
        }
        public async Task<ConversationDetail> GetConversationDetailAsync(string phone)
        {
            var messages = await _uow.Mensajes.GetByPhoneAsync(phone);
            return new ConversationDetail
            {
                PhoneNumber = phone,
                Messages = messages.ToList()
            };
        }
        public async Task<List<ConversationHistory>> GetAllConversationsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var allMessages = await _uow.Mensajes.GetAllAsync();
            
            // Filtrar por fechas si se proporcionan
            if (startDate.HasValue)
                allMessages = allMessages.Where(m => m.Fecha >= startDate.Value);
            if (endDate.HasValue)
                allMessages = allMessages.Where(m => m.Fecha <= endDate.Value);

            return allMessages.GroupBy(m => m.Telefono)
                            .Select(g => new ConversationHistory
                            {
                                PhoneNumber = g.Key,
                                LastMessageDate = g.Max(m => m.Fecha),
                                MessageCount = g.Count()
                            })
                            .OrderByDescending(c => c.LastMessageDate)
                            .ToList();
        }
    }
}
