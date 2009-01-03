using System;
using libtcodWrapper;
using Console = libtcodWrapper.Console;

public class Program
{
    public static void Main(String[] args)
    {
        CustomFontRequest fontReq = new CustomFontRequest("terminal.png", 8, 16, CustomFontRequestFontTypes.Grayscale);
        RootConsole.Width = 80;
        RootConsole.Height = 50;
        RootConsole.WindowTitle = "Hello World!";
        RootConsole.Fullscreen = false;
        RootConsole.Font = fontReq;

        RootConsole rootConsole = RootConsole.GetInstance();

        rootConsole.PrintLine("Hello world!", 30, 30, LineAlignment.Left);
        rootConsole.Flush();

        Keyboard.WaitForKeyPress(true);
    }
}