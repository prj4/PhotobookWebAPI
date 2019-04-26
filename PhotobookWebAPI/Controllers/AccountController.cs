using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NLog;
using PhotobookWebAPI.Data;
using PhotobookWebAPI.Models;
using PhotoBook.Repository.EventRepository;
using PhotoBook.Repository.GuestRepository;
using PhotoBook.Repository.HostRepository;
using PhotoBookDatabase.Model;


namespace PhotobookWebAPI.Controllers
{

    
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AccountController : Controller
    {

        private UserManager<AppUser> _userManager;
        private SignInManager<AppUser> _signInManager;
        private IEventRepository _eventRepo;
        private IHostRepository _hostRepo;
        private IGuestRepository _guestRepo;
        private Logger logger = LogManager.GetCurrentClassLogger();



        public AccountController(UserManager<AppUser> userManager,  SignInManager<AppUser> signInManager, IEventRepository eventRepo, IHostRepository hostRepo, IGuestRepository guestRepo)
        {            
            _userManager = userManager;
            _signInManager = signInManager;
            _eventRepo = eventRepo;
            _guestRepo = guestRepo;
            _hostRepo = hostRepo;
        }




        /// <summary>
        /// Gets a list of all AppUsers registered in the database
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET api/Account
        ///
        /// </remarks>
        /// <returns>Ok, list of AppUser</returns>
        /// <response code="200">Returns list of all AppUsers, empty list if no users</response> 
        [HttpGet]
        [AllowAnonymous]
        public async Task<List<AppUser>> GetAccounts()
        {
            var accountList = await _userManager.Users.ToListAsync();
            logger.Info("GetAccounts Called");
                return accountList;
        }


        /// <summary>
        /// Gets a the AppUser with the given username
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET api/Account/Name@mail.dk
        ///
        /// </remarks>
        /// <returns>Ok, AppUser</returns>
        /// <response code="200">Returns AppUser with specified username</response>
        /// <response code="204">User not found</response> 
        [HttpGet("{UserName}")]
        [AllowAnonymous]
        public async Task<AppUser> GetAccount(string UserName)
        {
            var user = await _userManager.FindByNameAsync(UserName);
            logger.Info($"GetAccount called with UserName: {UserName}");

            return user;
        }


        /// <summary>
        /// Can change the email/Username of a user
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     PUT api/Account/name@mail.dk
        ///     {
        ///        "UserName": "example@mail.dk",
        ///        "Email": "example@mail.dk"
        ///     }
        ///
        /// </remarks>
        /// <returns>NoContent</returns>
        /// <response code="204"> User changed successfully </response>
        /// <response code="404"> User not found </response> 
        [HttpPut("{UserName}")]
        [Authorize("IsHost")]
        public async Task<IActionResult> PutAccount(string UserName, AppUser newData)
        {
            if (User.Identity.Name == UserName)
            {
                AppUser user = await _userManager.FindByNameAsync(UserName);

                if (user == null)
                {
                    return NotFound();
                }

                if (newData.Email != null)
                    user.Email = newData.Email;
                if (newData.UserName != null)
                    user.UserName = newData.UserName;

                await _userManager.UpdateAsync(user);

                logger.Info($"PutAccount called on UserName: {UserName}: UserName changed to {newData.UserName}, Email Changed to {newData.UserName}");

                return NoContent();

            }

            return Unauthorized();

        }


        /// <summary>
        /// Deletes AppUser with specified username
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     DELETE api/Account/name@mail.dk
        ///
        /// </remarks>
        /// <returns>NoContent</returns>
        /// <response code="204"> User Deleted </response>
        /// <response code="404"> User not found </response> 
        [HttpDelete("{UserName}")]
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
                return NotFound();

          
               var result =  await _userManager.DeleteAsync(user);
               logger.Info($"AppUser with UserName {UserName} is deleted");
            if (result.Succeeded)
               {
                   if (userRole == "Host")
                   {
                       
                       await _hostRepo.DeleteHostByEmail(user.Email);
                      logger.Info($"Host with Email {user.Email} is deleted");

                }
                   else if (userRole == "Guest")
                   {

                    string[] guestStrings = user.UserName.Split(";");

                    await _guestRepo.DeleteGuestByNameAndEventPin(guestStrings[0], guestStrings[1]);
                    logger.Info($"Guest with Name {guestStrings[0]} and Eventpin {guestStrings[1]} is deleted");

                }
                }

            return NoContent();

        }


        /// <summary>
        /// Creates Admin user(NOT CURRENTLY WORKING)
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST api/Account/Admin
        ///     {
        ///        "UserName": "admin",
        ///        "Password": "admin"
        ///     }
        ///     
        ///
        /// </remarks>
        /// <returns>NoContent</returns>
        /// <response code="204"> Admin created </response>
        /// <response code="404"> Error in creating admin</response> 
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


