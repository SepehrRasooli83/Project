﻿using API.Model.DTOs;

namespace API.Service.Interfaces
{
    public interface IEnglishWordService
    {
        Task<WordImportResultDto> ImportWordsFromCsvAsync(IFormFile file);
    }
}
