using System.Diagnostics;
using CourseManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CourseManagement.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly CourseManagementDbContext _context;

        public HomeController(ILogger<HomeController> logger, CourseManagementDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult UnLoginHomePage()
        {
            return View();
        }

        public IActionResult Index()
        {
            var teacherId = User.FindFirst("TeacherId")?.Value;
            if (teacherId == null)
            {
                return RedirectToAction("Login", "Authen");
            }

            var teacher = _context.Users.FirstOrDefault(t => t.teacherId.ToString() == teacherId);
            var students = _context.Students
                .Include(s => s.teacher)
                .Where(s => s.TeacherId.ToString() == teacherId)
                .ToList();
            var courses = _context.Courses
                .Include(c => c.Teacher)
                .Where(c => c.TeacherId.ToString() == teacherId)
                .ToList();
            var enrollments = _context.Enrollments
                .Include(e => e.Student)
                .OrderByDescending(e => e.FinalScore)
                .Where(e => e.isActive)
                .Take(5);
            var top10 = _context.Enrollments
                .Include(e => e.Student)
                .Include(e => e.Course)
                .OrderByDescending(e => e.FinalScore)
                .Take(10)
                .ToList();
            double? netIncomeTotal = 0;
            foreach(var c in courses)
            {
                netIncomeTotal += c.netIncome;
            }
            ViewData["PageName"] = "Dashboard";
            ViewBag.Courses = courses;
            ViewBag.Students = students;
            ViewBag.Enrollments = enrollments;
            ViewBag.NetIncomeTotal = netIncomeTotal;
            return View(teacher);
        }

        [HttpPost]
        public IActionResult EditProfile (int id, string teacherName, string phoneNumber, string email)
        {
            var teacher = _context.Users.FirstOrDefault(t => t.teacherId == id);
            if(teacher == null)
            {
                return NotFound();
            }
            teacher.teacherName = teacherName;
            teacher.phoneNumber = phoneNumber;
            teacher.email = email;
            _context.Users.Update(teacher);
            _context.SaveChanges();
            TempData["Message"] = "Update Profile Successfully";
            return RedirectToAction("Index");

        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public async Task<IActionResult> UploadAvatar(int id, IFormFile avatar)
        {
            if (avatar == null || avatar.Length == 0)
            {
                TempData["Error"] = "Please select a file.";
                return RedirectToAction("Index");
            }

            try
            {
                var teacher = await _context.Users.FirstOrDefaultAsync(t => t.teacherId == id);
                if (teacher == null)
                {
                    TempData["Error"] = "Teacher not found.";
                    return RedirectToAction("Index");
                }

                var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img");
                if (!Directory.Exists(uploadDir))
                {
                    Directory.CreateDirectory(uploadDir);
                }

                var fileName = Path.GetFileName(avatar.FileName);
                var filePath = Path.Combine(uploadDir, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await avatar.CopyToAsync(stream);
                }

                teacher.avatar = "/img/" + avatar.FileName;
                _context.Users.Update(teacher);
                _context.SaveChanges();

                TempData["Message"] = $"Avatar updated successfully for {teacher.teacherName}.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Could not upload avatar: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(string oldPass, string newPass, string confirmNewPass)
        {
            var teacherId = User.FindFirst("TeacherId")?.Value;
            if (teacherId == null)
            {
                TempData["Error"] = "Not Found User";
                return RedirectToAction("Login", "Authen");
            }
            int parsedId;
            if (!int.TryParse(teacherId, out parsedId))
            {
                TempData["Error"] = "Not Found User";
                return RedirectToAction("Login", "Authen");
            }
            var teacher = await _context.Users.FirstOrDefaultAsync(t => t.teacherId.ToString() == teacherId);

            bool isValid = BCrypt.Net.BCrypt.Verify(oldPass, teacher.password);

            if (!isValid)
            {
                TempData["Error"] = "Old password is incorrect!";
                return RedirectToAction("Index", "Home");
            }

            if (newPass != confirmNewPass)
            {
                TempData["Error"] = "New password and confirmation do not match!";
                return RedirectToAction("Index", "Home");
            }

            if (newPass.Length < 6)
            {
                TempData["Error"] = "New password must be at least 6 characters long!";
                return RedirectToAction("Index", "Home");
            }

            // Hash the new password
            teacher.password = BCrypt.Net.BCrypt.HashPassword(newPass);
            _context.Users.Update(teacher);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Password changed successfully!";
            return RedirectToAction("Index", "Home");
        }
    }
}
