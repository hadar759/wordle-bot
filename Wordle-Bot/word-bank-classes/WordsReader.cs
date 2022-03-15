namespace Wordle_Bot;

public class WordsReader : IDisposable
{
    private bool _disposed;
    private StreamReader StringStream { get; }

    public WordsReader(string filePath)
    {
        StringStream = new StreamReader(filePath);
    }

    /// <summary>
    /// Reads each line asynchronously from a given file
    /// </summary>
    /// <param name="checkMethod">The optional method by which to check the output,
    /// and ignore if not valid</param>
    /// <returns>An <see cref="IAsyncEnumerable{string}"/> representing all lines in the file</returns>
    public async IAsyncEnumerable<string> ReadFile(WordChecker.Checker? checkMethod = null)
    {
        while (!StringStream.EndOfStream)
        {
            string word = await StringStream.ReadLineAsync();

            // Only return the word if it matches the check we've imposed
            if (checkMethod is null || checkMethod(word))
                yield return word;
        }
    }

    private void Dispose(bool disposing)
    {
        if (_disposed)
            return;
        
        if (disposing)
        {
            // Close the stream
            StringStream.Dispose();
        }

        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}