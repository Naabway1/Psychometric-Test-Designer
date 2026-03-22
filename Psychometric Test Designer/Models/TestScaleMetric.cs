using System.ComponentModel.DataAnnotations.Schema;

namespace Psychometric_Test_Designer.Models
{
    [Table("test_scale_metric")]
    public class TestScaleMetric // влияние шкалы на метрику в рамках теста
    {
        [Column("test_id")]
        public int TestId { get; set; }
        [Column("scale_id")]
        public int ScaleId { get; set; }
        [Column("metric_id")]
        public int MetricId { get; set; }
        [Column("weight")]
        public double Weight { get; set; } // вес влияния шкалы на метрику

        public Test Test { get; set; }
        public Scale Scale { get; set; }
        public Metric Metric { get; set; }
    }
}
