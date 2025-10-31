using System.Collections.Generic;

namespace WhatsAppBot.Services.Validation
{
    public class ValidationResult
    {
        public bool IsValid => Errors.Count == 0;
        public List<string> Errors { get; } = new();

        public void AddError(string error) => Errors.Add(error);

        public void AddErrors(IEnumerable<string> errors)
        {
            if (errors == null) return;
            Errors.AddRange(errors);
        }
    }
}