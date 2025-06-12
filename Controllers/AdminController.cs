using System.Security.Claims;
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
                TempData["Error"] = "Not Found User";
                return RedirectToAction("Login", "Authen");
            }
            int parsedId;
            if (!int.TryParse(teacherId, out parsedId))
            {
                TempData["Error"] = "Not Found User";
                return RedirectToAction("Login", "Authen");
            }
            var admin = _context.Users.FirstOrDefault(t => t.teacherId.ToString() == teacherId);
            var students = _context.Students.ToList();
            var courses = _context.Courses.ToList();
            var admins = _context.Users
                .Include(t => t.Role)
                .Where(t => t.isActive && t.Role.Any(r => r.Name == "Admin"))
                .ToList();
            var users = _context.Users
                .Where(u => u.isActive).ToList();

            ViewData["PageName"] = "Admin Dashboard";
            ViewBag.Courses = courses;
            ViewBag.Students = students;
            ViewBag.Users = users;
            ViewBag.Admins = admins;

            return View(admin);
        }
        //Admin
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


        //Student
        [HttpGet]
        public async Task<IActionResult> StudentList()
        {
            ViewData["PageName"] = "Student List";
            var teachers = await _context.Users
                .Include(t => t.students)
                .Where(t => t.isActive)
                .ToListAsync();

            return View(teachers);
        }



        //Course
        [HttpGet]
        public async Task<IActionResult> CourseList()
        {
            ViewData["PageName"] = "Course List";
            var teachers = await _context.Users
                .Include(t => t.courses)
                .Include(t => t.enrollments)
                .Where(t => t.isActive)
                .ToListAsync();

            return View(teachers);
        }

        //Enrollment
        public async Task<IActionResult> EnrollmentList()
        {
            ViewData["PageName"] = "Enrollment List";
            var teachers = await _context.Users
                .Include(t => t.courses)
                .Include(t => t.enrollments)
                .Where(t => t.isActive)
                .ToListAsync();

            return View(teachers);
        }

        //User
        public async Task<IActionResult> UserList()
        {
            ViewData["PageName"] = "User List";
            var teachers = await _context.Users
                .Include(t => t.Role)
                .Where(t => t.isActive)
                .ToListAsync();

            var users = teachers.Where(t => t.Role.Any(r => r.Name == "User")).ToList();

            var admins = teachers.Where(t => t.Role.Any(r => r.Name == "Admin")).ToList();

            ViewBag.Users = users;
            ViewBag.Admins = admins;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleAdmin(int userId, bool isAdmin)
        {
            var user = _context.Users.Include(u => u.Role).FirstOrDefault(u => u.teacherId == userId);
            if (user == null)
            {
                TempData["Erroe"] = "User Not Found";
                return NotFound();
            }

            var adminRole = _context.Roles.FirstOrDefault(r => r.Name == "Admin");
            if (adminRole == null) return BadRequest();

            if (isAdmin)
            {
                if (!user.Role.Any(r => r.Name == "Admin"))
                    user.Role.Add(adminRole);
            }
            else
            {
                var roleToRemove = user.Role.FirstOrDefault(r => r.Name == "Admin");
                if (roleToRemove != null)
                    user.Role.Remove(roleToRemove);
            }
            _context.Users.Update(user);
            _context.SaveChanges();
            TempData["Message"] = "Admin Updated!";
            return RedirectToAction("UserList"); // hoặc tên Action bạn dùng để hiển thị danh sách
        }

        [HttpPost]
        public IActionResult EditProfile(int id, string teacherName, string phoneNumber, string email)
        {
            var teacher = _context.Users.FirstOrDefault(t => t.teacherId == id);
            if (teacher == null)
            {
                return NotFound();
            }
            teacher.teacherName = teacherName;
            teacher.phoneNumber = phoneNumber;
            teacher.email = email;
            _context.Users.Update(teacher);
            _context.SaveChanges();
            TempData["Message"] = "Update Profile Successfully";
            return RedirectToAction("UserList");

        }

        public IActionResult DeleteUser(int id)
        {
            var students = _context.Students
                .Where(s => s.TeacherId == id)
                .ToList();
            var courses = _context.Courses
                .Where(c => c.TeacherId == id)
                .ToList();

            var enrollments = _context.Enrollments
                .Where(e => e.TeacherId == id)
                .ToList();
            var user = _context.Users.FirstOrDefault(u => u.teacherId == id);

            if (user == null)
            {
                TempData["Error"] = "User Not Found";
                return RedirectToAction("UserList");
            }
            foreach (var enrollment in enrollments)
            {
                _context.Enrollments.Remove(enrollment);
            }
            // Xóa tất cả sinh viên liên kết với giáo viên
            foreach (var student in students)
            {
                _context.Students.Remove(student);
            }
            // Xóa tất cả khóa học liên kết với giáo viên
            foreach (var course in courses)
            {
                _context.Courses.Remove(course);
            }
            // Xóa tất cả đăng ký liên kết với giáo viên
            
            // Xóa người dùng
            _context.Users.Remove(user);
            _context.SaveChanges();
            TempData["Message"] = "User Deleted Successfully";
            return RedirectToAction("UserList");
        }
    }
}
