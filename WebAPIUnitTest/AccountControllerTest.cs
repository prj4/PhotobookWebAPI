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

        private List<AppUser> _users = new List<AppUser>
        {
            new AppUser()
            {
                Email = "d@d",
                PasswordHash = "123456",
                Name = "Alfred"
            },
            new AppUser()
            {
                Email = "b@b",
                PasswordHash = "123456",
                Name = "Balfred"
            }
        };

        

        [SetUp]
        public void Setup()
        {


            _eventRepo = Substitute.For<IEventRepository>();
            _hostRepo = Substitute.For<IHostRepository>();
            _guestRepo = Substitute.For<IGuestRepository>();

            _userManager = MockUserManager<AppUser>(_users).Object;
            //_userManager = Mock.Of<UserManager<AppUser>>();
            //_signInManager = Mock.Of<SignInManager<AppUser>>();

           

            _uut = new AccountController(_userManager, _signInManager, _eventRepo, _hostRepo, _guestRepo);
        }

        [Test]
        public async Task accounts_get_returnsOK()
        {
            //_userManager.
            var accounts = await _uut.GetAccounts();
            Assert.That(accounts.IsNullOrEmpty());


        }


        public static Mock<UserManager<TUser>> MockUserManager<TUser>(List<TUser> ls) where TUser : class
        {
            var store = new Mock<IUserStore<TUser>>();
            var mgr = new Mock<UserManager<TUser>>(store.Object, null, null, null, null, null, null, null, null);
            mgr.Object.UserValidators.Add(new UserValidator<TUser>());
            mgr.Object.PasswordValidators.Add(new PasswordValidator<TUser>());

            mgr.Setup(x => x.DeleteAsync(It.IsAny<TUser>())).ReturnsAsync(IdentityResult.Success);
            mgr.Setup(x => x.CreateAsync(It.IsAny<TUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success).Callback<TUser, string>((x, y) => ls.Add(x));
            mgr.Setup(x => x.UpdateAsync(It.IsAny<TUser>())).ReturnsAsync(IdentityResult.Success);

            return mgr;
        }
    }
}