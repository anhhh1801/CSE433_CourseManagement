using System.Diagnostics;
using System.Security.Claims;
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
                .Take(10);
            ViewData["PageName"] = "Dashboard";
            ViewBag.Courses = courses;
            ViewBag.Students = students;
            ViewBag.Enrollments = enrollments;
            return View(teacher);
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

      

    }
}
