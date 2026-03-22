using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Psychometric_Test_Designer.Models
{
    [Table("user_scale_results")]
    public class UserScaleResult
    {
        [Key]
        [Column("usr_id")]
        public int UserScaleResultId { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }
        public User User { get; set; }

        [Column("scale_id")]
        public int ScaleId { get; set; }
        public Scale Scale { get; set; }

        [Column("raw_score")]
        public decimal RawScore { get; set; }

        [Column("normalized_score")]
        public decimal NormalizedScore { get; set; }

        [Column("source_test_id")]
        public int SourceTestId { get; set; }
        public Test SourceTest { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
