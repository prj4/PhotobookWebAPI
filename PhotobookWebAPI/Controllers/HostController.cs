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
        private readonly string _connectionString;
        private IConfiguration _configuration;


        private HostRepository _hostRepo;

        public HostController(IConfiguration iconfig)
        {
            _configuration = iconfig;

            _connectionString = _configuration.GetConnectionString("RemoteConnection");

            _hostRepo = new HostRepository(_connectionString);
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

            return Ok();

        }







    }
}
