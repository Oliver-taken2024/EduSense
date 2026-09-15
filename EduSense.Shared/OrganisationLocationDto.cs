namespace EduSense.Shared
{
    public class OrganisationLocationDto
    {
        public string OrganisationName { get; set; } = string.Empty;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public double AverageScore { get; set; }
        public int RespondentCount { get; set; }
    }
}
