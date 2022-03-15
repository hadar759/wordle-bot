using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;

namespace Wordle_Bot
{
    public class Letter
    {
        public char? Character { get; set; }
        public List<int> IndicesIn { get; set; }
        public bool[] IndicesNotIn { get; set; }

        public Letter()
        {
            Character = null;
            IndicesIn = new List<int>();
            // Initialize a completely false list
            IndicesNotIn = new bool[5];
        }
        
        public Letter(char character)
        {
            Character = character;
            IndicesIn = new List<int>();
            // Initialize a completely false list
            IndicesNotIn = new bool[5];
        }
        
        public Letter(char character, bool[] indicesNotIn)
        {
            Character = character;
            IndicesIn = new List<int>();
            IndicesNotIn = indicesNotIn;
        }
        
        public Letter(char character, List<int> indicesIn)
        {
            Character = character;
            IndicesIn = indicesIn;
            // Initialize a completely false list
            IndicesNotIn = new bool[5];
            for (int i = 0; i < IndicesNotIn.Length; i++)
            {
                // Turn all array values, except the index the letter is in, to true
                if (!IndicesIn.Contains(i))
                    IndicesNotIn[i] = true;
            }
        }

        /// <returns>Whether the <see cref="Letter"/> has any info</returns>
        public bool IsUnknown() => Character is null;

        public override string ToString()
        {
            var characterPart = $"Letter {Character} ";
            var indexPart = IndicesIn.Count > 0 ? $"in {ListToString(IndicesIn)}" : "";
            var indicesPart = !IndicesIn.Any() ? $"not in {ListToString(IndicesNotIn)}" : "";
            return characterPart + indexPart + indicesPart;
        }

        /// <returns>A string representation of all values in <paramref name="list"/></returns>
        private string ListToString<T>(IEnumerable<T> list)
        {
            var str = new StringBuilder();
            foreach (var val in list)
            {
                str.Append($"{val}, ");
            }

            return str.ToString();
        }
    }
}