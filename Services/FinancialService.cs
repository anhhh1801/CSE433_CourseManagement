using CourseManagement.Models;
using Microsoft.EntityFrameworkCore;
using CourseManagement.Controllers;

namespace CourseManagement.Services
{
    public class FinancialService
    {
        private readonly CourseManagementDbContext _context;

        public FinancialService(CourseManagementDbContext context)
        {
            _context = context;
        }

        public double GetTotalIncome(int courseId)
        {
            return _context.Incomes
                .Where(i => i.CourseId == courseId)
                .Sum(i => (double?)i.Amount) ?? 0;
        }

        public double GetTotalExpense(int courseId)
        {
            return _context.Expenses
                .Where(e => e.CourseId == courseId)
                .Sum(e => (double?)e.Amount) ?? 0;
        }

        public double UpdateCourseNetIncome(int courseId)
        {
            var course = _context.Courses.FirstOrDefault(c => c.courseId == courseId);
            if (course == null) return 0;

            double totalIncome = GetTotalIncome(courseId);
            double totalExpense = GetTotalExpense(courseId);
            course.netIncome = totalIncome - totalExpense;

            _context.Courses.Update(course);
            _context.SaveChanges();

            return course.netIncome ?? 0;
        }

        public void UpdateTeacherNetIncome(int teacherId)
        {
            var teacher = _context.Users
                .Include(u => u.courses)
                .FirstOrDefault(u => u.teacherId == teacherId);

            if (teacher != null)
            {
                teacher.NetIncome = teacher.courses.Sum(c => c.netIncome ?? 0);
                _context.Users.Update(teacher);
                _context.SaveChanges();
            }
        }

        


    }

}
