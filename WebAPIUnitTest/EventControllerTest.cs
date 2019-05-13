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
using System.IO;
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
        private EditEventModel _testEditEventModel;

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
                Pin = "1",
                HostId = 1
            };
            _testEditEventModel = new EditEventModel
            {
                Description = "newValue",
                Location = "newValue",
                Name = "newValue",
                StartDate = DateTime.Today,
                EndDate = DateTime.Today
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
        public async Task CreateEvent_HostRepo_GetHostByEmailCalled()
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
        public async Task CreateEvent_EventRepo_InsertEventCalled()
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
        public async Task CreateEvent_EventRepo_GetEventByPinCalled()
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
        public async Task CreateEvent_ReturnsOk()
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
        public async Task CreateEvent_ReturnsBadRequest()
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

        #region Dependedncy Call Testing
        
        [Test]
        public async Task GetEvents_EventRepo_GetEventsCalled()
        {
            //Arrange
            _eventRepo.GetEvents().ReturnsNull();

            
            //Act
            await _uut.GetEvents();

            //Assert
            await _eventRepo.Received(1).GetEvents();
            }

        #endregion

        #region Return Value Testing

        [Test]
        public async Task GetEvents_ReturnsEventModels()
        {
            //Arrange
            _eventRepo.GetEvents().Returns(new List<Event>{_testEvent}.AsEnumerable());

            //Act
            var response = await _uut.GetEvents();

            //Assert
            var result = response.FirstOrDefault();
            Assert.That(result.Name, Is.EqualTo(_testEvent.Name));
        }

        #endregion

        #endregion

        #region Get Specific

        #region Dependedncy Call Testing

        [Test]
        public async Task GetEvent_UsingPin_EventRepo_GetEventsByPinCalled()
        {
            //Arrange
            _eventRepo.GetEventByPin(Arg.Any<string>()).Returns(_testEvent);

            //Act
            var response = await _uut.GetEvent("1");

            //Assert
            await _eventRepo.Received(1).GetEventByPin("1");
        }

        #endregion

        #region Return Value Testing

        [Test]
        public async Task GetEvent_UsingPin_ReturnsOk()
        {
            //Arrange
            _eventRepo.GetEventByPin(Arg.Any<string>()).Returns(_testEvent);

            //Act
            var response = await _uut.GetEvent("1");
            var statCode = response as OkObjectResult;

            //Assert
            Assert.That(statCode, Is.Not.Null);
            Assert.That(statCode.StatusCode, Is.EqualTo(200));
        }

        [Test]
        public async Task GetEvent_UsingPin_ReturnsNotFound()
        {
            //Arrange
            _eventRepo.GetEventByPin(Arg.Any<string>()).ReturnsNull();

            //Act
            var response = await _uut.GetEvent("1");
            var statCode = response as NotFoundObjectResult;

            //Assert
            Assert.That(statCode, Is.Not.Null);
            Assert.That(statCode.StatusCode, Is.EqualTo(404));
        }

        #endregion

        #endregion

        #region Get By Host

        #region Dependedncy Call Testing

        [Test]
        public async Task GetEvent_UsingHost_CurrentUser_NameCalled()
        {
            //Arrange
            _fakeCurrentUser.Name().Returns(_testHost.Name);
            _hostRepo.GetHostByEmail(Arg.Any<string>()).Returns(_testHost);
            _eventRepo.GetEventsByHostId(_testHost.HostId)
                .Returns(new List<Event> { _testEvent }.AsEnumerable());

            //Act
            await _uut.GetEvent();

            //Assert
            _fakeCurrentUser.Received(1).Name();
        }

        [Test]
        public async Task GetEvent_UsingHost_HostRepo_GetHostByEmailCalled()
        {
            //Arrange
            _fakeCurrentUser.Name().Returns(_testHost.Name);
            _hostRepo.GetHostByEmail(Arg.Any<string>()).Returns(_testHost);
            _eventRepo.GetEventsByHostId(_testHost.HostId)
                .Returns(new List<Event> { _testEvent }.AsEnumerable());

            //Act
            await _uut.GetEvent();

            //Assert
            await _hostRepo.Received(1).GetHostByEmail(_testHost.Name);
        }

        [Test]
        public async Task GetEvent_UsingHost_HostRepo_GetEventByHostIdCalled()
        {
            //Arrange
            _fakeCurrentUser.Name().Returns(_testHost.Name);
            _hostRepo.GetHostByEmail(Arg.Any<string>()).Returns(_testHost);
            _eventRepo.GetEventsByHostId(_testHost.HostId)
                .Returns(new List<Event> { _testEvent }.AsEnumerable());

            //Act
            await _uut.GetEvent();

            //Assert
            await _eventRepo.Received(1).GetEventsByHostId(_testHost.HostId);
        }

        #endregion

        #region Return Value Testing

        [Test]
        public async Task GetEvent_UsingHost_ReturnsOk()
        {
            //Arrange
            _fakeCurrentUser.Name().Returns(_testHost.Name);
            _hostRepo.GetHostByEmail(Arg.Any<string>()).Returns(_testHost);
            _eventRepo.GetEventsByHostId(_testHost.HostId)
                .Returns(new List<Event> { _testEvent }.AsEnumerable());

            //Act
            var response = await _uut.GetEvent();
            var statCode = response as OkObjectResult;

            //Assert
            Assert.That(statCode, Is.Not.Null);
            Assert.That(statCode.StatusCode, Is.EqualTo(200));
        }

        [Test]
        public async Task GetEvent_UsingHost_ReturnsNotFound()
        {
            //Arrange
            _fakeCurrentUser.Name().Returns(_testHost.Name);
            _hostRepo.GetHostByEmail(Arg.Any<string>()).Returns(_testHost);
            _eventRepo.GetEventsByHostId(_testHost.HostId)
                .ReturnsNull();

            //Act
            var response = await _uut.GetEvent();
            var statCode = response as NotFoundObjectResult;

            //Assert
            Assert.That(statCode, Is.Not.Null);
            Assert.That(statCode.StatusCode, Is.EqualTo(404));
        }

        #endregion

        #endregion

        #endregion

        #region PutEvent Tests

        #region Dependency Call Testing

        [Test]
        public async Task PutEvent_EventRepo_GetEventByPinCalled()
        {
            //Arrange
            _eventRepo.GetEventByPin(Arg.Any<string>()).Returns(_testEvent);

            //Act
            await _uut.PutEvent("1", new EditEventModel());

            //Assert
            await _eventRepo.Received(1).GetEventByPin(Arg.Is("1"));
        }

        [Test]
        public async Task PutEvent_EventRepo_UpdateEventCalled()
        {
            //Arrange
            _eventRepo.GetEventByPin(Arg.Any<string>()).Returns(new Event());
            
            //Act
            await _uut.PutEvent("1", _testEditEventModel);

            //Assert
            await _eventRepo.Received(1).UpdateEvent(Arg.Is<Event>(e =>
                e.Description == _testEditEventModel.Description &&
                e.Location == _testEditEventModel.Location &&
                e.Name == _testEditEventModel.Name &&
                e.StartDate == _testEditEventModel.StartDate &&
                e.EndDate == _testEditEventModel.EndDate
            ));
        }

        #endregion

        #region Return Value Testing

        [Test]
        public async Task PutEvent_ReturnsNoContent()
        {
            //Arrange
            _eventRepo.GetEventByPin(Arg.Any<string>()).Returns(new Event());

            //Act
            var response = await _uut.PutEvent("1", _testEditEventModel);
            var statCode = response as NoContentResult;

            //Assert
            Assert.That(statCode, Is.Not.Null);
            Assert.That(statCode.StatusCode, Is.EqualTo(204));
        }

        [Test]
        public async Task PutEvent_ReturnsNotFound()
        {
            //Arrange
            _eventRepo.GetEventByPin(Arg.Any<string>()).ReturnsNull();

            //Act
            var response = await _uut.PutEvent("1", _testEditEventModel);
            var statCode = response as NotFoundObjectResult;

            //Assert
            Assert.That(statCode, Is.Not.Null);
            Assert.That(statCode.StatusCode, Is.EqualTo(404));
        }

        #endregion

        #endregion

        #region DeleteEvent Test

        #region Dependency Call Testing

        [Test]
        public async Task DeleteEvent_CurrentUser_NameCalled()
        {
            //Arrange
            _fakeCurrentUser.Name().Returns(_testHost.Name);
            _hostRepo.GetHostByEmail(_testHost.Name).Returns(_testHost);
            _eventRepo.GetEventByPin(Arg.Any<string>()).Returns(_testEvent);

            //Act
            await _uut.DeleteEvent("1");

            //Assert
            _fakeCurrentUser.Received(1).Name();
        }

        [Test]
        public async Task DeleteEvent_HostRepo_GetHostByEmailCalled()
        {
            //Arrange
            _fakeCurrentUser.Name().Returns(_testHost.Name);
            _hostRepo.GetHostByEmail(_testHost.Name).Returns(_testHost);
            _eventRepo.GetEventByPin(Arg.Any<string>()).Returns(_testEvent);

            //Act
            await _uut.DeleteEvent("1");

            //Assert
            await _hostRepo.Received(1).GetHostByEmail(_testHost.Name);
        }

        [Test]
        public async Task DeleteEvent_EventRepo_GetEventByPinCalled()
        {
            //Arrange
            _fakeCurrentUser.Name().Returns(_testHost.Name);
            _hostRepo.GetHostByEmail(_testHost.Name).Returns(_testHost);
            _eventRepo.GetEventByPin(Arg.Any<string>()).Returns(_testEvent);

            //Act
            await _uut.DeleteEvent("1");

            //Assert
            await _eventRepo.Received(1).GetEventByPin("1");
        }

        [Test]
        public async Task DeleteEvent_EventRepo_DeleteEventByPinCalled()
        {
            //Arrange
            _fakeCurrentUser.Name().Returns(_testHost.Name);
            _hostRepo.GetHostByEmail(_testHost.Name).Returns(_testHost);
            _eventRepo.GetEventByPin(Arg.Any<string>()).Returns(_testEvent);

            //Act
            await _uut.DeleteEvent("1");

            //Assert
            await _eventRepo.Received(1).DeleteEventByPin("1");
        }

        #endregion

        #region Return Value Testing

        [Test]
        public async Task DeleteEvent_ReturnsNoContent()
        {
            //Arrange
            _fakeCurrentUser.Name().Returns(_testHost.Name);
            _hostRepo.GetHostByEmail(_testHost.Name).Returns(_testHost);
            _eventRepo.GetEventByPin(Arg.Any<string>()).Returns(_testEvent);

            //Act
            var response = await _uut.DeleteEvent("1");
            var statCode = response as NoContentResult;

            //Assert
            Assert.That(statCode, Is.Not.Null);
            Assert.That(statCode.StatusCode, Is.EqualTo(204));
        }

        [Test]
        public async Task DeleteEvent_ReturnsBadRequest()
        {
            //Arrange
            _fakeCurrentUser.Name().Returns(_testHost.Name);
            _testHost.HostId = 2;
            _hostRepo.GetHostByEmail(_testHost.Name).Returns(_testHost);
            _eventRepo.GetEventByPin(Arg.Any<string>()).Returns(_testEvent);

            //Act
            var response = await _uut.DeleteEvent("1");
            var statCode = response as BadRequestObjectResult;

            //Assert
            Assert.That(statCode, Is.Not.Null);
            Assert.That(statCode.StatusCode, Is.EqualTo(400));
        }

        #endregion

        #endregion

    }
}