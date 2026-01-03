using APMCounter.Model;
using APMCounter.Util;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace APMCounter.Service
{
    internal class ActionService
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WH_MOUSE_LL = 14;
        private const int WM_KEYDOWN = 0x0100;
        private LowLevelKeyboardProc _proc;
        public IntPtr _hookID = IntPtr.Zero;
        private LowLevelKeyboardProc _procMouse;
        public IntPtr _hookIDMouse = IntPtr.Zero;
        private ActionBucket _actionBucket;

        private ActionService(ActionBucket actionBucket)
        {
            _actionBucket = actionBucket;
            _proc = HookCallback;
            _procMouse = HookCallbackMouse;
            _hookID = SetHook(_proc);
            _hookIDMouse = SetHookMouse(_procMouse);
        }

        public static ActionService Start(ActionBucket actionBucket)
        {
            return new ActionService(actionBucket);
        }

        public static void End(IntPtr _hookID)
        {
            UnhookWindowsHookEx(_hookID);
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private IntPtr SetHookMouse(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_MOUSE_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private IntPtr HookCallback(
            int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                DateTimeOffset unixTimestamp = DateTimeOffset.UtcNow;
                Model.Action action = new Model.Action(vkCode, ((Keys)vkCode).ToString(), unixTimestamp);
                _actionBucket.Insert(action);
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        const int WM_LBUTTONDOWN = 0x201; // Left mouse button down
        const int WM_RBUTTONDOWN = 0x204; // Right mouse button down
        const int WM_MBUTTONDOWN = 0x207; // Middle mouse button down
        const int WM_45BUTTONDOWN = 0x020B; // 4 5 mouse button down


        private IntPtr HookCallbackMouse(int nCode, IntPtr wParam, IntPtr lParam)
        {  
            if (nCode >= 0 
                && (wParam == (IntPtr)WM_LBUTTONDOWN || wParam == (IntPtr)WM_RBUTTONDOWN) || wParam == (IntPtr)WM_MBUTTONDOWN || wParam == (IntPtr)WM_45BUTTONDOWN)
            {
                MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
                DateTimeOffset unixTimestamp = DateTimeOffset.UtcNow;
                if (wParam == (IntPtr)WM_LBUTTONDOWN)
                {
                    Model.Action action = new Model.Action(-1, "LBUTTONDOWN", unixTimestamp);
                    _actionBucket.Insert(action);
                }
                if (wParam == (IntPtr)WM_RBUTTONDOWN)
                {
                    Model.Action action = new Model.Action(-2, "RBUTTONDOWN", unixTimestamp);
                    _actionBucket.Insert(action);
                }
                if (wParam == (IntPtr)WM_MBUTTONDOWN)
                {
                    Model.Action action = new Model.Action(-3, "MBUTTONDOWN", unixTimestamp);
                    _actionBucket.Insert(action);
                }
                if (wParam == (IntPtr)WM_45BUTTONDOWN)
                {
                    Model.Action action = new Model.Action(-4, "45BUTTONDOWN", unixTimestamp);
                    _actionBucket.Insert(action);
                }
            }

            // Call the next hook in the chain
            return CallNextHookEx(_hookIDMouse, nCode, wParam, lParam);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);


        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public int mouseData;
            public int flags;
            public int time;
            public IntPtr dwExtraInfo;
        }


        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

    }
}
