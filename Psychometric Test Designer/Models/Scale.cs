using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Psychometric_Test_Designer.Models
{
    [Table("scales")]
    public class Scale
    {
        [Key]
        [Column("scale_id")]
        public int ScaleId { get; set; }

        [MaxLength(100)]
        [Column("name")]
        public string Name { get; set; }

        [Column("description")]
        public string Description { get; set; }

        public List<QuestionScale> QuestionScales { get; set; }

        public List<TestScaleMetric> TestScaleMetrics { get; set; }
    }
}
