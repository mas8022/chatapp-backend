using FluentValidation;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;

namespace backend.features.users.dtos
{
    public class UpdateProfileDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public IFormFile? Avatar { get; set; }
    }

    public class UpdateProfileDtoValidator : AbstractValidator<UpdateProfileDto>
    {
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

        public UpdateProfileDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("نام الزامی است.")
                .Length(3, 100).WithMessage("نام باید بین ۳ تا ۱۰۰ کاراکتر باشد.");

            RuleFor(x => x.Bio)
                .MaximumLength(500).WithMessage("بیوگرافی حداکثر ۵۰۰ کاراکتر باشد.");

            RuleFor(x => x.Avatar)
                .Must(file => file == null || file.Length <= 2 * 1024 * 1024)
                .WithMessage("حجم عکس نباید بیشتر از ۲ مگابایت باشد.")
                .Must(file =>
                {
                    if (file == null) return true;
                    var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                    return _allowedExtensions.Contains(ext);
                })
                .WithMessage("فرمت عکس باید یکی از موارد jpg, jpeg, png یا webp باشد.");
        }
    }
}
