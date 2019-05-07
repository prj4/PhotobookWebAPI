using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace PhotobookWebAPI
{
    public class CurrentUser : ICurrentUser
    {

        private HttpContext _context;

        public CurrentUser(HttpContext context)
        {
            _context = context;
        }

        public virtual string Name()
        {
            return _context.User.Identity.Name;
        }
    }

    public interface ICurrentUser
    {
        string Name();

    }
}
