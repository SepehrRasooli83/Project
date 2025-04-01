namespace API.Model.DTOs
{
    public class WordImportResultDto
    {
        public int Valid { get; set; }
        public int Skipped { get; set; }
        public int Duplicates { get; set; }
        public int InvalidDifficulty { get; set; }
        public List<EnglishWord> ImportedWords { get; } = new();
    }
}
