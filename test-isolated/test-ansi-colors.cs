using System;

class TestAnsiColors
{
    static void Main()
    {
        Console.WriteLine("Testing ANSI color support:");
        Console.WriteLine();
        
        // Test foreground colors
        Console.WriteLine("Foreground colors:");
        Console.Write("\x1b[30mBlack\x1b[0m ");
        Console.Write("\x1b[31mRed\x1b[0m ");
        Console.Write("\x1b[32mGreen\x1b[0m ");
        Console.Write("\x1b[33mYellow\x1b[0m ");
        Console.Write("\x1b[34mBlue\x1b[0m ");
        Console.Write("\x1b[35mMagenta\x1b[0m ");
        Console.Write("\x1b[36mCyan\x1b[0m ");
        Console.Write("\x1b[37mWhite\x1b[0m");
        Console.WriteLine();
        Console.WriteLine();
        
        // Test background colors
        Console.WriteLine("Background colors:");
        Console.Write("\x1b[40mBlack\x1b[0m ");
        Console.Write("\x1b[41mRed\x1b[0m ");
        Console.Write("\x1b[42mGreen\x1b[0m ");
        Console.Write("\x1b[43mYellow\x1b[0m ");
        Console.Write("\x1b[44mBlue\x1b[0m ");
        Console.Write("\x1b[45mMagenta\x1b[0m ");
        Console.Write("\x1b[46mCyan\x1b[0m ");
        Console.Write("\x1b[47mWhite\x1b[0m");
        Console.WriteLine();
        Console.WriteLine();
        
        // Test bright background colors
        Console.WriteLine("Bright background colors:");
        Console.Write("\x1b[100mBright Black\x1b[0m ");
        Console.Write("\x1b[101mBright Red\x1b[0m ");
        Console.Write("\x1b[102mBright Green\x1b[0m ");
        Console.Write("\x1b[103mBright Yellow\x1b[0m ");
        Console.Write("\x1b[104mBright Blue\x1b[0m ");
        Console.Write("\x1b[105mBright Magenta\x1b[0m ");
        Console.Write("\x1b[106mBright Cyan\x1b[0m ");
        Console.Write("\x1b[107mBright White\x1b[0m");
        Console.WriteLine();
        Console.WriteLine();
        
        // Test combination
        Console.WriteLine("White text on cyan background:");
        Console.WriteLine("\x1b[37;46mThis should be white text on cyan background\x1b[0m");
        Console.WriteLine();
        
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}