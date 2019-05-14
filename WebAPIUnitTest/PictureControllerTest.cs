using System;
using System.Collections;
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
using PhotoBook.Repository.PictureRepository;

namespace Tests
{
    [TestFixture]
    public class PictureControllerTest
    {
        private IEventRepository _eventRepo;
        private IHostRepository _hostRepo;
        private IGuestRepository _guestRepo;
        private ICurrentUser _fakeCurrentUser;
        private IPictureRepository _picRepo;
        private PictureController _uut;

        private Host _testHost;
        private Guest _testGuest;
        private InsertPictureModel _testPictureModel;
        private Event _testEvent;
        private Picture _testPicture;


        [SetUp]
        public void Setup()
        {
            //Arange
            _eventRepo = Substitute.For<IEventRepository>();
            _hostRepo = Substitute.For<IHostRepository>();
            _guestRepo = Substitute.For<IGuestRepository>();
            _picRepo = Substitute.For<IPictureRepository>();
            _fakeCurrentUser = Substitute.For<ICurrentUser>();

            _uut = new PictureController(_eventRepo, _guestRepo, _hostRepo, _picRepo, _fakeCurrentUser);

            _testHost = new Host
            {
                Email = "test@test",
                HostId = 1,
                Name = "testHost"
            };
            _testGuest = new Guest
            {
                Name = "testGuest",
                Event = null,
                GuestId = 1,
                EventPin = "1"
            };
            _testPictureModel = new InsertPictureModel
            {
                EventPin = "1",
                PictureString = "test"
            };
            _testPicture = new Picture
            {
                EventPin = "1",
                PictureId = 1
            };
            _testEvent = new Event
            {
                Description = "test fest swing dance mesterskab",
                EndDate = DateTime.Now,
                StartDate = DateTime.Now,
                Location = "test stedet",
                Name = "test fest",
                Pin = "1",
                HostId = 1,
            };
        }

        #region InsertPicture

        #region Dependency Calls

        [Test]
        public async Task InsertPicture_CurrentUser_NameCalled()
        {
            //Arrange
            _fakeCurrentUser.Name().Returns(_testGuest.Name);
            _hostRepo.GetHostByEmail(Arg.Any<string>()).Returns(_testHost);
            _guestRepo.GetGuestByNameAndEventPin(Arg.Any<string>(), Arg.Any<string>()).Returns(_testGuest);
            _picRepo.InsertPicture(Arg.Any<Picture>()).Returns(1);


            //Act
            await _uut.InsertPicture(_testPictureModel);

            //Assert
            _fakeCurrentUser.Received(1).Name();
        }

        [Test]
        public async Task InsertPicture_HostRepo_GetHostByEmailCalled()
        {
            //Arrange
            _fakeCurrentUser.Name().Returns(_testHost.Email);
            _hostRepo.GetHostByEmail(_testHost.Email).Returns(_testHost);
            _guestRepo.GetGuestByNameAndEventPin(_testGuest.Name, _testPictureModel.EventPin).Returns(_testGuest);
            _picRepo.InsertPicture(Arg.Any<Picture>()).Returns(1);


            //Act
            await _uut.InsertPicture(_testPictureModel);

            //Assert
            await _hostRepo.Received(1).GetHostByEmail(_testHost.Email);
        }

        [Test]
        public async Task InsertPicture_GuestRepo_GetGuestByNameAndEventPinCalled()
        {
            //Arrange
            _fakeCurrentUser.Name().Returns(_testGuest.Name);
            _hostRepo.GetHostByEmail(_testHost.Email).Returns(_testHost);
            _guestRepo.GetGuestByNameAndEventPin(_testGuest.Name, _testPictureModel.EventPin).Returns(_testGuest);
            _picRepo.InsertPicture(Arg.Any<Picture>()).Returns(1);


            //Act
            await _uut.InsertPicture(_testPictureModel);

            //Assert
            await _guestRepo.Received(1).GetGuestByNameAndEventPin(_testGuest.Name, _testPictureModel.EventPin);
        }

