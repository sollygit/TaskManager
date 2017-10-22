using System.Threading.Tasks;
using Todo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace Todo.Authorization.Contacts
{
    public class ManagerAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, Contact>
    {
        protected override Task
            HandleRequirementAsync(AuthorizationHandlerContext context,
                                   OperationAuthorizationRequirement requirement,
                                   Contact resource)
        {
            if (context.User == null || resource == null)
            {
                return Task.FromResult(0);
            }

            // If not asking for approval/reject, return.
            if (requirement.Name != Constants.Approve &&
                requirement.Name != Constants.Reject)
            {
                return Task.FromResult(0);
            }

            // Managers can approve or reject.
            if (context.User.IsInRole(Constants.ContactManagersRole))
            {
                context.Succeed(requirement);
            }

            return Task.FromResult(0);
        }
    }
}