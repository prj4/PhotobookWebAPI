using System;
using System.Collections.Generic;
using System.IO;
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
using NLog;
using PB.Dto;
using PhotobookWebAPI.Data;
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
        private Logger logger = LogManager.GetCurrentClassLogger();


        public EventController(IEventRepository eventRepo, IHostRepository hostRepo, Microsoft.AspNetCore.Identity.UserManager<AppUser> userManager)
        {

            _userManager = userManager;
            _eventRepo = eventRepo;
            _hostRepo = hostRepo;
        }


        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("Index")]
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Index()
        {

            return View(await _eventRepo.GetEvents());
        }


        /// <summary>
        /// Gets a list of all Events registered in the database
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET api/Event
        ///
        /// </remarks>
        /// <returns>Ok, list of Events</returns>
        /// <response code="200">Returns list of all Events, empty list if no Events</response> 
        [HttpGet]
        public async Task<IEnumerable<EventModel>> GetEvents()
        {
            logger.Info("GetEvents called");

            return toEventModels(await _eventRepo.GetEvents());
        }


        /// <summary>
        /// Gets a Event with specified Eventpin
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET api/Event/abcd1234ef
        ///
        /// </remarks>
        /// <returns>Ok, Event</returns>
        /// <response code="200">Returns Specified Event</response>
        /// <response code="204">No event with given pin</response> 
        [HttpGet("{pin}")]
        public async Task<ActionResult> GetEvent(string pin)
        {
            var e = await _eventRepo.GetEventByPin(pin);
            logger.Info($"GetEvent called with pin: {pin}");
            if (e != null)
            {
                return Ok(toEventModel(e));
            }
            return NoContent();
        }

        /// <summary>
        /// Gets a Events with specified Host Id
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET api/Event/Host12
        ///
        /// </remarks>
        /// <returns>Ok, Events</returns>
        /// <response code="200">Returns list of Specified Events</response>
        /// <response code="204">No event with given Host</response> 
        [HttpGet]
        [Route("Host{hostId}")]
        public async Task<IActionResult> GetEvent(int hostId)
        {
            var e = await _eventRepo.GetEventsByHostId(hostId);
            logger.Info($"GetEvent called with hostId: {hostId}");
            if (e != null)
            {
                return Ok(toEventModels(e));
            }
            return NoContent();
        }

        /// <summary>
        /// Edits an event with the specified pin
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     PUT api/Event/abcd1234ef
        ///     {
        ///     "location": "My Crib",
        ///     "description": "Party at my crib",
        ///     "name": "Crib party",
        ///     "startDate": "2019-04-21T08:28:16.885Z",
        ///     "endDate": "2019-04-21T08:28:16.885Z"
        ///     }
        ///
        /// </remarks>
        /// <returns>Ok</returns>
        /// <response code="204">Event updated with values in JSON object</response>
        /// <response code="404">No event with given pin</response>
        [Authorize("IsHost")]
        [HttpPut("{pin}")]
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


                logger.Info($"Event with pin: {pin} changed with the new values Description: {newData.Description}, Location: {newData.Location}, Name: {newData.Name}, StartDate: {newData.StartDate}, EndDate: {newData.StartDate}" );

            await _eventRepo.UpdateEvent(e);
            return NoContent();
        }

        /// <summary>
        /// Deletes an event with the specified pin
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     DELETE api/Event/abcd1234ef
        ///
        /// </remarks>
        /// <returns>NoContent</returns>
        /// <response code="204">Event with specified pin has been deleted</response>
        /// <response code="400">Event</response>
        [Authorize("IsHost")]
        [HttpDelete("{pin}")]
        public async Task<IActionResult> DeleteEvent(string pin)
        {
            //Bestemmer den bruger som er logget ind
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            //bestemmer brugerens rolle
            string userRole = null;
            var userClaims = await _userManager.GetClaimsAsync(user);
            foreach (var userClaim in userClaims)
            {
                if (userClaim.Type == "Role")
                    userRole = userClaim.Value;
            }

            if (userRole == "Host")
            {
                if (_hostRepo.GetHostByEmail(user.Email).Result.HostId == _eventRepo.GetEventByPin(pin).Result.HostId)
                {
                    CurrentDirectoryHelpers.SetCurrentDirectory();
                    string filepath = Path.Combine(Directory.GetCurrentDirectory(), "Pictures", pin);

                    System.IO.Directory.Delete(filepath,true);

                    await _eventRepo.DeleteEventByPin(pin);

                    logger.Info($"Deleted Event with pin: {pin}");

                    return NoContent();
                }

            }

            return BadRequest();

        }

        /// <summary>
        /// Creates an event [IsHost]
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST api/Event
        ///     {
        ///     "location": "My Crib",
        ///     "description": "Party at my crib",
        ///     "name": "Crib party",
        ///     "startDate": "2019-04-21T08:28:16.885Z",
        ///     "endDate": "2019-04-21T08:28:16.885Z"
        ///     }
        /// </remarks>
        /// <returns>Ok</returns>
        /// <response code="200">Event has been created</response>
        /// <response code="400">Failure to create event</response>
        [HttpPost]
        [Authorize("IsHost")]
        public async Task<ActionResult> CreateEvent(EventModel model)
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
                HostId = currentHost.HostId,
                Pin = pin

            };

            logger.Info($"Created event with Name: {newEvent.Name}, Pin: {newEvent.Pin}, Description: {newEvent.Description}, Location: {newEvent.Location}, StartDate: {newEvent.StartDate}, EndDate: {newEvent.EndDate}, HostId: {newEvent.HostId}");

            //Inserts the Event in the DB
            await _eventRepo.InsertEvent(newEvent);

            //Validating that it is in the DB

            Event testEvent =await  _eventRepo.GetEventByPin(pin);
            if (testEvent!=null)
            {
                return Ok(new EventPinModel
                {
                    pin=newEvent.Pin
                });

            }

         
            return BadRequest();
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


        [NonAction]
        private IEnumerable<EventModel> toEventModels(IEnumerable<Event> events)
        {
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
                        Pin = ev.Name,
                        StartDate = ev.StartDate
                    });
                }
            }
            return eventModels;
        }

        [NonAction]
        private EventModel toEventModel(Event ev)
        {
            EventModel eventModel = new EventModel()
            {
                Description = ev.Description,
                EndDate = ev.EndDate,
                Location = ev.Location,
                Name = ev.Name,
                Pin = ev.Name,
                StartDate = ev.StartDate
            };

            return eventModel;
        }

    }
}
