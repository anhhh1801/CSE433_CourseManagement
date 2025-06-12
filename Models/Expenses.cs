using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CourseManagement.Models
{
    public class Expenses
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ExpenseId { get; set; }

        [Required]
        public double Amount { get; set; } = 0;
        public string? Description { get; set; }

        [Required]
        [ForeignKey("courseId")]
        public int CourseId { get; set; }
        public Course? course { get; set; }
    }
}
