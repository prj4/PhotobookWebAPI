using NSubstitute;
using NUnit.Framework;
using PhotoBook.Repository.EventRepository;
using PhotoBook.Repository.GuestRepository;
using PhotoBook.Repository.HostRepository;
using PhotobookWebAPI.Data;
using Microsoft.AspNetCore.Hosting;
using PhotobookWebAPI.Controllers;
using Moq;
using Microsoft.AspNetCore.Identity;

namespace Tests
{
    [TestFixture]
    public class AccountControllerTest
    {
        //private UserManager<AppUser> _userManager;
        

        private IEventRepository _eventRepo;
        private IHostRepository _hostRepo;
        private IGuestRepository _guestRepo;

        private UserManager<AppUser> _userManager;
        private SignInManager<AppUser> _signInManager;

        private AccountController _uut;

        

        [SetUp]
        public void Setup()
        {
            _eventRepo = Substitute.For<IEventRepository>();
            _hostRepo = Substitute.For<IHostRepository>();
            _guestRepo = Substitute.For<IGuestRepository>();

            var _userManager = Mock.Of<UserManager<AppUser>>();
            var _signInManager = Mock.Of<SignInManager<AppUser>>();

            _uut = new AccountController(_userManager, _signInManager, _eventRepo, _hostRepo, _guestRepo);
        }

        [Test]
        public void Test1()
        {
            
        }
    }
}