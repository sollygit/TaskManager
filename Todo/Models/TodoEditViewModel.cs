using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Todo.Models
{
    public class TodoEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required(ErrorMessage = "Tag is required")]
        public string Tag { get; set; }

        public SelectList TagList { get; set; }
    }
}