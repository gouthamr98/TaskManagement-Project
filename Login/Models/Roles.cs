using System.ComponentModel.DataAnnotations;

namespace Login.Models
{
    public class Roles
    {
        [Key]
        public int RoleId { get; set; }

        [Required,MaxLength(100)]
        public string RoleName { get; set; }

        //Navigation property
        public ICollection<Users> User { get; set; }

    }
}
