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

            var teacher = await _context.Users
                .Include(t => t.Role)
                .FirstOrDefaultAsync(t => t.email == email);
            if (teacher == null)
            {
                TempData["Error"] = "Email or password is incorrect!";
                return View();
            }

            bool isValid = BCrypt.Net.BCrypt.Verify(password, teacher.password);
            bool isActive = teacher.isActive;

            if (!isValid || !isActive)
            {
                TempData["Error"] = "Email or password is incorrect!";
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
            TempData["Message"] = "Login Successfully!";

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]

        public async Task<IActionResult> SignUp(User user)
        {
            
            if (_context.Users.Any(t => t.email == user.email))
            {
                TempData["Error"] = "Email existed!";
                return View();
            }
            
            var defaultRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User");

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(user.password);

            user.password = hashedPassword;
            user.Role.Add(defaultRole);
            user.avatar = "/img/avatar_default.jpg";
            user.isActive = true;

            _context.Users.Add(user);
            _context.SaveChanges();
            TempData["Message"] = "Sign Up Successfully! Please Login.";

            return RedirectToAction("Login");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["Message"] = "Log out Successfully!";
            return RedirectToAction("UnLoginHomePage", "Home");
        }


        public IActionResult NeedLogin()
        {
            TempData["Error"] = "You need to login to access this page.";
            return View();
        }
        public IActionResult AccessDenied()
        {
            TempData["Error"] = "You do not have permission to access this page.";
            return View();
        }
    }
}
