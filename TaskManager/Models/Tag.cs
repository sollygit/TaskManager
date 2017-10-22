using System.ComponentModel;

namespace TaskManager.Models
{
    public class Tag
    {
        public int Id { get; set; }

        [DisplayName("Tag")]
        public string Name { get; set; }
    }
}
