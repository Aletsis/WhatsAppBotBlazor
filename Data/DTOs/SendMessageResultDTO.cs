namespace WhatsAppBot.Data.DTOs
{
    public class SendMessageResultDto
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? MetaResponse { get; set; } // opcional: texto de respuesta de Meta si decides retornarlo
    }
}