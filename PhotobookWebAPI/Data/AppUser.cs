﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Identity;

namespace PhotobookWebAPI.Data
{
    public class AppUser : IdentityUser
    {
        public string Name { get; set; }

    }
}
