using API.Data;
using API.DTOs;
using API.Entities;
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

        public AccountController( DataContext dataContext)
        {
           
            this.dataContext = dataContext;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AppUser>> Register(RegistrerDto registerDto)
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
            return user;
        }

        [HttpPost("login")]
        public async Task<ActionResult<AppUser>> Login(LoginDto loginDto)
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

            return user;
        }

        private async Task<bool> UserExists(string username)
        {
            return await dataContext.Users.AnyAsync(s=>s.UserName== username);
        }
    }
}
