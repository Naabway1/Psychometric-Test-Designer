using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        [Column("password")]
        public string Password { get; set; }

        [Column("group_id")]
        public string GroupId { get; set; }
        public Group Group { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public List<UserMetric> Metrics { get; set; }
    }
}
