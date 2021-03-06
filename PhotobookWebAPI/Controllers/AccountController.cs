﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NLog;
using PB.Dto;
using PhotobookWebAPI.Data;
using PhotoBook.Repository.EventRepository;
using PhotoBook.Repository.GuestRepository;
using PhotoBook.Repository.HostRepository;
using PhotoBookDatabase.Model;
using PhotoSauce.MagicScaler;


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
        private ICurrentUser _currentUser;



        public AccountController(UserManager<AppUser> userManager,  SignInManager<AppUser> signInManager, IEventRepository eventRepo, IHostRepository hostRepo, IGuestRepository guestRepo, ICurrentUser currentUser)
        {            
            _userManager = userManager;
            _signInManager = signInManager;
            _eventRepo = eventRepo;
            _guestRepo = guestRepo;
            _hostRepo = hostRepo;
            _currentUser = currentUser;
        }




        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("Index")]
        [Authorize("IsAdmin")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var accountList = await _userManager.Users.ToListAsync();
            return View(accountList);
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
        [Authorize("IsAdmin")]
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
        /// <response code="401">No user found </response> 
        [HttpPut]
        [Authorize("IsHost")]
        public async Task<IActionResult> PutAccount(AppUser newData)
        {
                AppUser user = await _userManager.FindByNameAsync(_currentUser.Name());

                if (user == null)
                {
                    return Unauthorized();
                }

                if (newData.Email != null)
                    user.Email = newData.Email;
                if (newData.UserName != null)
                    user.UserName = newData.UserName;

                await _userManager.UpdateAsync(user);

                logger.Info($"PutAccount called on UserName: {user.Email}: UserName changed to {newData.UserName}, Email Changed to {newData.UserName}");

                return NoContent();
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
        [Authorize("IsAdmin")]
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
        /// Creates Admin user
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
        [Route("Admin/Create")]
        public async Task<ActionResult> CreateAdmin(RegisterAdminModel model)
        {
            
            var user = new AppUser
                {UserName = model.UserName};

            var result = await _userManager.CreateAsync(user, model.Password);
            
            if (result.Succeeded)
            {
                var roleClaim = new Claim("Role", "Admin");
                await _userManager.AddClaimAsync(user, roleClaim);

                return NoContent();
            }

            return NotFound();
        }


        /// <summary>
        /// Creates Admin user
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
        /// <response code="400"> Not an Admin</response> 
        [AllowAnonymous]
        [Route("Admin/Login")]
        [HttpPost]
        public async Task<ActionResult> LoginAdmin(RegisterAdminModel model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);
            string userRole = null;
            var userClaims = await _userManager.GetClaimsAsync(user);
            foreach (var userClaim in userClaims)
            {
                if (userClaim.Type == "Role")
                    userRole = userClaim.Value;
            }

            if (userRole == "Admin")
            {

                var result = await _signInManager.PasswordSignInAsync(model.UserName,
                    model.Password, false, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    logger.Info($"admin with login {model.UserName} signed in");


                    return NoContent();
                }
            }

            return BadRequest();
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
        /// <returns>Ok, Host info</returns>
        /// <response code="200"> Host created </response>
        /// <response code="400"> Error in creating Host</response> 
        [HttpPost]
        [AllowAnonymous]
        [Route("Host")]
        public async Task<IActionResult> CreateHost(RegisterHostModel model)
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

                return Ok(new ReturnHostModel
                {
                    Name = host.Name,
                    Email = host.Email,
                });
                
            }

            return BadRequest();
           
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
        /// <returns>Created, Event info</returns>
        /// <response code="201"> Guest created </response>
        /// <response code="400"> Error in creating Guest</response> 
        [HttpPost]
        [AllowAnonymous]
        [Route("Guest")]
        public async Task<IActionResult> CreateGuest(RegisterGuestModel model)
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

                    Guest guest = new Guest
                    {
                        Name = model.Name,
                        EventPin = model.Pin
                    };
                    await _guestRepo.InsertGuest(guest);
                    logger.Info($"Guest created with Name: {guest.Name} and EventPin: {guest.EventPin}");
                    
                    //EventModel
                    return Created("In Database", new EventModel
                    {
                        Description = e.Description,
                        EndDate = e.EndDate,
                        StartDate = e.StartDate,
                        Location = e.Location,
                        Name = e.Name,
                        Pin = e.Pin
                    });
                }
            }

            return BadRequest();
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
        /// <response code="200"> Success </response>
        /// <response code="400"> Error in creating</response> 
        [AllowAnonymous]
        [Route("Login")]
        [HttpPost]
        public async Task<ActionResult> Login(LoginModel loginInfo)
        {
            var result = await _signInManager.PasswordSignInAsync(loginInfo.UserName,
                loginInfo.Password, false, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                logger.Info($"AppUser with login {loginInfo.UserName} signed in");
                string email = loginInfo.UserName;
                var host = await _hostRepo.GetHostByEmail(email);
                var events = await _eventRepo.GetEventsByHostId(host.HostId);
                List<EventModel> eventModels = new List<EventModel>();
                if (events != null)
                {
                    foreach (var ev in events)
                    {
                        eventModels.Add(new EventModel()
                        {
                            Description = ev.Description,
                            EndDate = ev.EndDate,
                            Location = ev.Location,
                            Name = ev.Name,
                            Pin = ev.Pin,
                            StartDate = ev.StartDate
                        });
                    }
                }

                

                return Ok(new ReturnHostModel
                {
                    Name = host.Name,
                    Email = email,
                    Events = eventModels
                });
            }
            return BadRequest();
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
        /// Change password
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
        /// <response code="204"> Success, password changed </response>
        /// <response code="404"> Error, password didnt change</response> 
        [Route("Password")]
        [Authorize("IsHost")]
        [HttpPut]
        public async Task<IActionResult> Password(ChangePassModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            var result = await _userManager.ChangePasswordAsync(user, model.CurrPassword, model.NewPassword);

            if (result.Succeeded)
            {
               return NoContent();
            }
            return NotFound();
        }
    }
}

