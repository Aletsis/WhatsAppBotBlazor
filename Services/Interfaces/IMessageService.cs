using WhatsAppBot.Models;

namespace WhatsAppBot.Services.Interfaces
{
    public interface IMessageService
    {
        public WhatsAppMessage CrearMensaje(string to, string body);
        public WhatsAppMessage CrearMensajeConOpciones(string to, string body, List<string> opciones);
        public WhatsAppMessage CrearMensajeBienvenida(string to);
        public WhatsAppMessage CrearMensajeMenuPrincipal(string to);
    }
}