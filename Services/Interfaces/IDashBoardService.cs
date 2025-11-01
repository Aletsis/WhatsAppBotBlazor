using WhatsAppBot.Data.DTOs;

namespace WhatsAppBot.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardMetricsDTO> GetMetricsAsync();
        Task<int> GetActiveConversationsCountAsync();
        Task<int> GetNewClientsCountAsync(DateTime since);
        Task<int> GetMessagesSentTodayAsync();
        Task<List<EstadoPedidoMetricDTO>> GetPedidosEstadisticasAsync();
        Task<List<PedidoRecenteDTO>> GetPedidosRecientesAsync(int count = 10);
        Task<double> GetPedidosTrendAsync();
    }
}