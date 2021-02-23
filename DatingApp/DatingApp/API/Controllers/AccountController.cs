using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Mvc;
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

        [HttpPost]
        public async Task<ActionResult<AppUser>> Register(string username,string password)
        {
            using var hmc = new HMACSHA512();
            var user = new AppUser
            {
                UserName = username,
                PasswordHash = hmc.ComputeHash(Encoding.UTF8.GetBytes(password)),
                PasswordSalt = hmc.Key
            };
             dataContext.Users.Add(user);
            await dataContext.SaveChangesAsync();
            return user;
        }
    }
}
