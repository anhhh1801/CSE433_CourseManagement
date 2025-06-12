using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CourseManagement.Models
{

    public class Incomes
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IncomeId { get; set; }

        [Required]
        public double Amount { get; set; } = 0;
        public string? Description { get; set; }

        [Required]
        [ForeignKey("Course")]
        public int CourseId { get; set; }

        public Course? Course { get; set; }
    }
}