        [Test]
        public async Task InsertPicture_PictureRepo_InsertPictureCalled()
        {
            //Arrange
            _fakeCurrentUser.Name().Returns(_testGuest.Name);
            _hostRepo.GetHostByEmail(_testHost.Email).Returns(_testHost);
            _guestRepo.GetGuestByNameAndEventPin(_testGuest.Name, _testPictureModel.EventPin).Returns(_testGuest);
            _picRepo.InsertPicture(Arg.Any<Picture>()).Returns(1);


            //Act
            await _uut.InsertPicture(_testPictureModel);

            //Assert
            await _picRepo.Received(1).InsertPicture(Arg.Is<Picture>(p => 
                p.EventPin == _testPictureModel.EventPin));
        }

        #endregion

        #region Return Value Testing

        [Test]
        public async Task InsertPicture_ReturnsOk()
        {
            //Arrange
            _fakeCurrentUser.Name().Returns(_testGuest.Name);
            _hostRepo.GetHostByEmail(_testHost.Email).Returns(_testHost);
            _guestRepo.GetGuestByNameAndEventPin(_testGuest.Name, _testPictureModel.EventPin).Returns(_testGuest);
            _picRepo.InsertPicture(Arg.Any<Picture>()).Returns(1);
            
            //Act
            var response = await _uut.InsertPicture(_testPictureModel);
            var statCode = response as OkObjectResult;

            //Assert
            Assert.That(statCode, Is.Not.Null);
            Assert.That(statCode.StatusCode, Is.EqualTo(200));
        }

        #endregion

        #endregion

        #region GetPictureIds

        #region Dependency Testing

        [Test]
        public async Task GetPictureIds_EventRepo_GetEventByPinCalled()
        {
            //Arrange
            //_testEvent.Pictures = new List<Picture> {_testPicture};
            _eventRepo.GetEventByPin(_testEvent.Pin).Returns(_testEvent);

            
            //Act
            await _uut.GetPictureIds(_testEvent.Pin);

            //Assert
            await _eventRepo.Received(1).GetEventByPin(_testEvent.Pin);
        }

        #endregion

        #region Return Value Testing

        [Test]
        public async Task GetPictureIds_ReturnsOk()
        {
            //Arrange
            _testEvent.Pictures = new List<Picture> {_testPicture};
            _eventRepo.GetEventByPin(_testEvent.Pin).Returns(_testEvent);


            //Act
            var response = await _uut.GetPictureIds(_testEvent.Pin);
            var statCode = response as OkObjectResult;
            var result = statCode.Value as PicturesAnswerModel;

            //Assert
            Assert.That(statCode, Is.Not.Null);
            Assert.That(statCode.StatusCode, Is.EqualTo(200));
            Assert.That(result, Is.Not.Null);
            Assert.That(result.PictureList.Contains(_testPicture.PictureId));
        }

        [Test]
        public async Task GetPictureIds_ReturnsNoContent
()
        {
            //Arrange
            _eventRepo.GetEventByPin(_testEvent.Pin).Returns(_testEvent);


            //Act
            var response = await _uut.GetPictureIds(_testEvent.Pin);
            var statCode = response as NoContentResult;

            //Assert
            Assert.That(statCode, Is.Not.Null);
            Assert.That(statCode.StatusCode, Is.EqualTo(204));
        }

        #endregion

        #endregion

        #region GetPicture

        #region Dependency Call Testing

        [Test]
        public async Task GetPicture_PictureRepo_GetPictureByIdCalled()
        {
            //Arrange
            _picRepo.GetPictureById(_testPicture.PictureId).Returns(_testPicture);
            
            //Act
            _uut.GetPicture(_testEvent.Pin, _testPicture.PictureId);

            //Assert
            await _picRepo.Received(1).GetPictureById(_testPicture.PictureId);
        }

        [Test]
        public async Task GetPicture_GuestRepo_GetGuestByIdCalled()
        {
            //Arrange
            _testPicture.GuestId = _testGuest.GuestId;
            _picRepo.GetPictureById(_testPicture.PictureId).Returns(_testPicture);
            _guestRepo.GetGuestById(_testGuest.GuestId).Returns(_testGuest);

            //Act
            _uut.GetPicture(_testEvent.Pin, _testPicture.PictureId);

            //Assert
            await _guestRepo.Received(1).GetGuestById(_testGuest.GuestId);
        }

