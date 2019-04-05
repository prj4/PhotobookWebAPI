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


        public async Task<ActionResult> Register(AccountModels.RegisterHostModel model)
        {
            Host host = new Host { Name = model.Name, Email = model.Email };


            _hostRepo.InsertHost(host);

            //Finder information til returnering af host data.
            Host toReturn = await _hostRepo.GetHost(host.Name);
            AccountModels.ReturnHostModel ret = new AccountModels.ReturnHostModel
            {
                Name = toReturn.Name,
                Email = toReturn.Email,
                Events = toReturn.Events
            };

            return Ok();

        }







    }
}
