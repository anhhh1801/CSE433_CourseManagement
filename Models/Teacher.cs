using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CourseManagement.Models
{
    public class Teacher
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int teacherId { get; set; }
        public string? teacherName { get; set; }

        public string email { get; set; }

        public string password { get; set; }

        public string? avatar { get; set; }

    

        public int? phoneNumber { get; set; }

        public Role role = Role.USER;
        public ICollection<Course> courses { get; set; }

        public ICollection<Student> students { get; set; }
    }
}
