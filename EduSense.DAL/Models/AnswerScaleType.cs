namespace EduSense.DAL.Models
{
    // Diskriminerar vilken skala en AnswerOption hör till - utan den fanns inget
    // sätt att skilja standardskalans rader från NPS-skalans i databasen förutom
    // att jämföra Description-text, vilket duplicerade samma strängar i flera filer.
    public enum AnswerScaleType
    {
        Standard1To5 = 0,
        Nps1To10 = 1
    }
}
