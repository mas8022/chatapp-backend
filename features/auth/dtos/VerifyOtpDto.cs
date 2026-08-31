using FluentValidation;

namespace backend.features.auth.dtos
{
    public class VerifyOtpDto
    {
        public string Code { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
    }

    public class VerifyOtpDtoValidator : AbstractValidator<VerifyOtpDto>
    {
        public VerifyOtpDtoValidator()
        {
            RuleFor(x => x.Code)
                .NotEmpty()
                .WithMessage("Verification code is required.")
                .Matches(@"^\d{5}$")
                .WithMessage("Verification code must contain exactly 5 digits.");

            RuleFor(x => x.Phone)
                .NotEmpty()
                .WithMessage("Phone number is required.")
                .Matches(@"^(\+98|0098|98|0)?9\d{9}$")
                .WithMessage("Please enter a valid Iranian mobile phone number.");
        }
    }
}
