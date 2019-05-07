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

namespace Tests
{

    [TestFixture]
    public class AccountControllerTest
    {
        //private UserManager<AppUser> _userManager;
        

        private IEventRepository _eventRepo;
        private IHostRepository _hostRepo;
        private IGuestRepository _guestRepo;
        private ICurrentUser _fakeCurrentUser;

        private UserManager<AppUser> _userManager;
        private SignInManager<AppUser> _signInManager;

        private AccountController _uut;

        private List<AppUser> _users = new List<AppUser>
        {
            new AppUser() { },
            new AppUser() { }
        };



        [SetUp]
        public void Setup()
        {


            _eventRepo = Substitute.For<IEventRepository>();
            _hostRepo = Substitute.For<IHostRepository>();
            _guestRepo = Substitute.For<IGuestRepository>();
            _fakeCurrentUser = Substitute.For<ICurrentUser>();

            _userManager = MockUserManager<AppUser>(_users).Object;
            //_userManager = Mock.Of<UserManager<AppUser>>();
            //_signInManager = Mock.Of<SignInManager<AppUser>>();

           
            
            _uut = new AccountController(_userManager, _signInManager, _eventRepo, _hostRepo, _guestRepo,_fakeCurrentUser);
        }

        [Test]
        public async Task accounts_get_returnsOK()
        {
            var accounts = await _uut.GetAccounts();
            Assert.That(accounts.IsNullOrEmpty());
        }


        public static Mock<UserManager<AppUser>> MockUserManager<AppUser>(List<AppUser> ls) where AppUser : class
        {
            var store = new Mock<IUserStore<AppUser>>();
            var mgr = new Mock<UserManager<AppUser>>(store.Object, null, null, null, null, null, null, null, null);
            mgr.Object.UserValidators.Add(new UserValidator<AppUser>());
            mgr.Object.PasswordValidators.Add(new PasswordValidator<AppUser>());

            mgr.Setup(x => x.DeleteAsync(It.IsAny<AppUser>())).ReturnsAsync(IdentityResult.Success);
            mgr.Setup(x => x.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success).Callback<AppUser, string>((x, y) => ls.Add(x));
            mgr.Setup(x => x.UpdateAsync(It.IsAny<AppUser>())).ReturnsAsync(IdentityResult.Success);


            return mgr;
        }

    }
}