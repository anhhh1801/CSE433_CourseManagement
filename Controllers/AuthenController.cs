using CourseManagement.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using CourseManagement.Services;
using Newtonsoft.Json;
using System.Net.Mail;

namespace CourseManagement.Controllers
{
    public class AuthenController : Controller
    {
        private readonly ILogger<AuthenController> _logger;
        private readonly CourseManagementDbContext _context;
        private readonly EmailService _emailService;
        public AuthenController(ILogger<AuthenController> logger, CourseManagementDbContext context, EmailService emailService)
        {
            _logger = logger;
            _context = context;
            _emailService = emailService;
        }
        public IActionResult Login()
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

        [HttpGet]
        public IActionResult EmailVerify()
        {
            return View(); 
        }

        [HttpGet]
        public IActionResult EmailVerifyForgotPassword()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> SendOTP(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                TempData["Error"] = "Email is required.";
                return RedirectToAction("EmailVerify");
            }
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.email == email);
            if (existingUser != null)
            {
                TempData["Error"] = "Email is already registered.";
                return RedirectToAction("EmailVerify");
            }
            var otp = new Random().Next(100000, 999999).ToString();
            HttpContext.Session.SetString("OTP", otp);
            HttpContext.Session.SetString("EmailToVerify", email);

            try
            {
                await _emailService.SendEmailAsync(email, "[Home Tutor] Your OTP Code", $"Your verification code is: <b>{otp}</b>");
                return RedirectToAction("VerifyOTP");
            }
            catch (SmtpException)
            {
                TempData["Error"] = "Unable to send OTP. This email may not exist or is unreachable.";
                return RedirectToAction("EmailVerify");
            }
            catch (Exception)
            {
                TempData["Error"] = "An unexpected error occurred. Please try again.";
                return RedirectToAction("EmailVerify");
            }
        }

        [HttpPost]
        public async Task<IActionResult> SendOTPForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                TempData["Error"] = "Email is required.";
                return RedirectToAction("EmailVerifyForgotPassword");
            }
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.email == email);
            if (existingUser == null)
            {
                TempData["Error"] = "Email is not register.";
                return RedirectToAction("EmailVerifyForgotPassword");
            }
            var otp = new Random().Next(100000, 999999).ToString();
            HttpContext.Session.SetString("OTP", otp);
            HttpContext.Session.SetString("EmailToVerifyForgotPassword", email);

            try
            {
                await _emailService.SendEmailAsync(email, "[Home Tutor] Your OTP Code For Re-create Password", $"Your verification code is: <b>{otp}</b>");
                return RedirectToAction("VerifyOTPForgotPassword");
            }
            catch (SmtpException)
            {
                TempData["Error"] = "Unable to send OTP. This email may not exist or is unreachable.";
                return RedirectToAction("EmailVerifyForgotPassword");
            }
            catch (Exception)
            {
                TempData["Error"] = "An unexpected error occurred. Please try again.";
                return RedirectToAction("EmailVerifyForgotPassword");
            }
        }

        [HttpGet]
        public IActionResult VerifyOTP()
        {
            return View();
        }

        [HttpGet]
        public IActionResult VerifyOTPForgotPassword()
        {
            return View();
        }


        [HttpPost]
        public IActionResult VerifyOTP(string otp)
        {
            var sessionOtp = HttpContext.Session.GetString("OTP");
            if (otp == sessionOtp)
            {
                TempData["EmailVerified"] = true;
                TempData["Message"] = "OTP verified successfully. You can now sign up.";
                return RedirectToAction("SignUp");
            }

            TempData["Error"] = "OTP is incorrect";
            return View();
        }

        [HttpPost]
        public IActionResult VerifyOTPForgotPassword(string otp)
        {
            var sessionOtp = HttpContext.Session.GetString("OTP");
            if (otp == sessionOtp)
            {
                TempData["EmailVerified"] = true;
                TempData["Message"] = "OTP verified successfully. You can create new Password.";
                return RedirectToAction("ForgotPassword");
            }

            TempData["Error"] = "OTP is incorrect";
            return View();
        }

        [HttpGet]
        public IActionResult SignUp()
        {
            if (!(TempData["EmailVerified"] as bool? ?? false))
                return RedirectToAction("EmailVerify");

            return View(); // Form nhập tên, số điện thoại, mật khẩu
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            if (!(TempData["EmailVerified"] as bool? ?? false))
                return RedirectToAction("EmailVerifyForgotPassword");
            return View(); // Form nhập mật khẩu mới
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string newPassword, string confirmNewPassword)
        {
            if (newPassword != confirmNewPassword)
            {
                TempData["Error"] = "New Password and Confirm New Password do not match.";
                return View();
            }
            if (newPassword.Length < 6)
            {
                TempData["Error"] = "New Password must be at least 6 characters long.";
                return View();
            }
            var email = HttpContext.Session.GetString("EmailToVerifyForgotPassword");
            var user = await _context.Users.FirstOrDefaultAsync(u => u.email == email);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return View();
            }
            user.password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            TempData["Message"] = "Password updated successfully! Please login.";
            return RedirectToAction("Login");
        }

        [HttpPost]

        public async Task<IActionResult> SignUp(User user, string confirmPassword)
        {
            
            
            if (user.password != confirmPassword)
            {
                TempData["Error"] = "Password and Confirm Password do not match.";
                return View();
            }
            if (user.teacherName.Length < 4)
            {
                TempData["Error"] = "Teacher name must be at least 4 characters long.";
                return View();
            }
            if (user.password.Length < 6)
            {
                TempData["Error"] = "Password must be at least 6 characters long.";
                return View();
            }
            
            if ( string.IsNullOrEmpty(user.password) || string.IsNullOrEmpty(user.teacherName))
            {
                TempData["Error"] = "Password and Full Name are required.";
                return View();
            }


            var defaultRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User");

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(user.password);

            var email = HttpContext.Session.GetString("EmailToVerify");

            user.email = email;
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
