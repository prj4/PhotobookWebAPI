﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhotobookWebAPI.Data;
using PhotobookWebAPI.Models;
using PhotoBook.Repository.EventRepository;
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

        public PictureController(UserManager<AppUser> userManager, IEventRepository eventRepo, IPictureRepository picRepo )
        {
            _eventRepo = eventRepo;
            _picRepo = picRepo;
            _userManager = userManager;
           
        }

        [AllowAnonymous]
        [HttpGet]
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
        public IActionResult GetPicture(RetrievePictureModel model)
        {
            CurrentDirectoryHelpers.SetCurrentDirectory();

            var file = Path.Combine(Directory.GetCurrentDirectory(), "Pictures", model.EventPin, (model.PictureId.ToString() + ".PNG"));
            
            return PhysicalFile(file, "image/PNG");
        }


        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> InsertPicture(InsertPictureModel model)
        {

            string currentUser = User.Identity.Name;

            string userRole = null;
            var userClaims = await _userManager.GetClaimsAsync(user);
            foreach (var userClaim in userClaims)
            {
                if (userClaim.Type == "Role")
                    userRole = userClaim.Value;
            }


            var bytes = Convert.FromBase64String(model.PictureString);
            using (var imageFile = new FileStream(filePath, FileMode.Create))
            {
                imageFile.Write(bytes, 0, bytes.Length);
                imageFile.Flush();
            }

           

            return Ok();
        }

       
    }
}
