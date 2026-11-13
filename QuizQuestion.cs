namespace CybersecurityBot
{
    public class QuizQuestion
    {
        public string Question { get; set; } = "";
        public string[] Options { get; set; } = System.Array.Empty<string>();
        public int CorrectIndex { get; set; }
        public string Explanation { get; set; } = "";
    }
}
