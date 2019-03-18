using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WebAPIs.Data
{
    public class RoleHandler : AuthorizationHandler<RoleRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RoleRequirement requirement)
        {

            if (!context.User.HasClaim(c => c.Type == "Roles"))
            {
                return Task.CompletedTask;
            }
            string[] tokens = context.User.FindFirst(c => c.Type == "Roles").Value.Split(',');
            int[] userRoles = Array.ConvertAll(tokens, int.Parse);  
            int[] requiredRoles = Array.ConvertAll(requirement.roleID, value => (int)value);
            if (requiredRoles.Any(x => userRoles.Contains(x)))            
            {
                context.Succeed(requirement);
            }
            if (userRoles.Contains((int)Enums.SuperAdmin))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
