using Microsoft.EntityFrameworkCore;

namespace CourseManagement.Models
{
    public class CourseManagementDbContext : DbContext
    {
        public CourseManagementDbContext(DbContextOptions<CourseManagementDbContext> options)
            : base(options)
        {
        }

        public DbSet<Student> Students { get; set; }

        public DbSet<Teacher> Teachers { get; set; }

        public DbSet<Course> Courses { get; set; }

        public DbSet<Enrollment> Enrollments { get; set; }

        public DbSet<Revenue> Revenues { get; set; }

        public DbSet<Expense> Expenses { get; set; }
    }
}
