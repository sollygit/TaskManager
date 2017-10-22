using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace Todo.Authorization.Todos
{
    public class ManagerAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, Models.Todo>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
            OperationAuthorizationRequirement requirement, Models.Todo resource)
        {
            if (context.User == null || resource == null)
            {
                return Task.FromResult(0);
            }

            // If not asking for approval/reject, return.
            if (requirement.Name != Constants.Start &&
                requirement.Name != Constants.Finish)
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