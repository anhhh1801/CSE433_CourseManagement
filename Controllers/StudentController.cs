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

        public StudentController(CourseManagementDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var teacherId = User.FindFirstValue("TeacherId");

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
        public async Task<IActionResult> Add(Student model)
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
            model.Avatar = "/img/avatar_default.jpg";
            _context.Students.Add(model);
            _context.SaveChanges();
            ViewBag.Teacher = teacher;
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            var student = _context.Students.SingleOrDefault(s => s.studentId == id);
            if (student != null)
            {
                _context.Students.Remove(student);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var student = _context.Students.SingleOrDefault(s => s.studentId == id);
            if (student == null)
            {
                return NotFound();
            }
            return View(student);
        }

        [HttpPost]
        public IActionResult Edit(Student model)
        {
            var student = _context.Students.SingleOrDefault(s => s.studentId == model.studentId);
            if (student != null)
            {
                student.studentName = model.studentName;
                student.EntryDate = model.EntryDate;
                student.parentNumber = model.parentNumber;
                _context.Students.Update(student);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}