        [Test]
        public async Task GetPicture_HostRepo_GetHostByIdCalled()
        {
            //Arrange
            _testPicture.HostId = _testHost.HostId;
            _picRepo.GetPictureById(_testPicture.PictureId).Returns(_testPicture);
            _hostRepo.GetHostById(_testHost.HostId).Returns(_testHost);

            //Act
            _uut.GetPicture(_testEvent.Pin, _testPicture.PictureId);

            //Assert
            await _hostRepo.Received(1).GetHostById(_testHost.HostId);
        }

        #endregion

        #region Return Value Testing

        [Test] //Fungerer ikke optimalt
        public async Task GetPicture_ReturnsPhysicalFile()
        {
            //Arrange
            _testPicture.HostId = _testHost.HostId;
            _picRepo.GetPictureById(_testPicture.PictureId).Returns(_testPicture);
            _hostRepo.GetHostById(_testHost.HostId).Returns(_testHost);

            //Act
            var response = _uut.GetPicture(_testEvent.Pin, _testPicture.PictureId);
            var statCode = response as PhysicalFileResult;
            
            //Assert
            Assert.That(statCode, Is.Not.Null);
            Assert.That(statCode.ContentType, Is.EqualTo("image/PNG"));
        }

        [Test]
        public async Task GetPicture_ReturnsNotFound()
        {
            //Arrange
            _testPicture.HostId = _testHost.HostId;
            _testEvent.Pin = "2";
            _picRepo.GetPictureById(_testPicture.PictureId).Returns(_testPicture);
            _hostRepo.GetHostById(_testHost.HostId).Returns(_testHost);

            //Act
            var response = _uut.GetPicture(_testEvent.Pin, _testPicture.PictureId);
            var statCode = response as NotFoundObjectResult;

            //Assert
            Assert.That(statCode, Is.Not.Null);
            Assert.That(statCode.StatusCode, Is.EqualTo(404));
        }

        #endregion

        #endregion

        #region GetPicturePreview

        #region Dependency Call Testing

        [Test]
        public async Task GetPicturePreview_PictureRepo_GetPictureByIdCalled()
        {
            //Arrange
            _picRepo.GetPictureById(_testPicture.PictureId).Returns(_testPicture);

            //Act
            _uut.GetPicture(_testEvent.Pin, _testPicture.PictureId);

            //Assert
            await _picRepo.Received(1).GetPictureById(_testPicture.PictureId);
        }

        [Test]
        public async Task GetPicturePreview_GuestRepo_GetGuestByIdCalled()
        {
            //Arrange
            _testPicture.GuestId = _testGuest.GuestId;
            _picRepo.GetPictureById(_testPicture.PictureId).Returns(_testPicture);
            _guestRepo.GetGuestById(_testGuest.GuestId).Returns(_testGuest);

            //Act
            _uut.GetPicture(_testEvent.Pin, _testPicture.PictureId);

            //Assert
            await _guestRepo.Received(1).GetGuestById(_testGuest.GuestId);
        }

        [Test]
        public async Task GetPicturePreview_HostRepo_GetHostByIdCalled()
        {
            //Arrange
            _testPicture.HostId = _testHost.HostId;
            _picRepo.GetPictureById(_testPicture.PictureId).Returns(_testPicture);
            _hostRepo.GetHostById(_testHost.HostId).Returns(_testHost);

            //Act
            _uut.GetPicture(_testEvent.Pin, _testPicture.PictureId);

            //Assert
            await _hostRepo.Received(1).GetHostById(_testHost.HostId);
        }

        #endregion

        #region Return Value Testing

        [Test] //Fungerer ikke optimalt
        public async Task GetPicturePreview_ReturnsPhysicalFile()
        {
            //Arrange
            _testPicture.HostId = _testHost.HostId;
            _picRepo.GetPictureById(_testPicture.PictureId).Returns(_testPicture);
            _hostRepo.GetHostById(_testHost.HostId).Returns(_testHost);

            //Act
            var response = _uut.GetPicture(_testEvent.Pin, _testPicture.PictureId);
            var statCode = response as PhysicalFileResult;
            
            //Assert
            Assert.That(statCode, Is.Not.Null);
            Assert.That(statCode.ContentType, Is.EqualTo("image/PNG"));
        }

