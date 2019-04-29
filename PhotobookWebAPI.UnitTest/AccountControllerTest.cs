using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using PhotoBook.Repository.EventRepository;
using PhotoBook.Repository.GuestRepository;
using PhotoBook.Repository.HostRepository;

namespace PhotobookWebAPI.UnitTest
{
    [TestFixture]
    public class AccountControllerTest
    {
        private IEventRepository _eventRepo;
        private IHostRepository _hostRepo;
        private IGuestRepository _guestRepo;

        [SetUp]
        public void Setup()
        {
            _eventRepo = Substitute.For
        }
    }
}
