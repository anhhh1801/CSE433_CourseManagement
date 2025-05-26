using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CourseManagement.Models
{
    public class Course
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int courseId { get; set; }

        [Required]
        public string courseCode { get; set; }

        [Required]
        public string courseSubject { get; set; }

        [Required]
        public string courseName { get; set; }

        [ForeignKey("teacherId")]
        public int TeacherId { get; set; }
        public Teacher teacher { get; set; }


    }
}
