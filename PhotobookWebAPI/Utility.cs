using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using PhotobookWebAPI.Data;
using PhotoBook.Repository.HostRepository;
using PhotoBookDatabase.Model;

namespace PhotobookWebAPI
{
    public class Utility
    {
        public UserManager<AppUser> _userManager;
        public IHostRepository _hostRepo;
        public Utility(UserManager<AppUser> userManager, IHostRepository hostRepo)
        {
            _userManager = userManager;
            _hostRepo = hostRepo;
        }

        public async Task<AppUser> GetCurrentAppUser(string currentUserName)
        {

            var currentUser = await _userManager.FindByNameAsync(currentUserName);

            return currentUser;
        }

        public async Task<Host> GetCurrentHost(string currentUserName)
        {
            
            var currentHost = _hostRepo.GetHost(GetCurrentAppUser(currentUserName).Result.Name).Result;

            return currentHost;
        }



    }
}
