namespace Wordle_Bot;

public class WordsWriter : IDisposable
{
    private bool _disposed;
    private StreamWriter WordsStream { get; }

    public WordsWriter(string filePath)
    {
        WordsStream = new StreamWriter(filePath);
    }
    
    /// <summary>
    /// Writes <paramref name="data"/> to file if data matches <paramref name="checkMethod"/>
    /// </summary>
    /// <param name="checkMethod">An optional method by which to check each line of data</param>
    public void WriteToFile(string data, WordChecker.Checker? checkMethod = null)
    {
        // Only write the word if it matches the check we've imposed
        if (checkMethod is null || checkMethod(data))
        {
            WordsStream.WriteLine(data);
        }
    }

    private void Dispose(bool disposing)
    {
        // Already disposed
        if (_disposed)
            return;
        
        if (disposing)
        {
            // Close the stream
            WordsStream.Dispose();
        }

        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}