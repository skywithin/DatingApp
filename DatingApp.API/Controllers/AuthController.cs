using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _repo;

        public AuthController(IAuthRepository repo)
        {
            _repo = repo;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserForRegisterDto user)
        {
            //Todo: validate request

            user.Username = user.Username.ToLower();

            if (await _repo.UserExists(user.Username))
            {
                return BadRequest("User name already exists");
            }

            var userToCreate = new User
            {
                Username = user.Username
            };

            var createdUser = await _repo.Register(userToCreate, user.Password);

            //Todo: return CreatedAtRoute()
            return StatusCode(201);
        }
    }
}