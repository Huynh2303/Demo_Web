using Demo_web_MVC.Data.AppDatabase;
using Demo_web_MVC.Models;
using Demo_web_MVC.Models.ViewModel;
using Demo_web_MVC.Service.Sendemail;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Security.Claims;
using System.Security.Cryptography;

namespace Demo_web_MVC.Controllers
{

    public class UserController : Controller
    {
        private readonly IEmailServices _emailService;
        private readonly ILogger<UserController> _logger;
        private readonly AppDatabase _context;
        public UserController(ILogger<UserController> logger, AppDatabase context, IEmailServices emailService)
        {
            _logger = logger;
            _context = context;
            _emailService = emailService;

        }

        public IActionResult Index()
        {

            return View();
        }

        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]

        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (await _context.Users.AnyAsync(x => x.Username == model.Username))
            {
                ModelState.AddModelError("Username", "Tên đã tồn tại!");
                return View(model);
            }

            var hasher = new PasswordHasher<User>();
            var roleId = await _context.Roles
            .Where(r => r.Code == "USER")
            .Select(r => r.Id)
            .FirstAsync();
            var user = new User
            {
                Username = model.Username!,
                Email = model.Email!,
                FullName = model.FullName,
                RoleId = roleId,
                IsActive = false,
                CreatedAt = DateTime.UtcNow
            };

            user.PasswordHash = hasher.HashPassword(user, model.Password!);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var token = new UserToken
            {
                UserId = user.Id,
                Token = Guid.NewGuid().ToString(),
                Type = TokenType.EmailConfirm,
                ExpiredAt = DateTime.UtcNow.AddHours(24),
                CreatedAt = DateTime.UtcNow
            };

            _context.userTokens.Add(token);
            await _context.SaveChangesAsync();

            _ = Task.Run(async () =>
            {
                await _emailService.SendEmailAsync(
                user.Email,
                "Xác nhận tài khoản",
                $"<a href='{Url.Action("Confirm", "User", new { token = token.Token }, Request.Scheme)}'>Xác nhận</a>"
            );
            });

            return RedirectToAction("Login");
        }

        public async Task<IActionResult> Confirm(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Token không được để trống");
            }
            var tokenEntity = await _context.userTokens
             .Include(t => t.User)
             .FirstOrDefaultAsync(t =>
                 t.Token == token &&
                 t.Type == TokenType.EmailConfirm &&
                 !t.IsUsed &&
                 t.ExpiredAt > DateTime.UtcNow
             );
            if (tokenEntity == null)
                return BadRequest("Token không hợp lệ");

            tokenEntity.User.IsActive = true;
            tokenEntity.User.EmailConfirmedAt = DateTime.UtcNow;

            tokenEntity.IsUsed = true;
            tokenEntity.UsedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return RedirectToAction("Login", "User");
        }

        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // 1. Tìm user trong DB
            var user = await _context.Users.FirstOrDefaultAsync(x =>
                x.Username == model.UsernameOrEmail ||
                x.Email == model.UsernameOrEmail);

            if (user == null)
            {
                ModelState.AddModelError("", "Sai tên đăng nhập hoặc mật khẩu");
                return View(model);
            }

            if (!user.IsActive)
            {
                ModelState.AddModelError("", "Tài khoản chưa được kích hoạt, vui lòng kiểm tra email");
                return View(model);
            }

            // 2. Check lockout
            if (user.LockoutUntil != null && user.LockoutUntil > DateTime.UtcNow)
            {
                ModelState.AddModelError("",
                    $"Tài khoản bị khóa đến {user.LockoutUntil:HH:mm:ss}");
                return View("Lockout");
            }

            // 3. Verify password
            var hasher = new PasswordHasher<User>();
            var result = hasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                model.Password!
            );

            // 4. Sai mật khẩu
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

                if (user.LockoutUntil != null)
                    return View("Lockout");

                ModelState.AddModelError("", "Sai tên đăng nhập hoặc mật khẩu");
                return View(model);
            }

            // 5. Đăng nhập thành công → reset lockout
            user.FailedLoginCount = 0;
            user.LockoutUntil = null;
            await _context.SaveChangesAsync();

            // 6. Sign in
            var claims = new List<Claim>
    {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim("FullName", user.FullName ?? ""),
                //new Claim(ClaimTypes.Role, user.Role.Code)
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity)
            );

            return RedirectToAction("Index", "Home");
        }
        public IActionResult ForgotPassword()
        {
            return View("ForgotPassword");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
                return View();

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
                return View("ForgotPasswordConfirmation");
            if (!user.IsActive)
            {  
                
                return View("ForgotPasswordConfirmation");

            }
            if (user.UpdatedAt > DateTime.UtcNow.AddMinutes(-5))
                return View("ForgotPasswordConfirmation");
            

            // 1. Sinh password mới
            string newPassword = GeneratePassword(10);

            // 2. Hash & update
            var hasher = new PasswordHasher<User>();
            user.PasswordHash = hasher.HashPassword(user, newPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // 3. Gửi mail password mới
            await _emailService.SendEmailAsync(
                user.Email,
                "Mật khẩu mới của bạn",
                $@"
                <p>Xin chào {user.FullName},</p>
                <p>với tài khoản {user.Username} và Email là : {user.Email}</p>
                <p>Bạn đã yêu cầu đặt lại mật khẩu. </p>
                <p>Mật khẩu mới của bạn là:</p>
                <h3>{newPassword}</h3>
                <p>Vui lòng đăng nhập và đổi mật khẩu ngay.</p>
                "
            );
            return RedirectToAction("Login", "User");
        }
        public static string GeneratePassword(int length = 10)
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789@#$!";
            var bytes = new byte[length];

            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);

            return new string(bytes.Select(b => chars[b % chars.Length]).ToArray());
        }
       
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            return RedirectToAction("Login", "User");
        }
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model) 
        {
            if (!ModelState.IsValid)
            {
               
                return View(model);

            }
            var hasher = new PasswordHasher<User>();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null) 
                return Unauthorized();
            var user = await _context.Users.FindAsync(int.Parse(userId));
            if (user == null) 
                return NotFound();
            var verify = hasher.VerifyHashedPassword(
                    user,
                    user.PasswordHash,
                    model.OldPassword!
                );

            if (verify == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError("", "Mật khẩu hiện tại không đúng");
                return View(model);
            }
            if (model.OldPassword == model.NewPassword)
            {
                ModelState.AddModelError("", "Mật khẩu mới phải khác mật khẩu hiện tại");
                return View(model);
            }

            user.PasswordHash = hasher.HashPassword(user, model.NewPassword!);
                user.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Home");
        }

    }
}
