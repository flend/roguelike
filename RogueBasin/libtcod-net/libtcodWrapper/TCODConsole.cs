using System;
using System.Runtime.InteropServices;
using System.Text;

namespace libtcodWrapper
{

    internal class DLLName
    {
        /// <summary>
        /// Defines the name of the DLL we look for on disk. The runtime adds {.dll,.so} to the end
        /// </summary>
        internal const string name = @"libtcod";
    }

    /// <summary>
    /// Defines how draw operations affect background of the console.
    /// </summary>
    public class Background
    {
        internal int m_value;

        /// <summary>
        /// Background constant for no background change.
        /// </summary>
        public static readonly Background None = new Background(libtcodWrapper.BackgroundFlag.None);

        /// <summary>
        /// Create background with a given flag that does not take alpha paramater
        /// </summary>
        /// <param name="flag">Background Type</param>
        public Background(BackgroundFlag flag)
        {
            if (flag == BackgroundFlag.AddA || flag == BackgroundFlag.Alph)
                throw new Exception("Must use TCODBackagroudn constructor which takes value");
            m_value = (int)flag;
        }

        /// <summary>
        /// Create background with a given flag that does take alpha paramater
        /// </summary>
        /// <param name="flag">Background Type</param>
        /// <param name="val">Alpha Value</param>
        public Background(BackgroundFlag flag, float val)
        {
            NewBackgroundCore(flag, val);
        }

        /// <summary>
        /// Create background with a given flag that does take alpha paramater
        /// </summary>
        /// <param name="flag">Background Type</param>
        /// <param name="val">Alpha Value</param>
        public Background(BackgroundFlag flag, double val)
        {
            NewBackgroundCore(flag, (float)val);
        }

        /// <summary>
        /// Create a copy of a background flag 
        /// </summary>
        /// <param name="b">Background to copy</param>
        public Background(Background b)
        {
            m_value = b.m_value;
        }

        private void NewBackgroundCore(BackgroundFlag flag, float val)
        {
            if (flag != BackgroundFlag.AddA && flag != BackgroundFlag.Alph)
                throw new Exception("Must not use TCODBackagroudn constructor which takes value");
            m_value = (int)flag | (((byte)(val * 255)) << 8);
        }


        /// <summary>
        /// Increment background type to next background in BackgroundFlag enum
        /// </summary>
        /// <param name="lhs">Left Hand Side</param>
        /// <returns>New Background</returns>
        public static Background operator ++(Background lhs)
        {
            if (lhs.BackgroundFlag == BackgroundFlag.Alph)
                throw new Exception("Can not increment past end of BackgroundFlag enum");
            lhs.m_value += 1;
            return lhs;
        }

        /// <summary>
        /// Decrement background type to next background in BackgroundFlag enum
        /// </summary>
        /// <param name="lhs">Left Hand Side</param>
        /// <returns>New Background</returns>
        public static Background operator --(Background lhs)
        {
            if (lhs.BackgroundFlag == BackgroundFlag.None)
                throw new Exception("Can not decrement past end of BackgroundFlag enum");
            lhs.m_value -= 1;
            return lhs;
        }

        /// <summary>
        /// Get Current Background Type
        /// </summary>
        /// <returns>Background Enum</returns>
        public BackgroundFlag BackgroundFlag
        {
            get { return (BackgroundFlag)(m_value & 0xff); }
        }

        /// <summary>
        /// Get Current Alpha value
        /// </summary>
        public byte AlphaValue
        {
            get { return (byte)(m_value >> 8); }
        }
    }

    /// <summary>
    /// Request for console to draw with font other than "terminal.bmp"
    /// </summary>
    public class CustomFontRequest
    {
        /// <summary>
        /// Create new custom font request
        /// </summary>
        /// <param name="fontFile">File name to load font from</param>
        /// <param name="char_width">Pixels each character is wide</param>
        /// <param name="char_height">Pixels each character is high</param>
        /// <param name="type">Determines for custom font</param>
        public CustomFontRequest(String fontFile, int char_width, int char_height, CustomFontRequestFontTypes type)
        {
            m_fontFile = fontFile;
            m_char_width = char_width;
            m_char_height = char_height;
            m_type = type;
        }

