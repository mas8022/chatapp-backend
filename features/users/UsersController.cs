using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.features.users;
using backend.features.users.dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.features.user
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
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
   
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            return Ok(await usersService.GetProfile());
        }

        [HttpPut("edit-profile")]
        public async Task<IActionResult> EditProfile([FromForm] UpdateProfileDto dto)
        {
            return Ok(await usersService.EditProfile(dto));
        }
    }
}