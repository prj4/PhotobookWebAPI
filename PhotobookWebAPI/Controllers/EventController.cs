using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using PhotobookWebAPI.Data;
using PhotobookWebAPI.Models;
using PhotoBook.Repository.EventGuestRepository;
using PhotoBook.Repository.EventRepository;
using PhotoBook.Repository.GuestRepository;
using PhotoBook.Repository.HostRepository;
using PhotoBookDatabase.Model;

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
        public async Task<IActionResult> Index()
        {

            return View(await _eventRepo.GetEvents());
        }

        [HttpPost]
        [Authorize("IsHost")]
        [Route("CreateEvent")]
        public async Task<ActionResult> CreateEvent(CreateEventModel model)
        {
           

            var currentUserName = HttpContext.User.Identity.Name;

            var currentUser = await _userManager.FindByNameAsync(currentUserName);

            var currentHost = _hostRepo.GetHost(currentUser.Name).Result;
            
            

            int _min = 0000;
            int _max = 9999;
            Random _rdm = new Random();
            int pin = _rdm.Next(_min, _max);


            Event e = new Event
            {
                Name = model.Name,
                Description = model.Description,
                EndDate = model.EndDate,
                Location = model.Location,
                StartDate = model.StartDate,
                HostId = currentHost.PictureTakerId

            };

            _eventRepo.InsertEvent(e);

            return Ok();
        }

    }
}
