using System.ComponentModel.DataAnnotations;

namespace Todo.Models
{
    public class Tag
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tag is required")]
        public string Name { get; set; }
    }
}
