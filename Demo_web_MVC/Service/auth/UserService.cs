using Demo_web_MVC.Data.AppDatabase;
using Demo_web_MVC.Models;
using Demo_web_MVC.Models.ViewModel;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
namespace Demo_web_MVC.Service.auth
{
    public class UserService:IAuthService
    {
        private readonly AppDatabase _context;
        private readonly ILogger<UserService> _logger;
        public UserService(AppDatabase context,ILogger<UserService> logger)
        {
             _context = context;
            _logger = logger;
        }
        public async Task<bool> LoginService(LoginViewModel model)
        {

            
            // 1. Tìm user trong DB
            var user = await _context.Users
                 .Include(u => u.UserRoles)
                     .ThenInclude(ur => ur.Role)
                 .FirstOrDefaultAsync(x =>
                     x.Username == model.UsernameOrEmail! ||
                     x.Email == model.UsernameOrEmail!);
            if (user == null)
            {

                return false;
            }
            if (!user.IsActive)
            {
                return false;
            }
            if (user.LockoutUntil != null && user.LockoutUntil > DateTime.UtcNow)
            {
                return false;
            }
            var hasher = new PasswordHasher<User>();
            var result = hasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                model.Password!
            );
            if (result != PasswordVerificationResult.Success &&
                result != PasswordVerificationResult.SuccessRehashNeeded)
            {
                user.FailedLoginCount++;

                if (user.FailedLoginCount >= 3)
                {
                    user.LockoutUntil = DateTime.UtcNow.AddMinutes(5);
                    user.FailedLoginCount = 0;
                }
                await _context.SaveChangesAsync();
                return false;
            }
            user.FailedLoginCount = 0;
            user.LockoutUntil = null;
            await _context.SaveChangesAsync();
            var roleCode = user.UserRoles
                   .Select(ur => ur.Role.Code)
                   .FirstOrDefault();
            if (roleCode == null)
            {
                return false;
            }
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim("FullName", user.FullName ?? ""),
                //new Claim(ClaimTypes.Role, roleCode)
            };
            foreach (var role in user.UserRoles.Select(ur => ur.Role.Code))
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );
            return true;
        }
    }
}
