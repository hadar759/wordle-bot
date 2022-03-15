using System.IO;
using Wordle_Bot;

namespace Wordle_Bot
{
    class Program
    {
        static void Main(string[] args)
        {
            // Get project folder
            string projectDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
            string resourcesDirectory = Path.Combine(projectDirectory, "resources");
            string wordsFilePath = Path.Combine(resourcesDirectory, "words.txt");
            
            // Store only 5 letter words - valid for wordle
            string fiveLetterPath = Path.Combine(resourcesDirectory, "words-five-letters.txt");
            // Only create the 5 letter words if not already created
            if (!File.Exists(fiveLetterPath))
                CreateFiveLetters(wordsFilePath, fiveLetterPath);

            // Get the words list
            var wordList = new WordList(fiveLetterPath);
            
            // Run the bot and play the game
            var bot = new Bot(resourcesDirectory, wordList);

            // bot.Run(wordToGuess);
            bot.Run();

        }

        /// <summary>
        /// Creates a file at <paramref name="destinationPath"/> containing all 5 letters strings 
        /// from the file at <paramref name="originPath"/>. Won't close until file is done processing
        /// </summary>
        /// <param name="originPath">The path of the original file containing all words</param>
        /// <param name="destinationPath">The path of the file to cotain</param>
        public static void CreateFiveLetters(string originPath, string destinationPath)
        {
            // The directory containing the destination path
            var destDirectory = Path.GetDirectoryName(destinationPath);

            WordBankCreator.CreateFiveLetterWordsFile(originPath, destinationPath).Start();
            
            using var resourcesWatcher = new FileSystemWatcher(destDirectory);
            
            // Wait until all 5 letter words have been written
            while (!resourcesWatcher.WaitForChanged(WatcherChangeTypes.Changed, 50).TimedOut)
            {
                Console.WriteLine("Writing 5 letter words...");
            }

            Console.WriteLine("Finished writing 5 letter words");
        }
    }
}