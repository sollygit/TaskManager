using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Todo.Models
{
    public class TodoTagViewModel
    {
        public List<Todo> todos;
        public SelectList tags;

        [Required(ErrorMessage = "Tag is required")]
        public string TodoTag { get; set; }
    }
}
