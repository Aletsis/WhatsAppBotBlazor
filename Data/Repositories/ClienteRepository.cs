using Microsoft.EntityFrameworkCore;
using WhatsAppBot.Models;
using WhatsAppBot.Data.Repositories.Interfaces;

namespace WhatsAppBot.Data.Repositories
{
    public class ClienteRepository : Repository<Cliente>, IClienteRepository
    {
        public ClienteRepository(WhatsAppDbContext context) : base(context) { }

        public async Task<Cliente?> GetByPhoneAsync(string telefono)
        {
            return await _dbSet.FirstOrDefaultAsync(c => c.Telefono == telefono);
        }
    }
}