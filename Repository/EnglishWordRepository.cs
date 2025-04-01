using API.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Repository
{
    public class EnglishWordRepository : IEnglishWordRepository
    {
        private readonly AppDbContext _context;

        public EnglishWordRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddRangeAsync(IEnumerable<EnglishWord> words)
        {
            try
            {
                if (words == null || !words.Any()) return;

                // Get all words in one query to check for existing ones
                var wordTexts = words.Select(w => w.Word.Trim().ToLower()).ToList();
                var existingWords = await _context.EnglishWords
                    .Where(w => wordTexts.Contains(w.Word.ToLower()))
                    .Select(w => w.Word.ToLower())
                    .ToListAsync();

                // Filter out duplicates
                var uniqueWords = words
                    .Where(w => !existingWords.Contains(w.Word.Trim().ToLower()))
                    .ToList();

                if (uniqueWords.Any())
                {
                    await _context.EnglishWords.AddRangeAsync(uniqueWords);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public async Task<bool> ExistsAsync(string word)
        {
            try
            {
                return await _context.EnglishWords
                        .AsNoTracking()
                        .AnyAsync(w => w.Word.ToLower() == word.ToLower().Trim());
            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}
