using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;

namespace WebAPIs.Data
{
    public class Helper
    {
        private readonly ClaimsPrincipal principal;
        public Helper(IPrincipal _principal)
        {
            principal = _principal as ClaimsPrincipal;
        }

        public dynamic GetSpecificClaim(string type)
        {
            var claimsIdentity = (ClaimsIdentity)principal.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            var userName = claimsIdentity.FindFirst(ClaimTypes.Name).Value;
            var userEmail = claimsIdentity.FindFirst(ClaimTypes.Email).Value;
            var roleID = claimsIdentity.FindFirst("Roles").Value;
            if (type == "ID")
            {
                return Convert.ToInt32(userId);
            }
            else if (type == "Email")
            {
                return Convert.ToString(userEmail);
            }
            else if (type == "RoleID")
            {
                return roleID;
            }
            else
            {
                return Convert.ToString(userName);
            }
        }
    }
}
