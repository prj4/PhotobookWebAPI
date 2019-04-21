using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PhotobookWebAPI.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LogController:ControllerBase
    {

        [AllowAnonymous]
        [HttpGet]
        public IActionResult GetLog()
        {

            string executableLocation = Path.GetDirectoryName(
                Assembly.GetExecutingAssembly().Location);
            string file = Path.Combine(executableLocation, "log.txt");

            if (System.IO.File.Exists("file"))
            {
                return PhysicalFile(file, "text/txt");
            }

            return NotFound();

        }

    }
}
