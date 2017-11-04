using System.Threading.Tasks;
using TaskManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace TaskManager.Authorization
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
            if (context.User.IsInRole(Constants.ManagersRole))
            {
                context.Succeed(requirement);
            }

            return Task.FromResult(0);
        }
    }
}