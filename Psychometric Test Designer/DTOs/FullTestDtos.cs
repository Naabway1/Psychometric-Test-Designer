namespace Psychometric_Test_Designer.DTOs
{
    public class CreateFullTestDto
    {
        public string Title { get; set; }
        public int CreatedById { get; set; }
        public List<ScaleDefinitionDto> Scales { get; set; } = new();
        public List<MetricDefinitionDto> Metrics { get; set; } = new();
        public List<CreateFullQuestionDto> Questions { get; set; } = new();
        public List<TestScaleMetricLinkDto> MetricLinks { get; set; } = new();
    }

    public class ScaleDefinitionDto
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public bool IsPositive { get; set; }
    }

    public class MetricDefinitionDto
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public bool IsPositive { get; set; }
    }

    public class CreateFullQuestionDto
    {
        public string Text { get; set; }
        public List<CreateAnswerOptionDto> AnswerOptions { get; set; } = new();
        public List<QuestionScaleLinkDto> ScaleLinks { get; set; } = new();
    }

    public class CreateAnswerOptionDto
    {
        public string Text { get; set; }
        public decimal Value { get; set; }
    }

    public class QuestionScaleLinkDto
    {
        public string ScaleName { get; set; }
        public double Weight { get; set; }
    }

    public class TestScaleMetricLinkDto
    {
        public string ScaleName { get; set; }
        public string MetricName { get; set; }
        public double Weight { get; set; }
    }

    public class FullTestResponseDto
    {
        public int TestId { get; set; }
        public string Title { get; set; }
        public int CreatedById { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<ScaleResponseDto> Scales { get; set; } = new();
        public List<MetricResponseDto> Metrics { get; set; } = new();
        public List<FullQuestionResponseDto> Questions { get; set; } = new();
        public List<TestScaleMetricResponseDto> MetricLinks { get; set; } = new();
    }

    public class ScaleResponseDto
    {
        public int ScaleId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public bool IsPositive { get; set; }
    }

    public class MetricResponseDto
    {
        public int MetricId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public bool IsPositive { get; set; }
    }

    public class FullQuestionResponseDto
    {
        public int QuestionId { get; set; }
        public string Text { get; set; }
        public List<AnswerOptionResponseDto> AnswerOptions { get; set; } = new();
        public List<QuestionScaleResponseDto> ScaleLinks { get; set; } = new();
    }

    public class AnswerOptionResponseDto
    {
        public int AnswerId { get; set; }
        public string Text { get; set; }
        public decimal Value { get; set; }
    }

    public class QuestionScaleResponseDto
    {
        public int ScaleId { get; set; }
        public string ScaleName { get; set; }
        public bool ScaleIsPositive { get; set; }
        public double Weight { get; set; }
    }

    public class TestScaleMetricResponseDto
    {
        public int ScaleId { get; set; }
        public string ScaleName { get; set; }
        public bool ScaleIsPositive { get; set; }
        public int MetricId { get; set; }
        public string MetricName { get; set; }
        public bool MetricIsPositive { get; set; }
        public double Weight { get; set; }
    }
}
