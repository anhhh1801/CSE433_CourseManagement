using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CourseManagement.Models
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int teacherId { get; set; }

        [Required]
        public string teacherName { get; set; }

        [Required]
        public string email { get; set; }

        [Required]
        public string password { get; set; }

        public string? avatar { get; set; }

        public string? phoneNumber { get; set; }

        public bool isActive { get; set; } = true;

        public ICollection<Course> courses { get; set; }

        public ICollection<Student> students { get; set; }

        public ICollection<Role> Role { get; set; } = new List<Role>();

        public ICollection<Enrollment> enrollments { get; set; } = new List<Enrollment>();


        public User()
        {
            courses = new List<Course>();
            students = new List<Student>();
        }
    }
}
