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

        public DbSet<User> Users { get; set; }

        public DbSet<Course> Courses { get; set; }

        public DbSet<Enrollment> Enrollments { get; set; }

        public DbSet<Incomes> Incomes { get; set; }

        public DbSet<Expenses> Expenses { get; set; }

        public DbSet<Role> Roles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Student)
                .WithMany(s => s.enrollments)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Course)
                .WithMany(c => c.enrollments)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Teacher)
                .WithMany(u => u.enrollments)
                .HasForeignKey(e => e.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Incomes>()
                .Property(i => i.Amount)
                .HasDefaultValue(0);

            modelBuilder.Entity<Expenses>()
                .Property(e => e.Amount)
                .HasDefaultValue(0);

            modelBuilder.Entity<Role>().HasData(
                new Role
                {
                    RoleId = 1,
                    Name = "Admin"
                },
                new Role
                {
                    RoleId = 2,
                    Name = "User"
                }
            );
        }
    }
}
