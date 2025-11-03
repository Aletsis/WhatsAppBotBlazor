using System.ComponentModel.DataAnnotations;

namespace WhatsAppBot.Configuration
{
    public class AdminUserSettings
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = "admin@whatsappbot.com";

        [Required]
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
        public string Password { get; set; } = string.Empty;
    }
}