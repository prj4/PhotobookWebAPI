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
        private IGuestRepository _guestRepo;
        private IEventGuestRepository _eventGuestRepo;
        private IEventRepository _eventRepo;

        public GuestController(IGuestRepository guestRepo, IEventGuestRepository eventGuestRepo, IEventRepository eventRepo)
        {
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

        public async Task<ActionResult> LogIn(string name)
        {
            

            return Ok();
        }

        // GET: /<controller>/
     
        [AllowAnonymous]
        [Route("Register")]
        public async Task<AccountModels.ReturnGuestModel> Register(AccountModels.RegisterGuestModel model)
        {

            Event e = await _eventRepo.GetEvent(int.Parse(model.Password));

            if (e != null)
            {
                Guest guest = new Guest
                {
                    Name = model.Name
                };
                _guestRepo.InsertGuest(guest);

                EventGuest eventGuest = new EventGuest
                {
                    Event = e,
                    EventPin = int.Parse(model.Password),
                    Guest = guest,
                    //GuestID = guest.PictureTakerId
                };
                _eventGuestRepo.InsertEventGuest(eventGuest);

                return new AccountModels.ReturnGuestModel
                {
                    Event = e,
                    Name = guest.Name
                };
            }


            return null;
        }
    }
}