        internal String m_fontFile;
        internal int m_char_width;
        internal int m_char_height;
        internal CustomFontRequestFontTypes m_type;

    }

    /// <summary>
    /// Represents any console, either on screen or off
    /// </summary>
    public class Console : IDisposable
    {
        internal Console(IntPtr w, int width, int height)
        {
            m_consolePtr = w;
            m_width = width;
            m_height = height;
        }

        /// <summary>
        /// Destory unmanaged console resources
        /// </summary>
        public void Dispose()
        {
            //Don't try to dispose Root Consoles
            if (m_consolePtr != IntPtr.Zero)
                TCOD_console_delete(m_consolePtr);
        }

        internal IntPtr m_consolePtr;
        internal int m_width;
        internal int m_height;

        /// <summary>
        /// Returns console's width
        /// </summary>
        /// <returns>Width</returns>
        public int GetConsoleWidth()
        {
            return m_width;
        }

        /// <summary>
        /// Returns console's height
        /// </summary>
        /// <returns>Height</returns>
        public int GetConsoleHeight()
        {
            return m_height;
        }

        /// <summary>
        /// Gets/sets the default foreground color of the console.
        /// </summary>
        public Color ForegroundColor
        {
            get { return TCOD_console_get_foreground_color(m_consolePtr); }
            set { TCOD_console_set_foreground_color(m_consolePtr, value); }
        }

        /// <summary>
        /// Gets/sets the default background color of the console.
        /// </summary>
        public Color BackgroundColor
        {
            get { return TCOD_console_get_background_color(m_consolePtr); }
            set { TCOD_console_set_background_color(m_consolePtr, value); }
        }

        /// <summary>
        /// Clear the console by setting each cell to default background, foreground color, and ascii value to ' '
        /// </summary>
        public void Clear()
        {
            TCOD_console_clear(m_consolePtr);
        }

        /// <summary>
        /// Put ascii character onto console
        /// </summary>
        /// <param name="x">x (Width) position</param>
        /// <param name="y">y (Height) position</param>
        /// <param name="c">Ascii character</param>
        /// <param name="flag">Background flag</param>
        public void PutChar(int x, int y, char c, Background flag)
        {
            TCOD_console_put_char(m_consolePtr, x, y, (int)c, flag.m_value);
        }

        /// <summary>
        /// Put ascii character onto console
        /// </summary>
        /// <param name="x">x (Width) position</param>
        /// <param name="y">y (Height) position</param>
        /// <param name="c">SpecialCharacter or ascii byte</param>
        /// <param name="flag">Background flag</param>
        public void PutChar(int x, int y, byte c, Background flag)
        {
            TCOD_console_put_char(m_consolePtr, x, y, c, flag.m_value);
        }

        /// <summary>
        /// Put ascii character onto console
        /// </summary>
        /// <param name="x">x (Width) position</param>
        /// <param name="y">y (Height) position</param>
        /// <param name="c">Ascii character</param>
        public void PutChar(int x, int y, char c)
        {
            TCOD_console_put_char(m_consolePtr, x, y, (int)c, (int)BackgroundFlag.Set);
        }

        /// <summary>
        /// Put ascii character onto console
        /// </summary>
        /// <param name="x">x (Width) position</param>
        /// <param name="y">y (Height) position</param>
        /// <param name="c">Ascii character</param>
        public void PutChar(int x, int y, byte c)
        {
            TCOD_console_put_char(m_consolePtr, x, y, c, (int)BackgroundFlag.Set);
        }

        /// <summary>
        /// Set background color of single cell
        /// </summary>
        /// <param name="x">x (Width) position</param>
        /// <param name="y">y (Height) position</param>
        /// <param name="col">Background color</param>
        /// <param name="flag">Background flag</param>
        public void SetCharBackground(int x, int y, Color col, Background flag)
        {
            TCOD_console_set_back(m_consolePtr, x, y, col, flag.m_value);
        }

