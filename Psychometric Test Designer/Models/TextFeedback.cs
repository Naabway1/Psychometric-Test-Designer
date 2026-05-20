using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Psychometric_Test_Designer.Models
{
    [Table("text_feedback")]
    public class TextFeedback
    {
        [Key]
        [Column("feedback_id")]
        public int FeedbackId { get; set; }

        [Column("user_id")]
        public int? UserId { get; set; }
        public User? User { get; set; }

        [Column("group_id")]
        public int GroupId { get; set; }
        public Group Group { get; set; }

        [Column("text")]
        public string Text { get; set; }

        [Column("sentiment_score")]
        public decimal SentimentScore { get; set; }

        [Column("topics")]
        public string Topics { get; set; } = string.Empty;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
