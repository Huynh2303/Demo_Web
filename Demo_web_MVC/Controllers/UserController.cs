using Demo_web_MVC.Data.AppDatabase;
using Demo_web_MVC.Models;
using Demo_web_MVC.Service;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Demo_web_MVC.Models.ViewModel;

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
        public async Task<IActionResult> Login([Bind("Username,Email,Password")] User user)
        {
            if (!ModelState.IsValid)
                return View(user);
            // 1. Tìm user
            var conect = await _context.Users
                .FirstOrDefaultAsync(x => x.Username == user.Username || x.Email == user.Email);
            if (conect == null)
            {
                ModelState.AddModelError("", "Sai tên đăng nhập hoặc mật khẩu");
                return View(user);
            }
            if (!conect.IsActive)
            {
                ModelState.AddModelError("", "Tài khoản chưa được kích hoạt, vui lòng kiểm tra email để xác nhận!");
                return View();
            }

            // 2. Verify password
            var hasher = new PasswordHasher<User>();

            var result = hasher.VerifyHashedPassword(
                conect,
                conect.PasswordHash, // BẮT BUỘC
                user.PasswordHash
            );

            if (result != PasswordVerificationResult.Success &&
                result != PasswordVerificationResult.SuccessRehashNeeded)
            {
                ModelState.AddModelError("", "Sai tên đăng nhập hoặc mật khẩu");
                return View(user);
            }

            // 3. Tạo claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, conect.Id.ToString()),
                new Claim(ClaimTypes.Name, conect.Username),
                new Claim("FullName", conect.FullName ?? ""),
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            var principal = new ClaimsPrincipal(identity);

            // 4. Sign in
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal
            );

            return RedirectToAction("Index", "Home");
        }


        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            return RedirectToAction("Login", "User");
        }

    }
}
