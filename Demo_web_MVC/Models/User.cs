using System.ComponentModel.DataAnnotations;
using System.Data;

namespace Demo_web_MVC.Models
{
    public class User
    {
      
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; } = null!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        public string PasswordHash { get; set; } = null!;

        public string? FullName { get; set; }

        public bool IsActive { get; set; } = false;

        public DateTime? EmailConfirmedAt { get; set; }

        // FK Role
        public int RoleId { get; set; }
        public Role Role { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public ICollection<UserToken> Tokens { get; set; } = new List<UserToken>();
        public int FailedLoginCount { get; set; } = 0;
        public DateTime? LockoutUntil { get; set; }

    }
}