        [Test]
        public async Task GetPicturePreview_ReturnsNotFound()
        {
            //Arrange
            _testPicture.HostId = _testHost.HostId;
            _testEvent.Pin = "2";
            _picRepo.GetPictureById(_testPicture.PictureId).Returns(_testPicture);
            _hostRepo.GetHostById(_testHost.HostId).Returns(_testHost);

            //Act
            var response = _uut.GetPicture(_testEvent.Pin, _testPicture.PictureId);
            var statCode = response as NotFoundObjectResult;

            //Assert
            Assert.That(statCode, Is.Not.Null);
            Assert.That(statCode.StatusCode, Is.EqualTo(404));
        }

        #endregion

        #endregion

        #region DeletePicture

        #region Dependency Call Testing

        [Test]
        public async Task DeletePicture_CurrentUser_NameCalled()
        {
            //Arrange
            _fakeCurrentUser.Name().Returns(_testHost.Email);
            _eventRepo.GetEventByPin(_testEvent.Pin).Returns(_testEvent);
            _hostRepo.GetHostByEmail(_testHost.Email).Returns(_testHost);

            //Act
            await _uut.DeletePicture(_testEvent.Pin, _testPicture.PictureId);

            //Assert
            _fakeCurrentUser.Received(1).Name();
        }

        [Test]
        public async Task DeletePicture_EventRepo_GetEventByPinCalled()
        {
            //Arrange
            _testPicture.GuestId = _testGuest.GuestId;
            _testEvent.Pictures = new List<Picture>{_testPicture};
            _fakeCurrentUser.Name().Returns(_testGuest.Name);
            _eventRepo.GetEventByPin(_testEvent.Pin).Returns(_testEvent);
            _guestRepo.GetGuestByNameAndEventPin(_testGuest.Name,_testGuest.EventPin).Returns(_testGuest);

            //Act
            await _uut.DeletePicture(_testEvent.Pin, _testPicture.PictureId);

            //Assert
            await _eventRepo.Received(1).GetEventByPin(_testEvent.Pin);
        }

        [Test]
        public async Task DeletePicture_EventRepo_GetEventByHostIdCalled()
        {
            //Arrange
            _fakeCurrentUser.Name().Returns(_testHost.Email);
            _eventRepo.GetEventsByHostId(_testHost.HostId).Returns(new List<Event> { _testEvent }.AsEnumerable());
            _hostRepo.GetHostByEmail(_testHost.Email).Returns(_testHost);

            //Act
            await _uut.DeletePicture(_testEvent.Pin, _testPicture.PictureId);

            //Assert
            await _eventRepo.Received(1).GetEventsByHostId(_testHost.HostId);
        }

        [Test]
        public async Task DeletePicture_GuestRepo_GetGuestByNameAndEventPinCalled()
        {
            //Arrange
            _testPicture.GuestId = _testGuest.GuestId;
            _testEvent.Pictures = new List<Picture> { _testPicture };
            _fakeCurrentUser.Name().Returns(_testGuest.Name);
            _eventRepo.GetEventByPin(_testEvent.Pin).Returns(_testEvent);
            _guestRepo.GetGuestByNameAndEventPin(_testGuest.Name, _testGuest.EventPin).Returns(_testGuest);

            //Act
            await _uut.DeletePicture(_testEvent.Pin, _testPicture.PictureId);

            //Assert
            await _guestRepo.Received(1).GetGuestByNameAndEventPin(_testGuest.Name, _testGuest.EventPin);
        }

        [Test]
        public async Task DeletePicture_HostRepo_GetHostByEmailCalled()
        {
            //Arrange
            _fakeCurrentUser.Name().Returns(_testHost.Email);
            _eventRepo.GetEventsByHostId(_testHost.HostId).Returns(new List<Event> { _testEvent }.AsEnumerable());
            _hostRepo.GetHostByEmail(_testHost.Email).Returns(_testHost);

            //Act
            await _uut.DeletePicture(_testEvent.Pin, _testPicture.PictureId);

            //Assert
            await _hostRepo.Received(1).GetHostByEmail(_testHost.Email);
        }

