using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;

namespace Demo_web_MVC.Models
{
    public class UserToken
    {
        public int Id { get; set; }

        // FK User
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        [Required]
        public string Token { get; set; } = null!;

        public TokenType Type { get; set; }

        public DateTime ExpiredAt { get; set; }

        public bool IsUsed { get; set; } = false;

        public DateTime? UsedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
    public enum TokenType
    {
        EmailConfirm = 1,
        ResetPassword = 2
    }
}
