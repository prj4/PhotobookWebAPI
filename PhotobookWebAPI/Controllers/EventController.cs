using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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



        private IEventRepository _eventRepo;
        private IHostRepository _hostRepo;
        private Logger logger = LogManager.GetCurrentClassLogger();

        private ICurrentUser _currentUser;
        

        public EventController(IEventRepository eventRepo, IHostRepository hostRepo, ICurrentUser currentUser)
        {

            _eventRepo = eventRepo;
            _hostRepo = hostRepo;

            _currentUser = currentUser;
        }


        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("Index")]
        [Authorize("IsAdmin")]
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
            return NotFound("No event found");
        }

        /// <summary>
        /// Gets a Events with specified Host Id [IsHost]
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET api/Event/Host
        ///
        /// </remarks>
        /// <returns>Ok, Events</returns>
        /// <response code="200">Returns list of Specified Events</response>
        /// <response code="204">No event with given Host</response> 
        [HttpGet]
        [Route("Host")]
        [Authorize("IsHost")]
        public async Task<IActionResult> GetEvent()
        {
            
            Host currentHost = await GetCurrentHost(_currentUser.Name());

            var e = await _eventRepo.GetEventsByHostId(currentHost.HostId);
            logger.Info($"GetEvent called with hostId: {currentHost.HostId}");
            if (e != null)
            {
                return Ok(toEventModels(e));
            }
            return NotFound("Could not find any events");
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

            if (e == null)
            {
                return NotFound("No Event");
            }

            if (newData.Description != null)
                e.Description = newData.Description;
            if (newData.Location != null)
                e.Location = newData.Location;
            if (newData.Name != null)
                e.Name = newData.Name;
            if (newData.StartDate != DateTime.MinValue)
            {
                e.StartDate = newData.StartDate;
            }

            if (newData.EndDate != DateTime.MinValue)
            {
                e.EndDate = newData.EndDate;
            }


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
            string userName = _currentUser.Name();

                if (_hostRepo.GetHostByEmail(userName).Result.HostId == _eventRepo.GetEventByPin(pin).Result.HostId)
                {
                    CurrentDirectoryHelpers.SetCurrentDirectory();
                    string filepath = Path.Combine(Directory.GetCurrentDirectory(), "Pictures", pin);

                    try //kan ske det skal fjernes igen, men det kan vi jo lige kigge på..
                    {
                        System.IO.Directory.Delete(filepath, true);
                    }
                    catch (DirectoryNotFoundException e)
                    {
                        logger.Info($"Directory wasnt found, Database deletion will continue, exception caught: {e}");
                    }   
                    
                    await _eventRepo.DeleteEventByPin(pin);

                    logger.Info($"Deleted Event with pin: {pin}");

                    return NoContent();
                }
            return BadRequest("Permission Issue: Not Hosts Event");

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
        public async Task<IActionResult> CreateEvent(EventModel model)
        {
            //Gets the username of the current AppUser
            var currentUserName = _currentUser.Name();

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
            Event testEvent = await _eventRepo.GetEventByPin(pin);
            if (testEvent!=null)
            {
                CurrentDirectoryHelpers.SetCurrentDirectory();
                var subdir = Path.Combine(Directory.GetCurrentDirectory(), "Pictures", pin);
                if (!Directory.Exists(subdir))
                {
                    Directory.CreateDirectory(subdir);
                    logger.Info($"Subdir created for Event: {pin}");
                }

                return Ok(new EventPinModel
                {
                    pin=newEvent.Pin
                });

            }

         
            return BadRequest("Event Not Created");
        }



       
        [NonAction]
        private async Task<Host> GetCurrentHost(string currentUserName)
        {

            var currentHost =await _hostRepo.GetHostByEmail(currentUserName);

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
                        Pin = ev.Pin,
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
                Pin = ev.Pin,
                StartDate = ev.StartDate
            };

            return eventModel;
        }

    }
}
