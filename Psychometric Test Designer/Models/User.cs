using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Psychometric_Test_Designer.Core;

namespace Psychometric_Test_Designer.Models
{
    [Table("users")]
    public class User
    {
        [Key]
        [Column("user_id")]
        public int UserId { get; set; }
        [Column("login")]
        public string Login { get; set; }
        [Column("full_name")]
        public string FullName { get; set; } = string.Empty;
        [Column("password")]
        public string Password { get; set; }

        [Column("role")]
        public string Role { get; set; } = UserRoles.Student;

        [Column("group_id")]
        public int GroupId { get; set; }
        public Group Group { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<UserMetric> Metrics { get; set; }
    }
}
