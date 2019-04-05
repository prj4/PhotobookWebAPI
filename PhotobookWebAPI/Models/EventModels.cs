﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using PhotoBookDatabase.Model;

namespace PhotobookWebAPI.Models
{
    public class GetEventModel
    {
        [Required]
        [Display(Name = "Event")]
        public Event _event { get; set; }
        
    }
}