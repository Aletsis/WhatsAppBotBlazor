using WhatsAppBot.Data.Repositories.Interfaces;

namespace WhatsAppBot.Data.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly WhatsAppDbContext _context;
        private IClienteRepository? _clienteRepository;
        private IPedidoRepository? _pedidoRepository;
        private IMensajeRepository? _mensajeRepository;
        private IEstadoConversacionRepository? _estadoConversacionRepository;

        public UnitOfWork(WhatsAppDbContext context)
        {
            _context = context;
        }

        public IClienteRepository Clientes =>
            _clienteRepository ??= new ClienteRepository(_context);

        public IPedidoRepository Pedidos =>
            _pedidoRepository ??= new PedidoRepository(_context);

        public IMensajeRepository Mensajes =>
            _mensajeRepository ??= new MensajeRepository(_context);

        public IEstadoConversacionRepository EstadosConversacion =>
            _estadoConversacionRepository ??= new EstadoConversacionRepository(_context);

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }
        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
