using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Psychometric_Test_Designer.Models
{
    [Table("groups")]
    public class Group
    {
        [Key]
        [Column("group_id")]
        public int GroupId { get; set; }

        [Column("group_name")]
        [MaxLength(4)]
        public string GroupName { get; set; }

        [Column("specialization")]
        [MaxLength(200)]
        public string Specialization { get; set; }
        public List<User> Users { get; set; }
    }
}
