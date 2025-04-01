using API.Model.DTOs;
using CsvHelper.Configuration;

public sealed class CsvWordDtoMap : ClassMap<CsvWordDto>
{
    public CsvWordDtoMap()
    {
        Map(m => m.Word).Name("headword");
        Map(m => m.Difficulty).Name("CEFR");
    }
}