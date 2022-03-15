namespace Wordle_Bot;

public class WordList
{
    private List<Word> Words { get; set; }

    public WordList(string wordsPath)
    {
        // Cast the word strings in the file at the path into Word objects
        using var wordsReader = new StreamReader(wordsPath);
        Words = new List<Word>();
        while (!wordsReader.EndOfStream)
        {
            var word = wordsReader.ReadLine();
            Words.Add(new Word(word!));
        }
    }
    
    public int GetWordsCount() => Words.Count;
    
    /// <summary>
    /// Receives information from wordle after a guess, and updates the list accordingly
    /// </summary>
    /// <param name="curWord">The word information</param>
    public void UpdateList(Word curWord)
    {
        var newList = new List<Word>();
        
        foreach (var word in Words)
        {
            bool wordValid = true;
            
            // Letters known to not be in the given word are in the other word
            if (HasAbsentLetters(word, curWord))
            {
                continue;
            }            
                   
            foreach (var letter in curWord.Letters)
            {
                if (letter.IsUnknown())
                    continue;

                // Given letter isn't found in the word
                if (word.Letters.All(letterInWord => letterInWord.Character != letter.Character))
                {
                    wordValid = false;
                    break;
                }

                // Same letter is in different indices - not the right word
                if (!CorrectLetterMatches(word, letter))
                {
                    wordValid = false;
                    break;
                }


                // Same letter is in index which the game told us it's not in - not the right word
                if (!PresentLetterMatches(word, letter))
                {
                    wordValid = false;
                    break;
                }

            }

            // Word matches the known information
            if (wordValid)
            {
                newList.Add(word);
            }
        }
        
        // Update the word list
        Words = newList;
    }

    /// <summary>
    /// Checks whether <paramref name="letter"/>, is known to be in a <see cref="Word"/>, and not in a certain index,
    /// and is found at the same index in <paramref name="word"/>
    /// </summary>
    /// <param name="word"></param>
    /// <param name="letter">The <see cref="Letter"/> in the word guessed in wordle</param>
    /// <returns>False if the <paramref name="letter"/> is present and is found in <paramref name="word"/> at an incorrect index, otherwise true</returns>
    private static bool PresentLetterMatches(Word word, Letter letter)
    {
        return !(word.Letters.Any(
                                letterInWord => letterInWord.Character == letter.Character &&
                                                letterInWord.IndicesIn.Any(
                                                    index => letter.IndicesNotIn[index])));
    }

    /// <summary>
    /// Checks whether <paramref name="letter"/>, is known to be at a certain index in a <see cref="Word"/>,
    /// and is found at the same index in <paramref name="word"/>
    /// </summary>
    /// <param name="word"></param>
    /// <param name="letter">The <see cref="Letter"/> in the word guessed in wordle</param>
    /// <returns>False if the <paramref name="letter"/> is correct and doesn't match a <see cref="Letter"/> in <paramref name="word"/>, otherwise true</returns>
    private static bool CorrectLetterMatches(Word word, Letter letter)
    {
        return !(letter.IndicesIn.Count > 0 && // The letter is correct (i.e. known to appear in the word in a certain index)
                            word.Letters.Any( // Check if letter matches any of the letters in the word (char and index)
                                letterInWord => letter.Character == letterInWord.Character &&
                                                letterInWord.IndicesIn.Intersect(letter.IndicesIn).ToList().Count() <= 0));
    }

    /// <summary>
    /// Checks whether <paramref name="firstWord"/> contains any letters
    /// known not to appear in <paramref name="secondWord"/>
    /// </summary>
    /// <param name="firstWord">The <see cref="Word"/> guessed in wordle (and all it's info)</param>
    /// <param name="secondWord">The current <see cref="Word"/> to check</param>
    /// <returns>True if contains, false if not</returns>
    private static bool HasAbsentLetters(Word firstWord, Word secondWord) =>
        firstWord.Letters.Any(
            letterInWord => secondWord.LettersNotIn.Contains((char)letterInWord.Character));

    /// <summary>
    /// Picks a random word from the list, removes, and returns it
    /// </summary>
    /// <returns>The chosen <see cref="Word"/></returns>
    public Word PickRandomWord()
    {
        // TODO maybe do when we have 4 green letters and one unknown, guess a random word with a lot of the
        // available letters so we narrow the search down

        Random rand = new Random();
        // Generate a random index to choose a word from
        int randIndex = rand.Next(Words.Count);
        // Choose that word and remove it from the list
        var chosenWord = Words[randIndex];
        Words.RemoveAt(randIndex);
        return chosenWord;
    } 
}