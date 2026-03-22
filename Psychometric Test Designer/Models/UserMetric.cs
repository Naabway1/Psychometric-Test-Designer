using System.ComponentModel.DataAnnotations.Schema;

namespace Psychometric_Test_Designer.Models
{
    [Table("user_metrics")]
    public class UserMetric
    {
        [Column("user_id")]
        public int UserId { get; set; }
        [Column("metric_id")]
        public int MetricId { get; set; }
        [Column("user_metric_value")]
        public double Value { get; set; }

        public User User { get; set; }
        public Metric Metric { get; set; }
    }
}
