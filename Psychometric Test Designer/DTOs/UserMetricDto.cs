namespace Psychometric_Test_Designer.DTOs
{
    public class UserMetricDto
    {
        public int MetricId { get; set; }
        public string MetricName { get; set; }
        public bool IsPositive { get; set; }
        public decimal Value { get; set; } 
    }

    public class StudentResultSummaryDto
    {
        public int UserId { get; set; }
        public string Login { get; set; }
        public string FullName { get; set; }
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public DateTime? LastActivityAt { get; set; }
        public List<UserMetricDto> CurrentMetrics { get; set; } = new();
        public List<UserScaleResultDto> LatestScales { get; set; } = new();
    }
}
