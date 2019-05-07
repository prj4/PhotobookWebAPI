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
        private IGuestRepository _guestRepo;
        private ICurrentUser _fakeCurrentUser;
        private EventController _uut;

        private Host _testHost;
        private EventModel _testEventModel;
        


        [SetUp]
        public void Setup()
        {
            //Arange
            _eventRepo = Substitute.For<IEventRepository>();
            _hostRepo = Substitute.For<IHostRepository>();
            _guestRepo = Substitute.For<IGuestRepository>();
            _fakeCurrentUser = Substitute.For<ICurrentUser>();

            _uut = new EventController(_eventRepo, _hostRepo, _fakeCurrentUser);

            _testHost = new Host {Email = "test@test", Events = null, HostId = 1, Name = "test"};
            _testEventModel = new EventModel
            {
                Description = "test fest amok",
                EndDate = DateTime.Now,
                StartDate = DateTime.Now,
                Name = "test fest",
                Location = "Helvede"
            };
        }

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
            _fakeCurrentUser.Name().Returns(_testHost.Name);
            _hostRepo.GetHostByEmail(Arg.Any<string>()).Returns(_testHost);
            _eventRepo.GetEventByPin(Arg.Any<string>()).Returns(new Event());

            //Act
            await _uut.CreateEvent(_testEventModel);

            //Assert
            _hostRepo.Received(1).GetHostByEmail(Arg.Any<string>());
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
            _eventRepo.Received(1).InsertEvent(Arg.Any<Event>());
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
            _eventRepo.Received(1).GetEventByPin(Arg.Any<string>());
        }

        [Test]
        public async Task CreateEvent_HostRepo_ReturnsOk()
        {
            //Arrange
            _fakeCurrentUser.Name().Returns(_testHost.Name);
            _hostRepo.GetHostByEmail(Arg.Any<string>()).Returns(_testHost);
            _eventRepo.GetEventByPin(Arg.Any<string>()).Returns(new Event());

            //Act
            var result = await _uut.CreateEvent(_testEventModel);

            //Assert
            //Assert.That(_testEventModel.Name().IsEqalsTo(result.));
        }
    }
}