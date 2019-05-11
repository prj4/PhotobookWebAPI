using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Castle.Core.Internal;
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
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework.Internal;
using System.Data.Entity;
using System.Linq;
using NSubstitute.Core;
using NSubstitute.ReceivedExtensions;
using NSubstitute.ReturnsExtensions;
using PhotobookWebAPI;
using PhotoBookDatabase.Model;
using PB.Dto;

namespace Tests
{

    [TestFixture]
    public class EventControllerTest
    {     
        private IEventRepository _eventRepo;
        private IHostRepository _hostRepo;
        private ICurrentUser _fakeCurrentUser;
        private EventController _uut;

        private Host _testHost;
        private EventModel _testEventModel;
        private Event _testEvent;


        [SetUp]
        public void Setup()
        {
            //Arange
            _eventRepo = Substitute.For<IEventRepository>();
            _hostRepo = Substitute.For<IHostRepository>();
            _fakeCurrentUser = Substitute.For<ICurrentUser>();

            _uut = new EventController(_eventRepo, _hostRepo, _fakeCurrentUser);

            _testHost = new Host
            {
                Email = "test@test",
                Events = null,
                HostId = 1,
                Name = "test"
            };
            _testEventModel = new EventModel
            {
                Description = "test fest amok",
                EndDate = DateTime.Now,
                StartDate = DateTime.Now,
                Name = "test fest",
                Location = "Helvede"
            };
            _testEvent = new Event
            {
                Description = _testEventModel.Description,
                EndDate = _testEventModel.EndDate,
                StartDate = _testEventModel.StartDate,
                Location = _testEventModel.Location,
                Name = _testEventModel.Name,
                Pin = "1"
            };
        }

        #region CreatEvent Tests
        
        #region Dependency Call Testing

        [Test]
        public async Task CreateEvent_CurrentUser_NameCalled()
        {
            //Arrange
            _fakeCurrentUser.Name().Returns(_testHost.Name);
            _hostRepo.GetHostByEmail(Arg.Any<string>()).Returns(_testHost);
            _eventRepo.GetEventByPin(Arg.Any<string>()).Returns(new Event());
            
            //Act
            await _uut.CreateEvent(_testEventModel);

            //Assert
            _fakeCurrentUser.Received(1).Name();
        }

        [Test]
        public async Task CreateEvent_HostRepo_GetCurrentHostCalled()
        {
            //Arrange
            _fakeCurrentUser.Name().Returns(_testHost.Email);
            _hostRepo.GetHostByEmail(Arg.Any<string>()).Returns(_testHost);
            _eventRepo.GetEventByPin(Arg.Any<string>()).Returns(new Event());

            //Act
            await _uut.CreateEvent(_testEventModel);

            //Assert
            await _hostRepo.Received(1).GetHostByEmail(Arg.Is(_testHost.Email));
        }

        [Test]
        public async Task CreateEvent_HostRepo_InsertEventCalled()
        {
            //Arrange
            _fakeCurrentUser.Name().Returns(_testHost.Name);
            _hostRepo.GetHostByEmail(Arg.Any<string>()).Returns(_testHost);
            _eventRepo.GetEventByPin(Arg.Any<string>()).Returns(new Event());

            //Act
            await _uut.CreateEvent(_testEventModel);

            //Assert
            await _eventRepo.Received(1).InsertEvent(Arg.Any<Event>());
        }

        [Test]
        public async Task CreateEvent_HostRepo_GetEventByPinCalled()
        {
            //Arrange
            _fakeCurrentUser.Name().Returns(_testHost.Name);
            _hostRepo.GetHostByEmail(Arg.Any<string>()).Returns(_testHost);
            _eventRepo.GetEventByPin(Arg.Any<string>()).Returns(new Event());

            //Act
            await _uut.CreateEvent(_testEventModel);

            //Assert
            await _eventRepo.Received(1).GetEventByPin(Arg.Any<string>());
        }

        #endregion

        #region Return Value Testing

        [Test]
        public async Task CreateEvent_HostRepo_ReturnsOk()
        {
            //Arrange
            _fakeCurrentUser.Name().Returns(_testHost.Name);
            _hostRepo.GetHostByEmail(Arg.Any<string>()).Returns(_testHost);
            _eventRepo.GetEventByPin(Arg.Any<string>()).Returns(new Event());

            //Act
            var response = await _uut.CreateEvent(_testEventModel);
            var statCode = response as OkObjectResult;

            //Assert
            Assert.That(statCode, Is.Not.Null);
            Assert.That(statCode.StatusCode, Is.EqualTo(200));
        }

        [Test]
        public async Task CreateEvent_HostRepo_ReturnsBadRequest()
        {
            //Arrange
            _fakeCurrentUser.Name().Returns(_testHost.Name);
            _hostRepo.GetHostByEmail(Arg.Any<string>()).Returns(_testHost);
            _eventRepo.GetEventByPin(Arg.Any<string>()).ReturnsNull();

            //Act
            var response = await _uut.CreateEvent(_testEventModel);
            var statCode = response as BadRequestObjectResult;

            //Assert
            Assert.That(statCode, Is.Not.Null);
            Assert.That(statCode.StatusCode, Is.EqualTo(400));
        }

        #endregion

        #endregion

        #region GetEvent Test

        #region Get All


        #endregion

        #region Get Specific


        #endregion

        #region Get By Host



        #endregion

        #endregion

        #region PutEvent Tests

        #region Dependency Call Testing

        [Test]
        public async Task PutEvent__EventRepo_GetEventByPinCalled()
        {
            //Arrange
            _hostRepo.GetHostByEmail(Arg.Any<string>()).Returns(_testHost);
            _eventRepo.GetEventByPin(Arg.Any<string>()).Returns(_testEvent);

            //Act
            await _uut.PutEvent("1", new EditEventModel());

            //Assert
            await _eventRepo.Received(1).GetEventByPin(Arg.Is("1"));
        }

        [Test]
        public async Task PutEvent__EventRepo_UpdateEventCalled()
        {
            //Arrange
            _hostRepo.GetHostByEmail(Arg.Any<string>()).Returns(_testHost);
            _eventRepo.GetEventByPin(Arg.Any<string>()).Returns(_testEvent);

            //Act
            await _uut.PutEvent("1", new EditEventModel());

            //Assert
            await _eventRepo.Received(1).UpdateEvent(Arg.Is(_testEvent));
        }

        #endregion

        #region RouteTesting

        [TestCase("new","new",null)]
        [TestCase("new","new","new")]
        [TestCase(null,null,"new")]
        public async Task PutEvent__EventRepo_FunctioRouting(string nDes, string nLoc, string nNam)
        {
            //Arrange
            _hostRepo.GetHostByEmail(Arg.Any<string>()).Returns(_testHost);
            _eventRepo.GetEventByPin(Arg.Any<string>()).Returns(_testEvent);

            //Act
            await _uut.PutEvent("1", new EditEventModel
            {
                Description = nDes,
                Location = nLoc,
                Name = nNam
            });

            //Assert
            Event assertEvent = _testEvent;
            assertEvent.Description = nDes;
            assertEvent.Location = nLoc;
            assertEvent.Name = nNam;
            assertEvent.Pin = "100";
            _eventRepo.Received().UpdateEvent(Arg.Is(assertEvent));
        }


        #endregion

        #region Return Value Testing


        #endregion

        #endregion
    }
}