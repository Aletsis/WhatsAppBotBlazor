using Microsoft.EntityFrameworkCore;
using WhatsAppBot.Data.DTOs;
using WhatsAppBot.Data.Repositories.Interfaces;
using WhatsAppBot.Services.Interfaces;

namespace WhatsAppBot.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<DashboardService> _logger;

        public DashboardService(IUnitOfWork uow, ILogger<DashboardService> logger)
        {
            _uow = uow;
            _logger = logger;
        }

        public async Task<DashboardMetricsDTO> GetMetricsAsync()
        {
            try
            {
                var today = DateTime.Today;
                var yesterday = today.AddDays(-1);
                var weekAgo = today.AddDays(-7);

                // Obtener métricas en paralelo para mejor rendimiento
                var metricsTask = GetPedidosMetricsAsync(today, yesterday);
                var conversacionesTask = GetActiveConversationsCountAsync();
                var clientesTask = GetNewClientsCountAsync(weekAgo);
                var mensajesTask = GetMessagesMetricsAsync(today);

                await Task.WhenAll(metricsTask, conversacionesTask, clientesTask, mensajesTask);

                var pedidosMetrics = await metricsTask;
                var conversacionesActivas = await conversacionesTask;
                var clientesNuevos = await clientesTask;
                var mensajesMetrics = await mensajesTask;

                return new DashboardMetricsDTO
                {
                    PedidosHoy = pedidosMetrics.PedidosHoy,
                    PedidosTrend = pedidosMetrics.Trend,
                    ConversacionesActivas = conversacionesActivas,
                    ClientesNuevos = clientesNuevos,
                    MensajesEnviados = mensajesMetrics.Enviados,
                    MensajesRecibidos = mensajesMetrics.Recibidos,
                    FechaActualizacion = DateTime.Now
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener métricas del dashboard");
                return new DashboardMetricsDTO(); // Devolver métricas en cero en caso de error
            }
        }

        public async Task<int> GetActiveConversationsCountAsync()
        {
            try
            {
                var last24Hours = DateTime.Now.AddHours(-24);
                
                // Contar conversaciones con actividad en las últimas 24 horas
                var mensajesRecientes = await _uow.Mensajes.GetAllAsync();
                
                var conversacionesActivas = mensajesRecientes
                    .Where(m => m.Fecha >= last24Hours)
                    .GroupBy(m => m.Telefono)
                    .Count();

                return conversacionesActivas;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener conversaciones activas");
                return 0;
            }
        }

        public async Task<int> GetNewClientsCountAsync(DateTime since)
        {
            try
            {
                var clientes = await _uow.Clientes.GetAllAsync();
                return clientes.Count(c => c.FechaRegistro >= since && c.Activo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener clientes nuevos");
                return 0;
            }
        }

        public async Task<int> GetMessagesSentTodayAsync()
        {
            try
            {
                var today = DateTime.Today;
                var tomorrow = today.AddDays(1);
                
                var mensajes = await _uow.Mensajes.GetAllAsync();
                return mensajes.Count(m => m.Fecha >= today && 
                                         m.Fecha < tomorrow && 
                                         m.DireccionConversacion == "salida");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener mensajes enviados hoy");
                return 0;
            }
        }

        public async Task<List<EstadoPedidoMetricDTO>> GetPedidosEstadisticasAsync()
        {
            try
            {
                var pedidos = await _uow.Pedidos.GetAllAsync();
                var totalPedidos = pedidos.Count();

                if (totalPedidos == 0)
                    return new List<EstadoPedidoMetricDTO>();

                var estadisticas = pedidos
                    .GroupBy(p => p.Estado)
                    .Select(g => new EstadoPedidoMetricDTO
                    {
                        Estado = g.Key,
                        Cantidad = g.Count(),
                        Porcentaje = (double)g.Count() / totalPedidos * 100,
                        Color = GetEstadoColor(g.Key)
                    })
                    .OrderByDescending(e => e.Cantidad)
                    .ToList();

                return estadisticas;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas de pedidos");
                return new List<EstadoPedidoMetricDTO>();
            }
        }

        public async Task<List<PedidoRecenteDTO>> GetPedidosRecientesAsync(int count = 10)
        {
            try
            {
                var pedidos = await _uow.Pedidos.GetAllAsync();
                
                return pedidos
                    .OrderByDescending(p => p.FechaPedido)
                    .Take(count)
                    .Select(p => new PedidoRecenteDTO
                    {
                        PedidoId = p.PedidoId,
                        Folio = p.Folio,
                        ClienteNombre = p.Cliente?.Nombre ?? "Cliente no encontrado",
                        ClienteTelefono = p.Cliente?.Telefono ?? "N/A",
                        Estado = p.Estado,
                        FechaPedido = p.FechaPedido,
                        DetallePedido = p.DetallePedido.Length > 50 ? 
                            p.DetallePedido.Substring(0, 50) + "..." : 
                            p.DetallePedido
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener pedidos recientes");
                return new List<PedidoRecenteDTO>();
            }
        }

        public async Task<double> GetPedidosTrendAsync()
        {
            try
            {
                var today = DateTime.Today;
                var yesterday = today.AddDays(-1);
                
                var pedidos = await _uow.Pedidos.GetAllAsync();
                
                var pedidosHoy = pedidos.Count(p => p.FechaPedido.Date == today);
                var pedidosAyer = pedidos.Count(p => p.FechaPedido.Date == yesterday);
                
                if (pedidosAyer == 0)
                    return pedidosHoy > 0 ? 100 : 0;
                
                return ((double)(pedidosHoy - pedidosAyer) / pedidosAyer) * 100;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al calcular trend de pedidos");
                return 0;
            }
        }

        // Métodos privados auxiliares
        private async Task<(int PedidosHoy, double Trend)> GetPedidosMetricsAsync(DateTime today, DateTime yesterday)
        {
            var pedidos = await _uow.Pedidos.GetAllAsync();
            
            var pedidosHoy = pedidos.Count(p => p.FechaPedido.Date == today);
            var pedidosAyer = pedidos.Count(p => p.FechaPedido.Date == yesterday);
            
            var trend = pedidosAyer > 0 ? 
                ((double)(pedidosHoy - pedidosAyer) / pedidosAyer) * 100 : 
                (pedidosHoy > 0 ? 100 : 0);
            
            return (pedidosHoy, trend);
        }

        private async Task<(int Enviados, int Recibidos)> GetMessagesMetricsAsync(DateTime today)
        {
            var tomorrow = today.AddDays(1);
            var mensajes = await _uow.Mensajes.GetAllAsync();
            
            var mensajesHoy = mensajes.Where(m => m.Fecha >= today && m.Fecha < tomorrow);
            
            var enviados = mensajesHoy.Count(m => m.DireccionConversacion == "salida");
            var recibidos = mensajesHoy.Count(m => m.DireccionConversacion == "entrada");
            
            return (enviados, recibidos);
        }

        private string GetEstadoColor(string estado) => estado switch
        {
            "En espera" => "warning",
            "En proceso" => "info",
            "Entregado" => "success",
            "Cancelado" => "error",
            "Pendiente" => "default",
            _ => "secondary"
        };
    }
}