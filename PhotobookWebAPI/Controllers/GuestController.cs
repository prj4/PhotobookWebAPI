using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using PhotobookWebAPI.Data;
using PhotobookWebAPI.Models;
using PhotoBook.Repository.EventGuestRepository;
using PhotoBook.Repository.EventRepository;
using PhotoBook.Repository.GuestRepository;
using PhotoBook.Repository.HostRepository;
using PhotoBookDatabase.Model;


namespace PhotobookWebAPI.Controllers
{
    public class GuestController : Controller
    {
        private readonly string _connectionString;
        private IConfiguration _configuration;

        private UserManager<AppUser> _userManager;
        private SignInManager<AppUser> _signInManager;

        private IGuestRepository _guestRepo;
        private IEventGuestRepository _eventGuestRepo;
        private IEventRepository _eventRepo;

        public GuestController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, 
            IGuestRepository guestRepo, IEventRepository eventRepo, IEventGuestRepository eventGuestRepo)
        {
            _connectionString = _configuration.GetConnectionString("RemoteConnection");

            _userManager = userManager;
            _signInManager = signInManager;

            _guestRepo = guestRepo;
            _eventRepo = eventRepo;
            _eventGuestRepo = eventGuestRepo;
        }

        public async Task<IActionResult> Index()
        {

            return View(await _guestRepo.GetGuests());
        }

        public async Task<ActionResult> Delete(string name)
        {
            _guestRepo.DeleteGuest(name);

            return Ok();

        }

        // GET: /<controller>/
        [HttpPost]
        [AllowAnonymous]
        [Route("Register")]
        public async Task<ActionResult> Register(AccountModels.RegisterGuestModel model)
        {
            //Check if event exsists with model.password then do the following
            IQueryable<Event> Events = await _eventRepo.GetEvents();
            foreach (var _event in Events)
            {
                if (_event.Pin == int.Parse(model.Password))
                {
                    Event thisEvent = _event;

                    //Create user in Identity core
                    var user = new AppUser { UserName = model.UserName };
                    var result = await _userManager.CreateAsync(user, model.Password);
                    if (result.Succeeded)
                    {
                        var roleClaim = new Claim("Role", "Guest");
                        await _userManager.AddClaimAsync(user, roleClaim);
                        await _signInManager.SignInAsync(user, isPersistent: false);

                        //Add Guest to DB and connect to the found event. 
                        Guest guest = new Guest
                        {
                            Name = "Name"
                        };
                        _guestRepo.InsertGuest(guest);

                        EventGuest eventGuest = new EventGuest
                        {
                            Event = _event,
                            EventPin = int.Parse(model.Password),
                            Guest = guest
                        };
                        _eventGuestRepo.InsertEventGuest(eventGuest);

                        return Ok();
                    }
                    else
                    {
                        return NotFound();
                    }
                }

            }
            return NotFound();
        }
    }
}
