using Microsoft.EntityFrameworkCore;
using WhatsAppBot.Models;
using WhatsAppBot.Data.Repositories.Interfaces;

namespace WhatsAppBot.Data.Repositories
{
    public class MensajeRepository : Repository<MensajeWhatsApp>, IMensajeRepository
    {
        public MensajeRepository(WhatsAppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<MensajeWhatsApp>> GetByPhoneAsync(string telefono)
        {
            return await _dbSet
                .Where(m => m.Telefono == telefono)
                .OrderByDescending(m => m.Fecha)
                .ToListAsync();
        }
    }
}