namespace WhatsAppBot.Data.Repositories.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IClienteRepository Clientes { get; }
        IPedidoRepository Pedidos { get; }
        IMensajeRepository Mensajes { get; }
        IEstadoConversacionRepository EstadosConversacion { get; }
        Task<int> CompleteAsync();
    }
}
