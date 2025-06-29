﻿using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CourseManagement.Models
{
    public class Student
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int studentId { get; set; }

        [Required]
        public string studentName { get; set; }

        public DateTime EntryDate { get; set; }
        public string parentNumber { get; set; }

        public string? Avatar { get; set; }

        [ForeignKey("teacherId")]
        public int TeacherId { get; set; }
        public User? teacher { get; set; }

        public ICollection<Enrollment> enrollments { get; set; } = new List<Enrollment>();
    }
}