using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using PhotoBook.Repository.HostRepository;
using PhotoBookDatabase.Model;


namespace PhotobookWebAPI.Controllers
{
    public class HostController : Controller
    {
        private readonly string _connectionString;
        private IConfiguration _configuration;


        private HostRepository _hostRepository;

        public HostController(IConfiguration iconfig)
        {
            _configuration = iconfig;

            _connectionString = _configuration.GetConnectionString("RemoteConnection");

            _hostRepository = new HostRepository(_connectionString);
        }

        public async Task<IActionResult> Index()
        {

            return View(await _hostRepository.GetHosts());
        }



        

        
    }
}
