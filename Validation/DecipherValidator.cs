using FluentValidation;
using HelperAPI.Models;
using HelperAPI.Modules.Implements;

namespace HelperAPI.Validation
{
    public class DecipherValidator : AbstractValidator<DecipherRequest>
    {
        public DecipherValidator()
        {
            RuleFor(m => m.data).NotEmpty();
            RuleFor(m => m.decipherCommandEnum).NotEmpty();
        }
    }
}
