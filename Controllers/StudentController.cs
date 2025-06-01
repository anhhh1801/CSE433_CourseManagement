using System.Linq;
using System.Security.Claims;
using CourseManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CourseManagement.Controllers
{
    public class StudentController : Controller
    {
        private readonly CourseManagementDbContext _context;

        public StudentController (CourseManagementDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var teacherId = User.FindFirstValue("TeacherId"); // Get logged-in Teacher ID

            if (teacherId == null)
            {
                return RedirectToAction("NeedLogin", "Authen");
            }

            int parsedId;
            if (!int.TryParse(teacherId, out parsedId))
            {
                return RedirectToAction("Login", "Authen");
            }

            var students = _context.Students
                .Where(s => s.teacher != null && s.teacher.teacherId == parsedId)
                .ToList();

            return View(students);
        }

        [HttpGet]
        public async Task<IActionResult> Add()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddUser()
        {
            //if (avatar == null || avatar.Length == 0)
            //{
            //    TempData["error"] = "Please select a file";
            //    return RedirectToAction("Index", "Home");
            //}

            //// Kiểm tra định dạng file
            //var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            //var extension = Path.GetExtension(avatar.FileName).ToLowerInvariant();

            //if (!allowedExtensions.Contains(extension))
            //{
            //    TempData["error"] = "Only image files are allowed!";
            //    return RedirectToAction("Index", "Home");
            //}

            //// Tạo thư mục nếu chưa có
            //var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Assets", "img");
            //if (!Directory.Exists(uploadsFolder))
            //{
            //    Directory.CreateDirectory(uploadsFolder);
            //}

            //// Tạo tên file duy nhất
            //var fileName = Guid.NewGuid().ToString() + extension;
            //var filePath = Path.Combine(uploadsFolder, fileName);

            //// Lưu file
            //using (var stream = new FileStream(filePath, FileMode.Create))
            //{
            //    await avatar.CopyToAsync(stream);
            //}

            //// Giả định có biến currentTeacher và TeacherService
            //currentTeacher.AvatarImg = fileName;
            //await _teacherService.SaveAsync(currentTeacher); // hoặc Save nếu không dùng async

            //TempData["message"] = "Avatar updated!";
            //return RedirectToAction("Index");
            return View(); // Return the view for adding a student
        }
    }
}