        /// <summary>
        /// Set background color of single cell
        /// </summary>
        /// <param name="x">x (Width) position</param>
        /// <param name="y">y (Height) position</param>
        /// <param name="col">Background color</param>
        public void SetCharBackground(int x, int y, Color col)
        {
            SetCharBackground(x, y, col, new Background(BackgroundFlag.Set));
        }

        /// <summary>
        /// Set foreground color of single cell
        /// </summary>
        /// <param name="x">x (Width) position</param>
        /// <param name="y">y (Height) position</param>
        /// <param name="col">Foreground color</param>
        public void SetCharForeground(int x, int y, Color col)
        {
            TCOD_console_set_fore(m_consolePtr, x, y, col);
        }

        /// <summary>
        /// Get Background of single cell
        /// </summary>
        /// <param name="x">x (Width) position</param>
        /// <param name="y">y (Height) position</param>
        /// <returns>Background color</returns>
        public Color GetCharBackground(int x, int y)
        {
            return TCOD_console_get_back(m_consolePtr, x, y);
        }

        /// <summary>
        /// Get Forground of single cell
        /// </summary>
        /// <param name="x">x (Width) position</param>
        /// <param name="y">y (Height) position</param>
        /// <returns>Forground color</returns>
        public Color GetCharForeground(int x, int y)
        {
            return TCOD_console_get_fore(m_consolePtr, x, y);
        }

        /// <summary>
        /// Get ascii value of single cell
        /// </summary>
        /// <param name="x">x (Width) position</param>
        /// <param name="y">y (Height) position</param>
        /// <returns>Ascii value</returns>
        public char GetChar(int x, int y)
        {
            return (char)TCOD_console_get_char(m_consolePtr, x, y);
        }

        /// <summary>
        /// Print string to line of console, using default foreground/background colors
        /// </summary>
        /// <param name="str">String to print</param>
        /// <param name="x">x (Width) position of first character</param>
        /// <param name="y">y (Height) position of first character</param>
        /// <param name="align">Alignment of string</param>
        public void PrintLine(string str, int x, int y, LineAlignment align)
        {
            PrintLine(str, x, y, new Background(BackgroundFlag.Set), align);
        }

        /// <summary>
        /// Print string to line of console, using default foreground/background colors
        /// </summary>
        /// <param name="str">String to print</param>
        /// <param name="x">x (Width) position of first character</param>
        /// <param name="y">y (Height) position of first character</param>
        /// <param name="flag">Background flag</param>
        /// <param name="align">Alignment of string</param>
        public void PrintLine(string str, int x, int y, Background flag, LineAlignment align)
        {
            switch (align)
            {
                case LineAlignment.Left:
                    TCOD_console_print_left(m_consolePtr, x, y, flag.m_value, new StringBuilder(str));
                    break;
                case LineAlignment.Center:
                    TCOD_console_print_center(m_consolePtr, x, y, flag.m_value, new StringBuilder(str));
                    break;
                case LineAlignment.Right:
                    TCOD_console_print_right(m_consolePtr, x, y, flag.m_value, new StringBuilder(str));
                    break;
            }
        }

        /// <summary>
        /// Print aligned string inside the defined rectangle, truncating if bottom is reached
        /// </summary>
        /// <param name="str">String to print</param>
        /// <param name="x">x (Width) position of first character</param>
        /// <param name="y">y (Height) position of first character</param>
        /// <param name="w">Width of rectangle to print in</param>
        /// <param name="h">Height of rectangle to print in. If 0, string is only truncated if reaches bottom of console.</param>
        /// <param name="align">Alignment of string</param>
        /// <returns>Number of lines printed</returns>
        public int PrintLineRect(string str, int x, int y, int w, int h, LineAlignment align)
        {
            return PrintLineRect(str, x, y, w, h, new Background(BackgroundFlag.Set), align);
        }

