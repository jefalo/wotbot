using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace TankBot
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;        // x position of upper-left corner
        public int Top;         // y position of upper-left corner
        public int Right;       // x position of lower-right corner
        public int Bottom;      // y position of lower-right corner
    }

    public class Helper
    {

        #region Interop
        /// <summary>
        /// seems keyboard would only be used when input user name and password. 
        /// but user name and password can be rememberes
        /// or just ignore this feature is okay.         
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern bool SetCursorPos(int X, int Y);


        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(HandleRef hWnd, out RECT lpRect);


        [DllImport("user32.dll")]
        public static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("psapi.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public static extern uint GetModuleFileNameEx(IntPtr hProcess, IntPtr hModule, [Out] StringBuilder lpBaseName, uint nSize);
        [DllImport("psapi.dll")]
        public static extern uint GetProcessImageFileName(IntPtr hProcess, [Out] StringBuilder lpImageFileName, [In] [MarshalAs(UnmanagedType.U4)] int nSize);
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out uint ProcessId);

        [DllImport("user32.dll")]
        public static extern bool ClientToScreen(IntPtr hWnd, ref PointINT lpPoint);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, int dwExtraInfo);
        private const uint MOUSEEVENTF_LEFTDOWN = 0x02;
        private const uint MOUSEEVENTF_LEFTUP = 0x04;
        private const uint MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const uint MOUSEEVENTF_RIGHTUP = 0x10;

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);




        [DllImport("user32.dll", EntryPoint = "keybd_event", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern void keybd_event(byte vk, byte scan, int flags, int extrainfo);

        private const int KEYEVENTF_EXTENDEDKEY = 0x1; //Key down flags
        private const int KEYEVENTF_KEYUP = 0x2; //Key up flag
        private const int VK_LCONTROL = 0xA2; //Left Control key code
        private const int VK_LWIN = 0x5B;
        private const int VK_CONTROL = 0x11; //A Control key code

        private const int A = 0x41; //A Control key code
        private const int N = 0x4E; //A Control key code
        private const int R = 0x52; //A Control key code
        #endregion


        #region definition
        private static System.IO.StreamWriter file = null;
        private static System.IO.StreamWriter filemessage = null;
        static public MainForm mainForm;
        private static int foregroundPID = 0;
        #endregion

        static public void updateForegroundPID()
        {
            while (true)
            {
                uint pid;
                IntPtr handle = GetForegroundWindow();
                GetWindowThreadProcessId(handle, out pid);

                foregroundPID = (int)pid;
                
                Thread.Sleep(1000);
            }
        }

        static public void MoveCursorTo(int x, int y)
        {
            if (TBConst.pid != foregroundPID) return;
            Helper.LogDebug("move cursor To " + x + " " + y);
            x += TBConst.windowX;
            y += TBConst.windowY;
            SetCursorPos(x, y);
        }
        static public void MoveCursor(int x, int y)
        {
            if (TBConst.pid != foregroundPID) return;
            Helper.LogDebug("move cursor " + x + " " + y);

            mouse_event(0x0001, (uint)x, (uint)y, (uint)0, 0);
        }
        static public void leftClickSlow()
        {
            if (TBConst.pid != foregroundPID) return;
            Helper.LogDebug("leftClickSlow ");
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
            //MessageBox.Show(sMessage);
            Thread.Sleep(300);
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
            Thread.Sleep(100);
        }
        static public void leftClick()
        {
            if (TBConst.pid != foregroundPID) return;
            if (TBConst.noClick)
            {
                return;
            }
            Helper.LogDebug("leftClick ");
            if (Cursor.Position.X > TBConst.windowX + 1280 || Cursor.Position.X < TBConst.windowX + 0 || Cursor.Position.Y > TBConst.windowY + 768 || Cursor.Position.Y < TBConst.windowY + 0)
                return;
            leftClick(Cursor.Position.X, Cursor.Position.Y);
        }
        static public void leftClick(int x, int y)
        {
            if (TBConst.pid != foregroundPID) return;
            mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, 0, 0, (uint)0, 0);
        }

        private static byte char2int(char c)
        {
            byte rtn = 0;
            if (c >= 'A' && c <= 'Z')
                rtn = Convert.ToByte(Convert.ToByte(c));
            if (c >= 'a' && c <= 'z')
                rtn = Convert.ToByte(Convert.ToByte(c) - Convert.ToByte('a') + Convert.ToByte('A'));

            return rtn;

        }
        public enum KEY_TYPE { UP, DOWN, PRESS };
        private static char keyUpDownMap(char c, KEY_TYPE type)
        {
            if (c == 'a' && type == KEY_TYPE.DOWN) return 'k';
            if (c == 'a' && type == KEY_TYPE.UP) return 'l';
            if (c == 'd' && type == KEY_TYPE.DOWN) return 'o';
            if (c == 'd' && type == KEY_TYPE.UP) return 'p';
            throw new NotImplementedException();
        }

        static private void Log(string head, string lines)
        {

            if (true)
            {
                while(true)
                {
                    try
                    {
                        if (file == null) file = new System.IO.StreamWriter(TBConst.logFile);
                    }
                    catch
                    {
                        Helper.LogException(" open log file error " + TBConst.logFile);
                    }
                    try
                    {
                        if (filemessage == null) filemessage = new System.IO.StreamWriter(TBConst.logFileMessage);
                    }
                    catch
                    {
                        Helper.LogException(" open log file error " + TBConst.logFileMessage);
                    }
                    try
                    {
                        // Write the string to a file.append mode is enabled so that the log
                        // lines get appended to  test.txt than wiping content and writing the log

                        string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
                        if (head == "[Message]")
                            filemessage.WriteLine(head + " " + timestamp + " " + lines);
                        else
                            file.WriteLine(head + " " + timestamp + " " + lines);
                        file.Flush();
                        break;
                    }
                    catch
                    {
                        // 
                    }
                }
            }
            if (TBConst.LogToConsole)
            {
                if (head == "[INFO]")
                    Trace.WriteLine(head + " " + lines);
            }
        }
        static public void LogDebug(String lines)
        {
            if (TBConst.LogDebug)
                Log("[DEBUG]", lines);

        }
        static public void LogMessage(String lines)
        {
            if (TBConst.LogMessage)
                Log("[Message]", lines);

        }
        static public void LogInfo(String lines)
        {
            if (TBConst.LogInfo)
                Log("[INFO]", lines);

            if (mainForm != null)
                mainForm.appendTextLog(lines, true);
        }
        static public void LogException(String lines)
        {
            Log("[EXCEPTION]", lines);

        }

        static public void keyPress(string c, KEY_TYPE type, bool ignore = false)
        {
            
            if (!ignore)
                if (TBConst.pid != foregroundPID) return;
            
            if (c.Length == 1)
            {



                byte b;
                if (type == KEY_TYPE.PRESS)
                    b = char2int(c[0]);
                else
                    b = char2int(keyUpDownMap(c[0], type));
                //SendKeys.SendWait("{r}");
                
                keybd_event(b, 0, KEYEVENTF_EXTENDEDKEY, 0);
                
                Thread.Sleep(10);
                keybd_event(b, 0, KEYEVENTF_KEYUP, 0);
                Thread.Sleep(10);
                
            }
            
        }



        public static void wheelUp()
        {    
            Helper.keyPress("n", Helper.KEY_TYPE.PRESS);
        }
        public static void wheelDown()
        {
            Helper.keyPress("m", Helper.KEY_TYPE.PRESS);
        }

        internal static void bringToFront()
        {
            SetForegroundWindow(TBConst.wotHandle);
            Thread.Sleep(100);
        }

        internal static void LogPersistent(string p)
        {
            StreamWriter file = new System.IO.StreamWriter(TBConst.logPersistentFile, true);
            string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
            file.WriteLine("[persistent]" + " " + timestamp + " " + p);
            file.Flush();
        }
    }
    public struct PointINT
    {
        public int X;
        public int Y;

        public PointINT(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public PointINT(System.Drawing.Point pt) : this(pt.X, pt.Y) { }

        public static implicit operator System.Drawing.Point(PointINT p)
        {
            return new System.Drawing.Point(p.X, p.Y);
        }

        public static implicit operator PointINT(System.Drawing.Point p)
        {
            return new PointINT(p.X, p.Y);
        }
    }
    public class Point : IComparable
    {
        public int CompareTo(Point t)
        {

            if (this.x < t.x) return -1;
            if (this.x > t.x) return 1;

            if (this.y < t.y) return -1;
            if (this.y > t.y) return 1;
            return 0;
        }
        public int CompareTo(Object t)
        {
            return CompareTo((Point)t);
        }
        public double First { get; set; }
        public double Second { get; set; }
        public double x
        {
            set { this.First = value; }
            get { return this.First; }
        }
        public double y
        {
            set { this.Second = value; }
            get { return this.Second; }
        }
        public Point()
        {
            First = Second = 0;
        }

        public void Clear()
        {
            First = Second = 0;
        }
        public Point(double _x, double _y)
        {
            First = _x;
            Second = _y;
        }
        public override string ToString()
        {
            return "" + x + "," + y;
        }
    }



}
