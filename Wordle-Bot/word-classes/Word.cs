using System.Text;

namespace Wordle_Bot
{
    public class Word : IWord
    {
        public Letter[] Letters { get; set; }
        private List<char> _lettersNotIn = new ();
        public List<char> LettersNotIn 
        {
            get => _lettersNotIn; 
            // No repeats
            set => _lettersNotIn = LettersNotIn.Concat(value).Distinct().ToList();
        }

        /// <summary>
        /// Initalize an empty word
        /// </summary>
        public Word()
        {
            // Initalize with 5 unknown letters
            Letters = new [] {new Letter(), new Letter(), new Letter(), new Letter(), new Letter()};
            LettersNotIn = new List<char>();
        }

        /// <summary>
        /// Initalize a word from a string
        /// </summary>
        public Word(string word)
        {
            Letters = word.Select(
                letter => new Letter(
                    letter, getCharIndices(word, letter)
                    )
                ).ToArray();
            
            LettersNotIn = new List<char>();
        } 

        /// <summary>
        /// Returns all indices of a <see cref="char"/> in a <see cref="string"/>
        /// </summary>
        private List<int> getCharIndices(string word, char letter)
        {
            var indices = new List<int>();
            for (int i = 0; i < word.Length; i++)
            {
                if (word[i] == letter)
                    indices.Add(i);
            }

            return indices;
        }

        /// <returns>A string representation of all characters in the <see cref="Word"/></returns>
        public string GetWordString() => string.Concat(Letters.Select(letter => letter.Character));
        
        public override string ToString()
        {
            var wordToString = new StringBuilder();
            foreach (var letter in Letters)
            {
                wordToString.AppendLine(letter.ToString());
            }

            return wordToString.ToString();
        }
    }
}