        [Test] // Ustabil da den afhænger af fysiske filer
        public async Task DeletePicture_AsGuest_PictureRepo_DeletePictureCalled()
        {
            //Arrange
            //Skaber billede på disk
            _fakeCurrentUser.Name().Returns(_testGuest.Name);
            _guestRepo.GetGuestByNameAndEventPin(_testGuest.Name, _testPictureModel.EventPin).Returns(_testGuest);
            _picRepo.InsertPicture(Arg.Any<Picture>()).Returns(1);
            await _uut.InsertPicture(_testPictureModel);

            _testPicture.GuestId = _testGuest.GuestId;
            _testEvent.Pictures = new List<Picture> { _testPicture };
            _fakeCurrentUser.Name().Returns(_testGuest.Name);
            _eventRepo.GetEventByPin(_testEvent.Pin).Returns(_testEvent);
            _guestRepo.GetGuestByNameAndEventPin(_testGuest.Name, _testGuest.EventPin).Returns(_testGuest);
            
            //Act
            await _uut.DeletePicture(_testEvent.Pin, _testPicture.PictureId);

            //Assert
            await _picRepo.Received(1).DeletePictureById(_testPicture.PictureId);
        }

        [Test] // Ustabil da den afhænger af fysiske filer
        public async Task DeletePicture_AsHost_PictureRepo_DeletePictureCalled()
        {
            //Arrange
            //Skaber billede på disk
            _fakeCurrentUser.Name().Returns(_testGuest.Name);
            _guestRepo.GetGuestByNameAndEventPin(_testGuest.Name, _testPictureModel.EventPin).Returns(_testGuest);
            _picRepo.InsertPicture(Arg.Any<Picture>()).Returns(1);
            await _uut.InsertPicture(_testPictureModel);

            _testEvent.Pictures = new List<Picture> { _testPicture };
            _fakeCurrentUser.Name().Returns(_testHost.Email);
            _eventRepo.GetEventsByHostId(_testHost.HostId).Returns(new List<Event> { _testEvent }.AsEnumerable());
            _hostRepo.GetHostByEmail(_testHost.Email).Returns(_testHost);

            //Act
            await _uut.DeletePicture(_testEvent.Pin, _testPicture.PictureId);

            //Assert
            await _picRepo.Received(1).DeletePictureById(_testPicture.PictureId);
        }

        #endregion

        #region Return Value Testing

        [Test]
        public async Task DeletePicture_AsHost_ReturnsUnauthorized()
        {
            //Arrange
            _fakeCurrentUser.Name().Returns(_testHost.Email);
            _eventRepo.GetEventsByHostId(_testHost.HostId).Returns(new List<Event> { new Event() }.AsEnumerable());
            _hostRepo.GetHostByEmail(_testHost.Email).Returns(_testHost);

            //Act
            var response = await _uut.DeletePicture(_testEvent.Pin, _testPicture.PictureId);
            var statCode = response as UnauthorizedObjectResult;

            //Assert
            Assert.That(statCode, Is.Not.Null);
            Assert.That(statCode.StatusCode, Is.EqualTo(401));
        }

        [Test]
        public async Task DeletePicture_AsHost_ReturnsNotFound()
        {
            //Arrange
            _fakeCurrentUser.Name().Returns(_testHost.Email);
            _eventRepo.GetEventsByHostId(_testHost.HostId).Returns(new List<Event> { _testEvent }.AsEnumerable());
            _hostRepo.GetHostByEmail(_testHost.Email).Returns(_testHost);

            //Act
            var response = await _uut.DeletePicture(_testEvent.Pin, _testPicture.PictureId);
            var statCode = response as NotFoundObjectResult;

            //Assert
            Assert.That(statCode, Is.Not.Null);
            Assert.That(statCode.StatusCode, Is.EqualTo(404));
        }

