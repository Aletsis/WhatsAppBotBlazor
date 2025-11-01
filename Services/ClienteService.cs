using WhatsAppBot.Models;
using WhatsAppBot.Services.Interfaces;
using WhatsAppBot.Data.Repositories.Interfaces;
using WhatsAppBot.Data.DTOs;
using Microsoft.Extensions.Logging;

namespace WhatsAppBot.Services
{
    public class ClienteService : IClienteService
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<ClienteService> _logger;

        public ClienteService(IUnitOfWork uow, ILogger<ClienteService> logger)
        {
            _uow = uow;
            _logger = logger;
        }

        public async Task<Cliente?> ObtenerPorNumeroAsync(string numero)
        {
            return await _uow.Clientes.GetByPhoneAsync(numero);
        }

        public async Task<Cliente> CrearAsync(Cliente cliente)
        {
            await _uow.Clientes.AddAsync(cliente);
            await _uow.CompleteAsync();
            return cliente;
        }

        public async Task ActualizarAsync(Cliente cliente)
        {
            await _uow.Clientes.UpdateAsync(cliente);
            await _uow.CompleteAsync();
        }
        public async Task<Cliente?> ObtenerPorIdAsync(int id)
        {
            try
            {
                return await _uow.Clientes.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener cliente por ID: {ClienteId}", id);
                throw;
            }
        }

