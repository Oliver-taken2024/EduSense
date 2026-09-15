namespace EduSense.Shared
{
    public class SegmentResultDto
    {
        public RespondentSegmentDto Segment { get; set; }
        public int RespondentCount { get; set; }
        public double ResponseRate { get; set; }
        public double AverageScore { get; set; }
        public double? Nps { get; set; }
    }
}
