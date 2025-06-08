using CourseManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CourseManagement.Controllers
{
    
    public class AdminController : Controller
    {
        private readonly CourseManagementDbContext _context;
        public AdminController(CourseManagementDbContext context)
        {
            _context = context;
        }
        [Authorize(Roles = "Admin")]
        public IActionResult Index()
        {
            var teacherId = User.FindFirst("TeacherId")?.Value;
            if (teacherId == null)
            {
                return RedirectToAction("Login", "Authen");
            }
            int parsedId;
            if (!int.TryParse(teacherId, out parsedId))
            {
                return RedirectToAction("Login", "Authen");
            }
            var admin = _context.Users.FirstOrDefault(t => t.teacherId.ToString() == teacherId);
            var students = _context.Students.ToList();
            var courses = _context.Courses.ToList();
            var enrollments = _context.Enrollments.ToList();
            var users = _context.Users
                .Where(u => u.isActive).ToList();

            ViewData["PageName"] = "Admin Dashboard";
            ViewBag.Courses = courses;
            ViewBag.Students = students;
            ViewBag.Enrollments = enrollments;
            ViewBag.Users = users;

            return View(admin);
        }

        [HttpPost]
        public async Task<IActionResult> UploadAvatar(int id, IFormFile avatar)
        {
            if (avatar == null || avatar.Length == 0)
            {
                TempData["Error"] = "Please select a file.";
                return RedirectToAction("Index", "Admin");
            }

            try
            {
                var teacher = await _context.Users.FirstOrDefaultAsync(t => t.teacherId == id);
                if (teacher == null)
                {
                    TempData["Error"] = "Teacher not found.";
                    return RedirectToAction("Index", "Admin");
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

            return RedirectToAction("Index", "Admin");
        }
    }
}
