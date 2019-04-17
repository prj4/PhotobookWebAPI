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

using PhotoBook.Repository.EventRepository;
using PhotoBook.Repository.GuestRepository;
using PhotoBook.Repository.HostRepository;
using PhotoBookDatabase.Model;

namespace PhotobookWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class GuestController : Controller
    {
        private IGuestRepository _guestRepo;
        private IEventRepository _eventRepo;

        public GuestController(IGuestRepository guestRepo, IEventRepository eventRepo)
        {
            _guestRepo = guestRepo;
            _eventRepo = eventRepo;

        }

        [HttpGet]
        [Route("Index")]
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            return View(await _guestRepo.GetGuests());
        }


        // GET: api/Guest
        [HttpGet]
        [AllowAnonymous]
        public async Task<IEnumerable<Guest>> GetGuests()
        {
            return await _guestRepo.GetGuests();
        }

        [AllowAnonymous]
        [HttpDelete]
        public async Task<ActionResult> DeleteGuest(string name, string pin)
        {
            await _guestRepo.DeleteGuestByNameAndEventPin(name, pin);

            return NoContent();
        }


        // GET: /<controller>/
     
        [AllowAnonymous]
        [Route("Register")]
        [HttpPost]
        public async Task<AccountModels.ReturnGuestModel> RegisterGuest(AccountModels.RegisterGuestModel model)
        {
            var e = await _eventRepo.GetEventByPin(model.Pin);
            
            Guest guest = new Guest
            {
                Name = model.Name,
                EventPin = model.Pin
            };
            await _guestRepo.InsertGuest(guest);


            return new AccountModels.ReturnGuestModel
            {
                Event = e,
                Name = guest.Name
            };
        }
    }
}
