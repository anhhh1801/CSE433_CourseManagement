using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CourseManagement.Models
{
    public class Revenues
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int revenueId { get; set; }

        [Required]
        public double amount { get; set; }

        public string? description { get; set; }
        [Required]
        public DateTime createdAt { get; set; } = DateTime.Now;

        [Required]
        [ForeignKey("courseId")]
        public int CourseId { get; set; }
        public Course? course { get; set; }

       
    }
}
