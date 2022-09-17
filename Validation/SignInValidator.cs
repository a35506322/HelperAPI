using FluentValidation;
using HelperAPI.Models;
using static HelperAPI.Modules.Implements.CommonModule;
using static HelperAPI.Modules.Implements.SignModule;

namespace HelperAPI.Validation
{
    public class SignInValidator : AbstractValidator<SignInRequest>
    {
        public SignInValidator()
        {
            RuleFor(m => m.Account).NotEmpty();
            RuleFor(m => m.Password).NotEmpty();
        }
    }
}
