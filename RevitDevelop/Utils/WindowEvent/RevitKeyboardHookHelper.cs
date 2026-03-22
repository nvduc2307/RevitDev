using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RevitDevelop.Utils.WindowEvent
{
    public class RevitKeyboardHookHelper
    {
        #region Windows API Imports
        private const int WH_KEYBOARD = 2;
        private const int HC_ACTION = 0;
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        private static extern uint GetCurrentThreadId();
        private delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);
        #endregion
        private IntPtr _hookID = IntPtr.Zero;
        private HookProc _proc;
        // Khai báo 2 Action để chứa hàm Undo và Redo từ UI truyền vào
        private System.Action _onUndoAction;
        private System.Action _onRedoAction;
        /// <summary>
        /// Khởi tạo Keyboard Hook bắt Ctrl+Z và Ctrl+Y
        /// </summary>
        public RevitKeyboardHookHelper(System.Action onUndoAction, System.Action onRedoAction)
        {
            _onUndoAction = onUndoAction;
            _onRedoAction = onRedoAction;
            _proc = HookCallback;
        }
        public void Start()
        {
            if (_hookID == IntPtr.Zero)
            {
                _hookID = SetWindowsHookEx(WH_KEYBOARD, _proc, IntPtr.Zero, GetCurrentThreadId());
            }
        }
        public void Stop()
        {
            if (_hookID != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hookID);
                _hookID = IntPtr.Zero;
            }
        }
        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            try
            {
                if (nCode == HC_ACTION)
                {
                    int virtualKeyCode = wParam.ToInt32();

                    // Kiểm tra xem phím gõ có phải là Z (0x5A) hoặc Y (0x59) không
                    if (virtualKeyCode == 0x5A || virtualKeyCode == 0x59)
                    {
                        long lParamValue = lParam.ToInt64();
                        bool isKeyDown = ((lParamValue & (1L << 31)) == 0);
                        bool isPreviousStateDown = ((lParamValue & (1L << 30)) != 0);

                        // Nếu phím vừa được ấn xuống
                        if (isKeyDown && !isPreviousStateDown)
                        {
                            // Kiểm tra xem có giữ Ctrl không
                            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                            {
                                // Phân loại: Nếu là phím Z thì gọi Undo, phím Y thì gọi Redo
                                if (virtualKeyCode == 0x5A)
                                {
                                    _onUndoAction?.Invoke(); //phím Z thì gọi Undo
                                }
                                else if (virtualKeyCode == 0x59)
                                {
                                    _onRedoAction?.Invoke(); //phím Y thì gọi Redo
                                }

                                // Nuốt sự kiện, không trả về cho Revit
                                return new IntPtr(1);
                            }
                        }
                    }
                }

                return CallNextHookEx(_hookID, nCode, wParam, lParam);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public void Dispose()
        {
            Stop();
        }
    }
}
