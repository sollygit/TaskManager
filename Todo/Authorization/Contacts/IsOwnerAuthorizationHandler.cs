using System.Threading.Tasks;
using Todo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Identity;

namespace Todo.Authorization.Contacts
{
    public class IsOwnerAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, Contact>
    {
        UserManager<ApplicationUser> _userManager;

        public IsOwnerAuthorizationHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        protected override Task
            HandleRequirementAsync(AuthorizationHandlerContext context,
                                   OperationAuthorizationRequirement requirement,
                                   Contact resource)
        {
            if (context.User == null || resource == null)
            {
                return Task.FromResult(0);
            }

            // If we're not asking for CRUD permission, return.

            if (requirement.Name != Constants.Create &&
                requirement.Name != Constants.Read &&
                requirement.Name != Constants.Update &&
                requirement.Name != Constants.Delete)
            {
                return Task.FromResult(0);
            }

            if (resource.OwnerID == _userManager.GetUserId(context.User))
            {
                context.Succeed(requirement);
            }

            return Task.FromResult(0);
        }
    }
}