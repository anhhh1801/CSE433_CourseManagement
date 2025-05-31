using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Text.Json;
using CourseManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static System.Net.Mime.MediaTypeNames;
using System.Text.Json;
using System.Security.Claims;


namespace CourseManagement.Controllers
{
    public class CourseController : Controller
    {
        private readonly ILogger<CourseController> _logger;

        private readonly CourseManagementDbContext _context;

        public CourseController(ILogger<CourseController> logger, CourseManagementDbContext context)
        {
            _logger = logger;
            _context = context;
        }
        public IActionResult Index(int id)
        {
            var teacherId = User.FindFirstValue("TeacherId"); // Get logged-in Teacher ID

            if (teacherId == null)
            {
                return RedirectToAction("Login", "Authen");
            }

            int parsedId;
            if (!int.TryParse(teacherId, out parsedId))
            {
                return RedirectToAction("Login", "Authen");
            }

            var courses = _context.Courses
                .Where(c => c.teacher.teacherId == parsedId)
                .ToList();

            return View(courses);
        }

        public IActionResult Add()
        { 
            return View();
        }

        public IActionResult Edit(int id)
        {
            var course = _context.Courses.SingleOrDefault(c => c.courseId == id);
            return View(course);
        }
        public IActionResult Delete(int id)
        {
            var course = _context.Courses.SingleOrDefault(c => c.courseId == id);
            if (course != null)
            {
                _context.Courses.Remove(course);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
        [HttpPost]
        public IActionResult Edit(Course model)
        {
            var course = _context.Courses.SingleOrDefault(c => c.courseId == model.courseId);
            if (course != null)
            {
                course.courseName = model.courseName;
                course.courseCode = model.courseCode;
                course.courseSubject = model.courseSubject;

                _context.Courses.Update(course);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Add(Course model)
        {
            var teacherId = User.FindFirstValue("TeacherId");
            if (string.IsNullOrEmpty(teacherId) || !int.TryParse(teacherId, out int parsedTeacherId))
            {
                return RedirectToAction("Login", "Authen"); // Redirect if teacher is not logged in
            }
            var teacher = _context.Users.FirstOrDefault(t => t.teacherId == parsedTeacherId);
            if (teacher == null)
            {
                return BadRequest("Không tìm thấy giáo viên.");
            }
            model.teacher = teacher; 
            _context.Courses.Add(model);
            _context.SaveChanges();
            return RedirectToAction("Index");

        }

        
    }
}
