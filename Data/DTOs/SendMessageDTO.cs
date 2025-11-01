using System.ComponentModel.DataAnnotations;

namespace WhatsAppBot.Data.DTOs
{
    public class SendMessageDTO
    {
        [Required]
        [RegularExpression(@"^\d{10,15}$", ErrorMessage = "Número inválido. Usa solo dígitos en formato internacional (ej: 5215555555555).")]
        public string To { get; set; } = string.Empty;

        [Required]
        [StringLength(4096, ErrorMessage = "El mensaje es demasiado largo.")]
        public string Body { get; set; } = string.Empty;
    }
}