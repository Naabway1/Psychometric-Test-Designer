using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Psychometric_Test_Designer.Models
{
    [Table("tests")]
    public class Test
    {
        [Key]
        [Column("test_id")]
        public int TestId { get; set; }
        [Column("title")]
        [MaxLength(255)]
        public string Title { get; set; }

        [Column("created_by")]
        public int CreatedById { get; set; }
        public User CreatedBy { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public List<Question> Questions { get; set; }
    }
}
