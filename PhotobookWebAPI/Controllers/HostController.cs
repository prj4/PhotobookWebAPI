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
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class HostController : Controller
    {

        private IHostRepository _hostRepo;
        private IEventRepository _eventRepo;

        public HostController(IHostRepository hostRepo, IEventRepository eventRepo)
        {
            _eventRepo = eventRepo;
            _hostRepo = hostRepo;
        }

        [HttpGet]
        [Route("Index")]
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {

            return View(await _hostRepo.GetHosts());
        }

        [AllowAnonymous]
        [HttpDelete]
        public async Task<ActionResult> DeleteHost(string email)
        {
             await _hostRepo.DeleteHostByEmail(email);

            return NoContent();

        }

        [AllowAnonymous]
        [HttpGet]
        [Route("Login")]
        public async Task<AccountModels.ReturnHostModel> Login(string email)
        {
            var host = await _hostRepo.GetHostByEmail(email);
            var events = await _eventRepo.GetEventsByHostId(host.HostId);
            return new AccountModels.ReturnHostModel
            {
                Name = host.Name,
                Email = email,
                Events = events
            };

        }
        [AllowAnonymous]
        [HttpPost]
        public async Task<AccountModels.ReturnHostModel> RegisterHost(string name, string email, string password)
        {
            Host host = new Host { Name = name, Email = email };


            await _hostRepo.InsertHost(host);

            //Returnering af host data (Nyoprettet dermed ingen events).
            return new AccountModels.ReturnHostModel
            {
                Name = host.Name,
                Email = host.Email
            };

        }







    }
}