        /// <summary>
        /// Print aligned string inside the defined rectangle, truncating if bottom is reached
        /// </summary>
        /// <param name="str">String to print</param>
        /// <param name="x">x (Width) position of first character</param>
        /// <param name="y">y (Height) position of first character</param>
        /// <param name="w">Width of rectangle to print in</param>
        /// <param name="h">Height of rectangle to print in. If 0, string is only truncated if reaches bottom of console.</param>
        /// <param name="flag">Background flag</param>
        /// <param name="align">Alignment of string</param>
        /// <returns>Number of lines printed</returns>
        public int PrintLineRect(string str, int x, int y, int w, int h, Background flag, LineAlignment align)
        {
            switch (align)
            {
                case LineAlignment.Left:
                    return TCOD_console_print_left_rect(m_consolePtr, x, y, w, h, flag.m_value, new StringBuilder(str));
                case LineAlignment.Center:
                    return TCOD_console_print_center_rect(m_consolePtr, x, y, w, h, flag.m_value, new StringBuilder(str));
                case LineAlignment.Right:
                    return TCOD_console_print_right_rect(m_consolePtr, x, y, w, h, flag.m_value, new StringBuilder(str));
                default:
                    throw new Exception("Must Pass Alignment to PrintLineRect");
            }
        }

        /// <summary>
        /// Blit console onto another console
        /// </summary>
        /// <param name="xSrc">Upper left corner x coord of area to blit from</param>
        /// <param name="ySrc">Upper left corner y coord of area to blit from</param>
        /// <param name="wSrc">Width of source area</param>
        /// <param name="hSrc">Height of source area</param>
        /// <param name="dest">Destination console</param>
        /// <param name="xDst">Upper left corner x coord of area to blit to</param>
        /// <param name="yDst">Upper left corner y coord of area to blit to</param>
        public void Blit(int xSrc, int ySrc, int wSrc, int hSrc, Console dest, int xDst, int yDst)
        {
            Blit(xSrc, ySrc, wSrc, hSrc, dest, xDst, yDst, 255);
        }

        /// <summary>
        /// Blit console onto another console
        /// </summary>
        /// <param name="xSrc">Upper left corner x coord of area to blit from</param>
        /// <param name="ySrc">Upper left corner y coord of area to blit from</param>
        /// <param name="wSrc">Width of source area</param>
        /// <param name="hSrc">Height of source area</param>
        /// <param name="dest">Destination console</param>
        /// <param name="xDst">Upper left corner x coord of area to blit to</param>
        /// <param name="yDst">Upper left corner y coord of area to blit to</param>
        /// <param name="fade">Transparency of blitted console. 255 = fully replace destination. (0-254) simulate real transparency with varying degrees of fading.</param>
        public void Blit(int xSrc, int ySrc, int wSrc, int hSrc, Console dest, int xDst, int yDst, int fade)
        {
            TCOD_console_blit(m_consolePtr, xSrc, ySrc, wSrc, hSrc, dest.m_consolePtr, xDst, yDst, fade);
        }

        /// <summary>
        /// Draw rectangle of color to console, setting background color to default
        /// </summary>
        /// <param name="x">Upper left corner x coord</param>
        /// <param name="y">Upper left corner y coord</param>
        /// <param name="w">Width of rectangle</param>
        /// <param name="h">Height of rectangle</param>
        /// <param name="clear">Clear cells of any ascii character</param>
        /// <param name="flag">Background flag</param>
        public void DrawRect(int x, int y, int w, int h, bool clear, Background flag)
        {
            TCOD_console_rect(m_consolePtr, x, y, w, h, clear, flag.m_value);
        }

