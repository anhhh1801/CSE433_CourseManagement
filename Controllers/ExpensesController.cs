using CourseManagement.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CourseManagement.Controllers
{
    public class ExpensesController : Controller
    {
        private readonly ILogger<ExpensesController> _logger;

        private readonly CourseManagementDbContext _context;

        public ExpensesController(ILogger<ExpensesController> logger, CourseManagementDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // Thêm Expense
        [HttpPost]
        public IActionResult AddExpense(int courseId, string Description, double Amount)
        {
            var teacherId = User.FindFirstValue("TeacherId");
            if (string.IsNullOrEmpty(teacherId) || !int.TryParse(teacherId, out int parsedTeacherId))
                return RedirectToAction("Login", "Authen");

            var course = _context.Courses.FirstOrDefault(c => c.courseId == courseId && c.TeacherId == parsedTeacherId);
            if (course == null)
                return Unauthorized();

            var expense = new Expenses
            {
                description = Description,
                amount = Amount,
                CourseId = courseId,
                createdAt = DateTime.Now
            };
            _context.Expenses.Add(expense);
            _context.SaveChanges();
            ViewBag.Courses = _context.Courses.Where(c => c.TeacherId == parsedTeacherId).ToList();
            return RedirectToAction("Index");
        }

        // Xóa Expense
        [HttpPost]
        public IActionResult DeleteExpense(int courseId, int id)
        {
            var teacherId = User.FindFirstValue("TeacherId");
            if (string.IsNullOrEmpty(teacherId) || !int.TryParse(teacherId, out int parsedTeacherId))
                return RedirectToAction("Login", "Authen");

            var expense = _context.Expenses
                .Include(e => e.course)
                .FirstOrDefault(e => e.expenseId == id && e.CourseId == courseId && e.course.TeacherId == parsedTeacherId);

            if (expense != null)
            {
                _context.Expenses.Remove(expense);
                _context.SaveChanges();
            }
            return RedirectToAction("Index", new { id = courseId });
        }
        //// Sua Expense
        //[HttpPost]
        //public IActionResult EditExpense(int expenseId, int courseId, string description, double amount)
        //{
        //    var teacherId = User.FindFirstValue("TeacherId");
        //    if (string.IsNullOrEmpty(teacherId) || !int.TryParse(teacherId, out int parsedTeacherId))
        //        return RedirectToAction("Login", "Authen");

        //    var expense = _context.Expenses
        //        .Include(e => e.course)
        //        .FirstOrDefault(e => e.expenseId == expenseId && e.course.TeacherId == parsedTeacherId);

        //    if (expense == null)
        //        return NotFound();

        //    expense.description = description;
        //    expense.amount = amount;
        //    expense.CourseId = courseId;

        //    _context.SaveChanges();

        //    return RedirectToAction("Index"); 
        //}

        [HttpPost]
        public IActionResult EditExpense(Expenses model)
        {
            var teacherId = User.FindFirstValue("TeacherId");
            if (string.IsNullOrEmpty(teacherId) || !int.TryParse(teacherId, out int parsedTeacherId))
                return RedirectToAction("Login", "Authen");

            var expense = _context.Expenses
                .Include(e => e.course)
                .FirstOrDefault(e => e.expenseId == model.expenseId && e.course.TeacherId == parsedTeacherId);

            if (expense == null)
                return NotFound();

            expense.description = model.description;
            expense.amount = model.amount;
            _context.Expenses.Update(expense);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult Index()
        {
            var teacherId = User.FindFirstValue("TeacherId");
            if (string.IsNullOrEmpty(teacherId) || !int.TryParse(teacherId, out int parsedTeacherId))
                return RedirectToAction("Login", "Authen");

            var expenses = _context.Expenses
                .Include(e => e.course)
                .Where(e => e.course.TeacherId == parsedTeacherId)
                .OrderByDescending(e => e.createdAt)
                .ToList();

            ViewBag.Courses = _context.Courses
                .Where(c => c.TeacherId == parsedTeacherId)
                .ToList();

            return View(expenses);
        }


        ///////////////


    }
}
