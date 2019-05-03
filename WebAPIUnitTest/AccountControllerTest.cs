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

namespace Tests
{
    [TestFixture]
    public class AccountControllerTest
    {
        //private UserManager<AppUser> _userManager;
        

        private IEventRepository _eventRepo;
        private IHostRepository _hostRepo;
        private IGuestRepository _guestRepo;

        private UserManager<AppUser> _userManager;
        private SignInManager<AppUser> _signInManager;

        private AccountController _uut;

        

        [SetUp]
        public void Setup()
        {
            //AppDBContext dataBase = new AppDBContext();
            _eventRepo = Substitute.For<IEventRepository>();
            //_eventRepo.Setup()

            _hostRepo = Substitute.For<IHostRepository>();
            _guestRepo = Substitute.For<IGuestRepository>();

            //_userManager = new UserManager<AppUser>();
            _signInManager = Substitute.For<SignInManager<AppUser>>();
            
            _uut = new AccountController(_userManager, _signInManager, _eventRepo, _hostRepo, _guestRepo);
        }

        [Test]
        public async Task accounts_get_returnsOK()
        {
            
            var actionResult = await _uut.GetAccounts();
            
        }
    }
}