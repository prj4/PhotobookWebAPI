﻿using System;
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
using NLog;
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
        private Logger logger = LogManager.GetCurrentClassLogger();

        public PictureController(UserManager<AppUser> userManager, IEventRepository eventRepo, IGuestRepository guestRepo, IHostRepository hostRepo,
            IPictureRepository picRepo)
        {
            _eventRepo = eventRepo;
            _guestRepo = guestRepo;
            _hostRepo = hostRepo;
            _picRepo = picRepo;
            _userManager = userManager;
           
        }

        /// <summary>
        /// Gets all the picture data from the database.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <returns>A view of the pictures in the database.</returns>
        /// <response>A view of the pictures in the database.</response>
        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("Index")]
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return View(await _picRepo.GetPictures());
        }

        /// <summary>
        /// Gets all the picture Ids for a given Event
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET api/Picture/Ids/{EventPin}
        ///
        /// </remarks>
        /// <returns>List of Ids of pictures from the requested event.</returns>
        /// <response Task='null'>If no pictures have been taken at the event.</response>
        /// <response Task='List of Picture Ids'>If there are pictures from the event.</response>
        [HttpGet]
        [Route("Ids/{EventPin}")]
        public async Task<RequestPicturesAnswerModel> GetPictureIds(string EventPin)
        {
            //Finder først eventets billeder
            var event_ = await _eventRepo.GetEventByPin(EventPin);

            if (event_.Pictures == null)
            {
                return null;
            }
            List<Picture> pictures_ = event_.Pictures;

            //Gemmer billedernes Id'er over i en retur liste
            List<int> Ids = new List<int>();
            RequestPicturesAnswerModel ret = new RequestPicturesAnswerModel();
            foreach (var picture_ in pictures_)
            {
                Ids.Add(picture_.PictureId);
            }
            logger.Info($"GetPictureIds Called");
            //returnerer liste
            return new RequestPicturesAnswerModel
            {
                PictureList = Ids
            };
        }

        /// <summary>
        /// Gets a Picture from the server.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /api/Picture/{EventPin}/{PictureId}
        ///     
        ///{
        /// </remarks>
        /// <returns>A physical file, a picture.</returns>
        /// <response code='200'>Physical file, the requested picture.</response>
        /// /// <response code='404'>Picture file not found</response>
        [HttpGet("{EventPin}/{PictureId}")]
        public IActionResult GetPicture(string EventPin, int PictureId)
        {
            CurrentDirectoryHelpers.SetCurrentDirectory();

            var file = Path.Combine(Directory.GetCurrentDirectory(), "Pictures", EventPin,
                (PictureId + ".PNG"));


            if (System.IO.File.Exists(file))
            {
                logger.Info($"Returning picture at Event: {EventPin}, with Id: {PictureId}");
                return PhysicalFile(file, "image/PNG");
            }
            logger.Info($"Picture at Event: {EventPin}, with Id: {PictureId} requested but not found");
            return NotFound();
        }

        /// <summary>
        /// Insert a Picture onto the server and into the database.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST
        ///     {
        ///        "Picture string": base64,
        ///        "EventPin": 123wer12f1
        ///     }
        ///
        /// </remarks>
        /// <returns>Ok, Picture has been inserted into database and put on server.</returns>
        /// <response code="200">Picture has been inserted into database and put on server.</response>
        [HttpPost]
        public async Task<IActionResult> InsertPicture(InsertPictureModel model)
        {
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

            Picture newPicture = new Picture();

            //Getting guest or host
            if (userRole=="Guest")
            {
                Guest guest = await _guestRepo.GetGuestByNameAndEventPin(user.Name, model.EventPin);
                //Creating picture for database if a guest took the picture
                newPicture.EventPin = model.EventPin;
                newPicture.GuestId = guest.GuestId;
            }
            else if (userRole == "Host")
            {
                Host host = await _hostRepo.GetHostByEmail(user.Email);
                //Creating picture for database if host took the picture
                newPicture.EventPin = model.EventPin;
                newPicture.HostId = host.HostId;
            }

            
            
            //Inserting picture in database
            int picId = await _picRepo.InsertPicture(newPicture);

            logger.Info($"User with UserName: {user.UserName} Inserts picture in db with for Event: {newPicture.EventPin} with PictureId: {newPicture.PictureId}");

            //Setting the current directory correctly 
            CurrentDirectoryHelpers.SetCurrentDirectory();


            //Creating subdirectories for events
            var subdir = Path.Combine(Directory.GetCurrentDirectory(), "Pictures", model.EventPin);
            if (!Directory.Exists(subdir))
            {
                Directory.CreateDirectory(subdir);
                logger.Info($"Subdir created for Event: {model.EventPin}");
            }

            //Creating file and flushing to disk
            var file = Path.Combine(Directory.GetCurrentDirectory(), "Pictures", model.EventPin, picId+".PNG");

            var bytes = Convert.FromBase64String(model.PictureString);
            using (var imageFile = new FileStream(file, FileMode.Create))
            {
                imageFile.Write(bytes, 0, bytes.Length);
                imageFile.Flush();
                logger.Info($"Picture with Id: {picId} saved in  event subdir: {model.EventPin}");
            }
            
            return Ok();
        }


        /// <summary>
        /// Deletes a picture from the server and the database.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     DELETE
        ///     {
        ///        "EventPin": awf122km2,
        ///        "PictureId": 1
        ///     }
        ///
        /// </remarks>
        /// <returns>No Content, Picture deleted</returns>
        /// <response code="204">Deleted the requested picture</response>
        /// <response code="401">If you don't have the right to delete the picture</response>   
        /// <response code="404">If the users claim is not recognized or,
        ///                      If the the picture wasn't found on the server.</response>
        [HttpDelete]
        public async Task<IActionResult> DeletePicture(PictureModel model)
        {
            //Sætter stien til filen, ud fra det givne billede id og eventpin.
            CurrentDirectoryHelpers.SetCurrentDirectory();
            string filepath = Path.Combine(Directory.GetCurrentDirectory(), "Pictures", model.EventPin,
                (model.PictureId.ToString() + ".PNG"));

            //Bestemmer den bruger som er logget ind
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            //bestemmer brugerens rolle
            string userRole = null;
            var userClaims = await _userManager.GetClaimsAsync(user);
            foreach (var userClaim in userClaims)
            {
                if (userClaim.Type == "Role")
                    userRole = userClaim.Value;
            }

            if (userRole == "Guest")
            {
                var event_ = await _eventRepo.GetEventByPin(model.EventPin);
                var guest = await _guestRepo.GetGuestByNameAndEventPin(user.Name, model.EventPin);
                foreach (var picture in event_.Pictures)
                {
                    if ((picture.PictureId == model.PictureId) && (picture.GuestId == guest.GuestId)) //Hvis billedet findes i Guestens samling af billeder
                    {
                        if (System.IO.File.Exists(filepath))
                        {
                            System.IO.File.Delete(filepath);
                            await _picRepo.DeletePictureById(model.PictureId);
                            return NoContent();
                        }

                        return NotFound();
                    }
                }

                return Unauthorized();
            }
            if (userRole == "Host")
            {
                var host = await _hostRepo.GetHostByEmail(user.Email);
                foreach (var event_ in host.Events)
                {
                    if (event_.Pin == model.EventPin) //Hvis Hosten er host af dette event
                    {
                        if (System.IO.File.Exists(filepath))
                        {
                            System.IO.File.Delete(filepath);
                            await _picRepo.DeletePictureById(model.PictureId);
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

