using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Psychometric_Test_Designer.Models
{
    [Table("test_assignments")]
    public class TestAssignment
    {
        [Key]
        [Column("assignment_id")]
        public int AssignmentId { get; set; }

        [Column("test_id")]
        public int TestId { get; set; }
        public Test Test { get; set; }

        [Column("group_id")]
        public int GroupId { get; set; }
        public Group Group { get; set; }

        [Column("opens_at")]
        public DateTime OpensAt { get; set; }

        [Column("closes_at")]
        public DateTime ClosesAt { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

