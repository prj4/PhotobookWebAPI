using System;
using NSubstitute;
using NUnit.Framework;
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

        [SetUp]
        public void SetUp()
        {
            _eventRepo = Substitute.For<IEventRepository>();
            _hostRepo = Substitute.For<IHostRepository>();
            _guestRepo = Substitute.For<IGuestRepository>();
        }


    }
}
