using WhatsAppBot.Models;

namespace WhatsAppBot.Data.Repositories.Interfaces
{
    public interface IClienteRepository : IRepository<Cliente>
    {
        Task<Cliente?> GetByPhoneAsync(string telefono);
        Task<(IEnumerable<Cliente> Clientes, int TotalCount)> GetPagedAsync(
            int pageNumber, int pageSize, string? searchTerm = null);
        Task<IEnumerable<Cliente>> GetActiveClientsAsync();
        Task<IEnumerable<Cliente>> GetClientsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<bool> ExistsPhoneAsync(string telefono, int? excludeClienteId = null);
        Task<int> GetTotalActiveCountAsync();
        Task<IEnumerable<Cliente>> SearchByNameOrPhoneAsync(string searchTerm);
        Task<int> GetNewClientsCountAsync(DateTime since);
    }
}
