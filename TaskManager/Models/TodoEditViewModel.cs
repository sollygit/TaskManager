using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TaskManager.Models
{
    public class TodoEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Tag is required")]
        [DisplayName("Tag")]
        public int TagId { get; set; }

        [Required(ErrorMessage = "Date is required")]
        public DateTime Date { get; set; }

        public string Description { get; set; }
        public IEnumerable<Tag> TagList { get; set; }
    }
}