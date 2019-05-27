using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

/// <summary>
/// Source https://adrientorris.github.io/aspnet-core/identity/extend-user-model.html?fbclid=IwAR0yNKk07kV26OX_ztxHBf32SPFw9Exp6A5NRyEsnMdixdabs9Va_eOR6gM
/// </summary>

namespace PhotobookWebAPI.Data
{
    
    public class AppClaimsPrincipalFactory : UserClaimsPrincipalFactory<AppUser, IdentityRole>
    {
        public AppClaimsPrincipalFactory(
            UserManager<AppUser> userManager
            , RoleManager<IdentityRole> roleManager
            , IOptions<IdentityOptions> optionsAccessor)
            : base(userManager, roleManager, optionsAccessor)
        { }

        public async override Task<ClaimsPrincipal> CreateAsync(AppUser user)
        {
            var principal = await base.CreateAsync(user);

            if (!string.IsNullOrWhiteSpace(user.Name))
            {
                ((ClaimsIdentity)principal.Identity).AddClaims(new[] {
                    new Claim(ClaimTypes.GivenName, user.Name)
                });
            }

          

            return principal;
        }
    }
    
}
