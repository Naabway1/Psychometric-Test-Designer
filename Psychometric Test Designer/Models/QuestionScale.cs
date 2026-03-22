using System.ComponentModel.DataAnnotations.Schema;

namespace Psychometric_Test_Designer.Models
{
    [Table("question_scale")]
    public class QuestionScale // влияние вопроса на шкалу
    {
        [Column("question_id")]
        public int QuestionId { get; set; }
        [Column("scale_id")]
        public int ScaleId { get; set; }
        [Column("weight")]
        public double Weight { get; set; } // вес влияния вопроса на шкалу

        public Question Question { get; set; }
        public Scale Scale { get; set; }
    }
}
