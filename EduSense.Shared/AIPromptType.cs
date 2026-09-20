namespace EduSense.Shared
{
    // Här listas de olika typer av AI-promptar som kan användas i systemet.
    // Varje typ representerar en specifik uppgift eller analys som AI:n kan
    // utföra baserat på användarens behov.
    public enum AiPromptType
    {
        LowestSatisfactionActionPlan,   // "Ge förslag på handlingsplaner för de tre frågor med lägst nöjdhetsgrad"
        TrendSummaryReport,             // "Sammanfatta trenderna i en kort rapport"
    }
}