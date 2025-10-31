using WhatsAppBot.Models;

namespace WhatsAppBot.Data.Repositories.Interfaces
{
    public interface IMensajeRepository : IRepository<MensajeWhatsApp>
    {
        Task<IEnumerable<MensajeWhatsApp>> GetByPhoneAsync(string telefono);
    }
}