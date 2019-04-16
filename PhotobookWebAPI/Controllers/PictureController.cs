using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
        private IGuestRepository _guestRepo;
        private IHostRepository _hostRepo;
        private IPictureRepository _picRepo;

        public PictureController(UserManager<AppUser> userManager, IEventRepository eventRepo, IGuestRepository guestRepo, IHostRepository hostRepo,
            IPictureRepository picRepo)
        {
            _eventRepo = eventRepo;
            _guestRepo = guestRepo;
            _hostRepo = hostRepo;
            _picRepo = picRepo;
            _userManager = userManager;
           
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("GetId's")]
        public async Task<RequestPicturesAnswerModel> GetPictureIds(RequestPicturesModel eventpin)
        {
            //Finder først eventets billeder
            var event_ = await _eventRepo.GetEvent(eventpin.EventPin);
            var pictures_ = event_.Pictures;

            //Gemmer billedernes Id'er over i en retur liste
            RequestPicturesAnswerModel ret = new RequestPicturesAnswerModel();
            foreach (var picture_ in pictures_)
            {
                ret.PictureList.Add(picture_.PictureId);
            }

            //returnerer liste
            return ret;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult GetPicture(PictureModel model)
        {
            CurrentDirectoryHelpers.SetCurrentDirectory();

            var file = Path.Combine(Directory.GetCurrentDirectory(), "Pictures", model.EventPin,
                (model.PictureId.ToString() + ".PNG"));

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

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> DeletePicture(PictureModel model)
        {
            //Sætter stien til filen, ud fra det givne object
            CurrentDirectoryHelpers.SetCurrentDirectory();
            string filepath = Path.Combine(Directory.GetCurrentDirectory(), "Pictures", model.EventPin,
                (model.PictureId.ToString() + ".PNG"));

            //Er det en host eller en guest?
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            string userRole = null;
            var userClaims = await _userManager.GetClaimsAsync(user);
            foreach (var userClaim in userClaims)
            {
                if (userClaim.Type == "Role")
                    userRole = userClaim.Value;
            }

            if (userRole == null) //Hvis ingen af delene
                return NoContent();

            if (userRole == "Guest") //Hvis det er en Guest
            {
                var guest = await _guestRepo.GetGuest(user.Name);
                foreach (var picture in guest.Pictures)
                {
                    if (picture.PictureId == model.PictureId)//Hvis billedet findes i Guestens samling af billeder
                    {
                        if (System.IO.File.Exists(filepath))
                        {
                            System.IO.File.Delete(filepath);
                            return Ok();
                        }
                        return NotFound();
                    }
                }

                return NotFound();
            }

            if (userRole == "Host") //Hvis det er en Host
            {
                var host = await _hostRepo.GetHost(user.Name);
                foreach (var event_ in host.Events)
                {
                    if (event_.Pin == model.EventPin)//Hvis Hosten er host af dette event
                    {
                        if (System.IO.File.Exists(filepath))
                        {
                            System.IO.File.Delete(filepath);
                            return Ok();
                        }
                        return NotFound();
                    }
                }
                
                return NotFound();
            }

            else //Default
            {
                return NotFound();
            }
        }
    }

    }

