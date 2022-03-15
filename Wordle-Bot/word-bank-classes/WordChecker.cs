namespace Wordle_Bot;

public static class WordChecker
{
    public delegate bool Checker(string data);

    public static readonly Checker FiveLetter = FiveLetterString;
    
    /// <returns>a <see cref="bool"/> representing whether <paramref name="text"/> contains 5 characters</returns>
    private static bool FiveLetterString(string text) => text.Length == 5;
}