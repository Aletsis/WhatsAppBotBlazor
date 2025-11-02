using WhatsAppBot.Models;
using WhatsAppBot.Services.Interfaces;
using WhatsAppBot.Data.Repositories.Interfaces;
using WhatsAppBot.Data.DTOs;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace WhatsAppBot.Services
{
    public class PedidoService : IPedidoService
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<PedidoService> _logger;

        public PedidoService(IUnitOfWork uow, ILogger<PedidoService> logger)
        {
            _uow = uow;
            _logger = logger;
        }
        public async Task<List<Pedido>> ObtenerPedidosAsync()
        {
            var list = (await _uow.Pedidos.GetAllAsync()).ToList();
            return list.OrderByDescending(p => p.FechaPedido).ToList();
        }

        public async Task<Pedido?> ObtenerPedidoPorIdAsync(int id)
        {
            return await _uow.Pedidos.GetByIdAsync(id);
        }

        public async Task AgregarPedidoAsync(Pedido pedido)
        {
            await _uow.Pedidos.AddAsync(pedido);
            await _uow.CompleteAsync();
        }

        public async Task ActualizarEstadoAsync(int id, string nuevoEstado)
        {
            var pedido = await _uow.Pedidos.GetByIdAsync(id);
            if (pedido == null) return;

            pedido.Estado = nuevoEstado;
            await _uow.Pedidos.UpdateAsync(pedido);
            await _uow.CompleteAsync();
        }

        public async Task<Pedido> CrearAsync(Pedido pedido)
        {
            await _uow.Pedidos.AddAsync(pedido);
            await _uow.CompleteAsync();
            return pedido;
        }

        public async Task ActualizarAsync(Pedido pedido)
        {
            await _uow.Pedidos.UpdateAsync(pedido);
            await _uow.CompleteAsync();
        }

        public async Task<Pedido?> ObtenerPorFolioAsync(string folio)
        {
            return await _uow.Pedidos.GetByFolioAsync(folio);
        }

        public async Task<Pedido?> ObtenerUltimoPedidoAsync(string telefonoCliente)
        {
            return await _uow.Pedidos.GetLastOrderByPhoneAsync(telefonoCliente);
        }

        public string GenerarFolio()
        {
            string prefijo = "PED";
            string random = new Random().Next(1000, 9999).ToString();
            return $"{prefijo}{random}";
        }
        public async Task<ReportePedidosDTO> GenerarReporteAsync(ReporteFiltrosDTO filtros)
        {
            try
            {
                var (fechaDesde, fechaHasta) = ObtenerRangoFechas(filtros);
                
                _logger.LogInformation("Generando reporte de pedidos desde {FechaDesde} hasta {FechaHasta}", 
                    fechaDesde, fechaHasta);

                // Ejecutar todas las consultas en paralelo para mejor rendimiento
                var resumenTask = ObtenerResumenVentasAsync(fechaDesde, fechaHasta);
                var ventasPorDiaTask = ObtenerVentasPorDiaAsync(fechaDesde, fechaHasta);
                var ventasPorEstadoTask = ObtenerVentasPorEstadoAsync(fechaDesde, fechaHasta);
                var ventasPorFormapagoTask = ObtenerVentasPorFormaPagoAsync(fechaDesde, fechaHasta);
                var clientesTopTask = ObtenerClientesTopAsync(10, fechaDesde, fechaHasta);
                var productosPopularesTask = ObtenerProductosPopularesAsync(10, fechaDesde, fechaHasta);
                var tendenciasTask = ObtenerTendenciasAsync();

                await Task.WhenAll(resumenTask, ventasPorDiaTask, ventasPorEstadoTask, 
                    ventasPorFormapagoTask, clientesTopTask, productosPopularesTask, tendenciasTask);

                return new ReportePedidosDTO
                {
                    FechaDesde = fechaDesde,
                    FechaHasta = fechaHasta,
                    ResumenVentas = await resumenTask,
                    VentasPorDia = await ventasPorDiaTask,
                    VentasPorEstado = await ventasPorEstadoTask,
                    VentasPorFormaPago = await ventasPorFormapagoTask,
                    ClientesTop = await clientesTopTask,
                    ProductosPopulares = await productosPopularesTask,
                    Tendencias = await tendenciasTask
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar reporte de pedidos");
                throw;
            }
        }

        public async Task<List<VentasPorDiaDTO>> ObtenerVentasPorDiaAsync(DateTime fechaDesde, DateTime fechaHasta)
        {
            try
            {
                var pedidosPorDia = await _uow.Pedidos.GetPedidosPorDiaAsync(fechaDesde, fechaHasta);
                var pedidos = await _uow.Pedidos.GetByDateRangeAsync(fechaDesde, fechaHasta);

                var resultado = new List<VentasPorDiaDTO>();
                
                for (var fecha = fechaDesde.Date; fecha <= fechaHasta.Date; fecha = fecha.AddDays(1))
                {
                    var pedidosDelDia = pedidos.Where(p => p.FechaPedido.Date == fecha);
                    
                    resultado.Add(new VentasPorDiaDTO
                    {
                        Fecha = fecha,
                        CantidadPedidos = pedidosDelDia.Count(),
                        PedidosCompletados = pedidosDelDia.Count(p => p.Estado == "Entregado"),
                        PedidosCancelados = pedidosDelDia.Count(p => p.Estado == "Cancelado")
                    });
                }

                return resultado.OrderBy(v => v.Fecha).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ventas por día");
                throw;
            }
        }

        public async Task<List<VentasPorEstadoDTO>> ObtenerVentasPorEstadoAsync(DateTime? fechaDesde = null, DateTime? fechaHasta = null)
        {
            try
            {
                var estadisticas = await _uow.Pedidos.GetEstadisticasPorEstadoAsync(fechaDesde, fechaHasta);
                var total = estadisticas.Values.Sum();

                if (total == 0) return new List<VentasPorEstadoDTO>();

                return estadisticas.Select(kvp => new VentasPorEstadoDTO
                {
                    Estado = kvp.Key,
                    Cantidad = kvp.Value,
                    Porcentaje = (double)kvp.Value / total * 100,
                    Color = GetEstadoColor(kvp.Key),
                    IconoMaterial = GetEstadoIcon(kvp.Key)
                }).OrderByDescending(v => v.Cantidad).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ventas por estado");
                throw;
            }
        }

        public async Task<List<VentasPorFormapagoDTO>> ObtenerVentasPorFormaPagoAsync(DateTime? fechaDesde = null, DateTime? fechaHasta = null)
        {
            try
            {
                var estadisticas = await _uow.Pedidos.GetEstadisticasPorFormaPagoAsync(fechaDesde, fechaHasta);
                var total = estadisticas.Values.Sum();

                if (total == 0) return new List<VentasPorFormapagoDTO>();

                return estadisticas.Select(kvp => new VentasPorFormapagoDTO
                {
                    FormaPago = kvp.Key ?? "Sin especificar",
                    Cantidad = kvp.Value,
                    Porcentaje = (double)kvp.Value / total * 100,
                    Color = GetFormaPagoColor(kvp.Key)
                }).OrderByDescending(v => v.Cantidad).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ventas por forma de pago");
                throw;
            }
        }

        public async Task<List<ClienteTopDTO>> ObtenerClientesTopAsync(int cantidad = 10, DateTime? fechaDesde = null, DateTime? fechaHasta = null)
        {
            try
            {
                var clientesData = await _uow.Pedidos.GetClientesMasFrecuentesAsync(cantidad, fechaDesde, fechaHasta);
                
                return clientesData.Select(c => new ClienteTopDTO
                {
                    ClienteId = c.ClienteId,
                    Nombre = c.ClienteNombre,
                    Telefono = c.ClienteTelefono,
                    CantidadPedidos = c.CantidadPedidos,
                    UltimoPedido = c.UltimoPedido
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener clientes top");
                throw;
            }
        }

        public async Task<List<ProductoPopularDTO>> ObtenerProductosPopularesAsync(int cantidad = 10, DateTime? fechaDesde = null, DateTime? fechaHasta = null)
        {
            try
            {
                var pedidos = await _uow.Pedidos.GetWithFiltersAsync(fechaDesde, fechaHasta, null, null, null);
                
                // Extraer productos de los detalles de pedidos usando regex
                var productos = new Dictionary<string, int>();
                var clientesPorProducto = new Dictionary<string, HashSet<string>>();

                foreach (var pedido in pedidos)
                {
                    var productosEnPedido = ExtraerProductos(pedido.DetallePedido);
                    
                    foreach (var producto in productosEnPedido)
                    {
                        var productoNormalizado = NormalizarNombreProducto(producto);
                        
                        if (!productos.ContainsKey(productoNormalizado))
                        {
                            productos[productoNormalizado] = 0;
                            clientesPorProducto[productoNormalizado] = new HashSet<string>();
                        }
                        
                        productos[productoNormalizado]++;
                        clientesPorProducto[productoNormalizado].Add(pedido.Cliente?.Nombre ?? "Cliente Anónimo");
                    }
                }

                var totalMenciones = productos.Values.Sum();
                if (totalMenciones == 0) return new List<ProductoPopularDTO>();

                return productos
                    .OrderByDescending(kvp => kvp.Value)
                    .Take(cantidad)
                    .Select(kvp => new ProductoPopularDTO
                    {
                        Producto = kvp.Key,
                        Menciones = kvp.Value,
                        Porcentaje = (double)kvp.Value / totalMenciones * 100,
                        ClientesFrecuentes = clientesPorProducto[kvp.Key].Take(5).ToList()
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos populares");
                throw;
            }
        }

        public async Task<TendenciasDTO> ObtenerTendenciasAsync()
        {
            try
            {
                var hoy = DateTime.Today;
                var semanaAnterior = hoy.AddDays(-7);
                var mesAnterior = hoy.AddDays(-30);

                var pedidosEstaSemana = await _uow.Pedidos.GetCountByDateRangeAsync(semanaAnterior, hoy);
                var pedidosSemanaPasada = await _uow.Pedidos.GetCountByDateRangeAsync(semanaAnterior.AddDays(-7), semanaAnterior);
                
                var pedidosEsteMes = await _uow.Pedidos.GetCountByDateRangeAsync(mesAnterior, hoy);
                var pedidosMesPasado = await _uow.Pedidos.GetCountByDateRangeAsync(mesAnterior.AddDays(-30), mesAnterior);

                var tendenciaSemanal = pedidosSemanaPasada > 0 ? 
                    ((double)(pedidosEstaSemana - pedidosSemanaPasada) / pedidosSemanaPasada) * 100 : 
                    (pedidosEstaSemana > 0 ? 100 : 0);

                var tendenciaMensual = pedidosMesPasado > 0 ? 
                    ((double)(pedidosEsteMes - pedidosMesPasado) / pedidosMesPasado) * 100 : 
                    (pedidosEsteMes > 0 ? 100 : 0);

                // Obtener mejor día de la semana
                var pedidosUltimaSemana = await _uow.Pedidos.GetByDateRangeAsync(semanaAnterior, hoy);
                var mejorDia = pedidosUltimaSemana
                    .GroupBy(p => p.FechaPedido.DayOfWeek)
                    .OrderByDescending(g => g.Count())
                    .FirstOrDefault()?.Key.ToString() ?? "No determinado";

                var clientesNuevos = await _uow.Clientes.GetNewClientsCountAsync(mesAnterior);

                return new TendenciasDTO
                {
                    TendenciaSemanal = tendenciaSemanal,
                    TendenciaMensual = tendenciaMensual,
                    MejorDiaSemana = TraducirDiaSemana(mejorDia),
                    HoraPico = "12:00 - 14:00", // Esto se podría calcular con más detalle
                    CrecimientoClientesNuevos = clientesNuevos
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener tendencias");
                throw;
            }
        }

        public async Task<ResumenVentasDTO> ObtenerResumenVentasAsync(DateTime fechaDesde, DateTime fechaHasta)
        {
            try
            {
                var pedidos = await _uow.Pedidos.GetByDateRangeAsync(fechaDesde, fechaHasta);
                
                var totalPedidos = pedidos.Count();
                var completados = pedidos.Count(p => p.Estado == "Entregado");
                var pendientes = pedidos.Count(p => p.Estado == "Pendiente" || p.Estado == "En proceso");
                var cancelados = pedidos.Count(p => p.Estado == "Cancelado");

                // Calcular promedios por tipo de cliente
                var clientesConPedidos = pedidos.GroupBy(p => p.ClienteId);
                var clientesNuevos = clientesConPedidos.Where(g => g.Count() == 1);
                var clientesRecurrentes = clientesConPedidos.Where(g => g.Count() > 1);

                return new ResumenVentasDTO
                {
                    TotalPedidos = totalPedidos,
                    PedidosCompletados = completados,
                    PedidosPendientes = pendientes,
                    PedidosCancelados = cancelados,
                    PromedioClienteNuevo = clientesNuevos.Any() ? clientesNuevos.Average(g => g.Count()) : 0,
                    PromedioClienteRecurrente = clientesRecurrentes.Any() ? clientesRecurrentes.Average(g => g.Count()) : 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener resumen de ventas");
                throw;
            }
        }

        public async Task<List<Pedido>> ObtenerPedidosFiltradosAsync(ReporteFiltrosDTO filtros)
        {
            try
            {
                var (fechaDesde, fechaHasta) = ObtenerRangoFechas(filtros);
                var pedidos = await _uow.Pedidos.GetWithFiltersAsync(
                    fechaDesde, fechaHasta, filtros.Estado, filtros.FormaPago, filtros.ClienteId);
                
                return pedidos.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener pedidos filtrados");
                throw;
            }
        }

        public async Task<byte[]> ExportarReporteAsync(ReporteFiltrosDTO filtros, string formato = "PDF")
        {
            try
            {
                var reporte = await GenerarReporteAsync(filtros);
                
                // Por ahora retornamos un placeholder - se puede implementar con librerías como iTextSharp o EPPlus
                var contenido = $"Reporte de Pedidos - {reporte.FechaDesde:dd/MM/yyyy} a {reporte.FechaHasta:dd/MM/yyyy}\n";
                contenido += $"Total de Pedidos: {reporte.ResumenVentas.TotalPedidos}\n";
                contenido += $"Pedidos Completados: {reporte.ResumenVentas.PedidosCompletados}\n";
                contenido += $"Tasa de Completitud: {reporte.ResumenVentas.TasaCompletitud:F2}%\n";
                
                return System.Text.Encoding.UTF8.GetBytes(contenido);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al exportar reporte");
                throw;
            }
        }

        // Métodos auxiliares privados
        private (DateTime fechaDesde, DateTime fechaHasta) ObtenerRangoFechas(ReporteFiltrosDTO filtros)
        {
            if (filtros.FechaDesde.HasValue && filtros.FechaHasta.HasValue)
            {
                return (filtros.FechaDesde.Value, filtros.FechaHasta.Value);
            }

            var hoy = DateTime.Today;
            return filtros.TipoReporte switch
            {
                TipoReporte.Diario => (hoy, hoy.AddDays(1).AddTicks(-1)),
                TipoReporte.Semanal => (hoy.AddDays(-7), hoy),
                TipoReporte.Mensual => (hoy.AddDays(-30), hoy),
                TipoReporte.Trimestral => (hoy.AddDays(-90), hoy),
                TipoReporte.Anual => (hoy.AddDays(-365), hoy),
                _ => (hoy.AddDays(-7), hoy)
            };
        }

        private List<string> ExtraerProductos(string detallePedido)
        {
            if (string.IsNullOrWhiteSpace(detallePedido)) return new List<string>();

            // Expresiones regulares para extraer productos comunes
            var patrones = new[]
            {
                @"\b(?:kg|kilo|kilos?)\s+(?:de\s+)?([a-záéíóúñ\s]+?)(?:\s|$|,|\.)",
                @"\b(\d+)\s*(?:kg|kilo|kilos?)\s+(?:de\s+)?([a-záéíóúñ\s]+?)(?:\s|$|,|\.)",
                @"\b([a-záéíóúñ\s]+?)\s+(?:kg|kilo|kilos?)(?:\s|$|,|\.)",
                @"\b(carne|pollo|cerdo|res|ternera|cordero|pescado)(?:\s+[a-záéíóúñ\s]+?)?(?:\s|$|,|\.)"
            };

            var productos = new HashSet<string>();
            
            foreach (var patron in patrones)
            {
                var matches = Regex.Matches(detallePedido.ToLower(), patron, RegexOptions.IgnoreCase);
                foreach (Match match in matches)
                {
                    var producto = match.Groups[match.Groups.Count - 1].Value.Trim();
                    if (!string.IsNullOrWhiteSpace(producto) && producto.Length > 2)
                    {
                        productos.Add(producto);
                    }
                }
            }

            return productos.ToList();
        }

        private string NormalizarNombreProducto(string producto)
        {
            // Normalizar nombres similares
            var normalizado = producto.ToLower().Trim();
            
            var normalizaciones = new Dictionary<string, string>
            {
                { "carne de res", "carne" },
                { "carne molida", "carne molida" },
                { "pollo entero", "pollo" },
                { "pechuga de pollo", "pechuga" },
                { "cerdo", "cerdo" },
                { "ternera", "ternera" },
                { "pescado", "pescado" }
            };

            foreach (var kvp in normalizaciones)
            {
                if (normalizado.Contains(kvp.Key))
                {
                    return kvp.Value;
                }
            }

            return normalizado;
        }

        private string GetEstadoColor(string estado) => estado.ToLower() switch
        {
            "pendiente" => "warning",
            "en proceso" => "info", 
            "entregado" => "success",
            "cancelado" => "error",
            _ => "default"
        };

        private string GetEstadoIcon(string estado) => estado.ToLower() switch
        {
            "pendiente" => "Icons.Material.Filled.Schedule",
            "en proceso" => "Icons.Material.Filled.LocalShipping",
            "entregado" => "Icons.Material.Filled.CheckCircle",
            "cancelado" => "Icons.Material.Filled.Cancel",
            _ => "Icons.Material.Filled.Help"
        };

        private string GetFormaPagoColor(string? formaPago) => formaPago?.ToLower() switch
        {
            "efectivo" => "success",
            "tarjeta" => "info",
            "transferencia" => "primary",
            "contado" => "success",
            "credito" => "warning",
            _ => "default"
        };

        private string TraducirDiaSemana(string dia) => dia switch
        {
            "Monday" => "Lunes",
            "Tuesday" => "Martes", 
            "Wednesday" => "Miércoles",
            "Thursday" => "Jueves",
            "Friday" => "Viernes",
            "Saturday" => "Sábado",
            "Sunday" => "Domingo",
            _ => dia
        };
    }
}
