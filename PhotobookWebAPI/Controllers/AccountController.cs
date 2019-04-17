using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PhotobookWebAPI.Data;
using PhotobookWebAPI.Models;
using PhotoBook.Repository.EventRepository;
using PhotoBookDatabase.Model;


namespace PhotobookWebAPI.Controllers
{

    
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AccountController : ControllerBase
    {

        private UserManager<AppUser> _userManager;
        private SignInManager<AppUser> _signInManager;
        private IEventRepository _eventRepo;
 
 

        public AccountController(UserManager<AppUser> userManager,  SignInManager<AppUser> signInManager, IEventRepository eventRepo)
        {            
            _userManager = userManager;
            _signInManager = signInManager;
            _eventRepo = eventRepo;
        }


       /// <summary>
       /// Gets all the app users in a list
       /// </summary>
       /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<List<AppUser>> GetAccounts()
        {
            return await _userManager.Users.ToListAsync();
        }


        // GET: api/Account/"username"
        [HttpGet("{UserName}")]
        //[Authorize("IsAdmin")]
        [AllowAnonymous]
        public async Task<AppUser> GetAccount(string UserName)
        {
            var user = await _userManager.FindByNameAsync(UserName);
            return user;
        }



        // PUT: api/Account/"username"
        [HttpPut("{UserName}")]
        //[Authorize("IsAdmin")]
        [AllowAnonymous]
        public async Task<IActionResult> PutAccount(string UserName, AppUser newData)
        {
            AppUser user = await _userManager.FindByNameAsync(UserName);

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



        // DELETE: api/Account/"username"
        [HttpDelete("{UserName}")]
        //[Authorize("IsAdmin")]
        [AllowAnonymous]
        public async Task<IActionResult> DeleteAccount(string UserName)
        {
            var user = await _userManager.FindByNameAsync(UserName);
            if (user == null)
            {
                return NotFound();
            }

            string userRole = null;
            var userClaims = await _userManager.GetClaimsAsync(user);
            foreach (var userClaim in userClaims)
            {
                if (userClaim.Type == "Role")
                    userRole = userClaim.Value;
            }

            if(userRole == null)
                return NoContent();

          
               var result =  await _userManager.DeleteAsync(user);
               if (result.Succeeded)
               {
                   if (userRole == "Host")
                   {
                       return RedirectToAction("Delete", "Host", new { email = user.Email });
                   }
                   else if (userRole == "Guest")
                   {

                       string[] guestStrings = user.UserName.Split(";");
                       return RedirectToAction("Delete", "Guest", new { name = guestStrings[0],  pin=guestStrings[1] });
                   }
                }

               return NoContent();

        }



        [HttpPost]
        [AllowAnonymous]
        [Route("Admin")]
        public async Task<ActionResult> CreateAdmin(AccountModels.RegisterAdminModel model)
        {

            var user = new AppUser
                {UserName = model.UserName};

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                var roleClaim = new Claim("Role", "Admin");
                await _userManager.AddClaimAsync(user, roleClaim);
                await _signInManager.SignInAsync(user, isPersistent: false);


                return Ok();
            }

            return NotFound();
        }


        [HttpPost]
        [AllowAnonymous]
        [Route("Host")]
        public async Task<ActionResult> CreateHost(AccountModels.RegisterHostModel model)
        {

            var user = new AppUser {UserName = model.Email, Email = model.Email, Name = model.Name};

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                var roleClaim = new Claim("Role", "Host");
                await _userManager.AddClaimAsync(user, roleClaim);
                await _signInManager.SignInAsync(user, isPersistent: false);
               
    
                return RedirectToAction("Register", "Host", model);
            }
            return NotFound();
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("Guest")]
        public async Task<ActionResult> CreateGuest(AccountModels.RegisterGuestModel model)
        {
            string username = model.Name + ";" + model.Pin;
            var user = new AppUser { UserName = username, Name = model.Name};

            Event e = await _eventRepo.GetEventByPin(model.Pin);

            if (e!=null)
            {
                var result = await _userManager.CreateAsync(user, model.Pin);
                if (result.Succeeded)
                {
                    var roleClaim = new Claim("Role", "Guest");
                    await _userManager.AddClaimAsync(user, roleClaim);
                    await _signInManager.SignInAsync(user, isPersistent: true);

                    return RedirectToAction("Register", "Guest", model);
                }
            }

            return NotFound();
        }

        [AllowAnonymous]
        [Route("Login")]
        [HttpPost]
        public async Task<IActionResult> Login(AccountModels.LoginModel loginInfo)
        {
            var result = await _signInManager.PasswordSignInAsync(loginInfo.UserName,
                loginInfo.Password, false, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                return RedirectToAction("Login", "Host", new {email = loginInfo.UserName});
            }

            return NotFound();


        }


        [AllowAnonymous]
        [Route("Logout")]
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return Ok();

        }

        //[HttpPost]
        [Route("Password")]
        [Authorize("IsHost")]
        [HttpPut]
        public async Task<ActionResult> Password(AccountModels.ChangePassModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            var result = await _userManager.ChangePasswordAsync(user, model.CurrPassword, model.NewPassword);

            if (result.Succeeded)
            {
               return Ok();
            }
            return NotFound();
        }

        /*
        //[HttpPost]
        [AllowAnonymous]
        [Route("InitChangeEmail")]
        [HttpPut]
        public async Task<ActionResult> InitChangeEmail(AccountModels.ChangeEmailModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.OldEmail);

            string resettoken;
            try
            {
                resettoken = await _userManager.GenerateChangeEmailTokenAsync(user, model.NewEmail);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw;
            }
            

            var resetLink = Url.Action("Email", "Account",
                new {token = resettoken, oldEmail = user.Email, newEmail = model.NewEmail},
                protocol: HttpContext.Request.Scheme);

            var result = await _userManager.ChangeEmailAsync(user, model.NewEmail, resettoken);
            if (result.Succeeded)
            {
                return Ok();
            }
            return NotFound();
        //return Ok();
    }

        [AllowAnonymous]
        [Route("ChangeEmail")]
        [HttpPut]
        public async Task<ActionResult> Email([FromQuery]string token, [FromQuery]string oldEmail, [FromQuery]string newEmail)
        {
            var user = await _userManager.FindByEmailAsync(oldEmail);

            var result = await _userManager.ChangeEmailAsync(user, newEmail, token);
            if (result.Succeeded)
            {
                return Ok();
            }
            return NotFound();
        }
        */


    }
}

