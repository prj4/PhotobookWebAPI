using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using PhotobookWebAPI.Models;
using PhotoBook.Repository.EventRepository;
using PhotoBook.Repository.HostRepository;
using PhotoBookDatabase.Model;


namespace PhotobookWebAPI.Controllers
{
    public class HostController : Controller
    {

        private IHostRepository _hostRepo;
        private IEventRepository _eventRepo;

        public HostController(IHostRepository hostRepo, IEventRepository eventRepo)
        {
            _eventRepo = eventRepo;
            _hostRepo = hostRepo;
        }

        public async Task<IActionResult> Index()
        {

            return View(await _hostRepo.GetHosts());
        }


        public async Task<ActionResult> Delete(string email)
        {
            _hostRepo.DeleteHostByEmail(email);

            return Ok();

        }

        
        public async Task<AccountModels.ReturnHostModel> LogIn(string name, string email)
        {
            var host = await _hostRepo.GetHostByEmail(email);
            var events = await _eventRepo.GetEventsByHostId(host.PictureTakerId);
            return new AccountModels.ReturnHostModel
            {
                Name = name,
                Email = email,
                Events = events
            };

        }
        

        public async Task<AccountModels.ReturnHostModel> Register(AccountModels.RegisterHostModel model)
        {
            Host host = new Host { Name = model.Name, Email = model.Email };


            _hostRepo.InsertHost(host);

            //Returnering af host data (Nyoprettet dermed ingen events).
            return new AccountModels.ReturnHostModel
            {
                Name = host.Name,
                Email = host.Email
            };

            //return Ok();

        }







    }
}
