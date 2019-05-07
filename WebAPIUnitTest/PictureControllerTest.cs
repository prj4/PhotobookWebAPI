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
using PhotoBook.Repository.PictureRepository;

namespace WebAPIUnitTest
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

            _testHost = new Host { Email = "test@test", Events = null, HostId = 1, Name = "test" };
            _testGuest = new Guest { Name = "test", Event = null, GuestId = 1, EventPin = null};
        }
    }
}
