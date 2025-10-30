using Microsoft.AspNetCore.SignalR;
using WhatsAppBot.Models;

namespace WhatsAppBot.Hubs;

public class PedidosHub : Hub
{
    public async Task NotificarNuevoPedido(Pedido pedido)
    {
        await Clients.All.SendAsync("PedidoRecibido", pedido);
    }

    public async Task NotificarCambioEstado(Pedido pedido)
    {
        await Clients.All.SendAsync("PedidoActualizado", pedido);
    }
}
