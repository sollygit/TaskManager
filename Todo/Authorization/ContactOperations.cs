using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace Todo.Authorization
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

    public class Constants
    {
        public static readonly string Create = "Create";
        public static readonly string Read = "Read";
        public static readonly string Update = "Update";
        public static readonly string Delete = "Delete";
        public static readonly string Start = "Start";
        public static readonly string Finish = "Finish";

        public static readonly string ContactAdministratorsRole = "ContactAdministrators";
        public static readonly string ContactManagersRole = "ContactManagers";
    }
}