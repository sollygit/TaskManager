using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace TaskManager.Authorization
{
    public static class ContactOperations
    {
        public static OperationAuthorizationRequirement Create = new OperationAuthorizationRequirement { Name = Constants.Create };
        public static OperationAuthorizationRequirement Read = new OperationAuthorizationRequirement { Name = Constants.Read };
        public static OperationAuthorizationRequirement Update = new OperationAuthorizationRequirement { Name = Constants.Update };
        public static OperationAuthorizationRequirement Delete = new OperationAuthorizationRequirement { Name = Constants.Delete };
        public static OperationAuthorizationRequirement Approve = new OperationAuthorizationRequirement { Name = Constants.Approve };
        public static OperationAuthorizationRequirement Reject = new OperationAuthorizationRequirement { Name = Constants.Reject };
    }
}