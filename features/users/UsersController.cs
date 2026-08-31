using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.features.users;
using Microsoft.AspNetCore.Mvc;

namespace backend.features.user
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController(UsersService usersService) : ControllerBase
    {
        [HttpGet("search")]
        public async Task<IActionResult> GetUserBySearch([FromQuery(Name = "search")] string search)
        {
            return Ok(await usersService.GetUserBySearch(search));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            return Ok(await usersService.GetUserById(id));
        }
    }
}