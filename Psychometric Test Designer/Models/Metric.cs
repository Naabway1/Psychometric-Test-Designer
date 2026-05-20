using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Psychometric_Test_Designer.Models
{
    [Table("metrics")]
    public class Metric
    {
        [Key]
        [Column("metric_id")]
        public int MetricId { get; set; }

        [Column("name")]
        [MaxLength(100)]
        public string Name { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("is_positive")]
        public bool IsPositive { get; set; }

        public List<TestScaleMetric> TestScaleMetrics { get; set; }
    }
}
