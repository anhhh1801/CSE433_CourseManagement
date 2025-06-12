using Microsoft.AspNetCore.Mvc;
using CourseManagement.Models;
using CourseManagement.Services;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace CourseManagement.Controllers
{
    public class ExpensesController : Controller
    {
        private readonly CourseManagementDbContext _context;

        public ExpensesController(CourseManagementDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult Add(int courseId, string description, double amount)
        {
            var expense = new Expenses
            {
                CourseId = courseId,
                Description = description,
                Amount = amount
            };
            _context.Expenses.Add(expense);
            _context.SaveChanges();
            var course = _context.Courses.Include(c => c.expenses).Include(c => c.incomes).FirstOrDefault(c => c.courseId == courseId);
            if (course != null)
            {
                course.netIncome = course.incomes.Sum(i => i.Amount) - course.expenses.Sum(e => e.Amount);
                _context.Courses.Update(course);
                _context.SaveChanges();
                // Update teacher's net income
                var financialService = new FinancialService(_context);
                financialService.UpdateTeacherNetIncome(course.TeacherId);
            }
            // Redirect to the financial overview of the course

            return RedirectToAction("Financial", "Course", new { id = courseId });
        }

        [HttpPost]
        public IActionResult Delete(int courseId, int id)
        {
            var expense = _context.Expenses.FirstOrDefault(e => e.ExpenseId == id && e.CourseId == courseId);
            if (expense != null)
            {
                _context.Expenses.Remove(expense);
                _context.SaveChanges();
            }
            var course = _context.Courses.Include(c => c.expenses).FirstOrDefault(c => c.courseId == courseId);
            if (course != null) {
                course.netIncome = course.incomes.Sum(i => i.Amount) - course.expenses.Sum(e => e.Amount);
                _context.Courses.Update(course);
                _context.SaveChanges();
                // Update teacher's net income
                var financialService = new FinancialService(_context);
                financialService.UpdateTeacherNetIncome(course.TeacherId);
            }
            // Redirect to the financial overview of the course

            return RedirectToAction("Financial", "Course", new { id = courseId });
        }
    }
}
