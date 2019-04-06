using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using PhotobookWebAPI.Models;
using PhotoBook.Repository.HostRepository;
using PhotoBookDatabase.Model;


namespace PhotobookWebAPI.Controllers
{
    public class HostController : Controller
    {

        private IHostRepository _hostRepo;

        public HostController(IHostRepository hostRepo)
        {

            _hostRepo = hostRepo;
        }

        public async Task<IActionResult> Index()
        {

            return View(await _hostRepo.GetHosts());
        }


        public async Task<ActionResult> Delete(string name)
        {
            _hostRepo.DeleteHost(name);

            return Ok();

        }

        /*
        public async Task<AccountModels.RegisterHostModel> LogIn(string name)
        {


            return new AccountModels.ReturnHostModel
            {
                Name = host.Name,
                Email = host.Email,
                Events = 
            };

        }
        */

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
