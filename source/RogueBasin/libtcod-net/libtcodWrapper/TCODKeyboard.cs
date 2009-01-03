using System;
using System.Runtime.InteropServices;
using System.Text;

namespace libtcodWrapper
{
    /// <summary>
    /// Keystroke structure returned from Keyboard methods.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct KeyPress
    {
        private KeyCode keyCode;
        /// <summary>
        /// Key code for the current button press.
        /// </summary>
        public KeyCode KeyCode
        {
            get { return keyCode; }
        }
        
        private byte character;
        /// <summary>
        /// The textual character of this keypress. Set iff
        /// KeyCode == KeyCode.TCODK_CHAR, otherwise zero.
        /// </summary>
        public byte Character
        {
            get { return character; }
        }
        
        //This field is set by libtcod when struct is marshalled. Disable the incorrect warning. 
        #pragma warning disable 0649
#if Linux
        private byte modifiers;
#else
        private int modifiers;
#endif
        #pragma warning restore 0649

        /// <summary>
        /// Any key has been pressed.
        /// </summary>
        public bool Pressed
        {
            get { return ((modifiers & 0x01) > 0); }
        }
        /// <summary>
        /// Modified by the press of either alt key.
        /// </summary>
        public bool Alt
        {
            get { return LeftAlt || RightAlt; }
        }
        /// <summary>
        /// Modified by the press of either control key.
        /// </summary>
        public bool Control
        {
            get { return LeftControl || RightControl; }
        }
        /// <summary>
        /// Modified by the press of the left alt key.
        /// </summary>
        public bool LeftAlt
        {
            get { return ((modifiers & 0x02) > 0); }
        }
        /// <summary>
        /// Modified by the press of the left control key.
        /// </summary>
        public bool LeftControl
        {
            get { return ((modifiers & 0x04) > 0); }
        }
        /// <summary>
        /// Modified by the press of the rightt alt key.
        /// </summary>
        public bool RightAlt
        {
            get { return ((modifiers & 0x8) > 0); }
        }
        /// <summary>
        /// Modified by the press of the right control key.
        /// </summary>
        public bool RightControl
        {
            get { return ((modifiers & 0x10) > 0); }
        }
        /// <summary>
        /// Modified by the press of either shift key.
        /// </summary>
        public bool Shift
        {
            get { return ((modifiers & 0x20) > 0); }
        }
    }

    #pragma warning restore 1591  //Disable warning about lack of xml comments

    /// <summary>
    /// Holds static methods for interacting with keyboard
    /// </summary>
    public static class Keyboard
    {
        /// <summary>
        /// Block until user presses key
        /// </summary>
        /// <param name="flushInputBuffer">Flush all outstanding keystrokes and wait for next stroke</param>
        /// <returns>Keypress</returns>
        public static KeyPress WaitForKeyPress(bool flushInputBuffer)
        {
            return TCOD_console_wait_for_keypress(flushInputBuffer);
        }

        /// <summary>
        /// Non-blockingly check for user key press
        /// </summary>
        /// <param name="pressFlags">Determines what type of events are returned</param>
        /// <returns>Keypress</returns>
        public static KeyPress CheckForKeypress(KeyPressType pressFlags)
        {
            return TCOD_console_check_for_keypress(pressFlags);
        }

        /// <summary>
        /// Determine if a given key is currently being pressed
        /// </summary>
        /// <param name="key">Key in question</param>
        /// <returns>Is Key Pressed?</returns>
        public static bool IsKeyPressed(KeyCode key)
        {
            return TCOD_console_is_key_pressed(key);
        }

        /// <summary>
        /// Set repeat rate of keyboard
        /// </summary>
        /// <param name="initialDelay">How long before repeating</param>
        /// <param name="interval">How often afterwards to repeat</param>
        public static void SetRepeat(int initialDelay, int interval)
        {
            TCOD_console_set_keyboard_repeat(initialDelay, interval);
        }

        /// <summary>
        /// Disable all repeating of keystrokes
        /// </summary>
        public static void DisableRepeat()
        {
            TCOD_console_disable_keyboard_repeat();
        }

        #region DllImport
        [DllImport(DLLName.name)]
        private extern static KeyPress TCOD_console_wait_for_keypress(bool flush);

        [DllImport(DLLName.name)]
        private extern static KeyPress TCOD_console_check_for_keypress(KeyPressType flags);

        [DllImport(DLLName.name)]
        private extern static bool TCOD_console_is_key_pressed(KeyCode key);

        [DllImport(DLLName.name)]
        private extern static void TCOD_console_set_keyboard_repeat(int initial_delay, int interval);

        [DllImport(DLLName.name)]
        private extern static void TCOD_console_disable_keyboard_repeat();
        #endregion
    }
}
