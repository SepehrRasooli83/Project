using API.Data.Enums;
using API.Model.DTOs;
using API.Repository.Interfaces;
using API.Service.Interfaces;
using CsvHelper;
using CsvHelper.Configuration;
using System.Formats.Asn1;
using System.Globalization;

namespace API.Service
{
    public class EnglishWordService : IEnglishWordService
    {
        private readonly IEnglishWordRepository _repository;
        private readonly ILogger<EnglishWordService> _logger;

        public EnglishWordService(
            IEnglishWordRepository repository,
            ILogger<EnglishWordService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<WordImportResultDto> ImportWordsFromCsvAsync(IFormFile file)
        {
            try
            {
                var result = new WordImportResultDto();

                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    PrepareHeaderForMatch = args => args.Header.ToLower(),
                    HeaderValidated = null,
                    MissingFieldFound = null,
                    IgnoreBlankLines = true,
                    BadDataFound = context =>
                    {
                        _logger.LogWarning($"Bad data at row {context.Field}: {context.RawRecord}");
                        result.Skipped++;
                    }
                };

                using var reader = new StreamReader(file.OpenReadStream());
                using var csv = new CsvReader(reader, config);

                csv.Context.RegisterClassMap<CsvWordDtoMap>();

                await foreach (var record in csv.GetRecordsAsync<CsvWordDto>())
                {
                    try
                    {
                        // Validate word
                        if (string.IsNullOrWhiteSpace(record.Word))
                        {
                            result.Skipped++;
                            continue;
                        }

                        // Validate difficulty
                        if (!Enum.TryParse<DifficultyLevel>(record.Difficulty, out var difficultyLevel))
                        {
                            result.InvalidDifficulty++;
                            continue;
                        }

                        // Check for duplicates
                        if (await _repository.ExistsAsync(record.Word))
                        {
                            result.Duplicates++;
                            continue;
                        }

                        // Create new word entity
                        var word = new EnglishWord
                        {
                            Word = record.Word.Trim(),
                            Difficulty = difficultyLevel
                            // CreatedAt and UpdatedAt will be set automatically
                        };

                        result.ImportedWords.Add(word);
                        result.Valid++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error processing row {csv.Context.Parser.Row}");
                        result.Skipped++;
                    }
                }

                // Bulk insert valid words
                if (result.ImportedWords.Any())
                {
                    await _repository.AddRangeAsync(result.ImportedWords);
                }

                return result;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}
