using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.common.services;
using backend.features.media.dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.features.media
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MediaController(MediaService mediaService) : ControllerBase
    {
        [HttpPost("presign")]
        public async Task<IActionResult> GetPresignedUrl(PresignedUrlDto dto)
        {
            return Ok(await mediaService.GetPresignedUrl(dto));
        }
    }
}