        /// <summary>
        /// Draw rectangle of color to console, setting background color to default
        /// </summary>
        /// <param name="x">Upper left corner x coord</param>
        /// <param name="y">Upper left corner y coord</param>
        /// <param name="w">Width of rectangle</param>
        /// <param name="h">Height of rectangle</param>
        /// <param name="clear">Clear cells of any ascii character</param>
        public void DrawRect(int x, int y, int w, int h, bool clear)
        {
            DrawRect(x, y, w, h, clear, new Background(BackgroundFlag.Set));
        }

        /// <summary>
        /// Draw horizontal line using default background/foreground color
        /// </summary>
        /// <param name="x">Left endpoint x coord</param>
        /// <param name="y">Left endpoint y coord</param>
        /// <param name="l">Length</param>
        public void DrawHLine(int x, int y, int l)
        {
            TCOD_console_hline(m_consolePtr, x, y, l);
        }

        /// <summary>
        /// Draw vertical line using default background/foreground color
        /// </summary>
        /// <param name="x">Upper endpoint x coord</param>
        /// <param name="y">Upper endpoint y coord</param>
        /// <param name="l">Length</param>
        public void DrawVLine(int x, int y, int l)
        {
            TCOD_console_vline(m_consolePtr, x, y, l);
        }

        /// <summary>
        /// Draw "Frame" with title onto console
        /// </summary>
        /// <param name="x">Upper left corner x coord</param>
        /// <param name="y">Upper left corner y coord</param>
        /// <param name="w">Width</param>
        /// <param name="h">Height</param>
        /// <param name="clear">Clear area</param>
        /// <param name="str">Title of frame</param>
        public void DrawFrame(int x, int y, int w, int h, bool clear, String str)
        {
            TCOD_console_print_frame(m_consolePtr, x, y, w, h, clear, new StringBuilder(str));
        }

        /// <summary>
        /// Draw "Frame" with title onto console
        /// </summary>
        /// <param name="x">Upper left corner x coord</param>
        /// <param name="y">Upper left corner y coord</param>
        /// <param name="w">Width</param>
        /// <param name="h">Height</param>
        /// <param name="clear">Clear area</param>
        public void DrawFrame(int x, int y, int w, int h, bool clear)
        {
            TCOD_console_print_frame(m_consolePtr, x, y, w, h, clear, IntPtr.Zero);
        }

        /// <summary>
        /// Print a "Powered by libtcod x.y.z" screen. Skippable by pressing any key.
        /// </summary>
        public void ConsoleCredits()
        {
            TCOD_console_credits();
        }

        /// <summary>
        /// Render a frame of "Powered by libtcod x.y.z" onto screen.
        /// </summary>
        /// <param name="x">X Position of credits</param>
        /// <param name="y">Y Position of credits</param>
        /// <param name="alpha">If true, credits are transparently added on top of the existing screen. For this to work, this function must be placed between your screen rendering code and the console flush.</param>
        /// <returns>true when the credits screen is finished, indicating that you no longer need to call it.</returns>
        public bool ConsoleCreditsRender(int x, int y, bool alpha)
        {
            return TCOD_console_credits_render(x, y, alpha);
        }


        #region DLLImports

        /* Printing shapes to console */

        [DllImport(DLLName.name)]
        private extern static void TCOD_console_rect(IntPtr con, int x, int y, int w, int h, bool clear, /*BackgroundFlag*/ int flag);

        [DllImport(DLLName.name)]
        private extern static void TCOD_console_hline(IntPtr con, int x, int y, int l);

        [DllImport(DLLName.name)]
        private extern static void TCOD_console_vline(IntPtr con, int x, int y, int l);

        [DllImport(DLLName.name)]
        private extern static void TCOD_console_print_frame(IntPtr con, int x, int y, int w, int h, bool clear, StringBuilder str);

        [DllImport(DLLName.name)]
        private extern static void TCOD_console_print_frame(IntPtr con, int x, int y, int w, int h, bool clear, IntPtr nullStr);

        [DllImport(DLLName.name)]
        private extern static void TCOD_console_blit(IntPtr src, int xSrc, int ySrc, int wSrc, int hSrc, IntPtr dst, int xDst, int yDst, int fade);

        /* Prints Strings to Screen */

