using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WhatsAppBot.Models;
using WhatsAppBot.Data.DTOs;

namespace WhatsAppBot.Services.Interfaces
{
    public interface IHistoryMessageService
    {
        Task GuardarMensajeEnHistorial(MensajeWhatsApp mensaje);
        Task<List<MensajeWhatsApp>> ObtenerHistorialPorTelefono(string telefono);

        // Conversaciones agrupadas/resumen
        Task<List<ConversationHistory>> GetConversationsByPhoneAsync(string phone, DateTime? startDate = null, DateTime? endDate = null);

        // Detalle de una conversación (lista de mensajes)
        Task<ConversationDetail> GetConversationDetailAsync(string phone);
        Task<List<ConversationHistory>> GetAllConversationsAsync(DateTime? startDate = null, DateTime? endDate = null);
    }
}