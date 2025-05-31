using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CourseManagement.Models
{
    public class Expense
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int expenseId { get; set; }

        [Required]
        public double amount { get; set; }

        public string? description { get; set; }

        [Required]
        [ForeignKey("courseId")]
        public int CourseId { get; set; }
        public Course? course { get; set; }
    }
}
