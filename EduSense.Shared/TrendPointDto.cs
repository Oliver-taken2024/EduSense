namespace EduSense.Shared
{
    public class TrendPointDto
    {
        public DateTime PeriodStart { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public double AverageScore { get; set; }
    }
}
