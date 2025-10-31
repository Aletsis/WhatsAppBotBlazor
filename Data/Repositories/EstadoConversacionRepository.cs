using Microsoft.EntityFrameworkCore;
using WhatsAppBot.Models;
using WhatsAppBot.Data.Repositories.Interfaces;

namespace WhatsAppBot.Data.Repositories
{
    public class EstadoConversacionRepository : Repository<EstadoConversacion>, IEstadoConversacionRepository
    {
        public EstadoConversacionRepository(WhatsAppDbContext context) : base(context)
        {
        }

        public async Task<EstadoConversacion?> GetByPhoneAsync(string telefono)
        {
            return await _dbSet.FirstOrDefaultAsync(e => e.Telefono == telefono);
        }

        public async Task<EstadoConversacion> CreateOrUpdateAsync(string telefono, string estado)
        {
            var conversacion = await GetByPhoneAsync(telefono);
            
            if (conversacion == null)
            {
                conversacion = new EstadoConversacion
                {
                    Telefono = telefono,
                    EstadoActual = estado,
                    UltimaActualizacion = DateTime.Now
                };
                await AddAsync(conversacion);
            }
            else
            {
                conversacion.EstadoActual = estado;
                conversacion.UltimaActualizacion = DateTime.Now;
                await UpdateAsync(conversacion);
            }

            await SaveChangesAsync();
            return conversacion;
        }
    }
}