        [DllImport(DLLName.name)]
        private extern static void TCOD_console_print_left(IntPtr con, int x, int y, /*BackgroundFlag*/ int flag, StringBuilder str);

        [DllImport(DLLName.name)]
        private extern static void TCOD_console_print_right(IntPtr con, int x, int y, /*BackgroundFlag*/ int flag, StringBuilder str);

        [DllImport(DLLName.name)]
        private extern static void TCOD_console_print_center(IntPtr con, int x, int y, /*BackgroundFlag*/ int flag, StringBuilder str);

        //Returns height of the printed string
        [DllImport(DLLName.name)]
        private extern static int TCOD_console_print_left_rect(IntPtr con, int x, int y, int w, int h, /*BackgroundFlag*/ int flag, StringBuilder str);

        //Returns height of the printed string
        [DllImport(DLLName.name)]
        private extern static int TCOD_console_print_right_rect(IntPtr con, int x, int y, int w, int h, /*BackgroundFlag*/ int flag, StringBuilder str);

        //Returns height of the printed string
        [DllImport(DLLName.name)]
        private extern static int TCOD_console_print_center_rect(IntPtr con, int x, int y, int w, int h, /*BackgroundFlag*/ int flag, StringBuilder str);



        [DllImport(DLLName.name)]
        private extern static void TCOD_console_set_background_color(IntPtr con, Color back);

        [DllImport(DLLName.name)]
        private extern static void TCOD_console_set_foreground_color(IntPtr con, Color back);

        [DllImport(DLLName.name)]
        private extern static void TCOD_console_clear(IntPtr con);

        /* Single Character Manipulation */

        [DllImport(DLLName.name)]
        private extern static void TCOD_console_put_char(IntPtr con, int x, int y, int c, /*BackgroundFlag*/ int flag);

        [DllImport(DLLName.name)]
        private extern static void TCOD_console_set_back(IntPtr con, int x, int y, Color col, /*BackgroundFlag*/ int flag);

        [DllImport(DLLName.name)]
        private extern static void TCOD_console_set_fore(IntPtr con, int x, int y, Color col);

        [DllImport(DLLName.name)]
        private extern static void TCOD_console_set_char(IntPtr con, int x, int y, int c);


        /* Get things from console */

        [DllImport(DLLName.name)]
        private extern static Color TCOD_console_get_background_color(IntPtr con);

        [DllImport(DLLName.name)]
        private extern static Color TCOD_console_get_foreground_color(IntPtr con);

        [DllImport(DLLName.name)]
        private extern static Color TCOD_console_get_back(IntPtr con, int x, int y);

        [DllImport(DLLName.name)]
        private extern static Color TCOD_console_get_fore(IntPtr con, int x, int y);

        [DllImport(DLLName.name)]
        private extern static int TCOD_console_get_char(IntPtr con, int x, int y);

        [DllImport(DLLName.name)]
        private extern static void TCOD_console_delete(IntPtr console);

        /* Credits */

        [DllImport(DLLName.name)]
        private static extern void TCOD_console_credits();

        [DllImport(DLLName.name)]
        private static extern bool TCOD_console_credits_render(int x, int y, bool alpha);
        #endregion
    }

    /// <summary>
    /// "Root" console, one which blits onto window or fullscreen
    /// </summary>
    public class RootConsole : Console
    {
        /// <summary>
        /// Create the root console with the default font
        /// </summary>
        /// <param name="w">Width in characters</param>
        /// <param name="h">Height in characters</param>
        /// <param name="title">Title of window</param>
        /// <param name="fullscreen">Fullscreen?</param>
        private RootConsole(int w, int h, String title, bool fullscreen)
            : base(IntPtr.Zero, w, h)
        {
            TCOD_console_init_root(w, h, new StringBuilder(title), fullscreen);
        }

