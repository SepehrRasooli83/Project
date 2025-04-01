﻿using API.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    // EnglishWordController.cs
    [ApiController]
    [Route("api/[controller]")]
    public class EnglishWordController : ControllerBase
    {
        private readonly IEnglishWordService _englishWordService;

        public EnglishWordController(IEnglishWordService importService)
        {
            _englishWordService = importService;
        }

        [HttpPost("import-csv")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ImportCsv(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            if (Path.GetExtension(file.FileName).ToLower() != ".csv")
                return BadRequest("Only CSV files are supported");

            try
            {
                var result = await _englishWordService.ImportWordsFromCsvAsync(file);

                return Ok(new
                {
                    Success = true,
                    TotalProcessed = result.Valid + result.Skipped + result.Duplicates + result.InvalidDifficulty,
                    result.Valid,
                    result.Skipped,
                    result.Duplicates,
                    result.InvalidDifficulty,
                    ImportedWords = result.ImportedWords.Select(w => new { w.Word, w.Difficulty })
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error processing file: {ex.Message}");
            }
        }
    }
}
