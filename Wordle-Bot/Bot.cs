using System.Collections.ObjectModel;
using System.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace Wordle_Bot;

public class Bot : IDisposable
{
    private const string WordleLink = "https://www.powerlanguage.co.uk/wordle/";
    private const string IncorrectInputString = "Not in word list";
    private const int DefaultSleepTime = 1500;
    private bool _disposed;
    private WebDriver Driver { get; }
    
    private WordList WordsList { get; }

    public Bot(string chromeDriverPath, WordList wordsList)
    {
        var chromeOptions = new ChromeOptions();
        chromeOptions.AddArgument("--start-maximized");
        Driver = new ChromeDriver(chromeDriverPath, chromeOptions);
        WordsList = wordsList;
    }
    
    /// <summary>
    /// Enter Wordle site
    /// </summary>
    public void LoadWordle() => Driver.Navigate().GoToUrl(WordleLink);
    
    /// <summary>
    /// Play the game
    /// </summary>
    public void Run()
    {
        try
        {
            LoadWordle();
            
            // Actions builder by which to enter the words
            var actionsBuilder = new Actions(Driver);
            // The num of guesses
            int guess = 0;
            
            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(20));
            
            // Interact with the shadow DOMs to get the overlay over the game
            string getOverlayScript =
                @"return document.querySelector(""body > game-app"")." +
                @"shadowRoot.querySelector(""#game > game-modal"").shadowRoot.querySelector(""div"")";
            var overlayElement = (IWebElement)Driver.ExecuteScript(getOverlayScript);
            
            // Get the element to close the overlay and clicks it
            var closeElement = wait.Until(_ => overlayElement.FindElement(By.ClassName("close-icon")));
            closeElement.Click();

            // Get the game keyboard
            var getKeyboardScript =
                @"return document.querySelector(""body > game-app"")." +
                @"shadowRoot.querySelector(""#game > game-keyboard"").shadowRoot.querySelector(""#keyboard"")";
            var keyboard = (IWebElement)Driver.ExecuteScript(getKeyboardScript);

            // Get the main game element
            string getGameScript =
                @"return document.querySelector(""body > game-app"").shadowRoot.querySelector(""#game"")";
            var gameElement = (IWebElement)Driver.ExecuteScript(getGameScript);
            
            // Get the game board
            var board = wait.Until(_ => gameElement.FindElement(By.Id("board")));
            // Get the ingame feedback element, to intelligently interact with the game
            var toaster = wait.Until(_ => gameElement.FindElement(By.Id("game-toaster")));
            
            while (guess < 6)
            {
                // Chose new word
                var choice = WordsList.PickRandomWord();
                
                
                EnterWord(actionsBuilder, choice);

                ReadOnlyCollection<IWebElement> toasterElements;
                // Word was invalid
                while ((toasterElements = toaster.FindElements(By.XPath("*"))).Count > 0)
                {
                    // We've entered a correct input but still get a message from the game
                    // meaning we've succesfully finished the game.
                    if (toasterElements[0].GetAttribute("text") != IncorrectInputString)
                        return;
                    
                    // We've entered an incorrect word
                    // Delete the last word
                    for (int i = 0; i < 5; i++)
                        actionsBuilder.SendKeys(Keys.Backspace);
                    actionsBuilder.Perform();
                    actionsBuilder.Reset();
                    
                    
                    // Enter another word
                    choice = WordsList.PickRandomWord();
                    EnterWord(actionsBuilder, choice);
                }

                guess += 1;
                // Wait for word to update
                Thread.Sleep(500);
                // Get the last guess' row
                string getRowScript =
                    @"return document.querySelector(""body > game-app"").shadowRoot" +
                    @$".querySelector(""#board > game-row:nth-child({guess})"").shadowRoot." +
                    @"querySelector(""div"")";
                
                var guessRow = (IWebElement)Driver.ExecuteScript(getRowScript);

                // Get all letters in the row
                var letterTiles = guessRow.FindElements(By.XPath("*"));
                
                // Get the guessed word from wordle with info
                var wordResult = WordFromRow(letterTiles);
                wordResult.LettersNotIn = GetAbsentLettersFromKeyboard(keyboard);

                // Narrow the list for the next guess
                var oldCount = WordsList.GetWordsCount();
                WordsList.UpdateList(wordResult);
                Console.WriteLine($"{oldCount - WordsList.GetWordsCount()} words were eliminated");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        finally
        {
            Driver.Dispose();
        }
    }

    /// <summary>
    /// Get all <see cref="Letter"/>s not in the correct <see cref="Word"/>, from the ingame keyboard
    /// </summary>
    /// <param name="keyboardElement">The keyboard element on the screen with <see cref="Letter"/> info</param>
    /// <returns>All guessed <see cref="Letter"/> absent from the word</returns>
    private List<char> GetAbsentLettersFromKeyboard(IWebElement keyboardElement)
    {
        List<char> absentLetters = new List<char>(); 

        // Go over all rows in the ingame keyboard
        foreach (var row in keyboardElement.FindElements(By.ClassName("row")))
        {
            // Add all absent letters in the row
            absentLetters.AddRange(row.FindElements(By.TagName("button")) // All letter elements in the row
                .Where(key => key.GetAttribute("data-state") == "absent") // All absent letters
                .Select(key => key.GetAttribute("data-key")[0])); // Choose the letter char from the attribute
        }

        return absentLetters;
    }

    /// <summary>
    /// Create a <see cref="Word"/> element from a wordle guess
    /// </summary>
    /// <param name="letterTiles">The row representing the guess</param>
    /// <returns>a popluated <see cref="Word"/></returns>
    private Word WordFromRow(ReadOnlyCollection<IWebElement> letterTiles)
    {
        // Initalize an empty word which will be populated by the word which was guessed
        var word = new Word();

        for (int i = 0; i < letterTiles.Count; i++)
        {
            // The current letter of the word we're updating
            var curLetter = word.Letters[i];
            var letterTile = letterTiles[i];
            var character = letterTile.GetAttribute("letter")[0];
            // The evaluation of the letter. whether it's absent, present, or in the correct location
            // in this guess
            var eval = letterTile.GetAttribute("evaluation");

            switch (eval)
            {
                // Letter found at the incorrect place
                case "present":
                    curLetter.IndicesNotIn[i] = true;
                    curLetter.Character = character;
                    break;

                // Letter found at the incorrect place
                case "correct":
                    curLetter.IndicesIn.Add(i);
                    curLetter.Character = character;
                    break;

                default:
                    break;
            }
        }

        return word;
    }

    /// <summary>
    /// Enters a <paramref name="word"/> into wordle
    /// </summary>
    /// <param name="actionsBuilder"></param>
    /// <param name="word">The given <see cref="Word"/> to enter</param>
    /// <param name="sleepTime">The amount of time to wait after the guess</param>
    private static void EnterWord(Actions actionsBuilder, Word word, int sleepTime = 1000)
    {
        // Enter the word
        actionsBuilder.SendKeys(word.GetWordString());
        actionsBuilder.SendKeys(Keys.Enter);
        actionsBuilder.Perform();
        actionsBuilder.Reset();
        // Wait for word to register
        Thread.Sleep(sleepTime);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    private void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            // Close the driver
            Driver.Dispose();
        }

        _disposed = true;
    }
}