        /// <summary>
        /// Create the root console with custom font
        /// </summary>
        /// <param name="w">Width in characters</param>
        /// <param name="h">Height in characters</param>
        /// <param name="title">Title of window</param>
        /// <param name="fullscreen">Fullscreen?</param>
        /// <param name="font">Custom font request</param>
        private RootConsole(int w, int h, String title, bool fullscreen, CustomFontRequest font)
            : base(IntPtr.Zero, w, h)
        {
            TCOD_console_set_custom_font(new StringBuilder(font.m_fontFile), font.m_char_width,
                font.m_char_height, (int)font.m_type);
            TCOD_console_init_root(w, h, new StringBuilder(title), fullscreen);
        }

        /// <summary>
        /// Has the window been closed by the user
        /// </summary>
        /// <returns>Is Window Closed?</returns>
        public bool IsWindowClosed()
        {
            return TCOD_console_is_window_closed();
        }

        /// <summary>
        /// "Flush" console by rendering new frame
        /// </summary>
        public void Flush()
        {
            TCOD_console_flush();
        }

        /// <summary>
        /// Fade console to specified color
        /// </summary>
        /// <param name="fade">Fading amount (0 {fully faded} - 255 {no fade} )</param>
        /// <param name="fadingColor">Color to fade to</param>
        public void SetFade(byte fade, Color fadingColor)
        {
            TCOD_console_set_fade(fade, fadingColor);
        }

        /// <summary>
        /// Get current fade level
        /// </summary>
        /// <returns>Fading amount (0 {fully faded} - 255 {no fade} )</returns>
        public byte GetFadeLevel()
        {
            return TCOD_console_get_fade();
        }

        /// <summary>
        /// Get current fade color
        /// </summary>
        /// <returns>Fade Color</returns>
        public Color GetFadeColor()
        {
            return TCOD_console_get_fading_color();
        }

        /// <summary>
        /// Set console full screen status
        /// </summary>
        /// <param name="fullScreen">Fullscreen?</param>
        public void SetFullscreen(bool fullScreen)
        {
            TCOD_console_set_fullscreen(fullScreen);
        }

        /// <summary>
        /// Is console currently fullscreen
        /// </summary>
        /// <returns>Fullscreen?</returns>
        public bool IsFullscreen()
        {
            return TCOD_console_is_fullscreen();
        }

        /// <summary>
        /// Set title once console is created
        /// </summary>
        /// <param name="title">Title</param>
        private void SetTitle(string title)
        {
            TCOD_console_set_window_title(new StringBuilder(title));
        }

        /// <summary>
        /// Create new offscreen (secondary) console 
        /// </summary>
        /// <param name="w">Width in characters</param>
        /// <param name="h">Height in characters</param>
        /// <returns>New console</returns>
        public static Console GetNewConsole(int w, int h)
        {
            return new Console(TCOD_console_new(w, h), w, h);
        }

        #region Singleton Constructor Stuff

        private static RootConsole singletonInstance = null;

        private static Int32? width = null;
        /// <summary>
        /// Width, in tiles, of the root console window. Attempting to reset
        /// this once GetInstance() has been called will result in an exception.
        /// </summary>
        public static Int32 Width
        {
            get
            {
                if (width == null)
                    throw new TCODException("RootConsole.Width has not been set.");
                else
                    return (Int32)width;
            }
            set
            {
                if (singletonInstance != null)
                    throw new TCODException("RootConsole behavior members " +
                            "cannot be changed once GetInstance() has been called.");
                width = value;
            }
        }

        private static Int32? height = null;
        /// <summary>
        /// Height, in tiles, of the root console window. Attempting to reset
        /// this once GetInstance() has been called will result in an exception.
        /// </summary>
        public static Int32 Height
        {
            get
            {
                if (height == null)
                    throw new TCODException("RootConsole.Height has not been set.");
                else
                    return (Int32)height;
            }
            set
            {
                if (singletonInstance != null)
                    throw new TCODException("RootConsole behavior members " +
                            "cannot be changed once GetInstance() has been called.");
                height = value;
            }
        }

