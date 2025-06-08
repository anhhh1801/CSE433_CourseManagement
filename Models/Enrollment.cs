using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace CourseManagement.Models
{
    public class Enrollment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int enrollmentId { get; set; }


        public double? ProgressScore { get; set; }

        public double? TestScore { get; set; }

        public double? FinalScore { get; set; }

        public char? Performance { get; set; }

        public bool isActive { get; set; } = true;

        [Required]
        [ForeignKey("studentId")]
        public int StudentId { get; set; }
        public Student? Student { get; set; }

        [Required]
        [ForeignKey("courseId")]
        public int CourseId { get; set; }
        public Course? Course { get; set; }

        [Required]
        [ForeignKey("teacherId")]
        public int TeacherId { get; set; }
        public User? Teacher { get; set; }
        public DateTime enrollmentDate { get; set; } = DateTime.Now;
        public DateTime? unenrollmentDate { get; set; }

        public void updatePerformance()
        {
            double? finalScore = this.FinalScore;
            if (finalScore == null)
            {
                this.Performance = 'F';
                return;
            }

            float[] scoreRanges = { 0, 3.5f, 5, 6.5f, 8 };
            char[] performanceRanges = { 'F', 'D', 'C', 'B', 'A' };

            for (int i = 0; i < scoreRanges.Length; i++)
            {
                if (finalScore >= scoreRanges[i])
                {
                    this.Performance = performanceRanges[i];
                }
            }
        }

        public void setFinalScore()
        {
            if (this.ProgressScore == null || this.TestScore == null)
            {
                this.FinalScore = null;
                return;
            }
            this.FinalScore = (this.ProgressScore + (this.TestScore * 2)) / 3;
        }
    }
}
