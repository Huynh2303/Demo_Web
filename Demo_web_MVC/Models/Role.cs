using System.ComponentModel.DataAnnotations;

namespace Demo_web_MVC.Models
{
    public class Role
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Code { get; set; } = null!; // ADMIN, USER, STAFF

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
