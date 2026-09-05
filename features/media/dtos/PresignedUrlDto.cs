using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        // لیست پسوندهای مجاز (تصویر، ویدیو و فایل‌های صوتی/ویس)
        private static readonly string[] AllowedExtensions =
        [
            // تصاویر
            ".jpg", ".jpeg", ".png", ".webp", ".gif", ".svg",
            // ویدیوها
            ".mp4", ".mov", ".webm", ".mkv", ".avi",
            // فایل‌های صوتی و ویس
            ".mp3", ".wav", ".ogg", ".m4a", ".aac"
        ];

        // لیست Content-Typeهای مجاز
        private static readonly string[] AllowedContentTypes =
        [
            // تصاویر
            "image/jpeg", "image/pjpeg", "image/png", "image/webp", "image/gif", "image/svg+xml",
            // ویدیوها
            "video/mp4", "video/quicktime", "video/webm", "video/x-matroska", "video/mkv", "video/x-msvideo",
            // فایل‌های صوتی
            "audio/webm", "audio/ogg", "audio/mpeg", "audio/mp3", "audio/wav", "audio/x-wav", "audio/mp4", "audio/aac", "audio/x-m4a"
        ];

        public PresignedUrlDtoValidator()
        {
            RuleFor(x => x.FileName)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("نام فایل نمی‌تواند خالی باشد.")
                .MaximumLength(255).WithMessage("نام فایل بیش از حد طولانی است.")
                .Must(HaveValidExtension).WithMessage("فرمت فایل ارسالی مجاز نیست.");

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

            var cleanType = contentType.Split(';')[0].Trim().ToLowerInvariant();
            return AllowedContentTypes.Contains(cleanType);
        }
    }
}
