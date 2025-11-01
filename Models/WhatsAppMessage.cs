namespace WhatsAppBot.Models;

public class WhatsAppMessage
{
    public string To { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public List<string>? Opciones { get; set; } 
}
