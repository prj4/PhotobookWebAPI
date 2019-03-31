using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PhotobookWebAPI.Data;
using PhotobookWebAPI.Models;
using Microsoft.AspNet.Identity;

namespace PhotobookWebAPI.Controllers
{

    
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AccountController : ControllerBase
    {
        private Microsoft.AspNetCore.Identity.UserManager<AppUser> _userManager;
        private SignInManager<AppUser> _signInManager;
        readonly IConfiguration _configuration;

        public AccountController(Microsoft.AspNetCore.Identity.UserManager<AppUser> userManager, SignInManager<AppUser> signInManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }





        // GET: api/Account
        [HttpGet]
        [AllowAnonymous]
        public async Task<List<AppUser>> GetAccounts()
        {
            return await _userManager.Users.ToListAsync();
        }


        // GET: api/Account/Email@gmail.com
        [HttpGet("{Email}")]
        [AllowAnonymous]
        public async Task<AppUser> GetAccount(string Email)
        {
            var user = await _userManager.FindByEmailAsync(Email);
            return user;
        }



        // PUT: api/Account/Email@gmail.com
        [AllowAnonymous]
        [HttpPut("{Email}")]
        public async Task<IActionResult> PutTodoItem(string Email, AppUser newData)
        {
            AppUser user = await _userManager.FindByEmailAsync(Email);

            if (user == null)
            {
                return NotFound();
            }

            if(newData.Email!=null)
            user.Email = newData.Email;
            if (newData.UserName != null)
                user.UserName = newData.UserName;

            await _userManager.UpdateAsync(user);

            return NoContent();
        }



        // DELETE: api/Account/Email@gmail.com
        [HttpDelete("{Email}")]
        [AllowAnonymous]
        public async Task<IActionResult> DeleteAccount(string Email)
        {
            var user = await _userManager.FindByEmailAsync(Email);

            if (user == null)
            {
                return NotFound();
            }

            await _userManager.DeleteAsync(user);

            return NoContent();
        }


        [HttpPost]
        [AllowAnonymous]
        [Route("Register")]
        public async Task<ActionResult> Register(AccountModels.RegisterModel model)
        {

            var user = new AppUser { UserName = model.Email, Email = model.Email };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                var roleClaim = new Claim("Role", model.Role);
                await _userManager.AddClaimAsync(user, roleClaim);
                await _signInManager.SignInAsync(user, isPersistent: false);

                return Ok();
            }

            
            
            return NotFound();
        }

        [AllowAnonymous]
        [Route("Login")]
        public async Task<IActionResult> Login(AccountModels.LoginModel loginInfo)
        {


            var result = await _signInManager.PasswordSignInAsync(loginInfo.Email,
                loginInfo.Password, false, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                return Ok();
            }

            return NotFound();


        }


        [AllowAnonymous]
        [Route("Logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return Ok();

        }

        [HttpPost]
        [AllowAnonymous]
        [Route("ChangePassword")]
        public async Task<ActionResult> ChangePassword(AccountModels.ChangePassModel model)
        {
            var user = new AppUser { UserName = model.Email, Email = model.Email };

            var result = await _userManager.ChangePasswordAsync(user, model.CurrPassword, model.NewPassword);

            if (result.Succeeded)
            {
               return Ok();
            }
            return NotFound();
        }

        [Authorize("IsHost")]
        [Route("TestRoleHost")]
        public async Task<IActionResult> TestRoleHost()
        {
            return Ok();

        }


        [Authorize("IsGuest")]
        [Route("TestRoleGuest")]
        public async Task<IActionResult> TestRoleGuest()
        {
            return Ok();

        }



    }
}

