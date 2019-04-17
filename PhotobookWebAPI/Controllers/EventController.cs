using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
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


        public EventController(IEventRepository eventRepo, IHostRepository hostRepo, Microsoft.AspNetCore.Identity.UserManager<AppUser> userManager)
        {

            _userManager = userManager;
            _eventRepo = eventRepo;
            _hostRepo = hostRepo;
        }

        [Route("Index")]
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Index()
        {

            return View(await _eventRepo.GetEvents());
        }


        // GET: api/Event
        [HttpGet]
        [AllowAnonymous]
        public async Task<IEnumerable<Event>> GetEvents()
        {
            return await _eventRepo.GetEvents();
        }


        // GET: api/Event/1234
        [HttpGet("{pin}")]
        [AllowAnonymous]
        public async Task<Event> GetEvent(string pin)
        {
            var e = await _eventRepo.GetEventByPin(pin);
            return e;
        }

        // PUT: api/Event/1234
        [HttpPut("{pin}")]
        [AllowAnonymous]
        public async Task<IActionResult> PutEvent(string pin, EditEventModel newData)
        {
            Event e = await _eventRepo.GetEventByPin(pin);

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


            await _eventRepo.UpdateEvent(e);
            return Ok();
        }

        // DELETE: api/Event/1234
        [HttpDelete("{pin}")]
        [AllowAnonymous]
        public async Task<IActionResult> DeleteEvent(string pin)
        {
            await _eventRepo.DeleteEventByPin(pin);

            return Ok();
        }


        [HttpPost]
        [Authorize("IsHost")]
        public async Task<ActionResult> CreateEvent(CreateEventModel model)
        {
            //Gets the username of the current AppUser
            var currentUserName = User.Identity.Name;

            //Gets the corresponding Host in the DB
            var currentHost = await GetCurrentHost(currentUserName);


            //Gets a pin that is not used
            string pin = RandomPassword();

            //Creates Event
            Event newEvent = new Event
            {
                Name = model.Name,
                Description = model.Description,
                EndDate = model.EndDate,
                Location = model.Location,
                StartDate = model.StartDate,
                HostId = currentHost.PictureTakerId,
                Pin = pin

            };

            //Inserts the Event in the DB
            await _eventRepo.InsertEvent(newEvent);

            //Validating that it is in the DB

            Event testEvent =await  _eventRepo.GetEventByPin(pin);
            if (testEvent!=null)
            {
                return Ok();
            }

         
            return NotFound();
        }



        [NonAction]
        private async Task<AppUser> GetCurrentAppUser(string currentUserName)
        {

            var currentUser = await _userManager.FindByNameAsync(currentUserName);

            return currentUser;
        }
        [NonAction]
        private async Task<Host> GetCurrentHost(string currentUserName)
        {

            var currentHost =await _hostRepo.GetHostByEmail(GetCurrentAppUser(currentUserName).Result.Email);

            return currentHost;
        }



        [NonAction]
        // Generate a random number between two numbers    
        private int RandomNumber(int min, int max)
        {
            Random random = new Random();
            return random.Next(min, max);
        }


        [NonAction]
        // Generate a random string with a given size    
        public string RandomString(int size, bool lowerCase)
        {
            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }
            if (lowerCase)
                return builder.ToString().ToLower();
            return builder.ToString();
        }


        [NonAction]
        // Generate a random password    
        public string RandomPassword()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(RandomString(4, true));
            builder.Append(RandomNumber(1000, 9999));
            builder.Append(RandomString(2, true));
            return builder.ToString();
        }


    }
}
