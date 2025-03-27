using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManagementAPI.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required, StringLength(50, MinimumLength = 3)]
        public string Username { get; set; } // Unique Username for login

        [Required]
        public string Password { get; set; } // Hashed password

        [Required]
        public string Role { get; set; } // "Admin" or "TeamMember"
    }
}