        [Test] // Ustabil da den afhænger af fysiske filer
        public async Task DeletePicture_AsHost_ReturnsNoContent()
        {
            //Arrange
            //Skaber billede på disk
            _fakeCurrentUser.Name().Returns(_testGuest.Name);
            _guestRepo.GetGuestByNameAndEventPin(_testGuest.Name, _testPictureModel.EventPin).Returns(_testGuest);
            _picRepo.InsertPicture(Arg.Any<Picture>()).Returns(1);
            await _uut.InsertPicture(_testPictureModel);

            _testEvent.Pictures = new List<Picture> { _testPicture };
            _fakeCurrentUser.Name().Returns(_testHost.Email);
            _eventRepo.GetEventsByHostId(_testHost.HostId).Returns(new List<Event> { _testEvent }.AsEnumerable());
            _hostRepo.GetHostByEmail(_testHost.Email).Returns(_testHost);

            //Act
            var response = await _uut.DeletePicture(_testEvent.Pin, _testPicture.PictureId);
            var statCode = response as NoContentResult;

            //Assert
            Assert.That(statCode, Is.Not.Null);
            Assert.That(statCode.StatusCode, Is.EqualTo(204));
        }

        [Test] // Ustabil da den afhænger af fysiske filer
        public async Task DeletePicture_AsGuest_ReturnsUnauthorized()
        {
            //Arrange
            _testPicture.PictureId = 5;
            _testPicture.GuestId = _testGuest.GuestId;
            _testEvent.Pictures = new List<Picture> { new Picture() };
            _fakeCurrentUser.Name().Returns(_testGuest.Name);
            _eventRepo.GetEventByPin(_testEvent.Pin).Returns(_testEvent);
            _guestRepo.GetGuestByNameAndEventPin(_testGuest.Name, _testGuest.EventPin).Returns(_testGuest);

            //Act
            var response = await _uut.DeletePicture(_testEvent.Pin, _testPicture.PictureId);
            var statCode = response as UnauthorizedObjectResult;

            //Assert
            Assert.That(statCode, Is.Not.Null);
            Assert.That(statCode.StatusCode, Is.EqualTo(401));
        }

        [Test] // Ustabil da den afhænger af fysiske filer
        public async Task DeletePicture_AsGuest_ReturnsNotFound()
        {
            //Arrange
            _testPicture.GuestId = _testGuest.GuestId;
            _testEvent.Pictures = new List<Picture> { _testPicture };
            _fakeCurrentUser.Name().Returns(_testGuest.Name);
            _eventRepo.GetEventByPin(_testEvent.Pin).Returns(_testEvent);
            _guestRepo.GetGuestByNameAndEventPin(_testGuest.Name, _testGuest.EventPin).Returns(_testGuest);

            //Act
            var response = await _uut.DeletePicture(_testEvent.Pin, _testPicture.PictureId);
            var statCode = response as NotFoundObjectResult;

            //Assert
            Assert.That(statCode, Is.Not.Null);
            Assert.That(statCode.StatusCode, Is.EqualTo(404));
        }

        [Test] // Ustabil da den afhænger af fysiske filer
        public async Task DeletePicture_AsGuest_ReturnsNoContent()
        {
            //Arrange
            //Skaber billede på disk
            _fakeCurrentUser.Name().Returns(_testGuest.Name);
            _guestRepo.GetGuestByNameAndEventPin(_testGuest.Name, _testPictureModel.EventPin).Returns(_testGuest);
            _picRepo.InsertPicture(Arg.Any<Picture>()).Returns(1);
            await _uut.InsertPicture(_testPictureModel);

            _testPicture.GuestId = _testGuest.GuestId;
            _testEvent.Pictures = new List<Picture> { _testPicture };
            _fakeCurrentUser.Name().Returns(_testGuest.Name);
            _eventRepo.GetEventByPin(_testEvent.Pin).Returns(_testEvent);
            _guestRepo.GetGuestByNameAndEventPin(_testGuest.Name, _testGuest.EventPin).Returns(_testGuest);

            //Act
            var response = await _uut.DeletePicture(_testEvent.Pin, _testPicture.PictureId);
            var statCode = response as NoContentResult;

            //Assert
            Assert.That(statCode, Is.Not.Null);
            Assert.That(statCode.StatusCode, Is.EqualTo(204));
        }
        #endregion

        #endregion
    }
}
