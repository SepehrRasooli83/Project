using API.Data.Enums;
using System.ComponentModel.DataAnnotations;

namespace API.Model.DTOs
{
    public class ProcessPdfInput
    {
        [Required]
        public DifficultyLevel DifficultyLevel { get; set; }
        [Required]
        public IFormFile File { get; set; }
    }
}
