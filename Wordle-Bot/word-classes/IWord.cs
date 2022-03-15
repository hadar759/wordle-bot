using System.Collections.Generic;

namespace Wordle_Bot
{
    public interface IWord
    {
        Letter[] Letters { get; set; }
    }
}