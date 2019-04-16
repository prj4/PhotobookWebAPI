using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhotobookWebAPI.Models;
using PhotoBookDatabase.Model;

namespace PhotobookWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PictureController : Controller
    {
        [AllowAnonymous]
        [HttpGet]
        public RequestPicturesAnswerModel GetPictureIds(RequestPicturesModel eventpin)
        {
            //Finder listen af billeder i et event.
            return 
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult GetPicture()
        {
            CurrentDirectoryHelpers.SetCurrentDirectory();

            var file = Path.Combine(Directory.GetCurrentDirectory(), "Pictures", "Udklip.PNG");

            return PhysicalFile(file, "image/PNG");
        }


        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> InsertPicture(InsertPictureModel model)
        {


            
            return Ok();
        }
    }
}
