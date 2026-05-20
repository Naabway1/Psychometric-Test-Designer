namespace Psychometric_Test_Designer.DTOs
{
    public class UserMetricDto
    {
        public int MetricId { get; set; }
        public string MetricName { get; set; }
        public bool IsPositive { get; set; }
        public decimal Value { get; set; } 
    }
}
