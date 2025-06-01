using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CourseManagement.Models
{
    public class Role
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RoleId { get; set; }

        [Required, StringLength(50)]
        public string Name { get; set; }
        // Navigation
        public ICollection<User> Users
        {
            get; set;
        }
    }
}
