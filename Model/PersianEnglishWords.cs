public class EnglishPersianWords
{
    public string Id { get; set; }
    public char IDdataroot { get; set; }
    public string Category { get; set; }
    public string EnglishWord { get; set; }
    public string PersianWord { get; set; }

    public EnglishPersianWords() { }

    public EnglishPersianWords(string id, char idDataroot, string category, string englishWord, string persianWord)
    {
        Id = id;
        IDdataroot = idDataroot;
        Category = category;
        EnglishWord = englishWord;
        PersianWord = persianWord;
    }
}
