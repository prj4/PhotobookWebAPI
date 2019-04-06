using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using PhotobookWebAPI.Data;
using PhotobookWebAPI.Models;
using PhotoBook.Repository.EventGuestRepository;
using PhotoBook.Repository.EventRepository;
using PhotoBook.Repository.GuestRepository;
using PhotoBook.Repository.HostRepository;
using PhotoBookDatabase.Model;
using Remotion.Linq.Parsing.Structure.IntermediateModel;

namespace PhotobookWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EventController : Controller
    {


        private Microsoft.AspNetCore.Identity.UserManager<AppUser> _userManager;
        private IEventRepository _eventRepo;
        private IHostRepository _hostRepo;
        private Utility _utility;

        public EventController(IEventRepository eventRepo, IHostRepository hostRepo, Microsoft.AspNetCore.Identity.UserManager<AppUser> userManager)
        {

            _userManager = userManager;
            _eventRepo = eventRepo;
            _hostRepo = hostRepo;
            _utility= new Utility(_userManager, _hostRepo); 
        }

        [Route("Index")]
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {

            return View(await _eventRepo.GetEvents());
        }


        // GET: api/Event
        [HttpGet]
        [AllowAnonymous]
        public async Task<IQueryable<Event>> GetEvents()
        {
            return await _eventRepo.GetEvents();
        }


        // GET: api/Event/1234
        [HttpGet("{pin}")]
        [AllowAnonymous]
        public async Task<Event> GetEvent(int pin)
        {
            var e = await _eventRepo.GetEvent(pin);
            return e;
        }

        // PUT: api/Account/1234
        [HttpPut("{pin}")]
        [AllowAnonymous]
        public async Task<IActionResult> PutEvent(int pin, EditEventModel newData)
        {
            Event e = await _eventRepo.GetEvent(pin);

            if (e== null)
            {
                return NotFound();
            }

            if (newData.Description != null)
                e.Description = newData.Description;
            if (newData.Location != null)
                e.Location = newData.Location;
            if (newData.Name != null)
                e.Name = newData.Name;

                e.EndDate = newData.EndDate;
                e.StartDate = newData.StartDate;


            _eventRepo.UpdateEvent(e);
            return NoContent();
        }

        // DELETE: api/Event/1234
        [HttpDelete("{pin}")]
        [AllowAnonymous]
        public async Task<IActionResult> DeleteEvent(int pin)
        {
            _eventRepo.DeleteEvent(pin);

            return NoContent();
        }


        [HttpPost]
        [Authorize("IsHost")]
        [Route("CreateEvent")]
        public async Task<ActionResult> CreateEvent(CreateEventModel model)
        {
            //Gets the username of the current AppUser
            var currentUserName = HttpContext.User.Identity.Name;

            //Gets the corresponding Host in the DB
            var currentHost = _utility.GetCurrentHost(currentUserName).Result;


            //Gets a pin that is not used
            int pin = getRandomPin().Result;

            //Creates Event
            Event newEvent = new Event
            {
                Name = model.Name,
                Description = model.Description,
                EndDate = model.EndDate,
                Location = model.Location,
                StartDate = model.StartDate,
                HostId = currentHost.PictureTakerId
                //Pin is missing for now

            };

            //Inserts the Event in the DB
            _eventRepo.InsertEvent(newEvent);

            //Validating that it is in the DB

           // Event testEvent = _eventRepo.GetEvent(pin).Result;
            //if (testEvent!=null)
            //{
                return Ok();
            //}

         
            return NotFound();
        }


        private async Task<int> getRandomPin()
        {
            int _min = 0000;
            int _max = 9999;
            Random _rdm = new Random();
            int pin = _rdm.Next(_min, _max);

            IQueryable<Event> events = await _eventRepo.GetEvents();


            Event testEvent = _eventRepo.GetEvent(pin).Result;
            //Generates new pins until it finds one that is not used
            while (testEvent!=null)
            {
                testEvent = _eventRepo.GetEvent(pin).Result;
                pin = _rdm.Next(_min, _max);
            }

            return pin;
        }

    }
}