                return NoContent();
            }

            return NotFound();
        }


        /// <summary>
        /// Creates Host user
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST api/Account/Host
        ///     {
        ///        "Name": "Name",
        ///        "Email": "Name@mail.dk",
        ///        "Password": "123456"
        ///     }
        ///     
        ///
        /// </remarks>
        /// <returns>NoContent</returns>
        /// <response code="204"> Host created </response>
        /// <response code="404"> Error in creating Host</response> 
        [HttpPost]
        [AllowAnonymous]
        [Route("Host")]
        public async Task<AccountModels.ReturnHostModel> CreateHost(AccountModels.RegisterHostModel model)
        {
            
            var user = new AppUser {UserName = model.Email, Email = model.Email, Name = model.Name};

            var result = await _userManager.CreateAsync(user, model.Password);
            logger.Info($"AppUser created with Email: {user.Email}");

            if (result.Succeeded)
            {
                var roleClaim = new Claim("Role", "Host");
                await _userManager.AddClaimAsync(user, roleClaim);
                logger.Info($"Host Role Claim added to AppUser with Email: {user.Email}");
                await _signInManager.SignInAsync(user, isPersistent: false);
                logger.Info($"AppUser signed in with Email: {user.Email}");

                

                Host host = new Host { Name = model.Name, Email = model.Email };

                
                await _hostRepo.InsertHost(host);
                logger.Info($"Host created with Email: {host.Email} ");

                return new AccountModels.ReturnHostModel
                {
                    Name = host.Name,
                    Email = host.Email,
                    HostId = host.HostId
                };

                

                
            }

            return null;
           
        }
        /// <summary>
        /// Creates Guest user
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST api/Account/Guest
        ///     {
        ///        "Name": "Name",
        ///        "Pin": "abcd1234ef"
        ///     }
        ///     
        ///
        /// </remarks>
        /// <returns>NoContent</returns>
        /// <response code="204"> Guest created </response>
        /// <response code="404"> Error in creating Guest</response> 
        [HttpPost]
        [AllowAnonymous]
        [Route("Guest")]
        public async Task<AccountModels.ReturnGuestModel> CreateGuest(AccountModels.RegisterGuestModel model)
        {
            string username = model.Name + ";" + model.Pin;
            var user = new AppUser { UserName = username, Name = model.Name};

            Event e = await _eventRepo.GetEventByPin(model.Pin);

            if (e!=null)
            {
                var result = await _userManager.CreateAsync(user, model.Pin);
                
                if (result.Succeeded)
                {
                    logger.Info($"AppUser created with UserName: {user.UserName}");
                    var roleClaim = new Claim("Role", "Guest");
                    await _userManager.AddClaimAsync(user, roleClaim);
                    logger.Info($"Guest Role Claim added to AppUser with UserName: {user.UserName}");
                    await _signInManager.SignInAsync(user, isPersistent: true);
                    logger.Info($"AppUser signed in with UserName: {user.UserName}");

                    
                    var ev = await _eventRepo.GetEventByPin(model.Pin);

                    Guest guest = new Guest
                    {
                        Name = model.Name,
                        EventPin = model.Pin
                    };
                    await _guestRepo.InsertGuest(guest);
                    logger.Info($"Guest created with Name: {guest.Name} and EventPin: {guest.EventPin}");

                    return new AccountModels.ReturnGuestModel
                    {
                            Description = e.Description,
                            EndDate = e.EndDate,
                            StartDate = e.StartDate,
                            Location = e.Location,
                            Name = e.Name,
                            Pin = e.Pin
                    };

                    

                }
            }

            return null;
        }


        /// <summary>
        /// Login with Host login
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST api/Account/Login
        ///     {
        ///        "UserName": "Name",
        ///        "Password": "123456"
        ///     }
        ///     
        ///
        /// </remarks>
        /// <returns>NoContent</returns>
        /// <response code="204"> Success </response>
        /// <response code="404"> Error</response> 
        [AllowAnonymous]
        [Route("Login")]
        [HttpPost]
        public async Task<AccountModels.ReturnHostModel> Login(AccountModels.LoginModel loginInfo)
        {
            var result = await _signInManager.PasswordSignInAsync(loginInfo.UserName,
                loginInfo.Password, false, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                logger.Info($"AppUser with login {loginInfo.UserName} signed in");
                string email = loginInfo.UserName;
                var host = await _hostRepo.GetHostByEmail(email);
                var events = await _eventRepo.GetEventsByHostId(host.HostId);
                return new AccountModels.ReturnHostModel
                {
                    Name = host.Name,
                    Email = email,
                    HostId = host.HostId,
                    Events = events
                };

            }

            return null;
        }


        /// <summary>
        /// Logout 
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST api/Account/Logout
        ///    
        ///     
        ///
        /// </remarks>
        /// <returns>NoContent</returns>
        /// <response code="204"> Success </response>
        [AllowAnonymous]
        [Route("Logout")]
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return NoContent();

        }

        /// <summary>
        /// Change password [IsHost]
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     PUT api/Account/Password
        ///     {
        ///        "Email": "Name@mail.dk",
        ///        "CurrPassword": "123456",
        ///        "NewPassword": "234567"
        ///     }
        ///     
        ///
        /// </remarks>
        /// <returns>NoContent</returns>
        /// <response code="204"> Success </response>
        /// <response code="404"> Error</response> 
        [Route("Password")]
        [Authorize("IsHost")]
        [HttpPut]
        public async Task<ActionResult> Password(AccountModels.ChangePassModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            var result = await _userManager.ChangePasswordAsync(user, model.CurrPassword, model.NewPassword);

            if (result.Succeeded)
            {
               return NoContent();
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

