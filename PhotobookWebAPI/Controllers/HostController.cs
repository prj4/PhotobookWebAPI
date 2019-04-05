using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using PhotoBook.Repository.HostRepository;


namespace PhotobookWebAPI.Controllers
{
    public class HostController : Controller
    {
        private readonly string _connectionString =
            "Server=tcp:katrinesphotobook.database.windows.net,1433;Initial Catalog=PhotoBook4;" +
            "Persist Security Info=False;User ID=Ingeniørhøjskolen@katrinesphotobook.database.windows.net;" +
            "Password=Katrinebjergvej22;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        private IConfiguration _configuration;


        private HostRepository _hostRepository;

        public HostController(IConfiguration iconfig)
        {
            _configuration = iconfig;

            _connectionString = _configuration.GetConnectionString("DefaultConnection");

            _hostRepository = new HostRepository(_connectionString);
        }
        // GET: Host
        public async Task<IActionResult> Index()
        {

            return View(await _hostRepository.GetHosts());
        }
    }
}
