using System;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using NUnit.Framework;
using PhotobookWebAPI.Controllers;
using PhotobookWebAPI.Data;
using PhotoBook.Repository.EventRepository;
using PhotoBook.Repository.GuestRepository;
using PhotoBook.Repository.HostRepository;

namespace WebAPI.UnitTest
{
    public class AccountControllerTest
    {
        private IEventRepository _eventRepo;
        private IHostRepository _hostRepo;
        private IGuestRepository _guestRepo;

        [SetUp]
        public void SetUp()
        {
            _eventRepo = Substitute.For<IEventRepository>();
            _hostRepo = Substitute.For<IHostRepository>();
            _guestRepo = Substitute.For<IGuestRepository>();
        }

        [Test]
        public void GetUser_ValidUserNamer()
        {
            
        }
        /*
        [Test]
        public void GetUser_InvalidUserNamer G
        
        }
        */
    }
}
