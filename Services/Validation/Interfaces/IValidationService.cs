using System.Threading.Tasks;
using WhatsAppBot.Services.Validation;

namespace WhatsAppBot.Services.Validation.Interfaces
{
    public interface IValidationService<T>
    {
        Task<ValidationResult> ValidateAsync(T entity);
    }
}
