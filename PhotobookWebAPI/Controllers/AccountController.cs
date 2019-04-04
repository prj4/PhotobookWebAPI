using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PhotobookWebAPI.Data;
using PhotobookWebAPI.Models;


namespace PhotobookWebAPI.Controllers
{

    
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AccountController : ControllerBase
    {
        private UserManager<AppUser> _userManager;
        private SignInManager<AppUser> _signInManager;
        readonly IConfiguration _configuration;

        public AccountController(UserManager<AppUser> userManager,  SignInManager<AppUser> signInManager,
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

            if (user == null)
            {
                return NotFound();
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

                
                /*
                Host host = new Host{Name = "Morten", Email = "Morten@test.com", Username = "testuser", PW = "1234"};

                HostRepository hs = new HostRepository(
                    "Server=tcp:katrinesphotobook.database.windows.net,1433;Initial Catalog=PhotoBook4;Persist Security Info=False;User ID=Ingeniørhøjskolen@katrinesphotobook.database.windows.net;Password=Katrinebjergvej22;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");

                hs.InsertHost(host);
                */

    
                return Ok();
            }

            
            
            return NotFound();
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("RegisterGuest")]
        public async Task<ActionResult> RegisterGuest(AccountModels.RegisterGuestModel model)
        {

            var user = new AppUser { UserName = model.UserName};


            //Check if event exsists with model.password then do the following

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                var roleClaim = new Claim("Role", "Guest");
                await _userManager.AddClaimAsync(user, roleClaim);
                await _signInManager.SignInAsync(user, isPersistent: false);

                //Add Guest to DB and connect to the found event. 

                

                return Ok();
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

        [Authorize("IsHost")]
        [Route("TestRoleHost")]
        [HttpGet]
        public string TestRoleHost()
        {
            var test = HttpContext.User.Claims.ElementAt(1).Value;



            return test;

        }


        [Authorize("IsGuest")]
        [Route("TestRoleGuest")]
        public async Task<IActionResult> TestRoleGuest()
        {
            return Ok();

        }



    }
}

