using CourseManagement.Models;
using Microsoft.AspNetCore.Mvc;
using CourseManagement.Services;

namespace CourseManagement.Controllers
{
    public class IncomesController : Controller
    {
        private readonly CourseManagementDbContext _context;
        private readonly FinancialService _financialService;

        public IncomesController(CourseManagementDbContext context, FinancialService financialService)
        {
            _context = context;
            _financialService = financialService;
        }

        [HttpPost]
        public IActionResult Add(int courseId, string description, double amount)
        {
            var income = new Incomes
            {
                CourseId = courseId,
                Description = description,
                Amount = amount
            };
            _context.Incomes.Add(income);
            _context.SaveChanges();

            var course = _context.Courses.FirstOrDefault(c => c.courseId == courseId);
            if (course != null)
            {
                course.netIncome = _financialService.UpdateCourseNetIncome(courseId);
                _context.Courses.Update(course);
                _context.SaveChanges();
                // Update teacher's net income
                _financialService.UpdateTeacherNetIncome(course.TeacherId);
            }

            return RedirectToAction("Financial", "Course", new { id = courseId });
        }

        [HttpPost]
        public IActionResult Delete(int courseId, int id)
        {
            var income = _context.Incomes.FirstOrDefault(i => i.IncomeId == id && i.CourseId == courseId);
            if (income != null)
            {
                _context.Incomes.Remove(income);
                _context.SaveChanges();
            }
            var course = _context.Courses.FirstOrDefault(c => c.courseId == courseId);
            if (course != null)
            {
                course.netIncome = _financialService.UpdateCourseNetIncome(courseId);
                _context.Courses.Update(course);
                _context.SaveChanges();
                // Update teacher's net income
                _financialService.UpdateTeacherNetIncome(course.TeacherId);
            }
            return RedirectToAction("Financial", "Course", new { id = courseId });
        }
    }
}
