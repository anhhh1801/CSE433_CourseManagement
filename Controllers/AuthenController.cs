using CourseManagement.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace CourseManagement.Controllers
{
    public class AuthenController : Controller
    {
        private readonly ILogger<AuthenController> _logger;
        private readonly CourseManagementDbContext _context;
        public AuthenController(ILogger<AuthenController> logger, CourseManagementDbContext context)
        {
            _logger = logger;
            _context = context;
        }
        public IActionResult Login()
        {
            return View();
        }
        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {

            var teacher = _context.Users
                .Include(t => t.Role)
                .FirstOrDefault(t => t.email == email);
            bool isValid = BCrypt.Net.BCrypt.Verify(password, teacher.password);
            bool isActive = teacher.isActive;

            if (teacher == null || !isValid || !isActive)
            {
                ViewBag.ErrorMessage = "Email hoặc mật khẩu không đúng.";
                return View();
            }
            var roleNames = teacher.Role.Select(r => r.Name).ToList();

            var claims = new List<Claim>
                            {
                                new Claim(ClaimTypes.Email, teacher.email),
                                new Claim("TeacherId", teacher.teacherId.ToString())
                            };
            foreach (var roleName in roleNames)
            {
                claims.Add(new Claim(ClaimTypes.Role, roleName));
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]

        public IActionResult SignUp(User user)
        {
            
            if (_context.Users.Any(t => t.email == user.email))
            {
                ViewBag.ErrorMessage = "Email đã tồn tại!";
                return View();
            }
            
            var defaultRole = _context.Roles.FirstOrDefault(r => r.Name == "User");

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(user.password);

            user.password = hashedPassword;
            user.Role.Add(defaultRole);
            user.avatar = "/img/avatar_default.jpg";
            user.isActive = true;

            _context.Users.Add(user);
            _context.SaveChanges();

            return RedirectToAction("Login");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }


        public IActionResult NeedLogin()
        {
            return View();
        }
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
