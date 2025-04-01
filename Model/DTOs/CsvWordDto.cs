using CsvHelper.Configuration.Attributes;
using System.ComponentModel.DataAnnotations;

namespace API.Model.DTOs
{
    public class CsvWordDto
    {
        [Name("headword")]
        public string Word { get; set; }

        [Name("CEFR")]
        public string Difficulty { get; set; }
    }
}
