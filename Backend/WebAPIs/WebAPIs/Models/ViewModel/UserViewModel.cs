using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPIs.Models
{
    public class UserViewModel
    {
        public int UserID { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string EmailID { get; set; }

        public string Username { get; set; }       

        public string ImageContent { get; set; }

        public int[] RoleID { get; set; }
    }
}
