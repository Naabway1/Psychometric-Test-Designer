namespace Psychometric_Test_Designer.DTOs
{
    public class CreateTestAssignmentDto
    {
        public int TestId { get; set; }
        public int GroupId { get; set; }
        public DateTime OpensAt { get; set; }
        public DateTime ClosesAt { get; set; }
    }

    public class CreateTestAssignmentsDto
    {
        public int TestId { get; set; }
        public List<int> GroupIds { get; set; } = new();
        public DateTime OpensAt { get; set; }
        public DateTime ClosesAt { get; set; }
    }

    public class TestAssignmentDto
    {
        public int AssignmentId { get; set; }
        public int TestId { get; set; }
        public string TestTitle { get; set; } = string.Empty;
        public int GroupId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public DateTime OpensAt { get; set; }
        public DateTime ClosesAt { get; set; }
        public bool IsActive { get; set; }
    }
}

