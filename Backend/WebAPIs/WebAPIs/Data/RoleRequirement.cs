using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WebAPIs.Data
{
    public class RoleRequirement : IAuthorizationRequirement
    {
        public RoleRequirement(params RoleTypes[] role)
        {
            roleID = role;
        }

        public RoleTypes[] roleID { get; set; }
    }

}
