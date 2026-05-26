namespace Psychometric_Test_Designer.DTOs
{
    public class UserScaleResultDto
    {
        public int ScaleId { get; set; }
        public string ScaleName { get; set; }
        public bool IsPositive { get; set; }
        public decimal RawScore { get; set; }
        public decimal NormalizedScore { get; set; }
        public int SourceTestId { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class GroupMetricDto
    {
        public int MetricId { get; set; }
        public string MetricName { get; set; }
        public bool IsPositive { get; set; }
        public decimal AverageValue { get; set; }
        public int UsersCount { get; set; }
    }

    public class GroupRiskSummaryDto
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public int UsersCount { get; set; }
        public decimal RiskIndex { get; set; }
        public string RiskLevel { get; set; }
        public List<GroupMetricDto> Metrics { get; set; } = new();
    }

    public class GroupMetricHistoryPointDto
    {
        public int MetricId { get; set; }
        public string MetricName { get; set; }
        public bool IsPositive { get; set; }
        public DateTime Date { get; set; }
        public decimal AverageValue { get; set; }
    }

    public class GroupScaleDistributionDto
    {
        public int ScaleId { get; set; }
        public string ScaleName { get; set; }
        public bool IsPositive { get; set; }
        public decimal AverageValue { get; set; }
        public int LowCount { get; set; }
        public int MediumCount { get; set; }
        public int HighCount { get; set; }
    }

    public class FeedbackSubmitDto
    {
        public string Text { get; set; }
    }

    public class FeedbackResponseDto
    {
        public int FeedbackId { get; set; }
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public int? UserId { get; set; }
        public string StudentFullName { get; set; }
        public string StudentLogin { get; set; }
        public string Text { get; set; }
        public decimal SentimentScore { get; set; }
        public List<string> Topics { get; set; } = new();
        public DateTime CreatedAt { get; set; }
    }

    public class GroupFeedbackTrendDto
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public int TotalMessages { get; set; }
        public decimal AverageSentiment { get; set; }
        public List<FeedbackTopicDto> Topics { get; set; } = new();
        public List<FeedbackResponseDto> RecentMessages { get; set; } = new();
    }

    public class FeedbackTopicDto
    {
        public string Topic { get; set; }
        public int Count { get; set; }
    }

    public class AdminAnalyticsDto
    {
        public int TotalUsers { get; set; }
        public int TotalStudents { get; set; }
        public int TotalGroups { get; set; }
        public int TotalTests { get; set; }
        public int TotalTestsTaken { get; set; }
        public int TotalFeedback { get; set; }
        public int TotalAnswers { get; set; }
        public decimal FillRate { get; set; }
    }

    public class GroupFillRateDto
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public int StudentCount { get; set; }
        public int ActiveStudents { get; set; }
        public int FeedbackCount { get; set; }
    }

    public class StudentRecommendationDto
    {
        public string MetricName { get; set; }
        public string Level { get; set; }
        public string Advice { get; set; }
    }

    public class TriggerAlertDto
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public string MetricName { get; set; }
        public decimal CurrentValue { get; set; }
        public decimal Threshold { get; set; }
        public string Severity { get; set; }
        public string Message { get; set; }
    }
}
