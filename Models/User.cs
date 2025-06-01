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

        public int? phoneNumber { get; set; }

        public ICollection<Course> courses { get; set; }

        public ICollection<Student> students { get; set; }

        public ICollection<Role> Role { get; set; } = new List<Role>();


        public User()
        {
            courses = new List<Course>();
            students = new List<Student>();
        }
    }
}
