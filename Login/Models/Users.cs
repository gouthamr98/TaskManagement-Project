using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Login.Models
{
    public class Users
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        public int RoleID { get; set; }


        [ForeignKey("RoleID")]
        public Roles Role { get; set; }


        [Required,MaxLength(100)]
        public string UserName { get; set; }


        [Required, MaxLength(100)]
        public string Password { get; set; }


        [Required,MaxLength(100)]

        public string Email { get; set; }

        public DateTime CreatedDate { get; set; }=DateTime.Now;

        public DateTime? LastLoginDate { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsDelete { get; set; } = false;


        // Additional fields for token handling
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }

    }
}
