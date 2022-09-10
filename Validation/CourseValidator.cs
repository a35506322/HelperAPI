using FluentValidation;
using HelperAPI.Models;

namespace HelperAPI.Validation
{
    public class CourseValidator : AbstractValidator<CourseRequest>
    {
        public CourseValidator()
        {
            RuleFor(m => m.BookId).NotEmpty();
            RuleFor(m => m.BookName).NotEmpty();
        }
    }
}
