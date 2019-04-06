using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
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

        public Utility(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
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


        public async Task<bool> IsHost(AppUser user)
        {
            var claims = await _userManager.GetClaimsAsync(user);

            if (claims.Count > 0)
            {
                IList<AppUser> hostList = await _userManager.GetUsersForClaimAsync(claims.ElementAt(0));
                return hostList.Contains(user);

            }

            return false;
        }

        public async Task<bool> IsGuest(AppUser user)
        {
            var claims = await _userManager.GetClaimsAsync(user);

            if (claims.Count > 1)
            {
                IList<AppUser> guestList = await _userManager.GetUsersForClaimAsync(claims.ElementAt(1));
                 return guestList.Contains(user);

            }

            return false;
        }



        public string UserRole(IList<Claim> userClaims)
        {
            var claimToReturn = userClaims.FirstOrDefault(c => c.Type == "Role");
            return claimToReturn.Value;
        }
    }
}
