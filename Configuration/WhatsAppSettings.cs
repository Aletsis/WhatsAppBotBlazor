using System.ComponentModel.DataAnnotations;

namespace WhatsAppBot.Configuration
{
    public class WhatsAppSettings
    {
        [Required(ErrorMessage = "El Token de WhatsApp es obligatorio")]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "El PhoneNumberId es obligatorio")]
        public string PhoneNumberId { get; set; } = string.Empty;

        [Required(ErrorMessage = "El VerifyToken es obligatorio")]
        [MinLength(20, ErrorMessage = "El VerifyToken debe tener al menos 20 caracteres")]
        public string VerifyToken { get; set; } = string.Empty;
    }
}