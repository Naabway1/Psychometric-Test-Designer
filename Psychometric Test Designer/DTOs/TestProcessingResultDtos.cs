namespace Psychometric_Test_Designer.DTOs
{
    public class ProcessTestResultDto
    {
        public int UserId { get; set; }
        public int TestId { get; set; }
        public List<ProcessedScaleResultDto> Scales { get; set; } = new();
        public List<ProcessedMetricResultDto> Metrics { get; set; } = new();
    }

    public class ProcessedScaleResultDto
    {
        public int ScaleId { get; set; }
        public string ScaleName { get; set; }
        public bool IsPositive { get; set; }
        public decimal RawScore { get; set; }
        public decimal NormalizedScore { get; set; }
    }

    public class ProcessedMetricResultDto
    {
        public int MetricId { get; set; }
        public string MetricName { get; set; }
        public bool IsPositive { get; set; }
        public decimal CurrentValue { get; set; }
        public decimal EmaValue { get; set; }
    }
}
