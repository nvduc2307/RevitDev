using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RevitDevelop.Utils.PressHelpers
{
    public static class PressHelper
    {
        public enum KEYBOARD_MSG : uint
        {
            WM_KEYDOWN = 0x100,
            WM_KEYUP = 0x101,
            K_RETURN = 0x0D
        }
        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);
        [DllImport("USER32.DLL")]
        public static extern bool PostMessage(
            IntPtr hWnd, uint msg, uint wParam, uint lParam);

        [DllImport("user32.dll")]
        private static extern uint MapVirtualKey(
            uint uCode, uint uMapType);

        public static void OneKey(IntPtr handle, uint virtualKey)
        {
            var scanCode = MapVirtualKey(virtualKey,
                (uint)MVK_MAP_TYPE.VKEY_TO_SCANCODE);
            var keyDownCode = (uint)
                              WH_KEYBOARD_LPARAM.KEYDOWN
                              | (scanCode << 16);
            var keyUpCode = (uint)
                            WH_KEYBOARD_LPARAM.KEYUP
                            | (scanCode << 16);
            PostMessage(handle,
                (uint)KEYBOARD_MSG.WM_KEYDOWN,
                virtualKey, keyDownCode);
            PostMessage(handle,
                (uint)KEYBOARD_MSG.WM_KEYUP,
                virtualKey, keyUpCode);
        }

        public static void Keys(
            IntPtr revitHandle,
            string command)
        {
            foreach (var letter in command)
            {
                OneKey(revitHandle, (uint)letter);
            }
        }

        private enum WH_KEYBOARD_LPARAM : uint
        {
            KEYDOWN = 0x00000001,
            KEYUP = 0xC0000001
        }

        private enum MVK_MAP_TYPE : uint
        {
            VKEY_TO_SCANCODE = 0,
            SCANCODE_TO_VKEY = 1,
            VKEY_TO_CHAR = 2,
            SCANCODE_TO_LR_VKEY = 3
        }
        public static bool IsEnterKeyPressed()
        {
            short keyState = GetAsyncKeyState(0x0D);
            return (keyState & 0x8000) != 0;
        }
    }
}
