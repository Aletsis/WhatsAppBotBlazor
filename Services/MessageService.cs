using WhatsAppBot.Models;
using WhatsAppBot.Services.Interfaces;

namespace WhatsAppBot.Services
{
    public class MessageService : IMessageService
    {
        public WhatsAppMessage CrearMensaje(string to, string body)
        {
            return new WhatsAppMessage
            {
                To = to,
                Body = body
            };
        }

        public WhatsAppMessage CrearMensajeConOpciones(string to, string body, List<string> opciones)
        {
            return new WhatsAppMessage
            {
                To = to,
                Body = body,
                Opciones = opciones
            };
        }

        public WhatsAppMessage CrearMensajeBienvenida(string to)
        {
            var body = "👋 ¡Hola!, Bienvenido a *Carnicería La Blanquita* 🥩\nSoy Blanqui un bot diseñado para ayudarte a:\nHacer pedidos \nConsultar el estado de tu pedido\n Brindarte informacion sobre nuestras sucursales.";
            return CrearMensaje(to, body);
        }

        public WhatsAppMessage CrearMensajeMenuPrincipal(string to)
        {
            var body = "¿Como te puedo ayudar?";
            var opciones = new List<string> { "Hacer pedido", "Información" };
            return CrearMensajeConOpciones(to, body, opciones);
        }
    }
}