        private static String windowTitle = null;
        /// <summary>
        /// Title for the root console window. Attempting to reset
        /// this once GetInstance() has been called will result in an exception.
        /// </summary>
        public static String WindowTitle
        {
            get
            {
                if (windowTitle == null)
                    throw new TCODException("RootConsole.WindowTitle has not been set.");
                else
                    return windowTitle;
            }
            set
            {
                if (singletonInstance != null)
                    singletonInstance.SetTitle(value);
                windowTitle = value;
            }
        }

        private static Boolean? isFullscreen = null;
        /// <summary>
        /// Whether or not to run the application full-screen. Attempting to reset
        /// this once GetInstance() has been called will result in an exception.
        /// </summary>
        public static Boolean Fullscreen
        {
            get
            {
                if (isFullscreen == null)
                    throw new TCODException("RootConsole.Fullscreen has not been set.");
                else
                    return (Boolean)isFullscreen;
            }
            set
            {
                if (singletonInstance != null)
                    singletonInstance.SetFullscreen(value);
                isFullscreen = value;
            }
        }

        private static CustomFontRequest font = null;
        /// <summary>
        /// Font data for a font other than the default "terminal.bmp".
        /// </summary>
        public static CustomFontRequest Font
        {
            get
            {
                return font;
            }
            set
            {
                if (singletonInstance != null)
                    throw new TCODException("RootConsole behavior members " +
                            "cannot be changed once GetInstance() has been called.");
                font = value;
            }
        }

        /// <summary>
        /// Modified singleton pattern. Creates the root console based on
        /// the behavior values in RootConsole.Width, RootConsole.Height,
        /// RootConsole.WindowTitle, RootConsole.Fullscreen, and
        /// RootConsole.Font, or gets the already-created console if one
        /// exists.
        /// </summary>
        /// <returns>
        ///    The root console object.
        /// </returns>
        public static RootConsole GetInstance()
        {
            if (singletonInstance == null)
            {
                if (width == null)
                    throw new TCODException("RootConsole.Width is not set.");
                if (height == null)
                    throw new TCODException("RootConsole.Height is not set.");
                if (windowTitle == null)
                    throw new TCODException("RootConsole.WindowTitle is not set.");
                if (isFullscreen == null)
                    throw new TCODException("RootConsole.Fullscreen is not set.");

                if (font == null)
                    singletonInstance = new RootConsole((Int32)width, (Int32)height,
                        windowTitle, (Boolean)isFullscreen);
                else
                    singletonInstance = new RootConsole((Int32)width, (Int32)height,
                        windowTitle, (Boolean)isFullscreen, font);
            }

            return singletonInstance;
        }

        #endregion

        #region DLLImports

        [DllImport(DLLName.name)]
        private extern static void TCOD_console_set_window_title(StringBuilder title);

        [DllImport(DLLName.name)]
        private extern static void TCOD_console_init_root(int w, int h, StringBuilder title, bool fullscreen);

        [DllImport(DLLName.name)]
        private extern static void TCOD_console_set_custom_font(StringBuilder fontFile, int char_width, int char_height, int flags);

        [DllImport(DLLName.name)]
        private extern static bool TCOD_console_is_window_closed();

        [DllImport(DLLName.name)]
        private extern static void TCOD_console_flush();


        /* Fading */

        [DllImport(DLLName.name)]
        private extern static void TCOD_console_set_fade(byte fade, Color fadingColor);

        [DllImport(DLLName.name)]
        private extern static byte TCOD_console_get_fade();

        [DllImport(DLLName.name)]
        private extern static Color TCOD_console_get_fading_color();


        /* Fullscreen */

        [DllImport(DLLName.name)]
        private extern static bool TCOD_console_is_fullscreen();

        [DllImport(DLLName.name)]
        private extern static void TCOD_console_set_fullscreen(bool fullscreen);

        /* Offscreen console */

        [DllImport(DLLName.name)]
        private extern static IntPtr TCOD_console_new(int w, int h);



        #endregion
    }

}