        public async Task<ClientePagedResultDTO> ObtenerClientesPaginadosAsync(ClienteSearchFilterDTO filtros)
        {
            try
            {
                var (clientes, totalCount) = await _uow.Clientes.GetPagedAsync(
                    filtros.PageNumber, filtros.PageSize, filtros.SearchTerm);

                var clientesDTO = clientes.Select(c => new ClienteListDTO
                {
                    ClienteId = c.ClienteId,
                    Nombre = c.Nombre,
                    Telefono = c.Telefono,
                    Direccion = c.Direccion,
                    FechaRegistro = c.FechaRegistro,
                    Activo = c.Activo,
                    TotalPedidos = c.Pedidos?.Count ?? 0,
                    UltimoPedido = c.Pedidos?.OrderByDescending(p => p.FechaPedido).FirstOrDefault()?.FechaPedido
                }).ToList();

                return new ClientePagedResultDTO
                {
                    Clientes = clientesDTO,
                    TotalCount = totalCount,
                    PageNumber = filtros.PageNumber,
                    PageSize = filtros.PageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener clientes paginados");
                throw;
            }
        }

        public async Task<List<ClienteListDTO>> ObtenerTodosActivosAsync()
        {
            try
            {
                var clientes = await _uow.Clientes.GetActiveClientsAsync();
                return clientes.Select(c => new ClienteListDTO
                {
                    ClienteId = c.ClienteId,
                    Nombre = c.Nombre,
                    Telefono = c.Telefono,
                    Direccion = c.Direccion,
                    FechaRegistro = c.FechaRegistro,
                    Activo = c.Activo,
                    TotalPedidos = c.Pedidos?.Count ?? 0,
                    UltimoPedido = c.Pedidos?.OrderByDescending(p => p.FechaPedido).FirstOrDefault()?.FechaPedido
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener clientes activos");
                throw;
            }
        }

        public async Task<ClienteEstadisticasDTO> ObtenerEstadisticasAsync()
        {
            try
            {
                var todosClientes = await _uow.Clientes.GetAllAsync();
                var inicioMes = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

                return new ClienteEstadisticasDTO
                {
                    TotalClientes = todosClientes.Count(),
                    ClientesActivos = todosClientes.Count(c => c.Activo),
                    ClientesInactivos = todosClientes.Count(c => !c.Activo),
                    ClientesNuevosEsteMes = todosClientes.Count(c => c.FechaRegistro >= inicioMes),
                    ClientesConPedidos = todosClientes.Count(c => c.Pedidos != null && c.Pedidos.Any())
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas de clientes");
                throw;
            }
        }

        public async Task<bool> ExisteTelefonoAsync(string telefono, int? excludeId = null)
        {
            try
            {
                return await _uow.Clientes.ExistsPhoneAsync(telefono, excludeId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar existencia de teléfono: {Telefono}", telefono);
                throw;
            }
        }

        public async Task ActivarDesactivarAsync(int id, bool activo)
        {
            try
            {
                var cliente = await _uow.Clientes.GetByIdAsync(id);
                if (cliente != null)
                {
                    cliente.Activo = activo;
                    await _uow.Clientes.UpdateAsync(cliente);
                    await _uow.CompleteAsync();
                    
                    _logger.LogInformation("Cliente {ClienteId} {Estado}", id, activo ? "activado" : "desactivado");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al activar/desactivar cliente: {ClienteId}", id);
                throw;
            }
        }

        public async Task EliminarAsync(int id)
        {
            try
            {
                var cliente = await _uow.Clientes.GetByIdAsync(id);
                if (cliente != null)
                {
                    await _uow.Clientes.DeleteAsync(cliente);
                    await _uow.CompleteAsync();
                    
                    _logger.LogInformation("Cliente {ClienteId} eliminado", id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar cliente: {ClienteId}", id);
                throw;
            }
        }

        public async Task<List<ClienteListDTO>> BuscarAsync(string termino)
        {
            try
            {
                var clientes = await _uow.Clientes.SearchByNameOrPhoneAsync(termino);
                return clientes.Select(c => new ClienteListDTO
                {
                    ClienteId = c.ClienteId,
                    Nombre = c.Nombre,
                    Telefono = c.Telefono,
                    Direccion = c.Direccion,
                    FechaRegistro = c.FechaRegistro,
                    Activo = c.Activo,
                    TotalPedidos = c.Pedidos?.Count ?? 0,
                    UltimoPedido = c.Pedidos?.OrderByDescending(p => p.FechaPedido).FirstOrDefault()?.FechaPedido
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar clientes con término: {Termino}", termino);
                throw;
            }
        }

        public async Task<Cliente> CrearDesdeDTO(ClienteCreateDTO clienteDto)
        {
            try
            {
                var cliente = new Cliente
                {
                    Nombre = clienteDto.Nombre,
                    Telefono = clienteDto.Telefono,
                    Direccion = clienteDto.Direccion,
                    Activo = clienteDto.Activo,
                    FechaRegistro = DateTime.Now
                };

                await _uow.Clientes.AddAsync(cliente);
                await _uow.CompleteAsync();
                
                _logger.LogInformation("Cliente creado: {ClienteId} - {Nombre}", cliente.ClienteId, cliente.Nombre);
                return cliente;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear cliente desde DTO");
                throw;
            }
        }

        public async Task ActualizarDesdeDTO(ClienteUpdateDTO clienteDto)
        {
            try
            {
                var cliente = await _uow.Clientes.GetByIdAsync(clienteDto.ClienteId);
                if (cliente != null)
                {
                    cliente.Nombre = clienteDto.Nombre;
                    cliente.Telefono = clienteDto.Telefono;
                    cliente.Direccion = clienteDto.Direccion;
                    cliente.Activo = clienteDto.Activo;

                    await _uow.Clientes.UpdateAsync(cliente);
                    await _uow.CompleteAsync();
                    
                    _logger.LogInformation("Cliente actualizado: {ClienteId} - {Nombre}", cliente.ClienteId, cliente.Nombre);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar cliente desde DTO: {ClienteId}", clienteDto.ClienteId);
                throw;
            }
        }

        public async Task<ClienteUpdateDTO?> ObtenerParaEdicionAsync(int id)
        {
            try
            {
                var cliente = await _uow.Clientes.GetByIdAsync(id);
                if (cliente == null) return null;

                return new ClienteUpdateDTO
                {
                    ClienteId = cliente.ClienteId,
                    Nombre = cliente.Nombre,
                    Telefono = cliente.Telefono,
                    Direccion = cliente.Direccion,
                    Activo = cliente.Activo
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener cliente para edición: {ClienteId}", id);
                throw;
            }
        }
    }
}
