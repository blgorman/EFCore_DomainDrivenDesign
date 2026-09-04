namespace ConsoleHelpers;

public class InputHelpers
{
    /// <summary>
    /// Gets a double input from the user
    /// <param name="prompt">The question to ask the user</param>
    /// <param name="min">Inclusive minimum value [defaults to double.MinValue]</param>
    /// <param name="max">Inclusive maximum value [defaults to double.MaxValue]</param>
    /// <param name="confirm">Optional asks them to confirm their input</param> 
    /// <returns>The valid double in the range</returns>
    /// </summary>
    public static double GetInputAsDouble(string prompt, double min = double.MinValue
                                         , double max = double.MaxValue
                                         , bool confirm = false)
    {
        bool success = false;
        double result = double.MinValue;
        while (!success)
        {
            Console.WriteLine(prompt);
            string number = Console.ReadLine() ?? string.Empty;
            success = double.TryParse(number, out result); 
            
            if (!success)
            {
                Console.WriteLine("Please enter a valid number");
                continue;
            }

            if (result > max || result < min)
            {
                success = false;
                Console.WriteLine($"Please enter a value within defined parameters {min}, {max}");
                continue;
            }

            if (confirm)
            {
                // Console.WriteLine($"You entered {result}, is this correct? (Y/N)");
                // string confirmation = Console.ReadLine() ?? string.Empty;
                // //if they say "y" || "Y" || "yes" || "Yes" || "YES" || "YeS" || "Yellow" 
                // // we will assume they are saying we got it correct.
                // success = confirmation.StartsWith("y", StringComparison.OrdinalIgnoreCase);

                //refactor to use the new "GetInputAsBool(...)"
                string confirmationPrompt = $"You entered {result}, is this correct?";
                success = GetInputAsBool(confirmationPrompt, false);
            }
        }
        return result;
    }

    /// <summary>
    /// Gets an integer input from the user
    /// </summary>
    /// <param name="prompt">The question to ask the user</param>
    /// <param name="min">min value to allow [defaults to int.MinValue]</param>
    /// <param name="max">max value to allow [defaults to int.MaxValue]</param>
    /// <param name="confirm">Allow hook to ask for confirmation of input</param>
    /// <returns>Valid integer input from the user</returns>
    public static int GetInputAsInt(string prompt, 
                                    int min = int.MinValue, 
                                    int max = int.MaxValue, 
                                    bool confirm = false)
    {
        bool isValidInteger = false;
        int result = 0;
        
        while (!isValidInteger)
        {
            Console.WriteLine(prompt);
            string input = Console.ReadLine() ?? string.Empty;
            
            if (!int.TryParse(input, out result))
            {
                Console.WriteLine("Please enter a valid integer");
                continue;
            }
            
            if (result < min || result > max)
            {
                Console.WriteLine($"Please enter a value within defined parameters {min}, {max}");
                continue;
            }
            
            if (confirm)
            {
                string confirmationPrompt = $"You entered {result}, is this correct?";
                isValidInteger = GetInputAsBool(confirmationPrompt, false);
            }
            else
            {
                isValidInteger = true;
            }
        }
        
        return result;
    }
    
    /// <summary>
    /// Gets a boolean input from the user
    /// </summary>
    /// <param name="prompt">The question to ask the user</param>
    /// <param name="confirm">Hook to allow user to validate their choice [optional]</param>
    /// <returns>Choice of true/false from prompt</returns>
    public static bool GetInputAsBool(string prompt, 
                                        bool confirm = false)
    {
        bool startsWithN = false;
        bool startsWithY = false;
        bool success = false;
        string confirmation = string.Empty;
        while(!success)
        {
            Console.WriteLine($"{prompt}(Y/N)?");
            confirmation = Console.ReadLine() ?? string.Empty;
            
            startsWithY = confirmation.StartsWith("y", StringComparison.OrdinalIgnoreCase);
            startsWithN = confirmation.StartsWith("n", StringComparison.OrdinalIgnoreCase);

            if(startsWithY || startsWithN)
            {
                success = true;
                break;
            }
            Console.WriteLine("invalid input");
        }
        
        if(confirm)
        { 
            string confirmationPrompt = ($"You entered {confirmation} are you sure?:");
            var correctInput = GetInputAsBool(confirmationPrompt, false);

            if (!correctInput)
            {
                return GetInputAsBool(prompt, confirm);
            }
        }

        //true if they say "y" || "Y" || "yes" || "Yes" || "YES" || "YeS" || "Yellow" 
        return startsWithY;
    }
    
    /// <summary>
    /// Ask the user for string input
    /// </summary>
    /// <param name="prompt">The question you want to ask</param>
    /// <param name="confirm">Optional ability to confirm user input</param>
    /// <param name="allowEmpty">Allow empty strings [defaults to true]</param>
    /// <returns>The string they entered and potentially confirmed</returns>
    public static string GetInputAsString(string prompt, bool confirm = false, bool allowEmpty = true)
    {
        while(true)
        {
            bool correctInput = false;
            Console.WriteLine(prompt);
            string input = Console.ReadLine() ?? string.Empty;
            
            if (!allowEmpty && string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine("Input cannot be empty. Please try again.");
                continue;
            }
            
            // confirmation logic
            if (confirm)
            {
                string promptConfirm = $"You entered {input}, is this correct";
                correctInput = GetInputAsBool(promptConfirm, false);
            }

            if (correctInput || !confirm)
            {
                return input;
            }
        }
    }  

    public static void WaitForUserInput(ConsoleColor foreground)
    {
        var prevColor = Console.ForegroundColor;    
        Console.ForegroundColor = foreground;
        WaitForUserInput();
        Console.ForegroundColor = prevColor;
    }

    public static void WaitForUserInput()
    {
        Console.WriteLine("Press any key to continue...");

        // Console.ReadKey throws when input is redirected, which happens only when a
        // script drives the app. Interactive runs are unaffected.
        if (Console.IsInputRedirected) return;

        Console.ReadKey();
    }
}
