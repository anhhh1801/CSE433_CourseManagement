﻿using System.ComponentModel.DataAnnotations.Schema;
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

        public bool isActive = true;

        [Required][ForeignKey("studentId")]
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
    }
}
