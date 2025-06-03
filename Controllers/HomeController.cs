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
            ViewData["PageName"] = "Dashboard";
            ViewBag.Courses = courses;
            ViewBag.Students = students;
            return View(teacher);
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
