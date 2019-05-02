using NSubstitute;
using NUnit.Framework;
using PhotoBook.Repository.EventRepository;
using PhotoBook.Repository.GuestRepository;
using PhotoBook.Repository.HostRepository;
using PhotobookWebAPI.Data;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Hosting;
using PhotobookWebAPI.Controllers;

namespace Tests
{
    public class AccountControllerTest
    {
        //private UserManager<AppUser> _userManager;
        

        private IEventRepository _eventRepo;
        private IHostRepository _hostRepo;
        private IGuestRepository _guestRepo;

        private AccountController _uut;

        

        [SetUp]
        public void Setup()
        {
            _eventRepo = Substitute.For<IEventRepository>();
            _hostRepo = Substitute.For<IHostRepository>();
            _guestRepo = Substitute.For<IGuestRepository>();


            // _uut = new AccountController(_userManager, _signInManager, _eventRepo, _hostRepo, _guestRepo);
        }

        [Test]
        public void Test1()
        {

            Assert.Pass();
        }
    }
}