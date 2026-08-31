using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;

namespace backend.features.auth.dtos.sendOtp
{
    public class SendOtpDto
    {
        public string Phone { get; set; } = string.Empty;
    }

    public class SendOtpDtoValidator : AbstractValidator<SendOtpDto>
    {
        public SendOtpDtoValidator()
        {
            RuleFor(x => x.Phone)
                .NotEmpty()
                .WithMessage("Phone number is required.")

                .Matches(@"^(?:\+98|0098|98|0)?9\d{9}$")
                .WithMessage("The Iranian mobile phone number is not valid.");
        }
    }
}
