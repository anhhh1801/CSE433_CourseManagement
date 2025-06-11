using CourseManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CourseManagement.Controllers
{
    public class RevenuesController : Controller
    {
        private readonly CourseManagementDbContext _context;
        private readonly ILogger<RevenuesController> _logger;

        public RevenuesController(ILogger<RevenuesController> logger, CourseManagementDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            var teacherId = User.FindFirstValue("TeacherId");
            if (string.IsNullOrEmpty(teacherId) || !int.TryParse(teacherId, out int parsedTeacherId))
                return RedirectToAction("Login", "Authen");

            var revenues = _context.Revenues
                .Include(r => r.course)
                .Where(r => r.course.TeacherId == parsedTeacherId)
                .OrderByDescending(r => r.createdAt)
                .ToList();

            ViewBag.Courses = _context.Courses
                .Where(c => c.TeacherId == parsedTeacherId)
                .ToList();

            return View(revenues);
        }

        [HttpPost]
        public IActionResult AddRevenue(int courseId, string Description, double Amount)
        {
            var teacherId = User.FindFirstValue("TeacherId");
            if (string.IsNullOrEmpty(teacherId) || !int.TryParse(teacherId, out int parsedTeacherId))
                return RedirectToAction("Login", "Authen");

            var course = _context.Courses.FirstOrDefault(c => c.courseId == courseId && c.TeacherId == parsedTeacherId);
            if (course == null)
                return Unauthorized();

            var revenue = new Revenues
            {
                description = Description,
                amount = Amount,
                CourseId = courseId,
                createdAt = DateTime.Now
            };

            _context.Revenues.Add(revenue);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult DeleteRevenue(int courseId, int id)
        {
            var teacherId = User.FindFirstValue("TeacherId");
            if (string.IsNullOrEmpty(teacherId) || !int.TryParse(teacherId, out int parsedTeacherId))
                return RedirectToAction("Login", "Authen");

            var revenue = _context.Revenues
                .Include(r => r.course)
                .FirstOrDefault(r => r.revenueId == id && r.CourseId == courseId && r.course.TeacherId == parsedTeacherId);

            if (revenue != null)
            {
                _context.Revenues.Remove(revenue);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult EditRevenue(Revenues model)
        {
            var teacherId = User.FindFirstValue("TeacherId");
            if (string.IsNullOrEmpty(teacherId) || !int.TryParse(teacherId, out int parsedTeacherId))
                return RedirectToAction("Login", "Authen");

            var revenue = _context.Revenues
                .Include(r => r.course)
                .FirstOrDefault(r => r.revenueId == model.revenueId && r.course.TeacherId == parsedTeacherId);

            if (revenue == null)
                return NotFound();

            revenue.description = model.description;
            revenue.amount = model.amount;
            _context.Revenues.Update(revenue);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
