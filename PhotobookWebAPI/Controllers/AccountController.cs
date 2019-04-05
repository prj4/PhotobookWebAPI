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
using PhotoBook.Repository.EventGuestRepository;
using PhotoBook.Repository.EventRepository;
using PhotoBook.Repository.GuestRepository;
using PhotoBook.Repository.HostRepository;
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

        private IHostRepository _hostRepo;
        private IGuestRepository _guestRepo;
        private IEventRepository _eventRepo;
        private IEventGuestRepository _eventGuestRepo;

        public AccountController(UserManager<AppUser> userManager,  SignInManager<AppUser> signInManager,
             IHostRepository hostRepo, IGuestRepository guestRepo, IEventRepository eventRepo, IEventGuestRepository eventGuestRepo)
        {
            
            _userManager = userManager;
            _signInManager = signInManager;

            _hostRepo = hostRepo;
            _guestRepo = guestRepo;
            _eventRepo = eventRepo;
            _eventGuestRepo = eventGuestRepo;
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
        //[Authorize("IsAdmin")]
        [AllowAnonymous]
        public async Task<AppUser> GetAccount(string Email)
        {
            var user = await _userManager.FindByEmailAsync(Email);
            return user;
        }



        // PUT: api/Account/Email@gmail.com
        [HttpPut("{Email}")]
        //[Authorize("IsAdmin")]
        [AllowAnonymous]
        public async Task<IActionResult> PutAccount(string Email, AppUser newData)
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
        //[Authorize("IsAdmin")]
        [AllowAnonymous]
        public async Task<IActionResult> DeleteAccount(string Email)
        {
            var user = await _userManager.FindByEmailAsync(Email);


            var claims = await _userManager.GetClaimsAsync(user);
           

            if (user == null)
            {
                return NotFound();
            }
            
            if (claims.Count > 0)
            {
                IList<AppUser> hostList = await _userManager.GetUsersForClaimAsync(claims.ElementAt(0));
                if (hostList.Contains(user))
                {
                    _hostRepo.DeleteHost(user.Name);
                }
            }


            if (claims.Count > 1)
            {
                IList<AppUser> guestList = await _userManager.GetUsersForClaimAsync(claims.ElementAt(1));
                if (guestList.Contains(user))
                {
                    _guestRepo.DeleteGuest(user.Name);
                }
            }

    
            await _userManager.DeleteAsync(user);
            

            return NoContent();
        }



        [HttpPost]
        [AllowAnonymous]
        [Route("RegisterAdmin")]
        public async Task<ActionResult> RegisterAdmin(AccountModels.RegisterAdminModel model)
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
        [Route("RegisterHost")]
        public async Task<ActionResult> RegisterHost(AccountModels.RegisterHostModel model)
        {

            var user = new AppUser {UserName = model.Email, Email = model.Email, Name = model.Name};

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                var roleClaim = new Claim("Role", "Host");
                await _userManager.AddClaimAsync(user, roleClaim);
                await _signInManager.SignInAsync(user, isPersistent: false);

                
                
                Host host = new Host{Name = user.Name, Email = user.Email};

                
                _hostRepo.InsertHost(host);
                

    
                return Ok();
            }

            
            
            return NotFound();
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("RegisterGuest")]
        public async Task<ActionResult> RegisterGuest(AccountModels.RegisterGuestModel model)
        {
            return RedirectToAction("Register", "Guest", model);
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
                return Ok();
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
        [Route("ChangePassword")]
        [Authorize("IsHost")]
        public async Task<ActionResult> ChangePassword(AccountModels.ChangePassModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            var result = await _userManager.ChangePasswordAsync(user, model.CurrPassword, model.NewPassword);

            if (result.Succeeded)
            {
               return Ok();
            }
            return NotFound();
        }

        //[HttpPost]
        [AllowAnonymous]
        [Route("InitChangeEmail")]
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
            

            var resetLink = Url.Action("ChangeEmail", "Account",
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
        public async Task<ActionResult> ChangeEmail([FromQuery]string token, [FromQuery]string oldEmail, [FromQuery]string newEmail)
        {
            var user = await _userManager.FindByEmailAsync(oldEmail);

            var result = await _userManager.ChangeEmailAsync(user, newEmail, token);
            if (result.Succeeded)
            {
                return Ok();
            }
            return NotFound();
        }


    }
}

