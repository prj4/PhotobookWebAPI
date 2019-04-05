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
        
        private IGuestRepository _guestRepo;
        private IEventGuestRepository _eventGuestRepo;
        private IEventRepository _eventRepo;

        public GuestController(IConfiguration iconfig)
        {
            _connectionString = _configuration.GetConnectionString("RemoteConnection");

            _guestRepo = new GuestRepository(_connectionString);
            _eventRepo = new EventRepository(_connectionString);
            _eventGuestRepo = new EventGuestRepository(_connectionString);
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

        public async Task<ActionResult> LogIn(string name)
        {
            

            return Ok();
        }

        // GET: /<controller>/
        [HttpPost]
        [AllowAnonymous]
        [Route("Register")]
        public async Task<AccountModels.ReturnGuestModel> Register(AccountModels.RegisterGuestModel model)
        {//Note til selv... Den her funktion kan nok finpudses.. tror jeg.
            //Check if event exsists with model.password then do the following
            IQueryable<Event> Events = await _eventRepo.GetEvents();
            foreach (var _event in Events)
            {
                if (_event.Pin == int.Parse(model.Password))
                {
                    //Add Guest to DB and connect to the found event. 
                    Guest guest = new Guest
                    {
                        Name = model.Name
                    };
                    _guestRepo.InsertGuest(guest);

                    EventGuest eventGuest = new EventGuest
                    {
                        Event = _event,
                        EventPin = int.Parse(model.Password),
                        Guest = guest,
                        //GuestID = guest.PictureTakerId
                    };
                    _eventGuestRepo.InsertEventGuest(eventGuest);
                        
                    return new AccountModels.ReturnGuestModel
                    {
                        Event = _event,
                        Name = guest.Name
                    };
                }
            }
            return null;
        }
    }
}
