using Microsoft.EntityFrameworkCore;
using WhatsAppBot.Models;

namespace WhatsAppBot.Data
{
    public class WhatsAppDbContext : DbContext
    {
        public WhatsAppDbContext(DbContextOptions<WhatsAppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<MensajeWhatsApp> MensajesWhatsApp { get; set; }
        public DbSet<EstadoConversacion> EstadosConversacion { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Cliente>()
                .HasIndex(c => c.Telefono)
                .IsUnique();

            modelBuilder.Entity<EstadoConversacion>()
                .HasKey(e => e.Telefono);

            modelBuilder.Entity<MensajeWhatsApp>()
                .HasKey(d => d.MensajeId);

            modelBuilder.Entity<Pedido>()
                .HasKey(c => c.PedidoId);

        }
    }
}
