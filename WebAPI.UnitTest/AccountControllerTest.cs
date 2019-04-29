using System;
using NSubstitute;
using NUnit.Framework;
using PhotobookWebAPI.Controllers;
using PhotoBook.Repository.EventRepository;
using PhotoBook.Repository.GuestRepository;
using PhotoBook.Repository.HostRepository;

namespace WebAPI.UnitTest
{
    [TestFixture]
    public class AccountControllerTest
    {
        private IEventRepository _eventRepo;
        private IHostRepository _hostRepo;
        private IGuestRepository _guestRepo;
        private AccountController _uut;

        [SetUp]
        public void SetUp()
        {

            _eventRepo = Substitute.For<IEventRepository>();
            _hostRepo = Substitute.For<IHostRepository>();
            _guestRepo = Substitute.For<IGuestRepository>();
            _uut = new AccountController();
        }

        [Test]
        public void GetUser_ValidUserNamer()
        {
            
        }

        [Test]
        public void GetUser_InvalidUserNamer Get()
        {

        }
    }
}
