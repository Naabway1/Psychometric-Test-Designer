namespace PsychometricTestDesigner.Frontend.Models;

public sealed class AuthResponse
{
    public int UserId { get; set; }
    public int GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}

public sealed class LoginRequest
{
    public string Login { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public sealed class RegisterRequest
{
    public string Login { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}

public sealed class RegisterStaffRequest
{
    public string Login { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int GroupId { get; set; }
    public string Role { get; set; } = "Admin";
    public string BootstrapKey { get; set; } = string.Empty;
}

public sealed class GroupDto
{
    public int GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public int? StudentCount { get; set; }
}

public sealed class CreateTokenRequest
{
    public int GroupId { get; set; }
    public int? NumberOfUses { get; set; }
}

public sealed class TokenResponse
{
    public string TokenId { get; set; } = string.Empty;
    public int GroupId { get; set; }
    public int NumberOfUses { get; set; }
}

public sealed class TestSummary
{
    public int TestId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int CreatedById { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class TestAssignment
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

public sealed class CreateTestAssignment
{
    public int TestId { get; set; }
    public int GroupId { get; set; }
    public DateTime OpensAt { get; set; }
    public DateTime ClosesAt { get; set; }
}

public sealed class CreateTestAssignments
{
    public int TestId { get; set; }
    public List<int> GroupIds { get; set; } = new();
    public DateTime OpensAt { get; set; }
    public DateTime ClosesAt { get; set; }
}

public sealed class FullTestResponse
{
    public int TestId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int CreatedById { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ScaleResponse> Scales { get; set; } = new();
    public List<MetricResponse> Metrics { get; set; } = new();
    public List<FullQuestionResponse> Questions { get; set; } = new();
    public List<TestScaleMetricResponse> MetricLinks { get; set; } = new();
}

public sealed class ScaleResponse
{
    public int ScaleId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsPositive { get; set; }
}

public sealed class MetricResponse
{
    public int MetricId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsPositive { get; set; }
}

public sealed class FullQuestionResponse
{
    public int QuestionId { get; set; }
    public string Text { get; set; } = string.Empty;
    public List<AnswerOptionResponse> AnswerOptions { get; set; } = new();
    public List<QuestionScaleResponse> ScaleLinks { get; set; } = new();
}

public sealed class AnswerOptionResponse
{
    public int AnswerId { get; set; }
    public string Text { get; set; } = string.Empty;
    public decimal Value { get; set; }
}

public sealed class QuestionScaleResponse
{
    public int ScaleId { get; set; }
    public string ScaleName { get; set; } = string.Empty;
    public bool ScaleIsPositive { get; set; }
    public double Weight { get; set; }
}

public sealed class TestScaleMetricResponse
{
    public int ScaleId { get; set; }
    public string ScaleName { get; set; } = string.Empty;
    public bool ScaleIsPositive { get; set; }
    public int MetricId { get; set; }
    public string MetricName { get; set; } = string.Empty;
    public bool MetricIsPositive { get; set; }
    public double Weight { get; set; }
}

public sealed class SubmitCurrentUserTest
{
    public int AssignmentId { get; set; }
    public int TestId { get; set; }
    public List<SubmitAnswer> Answers { get; set; } = new();
}

public sealed class SubmitAnswer
{
    public int QuestionId { get; set; }
    public int AnswerId { get; set; }
}

public sealed class ProcessTestResult
{
    public int UserId { get; set; }
    public int TestId { get; set; }
    public List<ProcessedScaleResult> Scales { get; set; } = new();
    public List<ProcessedMetricResult> Metrics { get; set; } = new();
}

public sealed class ProcessedScaleResult
{
    public int ScaleId { get; set; }
    public string ScaleName { get; set; } = string.Empty;
    public bool IsPositive { get; set; }
    public decimal RawScore { get; set; }
    public decimal NormalizedScore { get; set; }
}

public sealed class ProcessedMetricResult
{
    public int MetricId { get; set; }
    public string MetricName { get; set; } = string.Empty;
    public bool IsPositive { get; set; }
    public decimal CurrentValue { get; set; }
    public decimal EmaValue { get; set; }
}

public sealed class UserMetric
{
    public int MetricId { get; set; }
    public string MetricName { get; set; } = string.Empty;
    public bool IsPositive { get; set; }
    public decimal Value { get; set; }
}

public sealed class UserScaleResult
{
    public int ScaleId { get; set; }
    public string ScaleName { get; set; } = string.Empty;
    public bool IsPositive { get; set; }
    public decimal RawScore { get; set; }
    public decimal NormalizedScore { get; set; }
    public int SourceTestId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class GroupMetric
{
    public int MetricId { get; set; }
    public string MetricName { get; set; } = string.Empty;
    public bool IsPositive { get; set; }
    public decimal AverageValue { get; set; }
    public int UsersCount { get; set; }
}

public sealed class GroupRiskSummary
{
    public int GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public int UsersCount { get; set; }
    public decimal RiskIndex { get; set; }
    public string RiskLevel { get; set; } = string.Empty;
    public List<GroupMetric> Metrics { get; set; } = new();
}

public sealed class GroupMetricHistoryPoint
{
    public int MetricId { get; set; }
    public string MetricName { get; set; } = string.Empty;
    public bool IsPositive { get; set; }
    public DateTime Date { get; set; }
    public decimal AverageValue { get; set; }
}

public sealed class GroupScaleDistribution
{
    public int ScaleId { get; set; }
    public string ScaleName { get; set; } = string.Empty;
    public bool IsPositive { get; set; }
    public decimal AverageValue { get; set; }
    public int LowCount { get; set; }
    public int MediumCount { get; set; }
    public int HighCount { get; set; }
}

public sealed class FeedbackSubmit
{
    public string Text { get; set; } = string.Empty;
}

public sealed class FeedbackResponse
{
    public int FeedbackId { get; set; }
    public int GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public int? UserId { get; set; }
    public string StudentFullName { get; set; } = string.Empty;
    public string StudentLogin { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public decimal SentimentScore { get; set; }
    public List<string> Topics { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public sealed class GroupFeedbackTrend
{
    public int GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public int TotalMessages { get; set; }
    public decimal AverageSentiment { get; set; }
    public List<FeedbackTopic> Topics { get; set; } = new();
    public List<FeedbackResponse> RecentMessages { get; set; } = new();
}

public sealed class FeedbackTopic
{
    public string Topic { get; set; } = string.Empty;
    public int Count { get; set; }
}

public sealed class StudentResultSummary
{
    public int UserId { get; set; }
    public string Login { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public int GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public DateTime? LastActivityAt { get; set; }
    public List<UserMetric> CurrentMetrics { get; set; } = new();
    public List<UserScaleResult> LatestScales { get; set; } = new();
}

public sealed class AdminAnalytics
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

public sealed class GroupFillRate
{
    public int GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public int StudentCount { get; set; }
    public int ActiveStudents { get; set; }
    public int FeedbackCount { get; set; }
}

public sealed class StudentRecommendation
{
    public string MetricName { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public string Advice { get; set; } = string.Empty;
}

public sealed class TriggerAlert
{
    public int GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public string MetricName { get; set; } = string.Empty;
    public decimal CurrentValue { get; set; }
    public decimal Threshold { get; set; }
    public string Severity { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public sealed class CreateFullTest
{
    public string Title { get; set; } = string.Empty;
    public int CreatedById { get; set; }
    public List<ScaleDefinition> Scales { get; set; } = new();
    public List<MetricDefinition> Metrics { get; set; } = new();
    public List<CreateFullQuestion> Questions { get; set; } = new();
    public List<TestScaleMetricLink> MetricLinks { get; set; } = new();
}

public sealed class ScaleDefinition
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsPositive { get; set; }
}

public sealed class MetricDefinition
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsPositive { get; set; }
}

public sealed class CreateFullQuestion
{
    public string Text { get; set; } = string.Empty;
    public List<CreateAnswerOption> AnswerOptions { get; set; } = new();
    public List<QuestionScaleLink> ScaleLinks { get; set; } = new();
}

public sealed class CreateAnswerOption
{
    public string Text { get; set; } = string.Empty;
    public decimal Value { get; set; }
}

public sealed class QuestionScaleLink
{
    public string ScaleName { get; set; } = string.Empty;
    public double Weight { get; set; }
}

public sealed class TestScaleMetricLink
{
    public string ScaleName { get; set; } = string.Empty;
    public string MetricName { get; set; } = string.Empty;
    public double Weight { get; set; }
}
