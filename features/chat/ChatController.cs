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
        public async Task<IActionResult> GetPvMessages(int receiverId, CancellationToken cancellationToken)
        {
            return Ok(await chatService.GetPvMessages(receiverId, cancellationToken));
        }

        [HttpGet("contacts")]
        public async Task<IActionResult> GetConvGetContactsersations()
        {
            return Ok(await chatService.GetContacts());
        }

    }
}



