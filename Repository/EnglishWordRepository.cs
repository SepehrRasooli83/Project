using API.Data.Enums;
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

        #region Add Word To Db Methods
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
        #endregion


        public async Task<List<string>> FilterWordsByDifficultyLevel(List<string> words, DifficultyLevel difficultyLevel)
        {
            // Convert input words to lowercase for case-insensitive comparison
            var normalizedWords = words.Select(w => w.ToLower().Trim()).ToList();

            // Determine which difficulty levels to include based on input
            List<DifficultyLevel> targetLevels = difficultyLevel switch
            {
                //if a1 or a2 selected the difficulties should be a1 - c2
                DifficultyLevel.A1 => new() { DifficultyLevel.A1, DifficultyLevel.A2 , DifficultyLevel.B1,DifficultyLevel.B2,DifficultyLevel.C1,DifficultyLevel.C2 },
                DifficultyLevel.A2 => new() { DifficultyLevel.A1, DifficultyLevel.A2, DifficultyLevel.B1, DifficultyLevel.B2, DifficultyLevel.C1, DifficultyLevel.C2 },

                //if b1 or b2 selected teh difficulties should be b1-c2
                DifficultyLevel.B1 => new() { DifficultyLevel.B1, DifficultyLevel.B2, DifficultyLevel.C1,DifficultyLevel.C2 },
                DifficultyLevel.B2 => new() { DifficultyLevel.B1, DifficultyLevel.B2, DifficultyLevel.C1, DifficultyLevel.C2 },

                DifficultyLevel.C1 => new() { DifficultyLevel.C1, DifficultyLevel.C2 },
                DifficultyLevel.C2 => new() { DifficultyLevel.C1, DifficultyLevel.C2 },
                _ => new() { difficultyLevel }
            };

            // Query database for matching words with specified difficulty levels
            var filteredWords = await _context.EnglishWords
                .Where(ew => normalizedWords.Contains(ew.Word.ToLower()) &&
                             targetLevels.Contains(ew.Difficulty))
                .Select(ew => ew.Word)
                .Distinct() // Ensure no duplicates
                .ToListAsync();

            return filteredWords;
        }
    }
}
