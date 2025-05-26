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
            var teacher = _context.Teachers.FirstOrDefault(t => t.teacherId.ToString() == teacherId);
            return View(teacher);
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
