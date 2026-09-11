namespace EduSense.Shared
{
    public class RespondentAnswerOptionDto
    {
        // QuestionAnswerOptionModel.Id - detta är värdet som skickas tillbaka
        // som ResponseModel.QuestionAnswerOptionId när respondenten svarar.
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public int Value { get; set; }
    }
}
