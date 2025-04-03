using API.Data.Enums;
using API.Model.DTOs;
using API.Repository.Interfaces;
using API.Service.Interfaces;
using CsvHelper;
using CsvHelper.Configuration;
using System.Formats.Asn1;
using System.Globalization;
using System.Text.RegularExpressions;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using DocumentFormat.OpenXml.Packaging;

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

        public async Task<byte[]> ReturnImportantFilesBasedOnDifficultyLevel(IFormFile file, DifficultyLevel difficultyLevel)
        {
            List<string> fileWords = await ExtractFileWordsAsync(file);

            List<string> filteredWords = await FilterWordsByDifficultyLevel(fileWords,difficultyLevel);

            var result = CreatePdfFromWords(filteredWords);

            return result;
        }

        #region Extract File Words Methods
        public async Task<List<string>> ExtractFileWordsAsync(IFormFile file)
        {
            var words = new List<string>();
            var fileExtension = System.IO.Path.GetExtension(file.FileName).ToLowerInvariant();

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                stream.Position = 0; // Reset position after copying

                switch (fileExtension)
                {
                    case ".pdf":
                        words = ExtractWordsFromPdf(stream);
                        break;
                    case ".docx":
                        words = ExtractWordsFromDocx(stream);
                        break;
                    case ".txt":
                        words = ExtractWordsFromText(stream);
                        break;
                    default:
                        throw new NotSupportedException($"File type {fileExtension} is not supported");
                }
            }

            return words;
        }

        private List<string> ExtractWordsFromPdf(Stream stream)
        {
            var words = new List<string>();
            var reader = new iTextSharp.text.pdf.PdfReader(stream);

            try
            {
                for (int page = 1; page <= reader.NumberOfPages; page++)
                {
                    var text = PdfTextExtractor.GetTextFromPage(reader, page);
                    words.AddRange(SplitTextIntoWords(text));
                }
            }
            finally
            {
                // Manually clean up resources
                reader.Close();
            }

            return words;
        }

        private List<string> ExtractWordsFromDocx(Stream stream)
        {
            // Install required package: DocumentFormat.OpenXml
            // dotnet add package DocumentFormat.OpenXml --version 2.19.0
            using (var doc = WordprocessingDocument.Open(stream, false))
            {
                var body = doc.MainDocumentPart?.Document.Body;
                var text = body?.InnerText ?? string.Empty;
                return SplitTextIntoWords(text);
            }
        }

        private List<string> ExtractWordsFromText(Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                var text = reader.ReadToEnd();
                return SplitTextIntoWords(text);
            }
        }

        private List<string> SplitTextIntoWords(string text)
        {
            // Split by whitespace and remove punctuation
            return text.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries)
                       .Select(word => Regex.Replace(word, @"[^\w]", "")) // Remove punctuation
                       .Where(word => !string.IsNullOrWhiteSpace(word))
                       .Select(word => word.ToLowerInvariant())
                       .ToList();
        }

        #endregion

        public async Task<List<string>> FilterWordsByDifficultyLevel(List<string> words,DifficultyLevel difficultyLevel)
        {
            var result = await _repository.FilterWordsByDifficultyLevel(words,difficultyLevel);

            return result;
        }

        public byte[] CreatePdfFromWords(List<string> words)
        {
            using (var memoryStream = new MemoryStream())
            {
                // Create document
                var document = new iTextSharp.text.Document();
                var writer = iTextSharp.text.pdf.PdfWriter.GetInstance(document, memoryStream);

                document.Open();

                // Add title
                document.Add(new iTextSharp.text.Paragraph("Filtered Words List"));
                document.Add(new iTextSharp.text.Paragraph(" ")); // Blank line

                // Add words
                foreach (var word in words)
                {
                    document.Add(new iTextSharp.text.Paragraph(word));
                }

                document.Close();
                return memoryStream.ToArray();
            }
        }
    }
}
