﻿using System.ComponentModel.DataAnnotations;

namespace Todo.Models
{
    public class TodoEditViewModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }
        public string Description { get; set; }
    }
}