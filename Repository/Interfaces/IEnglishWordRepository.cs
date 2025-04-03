using API.Data.Enums;

namespace API.Repository.Interfaces
{
    public interface IEnglishWordRepository
    {
        Task AddRangeAsync(IEnumerable<EnglishWord> words);
        Task<bool> ExistsAsync(string word);
        Task<List<string>> FilterWordsByDifficultyLevel(List<string> words,DifficultyLevel difficultyLevel);
    }

}
