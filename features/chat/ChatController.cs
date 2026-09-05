using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.features.chat
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChatController(ChatService chatService) : ControllerBase
    {
        [HttpGet("pv-messages/{receiverId}")]
        public async Task<IActionResult> GetPvMessages(
            int receiverId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            return Ok(
                await chatService.GetPvMessages(
                    receiverId,
                    page,
                    pageSize,
                    cancellationToken)
            );
        }


        [HttpGet("contacts")]
        public async Task<IActionResult> GetConvGetContactsersations()
        {
            return Ok(await chatService.GetContacts());
        }

    }
}



