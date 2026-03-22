using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Psychometric_Test_Designer.Models
{
    [Table("tokens_for_groups")]
    public class Token
    {
        [Key]
        [Column("token_id")]
        [MaxLength(50)]
        public string TokenId { get; set; }
        [MaxLength(4)]
        [Column("group_id")]
        public string GroupId { get; set; }
        public Group Group { get; set; }
        [Column("number_of_uses")]
        public int NumberOfUses { get; set; }
    }
}
