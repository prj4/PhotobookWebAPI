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
using PhotoBookDatabase.Model;

namespace PhotobookWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EventController : Controller
    {
        private readonly string _connectionString;
        private IConfiguration _configuration;

        private UserManager<AppUser> _userManager;
        private SignInManager<AppUser> _signInManager;

        private IEventRepository _eventRepo;

        public EventController(IEventRepository eventRepo)
        {
            _connectionString = _configuration.GetConnectionString("RemoteConnection");

            _eventRepo = eventRepo;
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("ReturnEvent")]
        public GetEventModel ReturnEvent()
        {
            List<Event> test = new List<Event>();
            test.Add(new Event
            {
                Description = "Ting kommer til at ske",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now,
                Location = "Der hjemme",
                Name = "PartyUartig",
                Pin = 1234
            });
            test.Add(new Event
            {
                Description = "Flere ting kommer til at ske",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now,
                Location = "Der hjemme",
                Name = "PartyMereUartig",
                Pin = 3456
            });
            test.Add(new Event
            {
                Description = "Flest ting kommer til at ske",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now,
                Location = "Der hjemme",
                Name = "PartyMestUartig",
                Pin = 5678
            });
                
            GetEventModel _model = new GetEventModel();
            _model.Events = test;
            return _model;
        }
    }
}
