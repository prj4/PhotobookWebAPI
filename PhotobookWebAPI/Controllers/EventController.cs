﻿using System;
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

        public async Task<IActionResult> Index()
        {

            return View(await _eventRepo.GetEvents());
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("RegisterAdmin")]
        public async Task<ActionResult> RegisterAdmin(AccountModels.RegisterAdminModel model)
        {

            var user = new AppUser
                { UserName = model.UserName };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                var roleClaim = new Claim("Role", "Admin");
                await _userManager.AddClaimAsync(user, roleClaim);
                await _signInManager.SignInAsync(user, isPersistent: false);


                return Ok();
            }

            return NotFound();
        }


        [HttpPost]
        [Authorize("IsHost")]
        [Route("CreateEvent")]
        public async Task<ActionResult> CreateEvent(CreateEventModel model)
        {
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
                Pin = pin

            };
            

            _eventRepo.InsertEvent(e);

            return Ok();
        }

    }
}
