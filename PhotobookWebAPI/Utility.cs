using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PhotobookWebAPI
{
    public class Utility
    {
        public string UserRole(IList<Claim> userClaims)
        {
            var claimToReturn = userClaims.FirstOrDefault(c => c.Type == "Role");
            return claimToReturn.Value;
        }
    }
}
