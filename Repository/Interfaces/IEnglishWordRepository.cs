namespace API.Repository.Interfaces
{
    public interface IEnglishWordRepository
    {
        Task AddRangeAsync(IEnumerable<EnglishWord> words);
        Task<bool> ExistsAsync(string word);
    }

}
