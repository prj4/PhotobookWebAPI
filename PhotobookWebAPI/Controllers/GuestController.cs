using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhotoBook.Repository.GuestRepository;

namespace PhotobookWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class GuestController:Controller
    {

        private IGuestRepository _guestRepo;

        public GuestController(IGuestRepository guestRepo)
        {
             _guestRepo = guestRepo;

        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("Index")]
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Index()
        {

            return View(await _guestRepo.GetGuests());
        }
    }
}
