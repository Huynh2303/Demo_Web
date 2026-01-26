using System.ComponentModel.DataAnnotations;
using System.Data;

namespace Demo_web_MVC.Models
{
    public class User
    {
        //public int Id { get; set; }
        //[Required(ErrorMessage ="hãy nhập đủ thông tin!")]
        //[StringLength(20, ErrorMessage = "Tên không được vượt quá 50 ký tự!")]
        //public string? Username { get; set; }
        //[Required(ErrorMessage ="hãy nhập đủ thông tin!")]
        //[StringLength(255, MinimumLength = 8, ErrorMessage = "Mật khẩu phải từ 8 đến 255 ký tự!")]
        //public string? Password { get; set; }
        //[EmailAddress(ErrorMessage ="hãy nhập đúng định dạng email!")]
        //public string? Email { get; set; }
        //public string? FullName { get; set; }
        //public bool IsActive { get; set; }
        //public DateTime CreatedAt { get; set; }= DateTime.Now;
        //public DateTime? UpdatedAt { get; set; }= DateTime.Now;
        //public UserRole Role { get; set; }
        //public string ? ConfirmToken { get; set; } = Guid.NewGuid().ToString();
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
    }
}
