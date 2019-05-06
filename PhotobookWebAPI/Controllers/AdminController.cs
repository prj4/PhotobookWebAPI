using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PhotobookWebAPI.Data;
using Microsoft.EntityFrameworkCore;


namespace PhotobookWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AdminController : Controller
    {



        private UserManager<AppUser> _userManager;




        public AdminController(UserManager<AppUser> userManager)
        {

            _userManager = userManager;

        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("Data")]
        [Authorize("IsAdmin")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var accountList = await _userManager.Users.ToListAsync();
            return View("DataPage", accountList);
        }
    }
}
