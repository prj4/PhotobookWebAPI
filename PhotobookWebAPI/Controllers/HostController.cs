using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PhotobookWebAPI.Data;
using PhotoBook.Repository.EventRepository;
using PhotoBook.Repository.GuestRepository;
using PhotoBook.Repository.HostRepository;

namespace PhotobookWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class HostController : Controller
    {



        private IHostRepository _hostRepo;




        public HostController(IHostRepository hostRepo)
        {

            _hostRepo = hostRepo;

        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("Index")]
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Index()
        {

            return View(await _hostRepo.GetHosts());
        }
    }
}
