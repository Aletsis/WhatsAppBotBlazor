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
            // ✅ FAIL-SAFE: Retornar datos por defecto si algo falla
            var defaultMetrics = new DashboardMetricsDTO
            {
                PedidosHoy = 0,
                PedidosTrend = 0,
                ConversacionesActivas = 0,
                ClientesNuevos = 0,
                MensajesEnviados = 0,
                MensajesRecibidos = 0,
                FechaActualizacion = DateTime.Now
            };

            try
            {
                _logger.LogInformation("Iniciando carga de métricas del dashboard");

                var today = DateTime.Today;
                var yesterday = today.AddDays(-1);
                var weekAgo = today.AddDays(-7);

                // ✅ Ejecutar cada métrica por separado con try-catch individual
                var pedidosHoy = await GetPedidosCountSafeAsync(today);
                var pedidosAyer = await GetPedidosCountSafeAsync(yesterday);
                var conversacionesActivas = await GetConversacionesActivasSafeAsync();
                var clientesNuevos = await GetClientesNuevosSafeAsync(weekAgo);
                var (enviados, recibidos) = await GetMensajesMetricsSafeAsync(today);

                // Calcular trend de forma segura
                var trend = pedidosAyer > 0 ? 
                    Math.Round(((double)(pedidosHoy - pedidosAyer) / pedidosAyer) * 100, 1) : 
                    (pedidosHoy > 0 ? 100.0 : 0.0);

                var metrics = new DashboardMetricsDTO
                {
                    PedidosHoy = pedidosHoy,
                    PedidosTrend = trend,
                    ConversacionesActivas = conversacionesActivas,
                    ClientesNuevos = clientesNuevos,
                    MensajesEnviados = enviados,
                    MensajesRecibidos = recibidos,
                    FechaActualizacion = DateTime.Now
                };

                _logger.LogInformation("Métricas del dashboard cargadas exitosamente: {@Metrics}", metrics);
                return metrics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico al obtener métricas del dashboard");
                return defaultMetrics;
            }
        }

        // ✅ Métodos auxiliares fail-safe
        private async Task<int> GetPedidosCountSafeAsync(DateTime date)
        {
            try
            {
                var pedidos = await _uow.Pedidos.GetAllAsync();
                return pedidos.Count(p => p.FechaPedido.Date == date);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al obtener pedidos para fecha {Date}", date);
                return 0;
            }
        }

        private async Task<int> GetConversacionesActivasSafeAsync()
        {
            try
            {
                var estados = await _uow.EstadosConversacion.GetAllAsync();
                return estados.Count();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al obtener conversaciones activas");
                return 0;
            }
        }

        private async Task<int> GetClientesNuevosSafeAsync(DateTime since)
        {
            try
            {
                var clientes = await _uow.Clientes.GetAllAsync();
                return clientes.Count(c => c.FechaRegistro >= since && c.Activo);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al obtener clientes nuevos desde {Since}", since);
                return 0;
            }
        }

        private async Task<(int Enviados, int Recibidos)> GetMensajesMetricsSafeAsync(DateTime today)
        {
            try
            {
                var tomorrow = today.AddDays(1);
                var mensajes = await _uow.Mensajes.GetAllAsync();
                
                var mensajesHoy = mensajes.Where(m => m.Fecha >= today && m.Fecha < tomorrow);
                
                var enviados = mensajesHoy.Count(m => m.DireccionConversacion == "outbound");
                var recibidos = mensajesHoy.Count(m => m.DireccionConversacion == "inbound");
                
                return (enviados, recibidos);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al obtener métricas de mensajes para {Today}", today);
                return (0, 0);
            }
        }

        // ✅ Implementar métodos requeridos por la interfaz con fail-safe
        public async Task<int> GetActiveConversationsCountAsync()
        {
            return await GetConversacionesActivasSafeAsync();
        }

        public async Task<int> GetNewClientsCountAsync(DateTime since)
        {
            return await GetClientesNuevosSafeAsync(since);
        }

        public async Task<int> GetMessagesSentTodayAsync()
        {
            try
            {
                var today = DateTime.Today;
                var (enviados, _) = await GetMensajesMetricsSafeAsync(today);
                return enviados;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al obtener mensajes enviados hoy");
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

                return pedidos
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
                
                var pedidosHoy = await GetPedidosCountSafeAsync(today);
                var pedidosAyer = await GetPedidosCountSafeAsync(yesterday);
                
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