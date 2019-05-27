using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NLog;
using PB.Dto;
using PhotobookWebAPI.Wrappers;
using PhotoBook.Repository.EventRepository;
using PhotoBook.Repository.GuestRepository;
using PhotoBook.Repository.HostRepository;
using PhotoBook.Repository.PictureRepository;
using PhotoBookDatabase.Model;
using PhotoSauce.MagicScaler;

namespace PhotobookWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PictureController : Controller
    {
        private IEventRepository _eventRepo;
        private IGuestRepository _guestRepo;
        private IHostRepository _hostRepo;
        private IPictureRepository _picRepo;
        private Logger logger = LogManager.GetCurrentClassLogger();
        private ICurrentUser _currentUser;
        private IFileSystem _fileSystem;

        public PictureController( IEventRepository eventRepo, IGuestRepository guestRepo, IHostRepository hostRepo,
            IPictureRepository picRepo, ICurrentUser currentUser, IFileSystem fileSystem)
        {
            _eventRepo = eventRepo;
            _guestRepo = guestRepo;
            _hostRepo = hostRepo;
            _picRepo = picRepo;
            _currentUser = currentUser;
            _fileSystem = fileSystem;
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
        /// <response Task='204'>No pictures at the Event</response>
        /// <response Task='200'>En liste af billede Id'er</response>
        [HttpGet]
        [Route("Ids/{EventPin}")]
        public async Task<IActionResult> GetPictureIds(string EventPin)
        {
            //Finder først eventets billeder
            var event_ = await _eventRepo.GetEventByPin(EventPin);

            if (event_.Pictures == null)
            {
                return NoContent();
            }
            List<Picture> pictures_ = event_.Pictures;

            //Gemmer billedernes Id'er over i en retur liste
            List<int> Ids = new List<int>();
            PicturesAnswerModel ret = new PicturesAnswerModel();
            foreach (var picture_ in pictures_)
            {
                Ids.Add(picture_.PictureId);
            }
            logger.Info($"GetPictureIds Called");

            //returnerer liste
            return Ok(new PicturesAnswerModel
            {
                PictureList = Ids
            });
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
        /// <response code='404'>Picture file not found</response>
        [HttpGet("{EventPin}/{PictureId}")]
        public IActionResult GetPicture(string EventPin, int PictureId)
        {
            CurrentDirectoryHelpers.SetCurrentDirectory();


            Picture pic = _picRepo.GetPictureById(PictureId).Result;

            int? guestId = pic.GuestId;
            int? hostId = pic.HostId;
            string pictureTakerName = "";
            Host h = new Host();
            Guest g = new Guest();
            if (guestId != null)
            {
                g = _guestRepo.GetGuestById((int)guestId).Result;
                pictureTakerName = g.Name;
            }else if (hostId!=null)
            {
                h = _hostRepo.GetHostById((int)hostId).Result;
                pictureTakerName = h.Name;
            }

            

            var file = Path.Combine(Directory.GetCurrentDirectory(), "Pictures", EventPin,
                (PictureId + ".PNG"));

            if (_fileSystem.FileExists(file))
            {
                logger.Info($"Returning picture at Event: {EventPin}, with Id: {PictureId}");
                return PhysicalFile(file, "image/PNG", pictureTakerName);
            }

            logger.Info($"Picture at Event: {EventPin}, with Id: {PictureId} requested but not found");
            return NotFound("Picture file wasn't found");
        }

        /// <summary>
        /// Gets a Preview picture from the server.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /api/Picture/{EventPin}/Preview/{PictureId}
        ///     
        ///{
        /// </remarks>
        /// <returns>A physical file, a picture.</returns>
        /// <response code='200'>Physical file, the requested preview picture.</response>
        /// <response code='404'> Preview picture file not found</response>
        [HttpGet]
        [Route("Preview/{EventPin}/{PictureId}")]
        public IActionResult GetPicturePreview(string EventPin, int PictureId)
        {
            CurrentDirectoryHelpers.SetCurrentDirectory();

            Picture pic = _picRepo.GetPictureById(PictureId).Result;

            int? guestId = pic.GuestId;
            int? hostId = pic.HostId;
            string pictureTakerName = "";
            Host h;
            Guest g;
            if (guestId != null)
            {
                g = _guestRepo.GetGuestById((int)guestId).Result;
                pictureTakerName = g.Name;
            }
            else if (hostId != null)
            {
                h = _hostRepo.GetHostById((int)hostId).Result;
                pictureTakerName = h.Name;
            }

            var file = Path.Combine(Directory.GetCurrentDirectory(), "Pictures", EventPin, "Preview",
                (PictureId + ".PNG"));

            if (_fileSystem.FileExists(file))
            {
                logger.Info($"Returning picture at Event: {EventPin}, with Id: {PictureId}");
                return PhysicalFile(file, "image/PNG", pictureTakerName);
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

            string userName = _currentUser.Name();
            Picture newPicture = new Picture();
            string[] strArray = new string[2];
               strArray = userName.Split(';');
            //Getting guest or host
            if (!userName.Contains('@'))
            {
                Guest guest = await _guestRepo.GetGuestByNameAndEventPin(strArray[0], model.EventPin);
                //Creating picture for database if a guest took the picture
                newPicture.EventPin = model.EventPin;
                newPicture.GuestId = guest.GuestId;
            }
            else if (userName.Contains('@'))
            {
                Host host = await _hostRepo.GetHostByEmail(userName);
                //Creating picture for database if host took the picture
                newPicture.EventPin = model.EventPin;
                newPicture.HostId = host.HostId;
            }

            
            
            //Inserting picture in database
            int picId = await _picRepo.InsertPicture(newPicture);

            logger.Info($"User with UserName: {userName} Inserts picture in db with for Event: {newPicture.EventPin} with PictureId: {newPicture.PictureId}");

            //Setting the current directory correctly 
            CurrentDirectoryHelpers.SetCurrentDirectory();


            //Creating subdirectories for events
            var subdir = Path.Combine(Directory.GetCurrentDirectory(), "Pictures", model.EventPin);
            if (!_fileSystem.DirectoryExists(subdir))
            {
                _fileSystem.DirectoryCreate(subdir);
                logger.Info($"Subdir created for Event: {model.EventPin}");
            }
            var subdirPreview = Path.Combine(Directory.GetCurrentDirectory(), "Pictures", model.EventPin,"Preview");
            if (!_fileSystem.DirectoryExists(subdirPreview))
            {
                _fileSystem.DirectoryCreate(subdirPreview);
                logger.Info($"Subdir created for Event: {model.EventPin}, Preview");
            }

            //Creating file and flushing to disk
            var file = Path.Combine(Directory.GetCurrentDirectory(), "Pictures", model.EventPin, picId+".PNG");

            var bytes = Convert.FromBase64String(model.PictureString);
            _fileSystem.FileCreate(file, bytes);
            
            //Creating Smaller image
            string inPath = file;
            string outPath = Path.Combine(Directory.GetCurrentDirectory(), "Pictures", model.EventPin, "Preview", picId + ".PNG");
            var settings = new ProcessImageSettings { Width = 200 };

            _fileSystem.SmallFileCreate(inPath,outPath, settings);

            return Ok(new ReturnPictureIdModel()
            {
                PictureId = picId
            });
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
        [HttpDelete("{EventPin}/{PictureId}")]
        public async Task<IActionResult> DeletePicture(string EventPin, int PictureId)
        {
            //Sætter stien til filen, ud fra det givne billede id og eventpin.
            CurrentDirectoryHelpers.SetCurrentDirectory();
            string filepath = Path.Combine(Directory.GetCurrentDirectory(), "Pictures", EventPin,
                (PictureId.ToString() + ".PNG"));


            string userName = _currentUser.Name();

            if (!userName.Contains('@'))
            {
                var event_ = await _eventRepo.GetEventByPin(EventPin);
                var guest = await _guestRepo.GetGuestByNameAndEventPin(userName, EventPin);
                foreach (var picture in event_.Pictures)
                {
                    if ((picture.PictureId == PictureId) && (picture.GuestId == guest.GuestId)) //Hvis billedet findes i Guestens samling af billeder
                    {
                        if (_fileSystem.FileExists(filepath))
                        {
                            _fileSystem.FileDelete(filepath);
                            await _picRepo.DeletePictureById(PictureId);
                            return NoContent();
                        }

                        return NotFound("File not found");
                    }
                }

                return Unauthorized("Not your picture");
            }
            if (userName.Contains('@'))
            {
                var host = await _hostRepo.GetHostByEmail(userName);
                var events = await _eventRepo.GetEventsByHostId(host.HostId);
                foreach (var event_ in events)
                {
                    if (event_.Pin == EventPin) //Hvis Hosten er host af dette event
                    {
                        if (_fileSystem.FileExists(filepath))
                        {
                            _fileSystem.FileDelete(filepath);
                            await _picRepo.DeletePictureById(PictureId);
                            return NoContent();
                        }

                        return NotFound("File not Found");
                    }
                }

                return Unauthorized("Not a picture in hosts events");
            }
            
            return NotFound("User not found");
        }
        
    }

}

