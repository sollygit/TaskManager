using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Todo.Models
{
    public class TodoTagViewModel
    {
        public IEnumerable<Todo> TodoList { get; set; }
        public IEnumerable<Tag> TagList { get; set; }

        [Required(ErrorMessage = "Tag is required")]
        public int TagId { get; set; }
    }
}
