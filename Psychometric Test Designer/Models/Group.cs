using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Psychometric_Test_Designer.Models
{
    [Table("groups")]
    public class Group
    {
        [Key]
        [Column("group_id")]
        public string GroupId { get; set; }

        [Column("specialization")]
        [MaxLength(100)]
        public string Specialization { get; set; }

        public List<User> Users { get; set; }
    }
}
