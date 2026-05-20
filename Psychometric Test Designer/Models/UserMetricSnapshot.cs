using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Psychometric_Test_Designer.Models
{
    [Table("user_metric_snapshots")]
    public class UserMetricSnapshot
    {
        [Key]
        [Column("ums_id")]
        public int UserMetricSnapshotId { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }
        public User User { get; set; }

        [Column("metric_id")]
        public int MetricId { get; set; }
        public Metric Metric { get; set; }

        [Column("value")]
        public decimal Value { get; set; }

        [Column("source_test_id")]
        public int SourceTestId { get; set; }
        public Test SourceTest { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
