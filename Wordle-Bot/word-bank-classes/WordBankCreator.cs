namespace Wordle_Bot;

public static class WordBankCreator
{
    /// <summary>
    /// Creates a file in <paramref name="destinationPath"/> with each 5 letter word from <paramref name="originPath"/>
    /// </summary>
    /// <param name="originPath">Path of the file to be read from</param>
    /// <param name="destinationPath">Path of the file to be written to</param>
    public static async Task CreateFiveLetterWordsFile(string originPath, string destinationPath)
    {
        // Create the reader and the writer to interact with the files
        using var reader = new WordsReader(originPath);
        using var writer = new WordsWriter(destinationPath);

        // Only read 5 letter words
        await foreach (var word in reader.ReadFile(WordChecker.FiveLetter))
        {
            writer.WriteToFile(word);
        }
    }
}