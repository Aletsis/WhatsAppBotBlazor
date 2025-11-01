using WhatsAppBot.Models;
using WhatsAppBot.Data.DTOs;

namespace WhatsAppBot.Services.Interfaces
{
    public interface IClienteService
    {
        Task<Cliente?> ObtenerPorNumeroAsync(string numero);
        Task<Cliente> CrearAsync(Cliente cliente);
        Task ActualizarAsync(Cliente cliente);
        Task<Cliente?> ObtenerPorIdAsync(int id);
        Task<ClientePagedResultDTO> ObtenerClientesPaginadosAsync(ClienteSearchFilterDTO filtros);
        Task<List<ClienteListDTO>> ObtenerTodosActivosAsync();
        Task<ClienteEstadisticasDTO> ObtenerEstadisticasAsync();
        Task<bool> ExisteTelefonoAsync(string telefono, int? excludeId = null);
        Task ActivarDesactivarAsync(int id, bool activo);
        Task EliminarAsync(int id);
        Task<List<ClienteListDTO>> BuscarAsync(string termino);
        Task<Cliente> CrearDesdeDTO(ClienteCreateDTO clienteDto);
        Task ActualizarDesdeDTO(ClienteUpdateDTO clienteDto);
        Task<ClienteUpdateDTO?> ObtenerParaEdicionAsync(int id);
    }
}
