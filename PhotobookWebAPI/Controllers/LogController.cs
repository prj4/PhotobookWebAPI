using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PhotobookWebAPI.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
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

            if (System.IO.File.Exists(file))
            {
                return PhysicalFile(file, "text/txt");
            }

            return NotFound();

        }

    }
}
