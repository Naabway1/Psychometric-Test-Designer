using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Psychometric_Test_Designer.Models
{
    [Table("questions")]
    public class Question
    {
        [Key]
        [Column("question_id")]
        public int QuestionId { get; set; }

        [Required]
        [Column("text")]
        public string Text { get; set; }

        [Column("test_id")]
        public int TestId { get; set; }
        public Test Test { get; set; }

        public List<AnswerOption> AnswerOptions { get; set; }
        public List<QuestionScale> QuestionScales { get; set; }
    }
}
