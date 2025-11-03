using System.ComponentModel.DataAnnotations;

namespace WhatsAppBot.Configuration
{
    public class SecuritySettings
    {
        [Required(ErrorMessage = "El AppSecret de Meta es obligatorio")]
        public string AppSecret { get; set; } = string.Empty;
    }
}