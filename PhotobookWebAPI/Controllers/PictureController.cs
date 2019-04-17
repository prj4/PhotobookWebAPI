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

        [Route("Index")]
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return View(await _picRepo.GetPictures());
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("Ids")]
        public async Task<RequestPicturesAnswerModel> GetPictureIds(RequestPicturesModel eventpin)
        {
            //Finder først eventets billeder
            var event_ = await _eventRepo.GetEventByPin(eventpin.EventPin);

            if (event_.Pictures == null)
            {
                return null;
            }

            List<Picture> pictures_ = event_.Pictures;

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
                (model.PictureId + ".PNG"));

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
                Guest guest = await _guestRepo.GetGuestByNameAndEventPin(user.Name, model.EventPin);
                pictureTakerId = guest.PictureTakerId;
            }else if (userRole == "Host")
            {
                Host host = await _hostRepo.GetHostByEmail(user.Email);
                pictureTakerId = host.PictureTakerId;
            }

            //Creating picture
            Picture newPicture = new Picture
            {
                EventPin = model.EventPin,
                TakerId = pictureTakerId
            };

            //Inserting picture in database
            int picId = await _picRepo.InsertPicture(newPicture);

            //Setting the current directory correctly 
            CurrentDirectoryHelpers.SetCurrentDirectory();


            //Creating subdirectories for events
            var subdir = Path.Combine(Directory.GetCurrentDirectory(), "Pictures", model.EventPin);
            if (!Directory.Exists(subdir))
            {
                Directory.CreateDirectory(subdir);
            }

            //Creating file and flushing to disk
            var file = Path.Combine(Directory.GetCurrentDirectory(), "Pictures", model.EventPin, picId+".PNG");

            var bytes = Convert.FromBase64String(model.PictureString);
            using (var imageFile = new FileStream(file, FileMode.Create))
            {
                imageFile.Write(bytes, 0, bytes.Length);
                imageFile.Flush();
            }
            
            return Ok();
        }


        /// <summary>
        /// Deletes a picture from the server and the database.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     DELETE /Todo
        ///     {
        ///        "EventPin": awf122km2,
        ///        "PictureId": 1
        ///     }
        ///
        /// </remarks>
        /// <param name="item"></param>
        /// <returns>A newly created TodoItem</returns>
        /// <response code="201">Returns the newly created item</response>
        /// <response code="400">If the item is null</response>   
        [AllowAnonymous]
        [HttpDelete]
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
                return NotFound();

            if (userRole == "Guest") //Hvis det er en Guest
            {
                var guest = await _guestRepo.GetGuestByNameAndEventPin(user.Name, model.EventPin);
                foreach (var picture in guest.Pictures)
                {
                    if (picture.PictureId == model.PictureId) //Hvis billedet findes i Guestens samling af billeder
                    {
                        if (System.IO.File.Exists(filepath))
                        {
                            System.IO.File.Delete(filepath);
                            return NoContent();
                        }

                        return NotFound();
                    }
                }

                return Unauthorized();
            }

            if (userRole == "Host") //Hvis det er en Host
            {
                var host = await _hostRepo.GetHostByEmail(user.Email);
                foreach (var event_ in host.Events)
                {
                    if (event_.Pin == model.EventPin) //Hvis Hosten er host af dette event
                    {
                        if (System.IO.File.Exists(filepath))
                        {
                            System.IO.File.Delete(filepath);
                            return NoContent();
                        }

                        return NotFound();
                    }
                }

                return Unauthorized();
            }

            return NotFound();
        }
    }

}

