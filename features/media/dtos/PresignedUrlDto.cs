using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;

namespace backend.features.media.dtos
{
    public class PresignedUrlDto
    {
        public required string FileName { get; set; }
        public required string ContentType { get; set; }
    }

    public class PresignedUrlDtoValidator : AbstractValidator<PresignedUrlDto>
    {
        // لیست پسوندهای مجاز به همراه Content-Typeهای متناظر
        private static readonly string[] AllowedExtensions =
        [
            ".jpg", ".jpeg", ".png", ".webp", ".gif", ".svg",
        ".mp4", ".mov", ".webm", ".mkv", ".avi"
        ];

        private static readonly string[] AllowedContentTypes =
        [
            "image/jpeg", "image/pjpeg", "image/png", "image/webp", "image/gif", "image/svg+xml",
        "video/mp4", "video/quicktime", "video/webm", "video/x-matroska", "video/mkv", "video/x-msvideo"
        ];

        public PresignedUrlDtoValidator()
        {
            RuleFor(x => x.FileName)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("نام فایل نمی‌تواند خالی باشد.")
                .MaximumLength(255).WithMessage("نام فایل بیش از حد طولانی است.")
                .Must(HaveValidExtension).WithMessage("فرمت فایل ارسالی مجاز نیست (فقط عکس و ویدیو).");

            RuleFor(x => x.ContentType)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("نوع محتوا (ContentType) الزامی است.")
                .Must(HaveValidContentType).WithMessage("نوع Content-Type ارسالی نامعتبر است.");
        }

        private static bool HaveValidExtension(string? fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return false;

            try
            {
                var ext = Path.GetExtension(fileName)?.Trim().ToLowerInvariant();
                return !string.IsNullOrEmpty(ext) && AllowedExtensions.Contains(ext);
            }
            catch
            {
                return false;
            }
        }

        private static bool HaveValidContentType(string? contentType)
        {
            if (string.IsNullOrWhiteSpace(contentType)) return false;

            // حذف فاصله‌ها و پارامترهای اضافی مثل charset=utf-8
            var cleanType = contentType.Split(';')[0].Trim().ToLowerInvariant();

            return AllowedContentTypes.Contains(cleanType);
        }
    }
}