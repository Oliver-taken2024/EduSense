namespace EduSense.Shared
{
    public class SurveyResultDto
    {
        public int TotalResponses { get; set; }
        public double ResponseRate { get; set; }
        public double AverageScore { get; set; }
        public double? Nps { get; set; }
        public int CriticalAreasCount { get; set; }
        public List<CategoryScoreDto> CategoryScores { get; set; } = [];
        public List<AnswerDistributionDto> AnswerDistribution { get; set; } = [];
        public List<SegmentResultDto> SegmentResults { get; set; } = [];
        public List<QuestionScoreDto> TopStrengths { get; set; } = [];
        public List<QuestionScoreDto> Challenges { get; set; } = [];
        public List<TrendPointDto> Trend { get; set; } = [];
    }
}
