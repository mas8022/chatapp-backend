using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.common;
using backend.common.services;
using backend.features.media.dtos;
using Microsoft.AspNetCore.Http.HttpResults;

namespace backend.features.media
{
    public class MediaService(StorageService storageService)
    {
        public async Task<Result> GetPresignedUrl(PresignedUrlDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.FileName) || string.IsNullOrWhiteSpace(dto.ContentType))
                throw new BadRequestException("نام فایل و نوع آن الزامی است.");

            var (uploadUrl, fileUrl) = storageService.GeneratePreSignedUrl(dto.FileName, dto.ContentType);
            return new Result
            {
                Status = StatusCodes.Status200OK,
                Data = new { uploadUrl, fileUrl }
            };
        }
    }
}