using NSubstitute;
using NUnit.Framework;
using PhotobookWebAPI;
using PhotobookWebAPI.Controllers;
using PhotoBook.Repository.EventRepository;
using PhotoBook.Repository.HostRepository;

namespace IntegrationsTest
{
    public class Step
    {

        private IEventRepository _eventRepo;
        private IHostRepository _hostRepo;
        private ICurrentUser _fakeCurrentUser;
        private EventController _uut;


        [SetUp]
        public void Setup()
        {

            _eventRepo = Substitute.For<IEventRepository>();
            _fakeCurrentUser = Substitute.For<ICurrentUser>();

            _hostRepo = new HostRepository();

        }

        [Test]
        public void Test1()
        {
            Assert.Pass();
        }
    }
}