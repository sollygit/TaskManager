namespace Todo.Models
{
    public class Todo
    {
        public int Id { get; set; }
        
        // user ID from AspNetUser table
        public string OwnerID { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }
        public TodoStatus Status { get; set; }
    }

    public enum TodoStatus
    {
        New,
        InProgress,
        Done
    }
}
