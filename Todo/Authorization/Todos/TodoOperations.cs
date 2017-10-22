using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace Todo.Authorization.Todos
{
    public static class TodoOperations
    {
        public static OperationAuthorizationRequirement Create = new OperationAuthorizationRequirement { Name = Constants.Create };
        public static OperationAuthorizationRequirement Read = new OperationAuthorizationRequirement { Name = Constants.Read };
        public static OperationAuthorizationRequirement Update = new OperationAuthorizationRequirement { Name = Constants.Update };
        public static OperationAuthorizationRequirement Delete = new OperationAuthorizationRequirement { Name = Constants.Delete };
        public static OperationAuthorizationRequirement Start = new OperationAuthorizationRequirement { Name = Constants.Start };
        public static OperationAuthorizationRequirement Finish = new OperationAuthorizationRequirement { Name = Constants.Finish };
    }
}