using CourseManagement.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

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

            var teacher = _context.Teachers.FirstOrDefault(t => t.email == email);
            bool isValid = BCrypt.Net.BCrypt.Verify(password, teacher.password);

            if (teacher == null || !isValid)
            {
                ViewBag.ErrorMessage = "Email hoặc mật khẩu không đúng.";
                return View();
            }

            var claims = new List<Claim>
                            {
                                new Claim(ClaimTypes.Email, teacher.email),
                                new Claim("TeacherId", teacher.teacherId.ToString())
                            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]

        public IActionResult SignUp(string email, string password)
        {
            
            if (_context.Teachers.Any(t => t.email == email))
            {
                ViewBag.ErrorMessage = "Email đã tồn tại!";
                return View();
            }


            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            Teacher teacher = new Teacher
            {
                email = email,
                password = hashedPassword
            };

            _context.Teachers.Add(teacher);
            _context.SaveChanges();

            return RedirectToAction("Login");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}
