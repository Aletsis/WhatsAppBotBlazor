using System.Threading.Tasks;
using WhatsAppBot.Models;
using WhatsAppBot.Services.Validation.Interfaces;

namespace WhatsAppBot.Services.Validation
{
    public class ClienteValidationService : IValidationService<Cliente>
    {
        public Task<ValidationResult> ValidateAsync(Cliente cliente)
        {
            var result = new ValidationResult();

            if (cliente == null)
            {
                result.AddError("El cliente es nulo.");
                return Task.FromResult(result);
            }

            if (string.IsNullOrWhiteSpace(cliente.Nombre))
                result.AddError("El nombre es requerido.");

            if (string.IsNullOrWhiteSpace(cliente.Telefono))
                result.AddError("El teléfono es requerido.");
            else if (!IsValidPhoneNumber(cliente.Telefono))
                result.AddError("El formato del teléfono no es válido. Debe contener solo dígitos y al menos 10 caracteres.");

            return Task.FromResult(result);
        }

        private bool IsValidPhoneNumber(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return false;
            var digitsOnly = new string(phone.Where(char.IsDigit).ToArray());
            return digitsOnly.Length >= 10;
        }
    }
}