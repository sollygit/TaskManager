using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Todo.Models
{
    public class Todo
    {
        public int Id { get; set; }
        public string OwnerID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        [DisplayName("Tag")]
        public int TagId { get; set; }

        public TodoStatus Status { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayName("Start Date")]
        public DateTime StartDate { get; set; }
    }

    public enum TodoStatus
    {
        New,
        InProgress,
        Done
    }
}
