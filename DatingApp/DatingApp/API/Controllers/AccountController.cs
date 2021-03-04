using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace API.Controllers
{

    public class AccountController : BaseAPIController
    {

        private readonly DataContext dataContext;
        private readonly ITokenService tokenService;

        public AccountController( DataContext dataContext,ITokenService tokenService)
        {
           
            this.dataContext = dataContext;
            this.tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegistrerDto registerDto)
        {
            if (await UserExists(registerDto.UserName))
            {
                return BadRequest("UserName is already taken");
            }
            using var hmc = new HMACSHA512();
            var user = new AppUser
            {
                UserName = registerDto.UserName.ToLower(),
                PasswordHash = hmc.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
                PasswordSalt = hmc.Key
            };
             dataContext.Users.Add(user);
            await dataContext.SaveChangesAsync();
            return new UserDto()
            { 
                UserName=user.UserName,
                Token = tokenService.CreateToken(user)
            };
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await dataContext.Users.SingleOrDefaultAsync(s => s.UserName == loginDto.UserName.ToLower());
            if (user == null)
            {
                return Unauthorized("Invlaid User Name");
            }
            using var hmc = new HMACSHA512(user.PasswordSalt);
            var computedHash = hmc.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

            for (int i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i]!= user.PasswordHash[i])
                {
                    return Unauthorized("Invalid Password ");
                }
            }

            return new UserDto()
            {
                UserName = user.UserName,
                Token = tokenService.CreateToken(user)
            };
        }

        private async Task<bool> UserExists(string username)
        {
            return await dataContext.Users.AnyAsync(s=>s.UserName== username);
        }
    }
}
