﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PhotobookWebAPI.Models
{
    public class InsertPictureModel
    {

        [Required]
        public string PictureString { get; set; }
      
    }

    public class RequestPicturesModel
    {
        [Required]
        public string EventId { get; set; }
    }

    public class RequestPicturesAnswerModel
    {
        [Required]
        public List<string> PictureList { get; set; }

    }
}
