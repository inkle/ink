using System;

namespace Ink
{
    public enum ConsoleColour {
        None,
        Red,
        Green,
        Blue
    }

    public static class ColourConsole
    {
        public static void WriteLine(string msg, ConsoleColour colour)
        {
            SetConsoleTextColour (colour);
            Console.WriteLine (msg);
            ResetConsoleTextColour ();
        }

        public static void Write(string msg, ConsoleColour colour)
        {
            SetConsoleTextColour (colour);
            Console.Write (msg);
            ResetConsoleTextColour ();
        }

        public static void SetConsoleTextColour(ConsoleColour colour)
        {
            // ANSI colour codes:
            // http://stackoverflow.com/questions/2353430/how-can-i-print-to-the-console-in-color-in-a-cross-platform-manner
            const char escapeChar = (char)27;
            switch (colour) {
            case ConsoleColour.Red:
                Console.Write ("{0}[1;31m", escapeChar);
                break;
            case ConsoleColour.Green:
                Console.Write ("{0}[1;32m", escapeChar);
                break;
            case ConsoleColour.Blue:
                Console.Write ("{0}[1;34m", escapeChar);
                break;
            case ConsoleColour.None:
                Console.Write ("{0}[0m", escapeChar);
                break;
            }
        }

        public static void ResetConsoleTextColour()
        {
            SetConsoleTextColour (ConsoleColour.None);
        }
    }


}

