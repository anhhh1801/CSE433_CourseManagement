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
        [Required]
        public double TuitionFee { get; set; } = 0;

        public double? netIncome { get; set; } = 0;

        [ForeignKey("teacherId")]
        public int TeacherId { get; set; }
        public User? Teacher { get; set; }

        public ICollection<Enrollment> enrollments { get; set; } = new List<Enrollment>();
        public ICollection<Incomes> incomes { get; set; }
        public ICollection<Expenses> expenses { get; set; }


    }
}
