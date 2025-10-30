using Microsoft.EntityFrameworkCore;
using WhatsAppBot.Data;
using WhatsAppBot.Models;
using WhatsAppBot.Services.Interfaces;

namespace WhatsAppBot.Services
{
    public class ClienteService : IClienteService
    {
        private readonly WhatsAppDbContext _context;

        public ClienteService(WhatsAppDbContext context)
        {
            _context = context;
        }

        public async Task<Cliente?> ObtenerPorNumeroAsync(string numero)
        {
            return await _context.Clientes
                .FirstOrDefaultAsync(c => c.Telefono == numero);
        }

        public async Task<Cliente> CrearAsync(Cliente cliente)
        {
            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();
            return cliente;
        }

        public async Task ActualizarAsync(Cliente cliente)
        {
            _context.Clientes.Update(cliente);
            await _context.SaveChangesAsync();
        }
    }
}
