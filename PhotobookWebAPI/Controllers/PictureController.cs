using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhotobookWebAPI.Data;
using PhotobookWebAPI.Models;
using PhotoBook.Repository.EventRepository;
using PhotoBook.Repository.GuestRepository;
using PhotoBook.Repository.HostRepository;
using PhotoBook.Repository.PictureRepository;
using PhotoBookDatabase.Model;

namespace PhotobookWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PictureController : Controller
    {

        private UserManager<AppUser> _userManager;
        private IEventRepository _eventRepo;
        private IPictureRepository _picRepo;
        private IGuestRepository _guestRepo;
        private IHostRepository _hostRepo;


        public PictureController(UserManager<AppUser> userManager, IEventRepository eventRepo, IPictureRepository picRepo, IGuestRepository guestRepo, IHostRepository hostRepo )
        {
            _eventRepo = eventRepo;
            _picRepo = picRepo;
            _userManager = userManager;
            _hostRepo = hostRepo;
            _guestRepo = guestRepo;


        }

        [AllowAnonymous]
        [Route("GetImage")]
        [HttpGet]
        public IActionResult GetImage()
        {
            CurrentDirectoryHelpers.SetCurrentDirectory();

            var file = Path.Combine(Directory.GetCurrentDirectory(), "Pictures", "Udklip.PNG");

            return PhysicalFile(file, "image/PNG");
        }


        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> InsertPicture(InsertPictureModel model)
        {
            //Local variables
            int pictureTakerId = 0;

            //Finding logged in user
            var user =  await _userManager.FindByNameAsync(User.Identity.Name);

            //Determining user role
            string userRole = null;
            var userClaims = await _userManager.GetClaimsAsync(user);
            foreach (var userClaim in userClaims)
            {
                if (userClaim.Type == "Role")
                    userRole = userClaim.Value;
            }

            //Getting guest or host
            if (userRole=="Guest")
            {
                Guest guest = await _guestRepo.GetGuest(user.Name);
                pictureTakerId = guest.PictureTakerId;
            }else if (userRole == "Host")
            {
                Host host = await _hostRepo.GetHost(user.Name);
                pictureTakerId = host.PictureTakerId;
            }

            //Generating picture id
            int picId = RandomUnusedNumber(0, 999999);

            
            //Creating picture
            Picture newPicture = new Picture
            {
                EventPin = model.EventPin,
                TakerId = pictureTakerId,
                URL = "TestString"
            };

            //Inserting picture in database
            _picRepo.InsertPicture(newPicture);

            //Setting the current directory correctly 
            CurrentDirectoryHelpers.SetCurrentDirectory();


            //Creating subdirectories for events
            var subdir = Path.Combine(Directory.GetCurrentDirectory(), "Pictures", model.EventPin);
            if (!Directory.Exists(subdir))
            {
                Directory.CreateDirectory(subdir);
            }

            //Creating file and flushing to disk
            var file = Path.Combine(Directory.GetCurrentDirectory(), "Pictures", model.EventPin, picId.ToString()+".PNG");

            var bytes = Convert.FromBase64String(model.PictureString);
            using (var imageFile = new FileStream(file, FileMode.Create))
            {
                imageFile.Write(bytes, 0, bytes.Length);
                imageFile.Flush();
            }


            return Ok();
        }

        private int RandomUnusedNumber(int min, int max)
        {
            Random random = new Random();

            int picId = random.Next(min, max);

            while (_picRepo.GetPicture(picId).Result != null)
            {
                picId = random.Next(min, max);
            }

            return random.Next(min, max);
        }


    }

    }

