using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Psychometric_Test_Designer.Models
{
    [Table("answers")]
    public class AnswerOption
    {
        [Key]
        [Column("answer_id")]
        public int AnswerId { get; set; }

        [Column("text")]
        public string Text { get; set; }

        [Column("answer_value")]
        public decimal Value { get; set; }

        [Column("question_id")]
        public int QuestionId { get; set; }
        public Question? Question { get; set; }
    }
}
