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
    class Helper
    {

        #region importDLL
        /// <summary>
        /// seems keyboard would only be used when input user name and password. 
        /// but user name and password can be rememberes
        /// or just ignore this feature is okay.         
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int X, int Y);


        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(HandleRef hWnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;        // x position of upper-left corner
            public int Top;         // y position of upper-left corner
            public int Right;       // x position of lower-right corner
            public int Bottom;      // y position of lower-right corner
        }
        [DllImport("user32.dll")]
        static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("psapi.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        static extern uint GetModuleFileNameEx(IntPtr hProcess, IntPtr hModule, [Out] StringBuilder lpBaseName, uint nSize);
        [DllImport("psapi.dll")]
        static extern uint GetProcessImageFileName(IntPtr hProcess, [Out] StringBuilder lpImageFileName, [In] [MarshalAs(UnmanagedType.U4)] int nSize);
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out uint ProcessId);

        [DllImport("user32.dll")]
        static extern bool ClientToScreen(IntPtr hWnd, ref PointINT lpPoint);

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

        static private int getForeGroundPID()
        {
            uint pid;
            IntPtr handle = GetForegroundWindow();
            GetWindowThreadProcessId(handle, out pid);
            //Helper.LogDebug("processId"  + pid);
            Process process = Process.GetProcessById((int)pid);
            return (int)pid;
        }
        static public void initWOTArgs()
        {
            bool wot_alive = false;
            if (Process.GetProcessesByName("WorldOfTanks").Length > 0)
                wot_alive = true;
            
            if (!wot_alive)
            {
                Helper.LogInfo("world of tanks not alive");
                return;
            }
            /*
            StringBuilder b = new StringBuilder(2000);
            GetWindowText(handle, b, 2000);
            Helper.LogDebug("GetWindowText" + b.ToString());
             */

            Process [] processes = Process.GetProcessesByName("WorldOfTanks");
            if (processes.Length == 0)
            {
                Helper.LogException("There's no open worldoftanks.exe");
                MessageBox.Show("There's no open worldoftanks.exe");
                Environment.Exit(0);
            }
            Process process = processes[0];
            
            TBConst.pid = process.Id;
            IntPtr handle = process.MainWindowHandle;
            RECT rct;
            int width;
            int height;
            GetWindowRect(new HandleRef(null, handle), out rct);
            width = rct.Right - rct.Left;
            height = rct.Bottom - rct.Top;

            RECT client;

            GetClientRect(handle, out client);
            Helper.LogInfo("client.right  " + client.Right + " client.bottom " + client.Bottom);
            width += 1280 - client.Right;
            height += 768 - client.Bottom;

            PointINT p = new PointINT(0, 0);
            ClientToScreen(handle, ref p);
            // correct the window size
            if (client.Right != 1280 || client.Bottom != 768)
            {
                Helper.LogInfo("client height and width not correct. correcting it");
                
                MoveWindow(handle, rct.Left, rct.Top, width, height, false);
            }
            Helper.LogInfo("width " + width + " height " + height);

            GetClientRect(handle, out client);
            if (client.Right != 1280 || client.Bottom != 768)
            {
                Helper.LogInfo("client Height and Width is not 1280x768");
                MessageBox.Show("client Height and Width is not 1280x768");
                Environment.Exit(0);
            }

            Thread.Sleep(20);
            TBConst.windowX = p.X;
            TBConst.windowY = p.Y;

            TBConst.botPort = TBConst.pid % 10000 + 10000;
            
        }
        static public void MoveCursorTo(int x, int y)
        {
            if (TBConst.pid != getForeGroundPID()) return;
            Helper.LogDebug("move cursor To " + x + " " + y);
            x += TBConst.windowX;
            y += TBConst.windowY;
            SetCursorPos(x, y);
        }
        static public void MoveCursor(int x, int y)
        {
            if (TBConst.pid != getForeGroundPID()) return;
            Helper.LogDebug("move cursor " + x + " " + y);
            
            mouse_event(0x0001, (uint)x, (uint)y, (uint)0, 0);
        }
        static public void leftClickSlow()
        {
            if (TBConst.pid != getForeGroundPID()) return;
            Helper.LogInfo("leftClickSlow ");
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
            //MessageBox.Show(sMessage);
            Thread.Sleep(300);
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
            Thread.Sleep(100);
        }
        static public void leftClick()
        {
            if (TBConst.pid != getForeGroundPID()) return;
            if (TBConst.noClick)
            {
                return;
            }
            Helper.LogInfo("leftClick ");
            if (Cursor.Position.X > TBConst.windowX + 1280 || Cursor.Position.X < TBConst.windowX + 0 || Cursor.Position.Y > TBConst.windowY + 768 || Cursor.Position.Y < TBConst.windowY + 0)
                return;
            leftClick(Cursor.Position.X, Cursor.Position.Y);
        }
        static public void leftClick(int x, int y)
        {
            if (TBConst.pid != getForeGroundPID()) return;
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
        private static System.IO.StreamWriter file = null;
        private static System.IO.StreamWriter filemessage = null;
        static private void Log(string head, string lines)
        {

            if (true)
            {
                try
                {
                    if (file == null) file = new System.IO.StreamWriter(TBConst.logFile_d);
                }
                catch
                {
                    Helper.LogException(" open log file error " + TBConst.logFile_d);
                }
                try
                {
                    if (filemessage == null) filemessage = new System.IO.StreamWriter(TBConst.logFileMessage_d);
                }
                catch
                {
                    Helper.LogException(" open log file error " + TBConst.logFileMessage_d);
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
                }
                catch
                {
                }
            }
            if (TBConst.LogToConsole)
            {
                if(head=="[INFO]")
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
            
            mainForm.setTextLog(lines, true);
        }
        static public MainForm mainForm;
        static public void LogException(String lines)
        {
            Log("[EXCEPTION]", lines);

        }
        static public void key_press(string c, KEY_TYPE type)
        {
            if (TBConst.pid != getForeGroundPID()) return;
            if (c.Length == 1)
            {
                /*
                if (type == KEY_TYPE.DOWN && c[0] == 'a' && a_pressed == true) return;
                if (type == KEY_TYPE.DOWN && c[0] == 'd' && d_pressed == true) return;

                if (type == KEY_TYPE.UP && c[0] == 'a' && a_pressed == false) return;
                if (type == KEY_TYPE.UP && c[0] == 'd' && d_pressed == false) return;
                */


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
                //keybd_event(R, 0, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);
            }
        }



        internal static void startWOT()
        {             

            bool wot_alive = false;
            if (Process.GetProcessesByName("WorldOfTanks").Length > 0)
                wot_alive = true;

            if (!wot_alive )                
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.WorkingDirectory = TBConst.wotRootPath;
                startInfo.FileName = "WorldOfTanks.exe";
                Process proc = Process.Start(startInfo);
                Thread.Sleep(30000);
                initWOTArgs();
                Helper.key_press("i", Helper.KEY_TYPE.PRESS);
                Helper.LogInfo("enter the game");
                Trace.WriteLine("enter the game");
                Thread.Sleep(10000);
                TankBot.getInstance().status = TankBot.Status.IN_HANGAR;
            }
            
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

    class Vehicle
    {
        public VehicleInfo vInfo
        {
            get
            {
                if (this.icon != null && this.icon != "")
                {
                    string[] sp = this.icon.Split('/');
                    string last = sp[sp.Length - 1];

                    last = last.Substring(last.IndexOf('-') + 1);
                    string tankname = last.Substring(0, last.Length - 4);
                    return VehicleInfoGet.get(tankname);
                }
                else
                    return VehicleInfoGet.get("");
            }
        }
        public void Clear()
        {
            username = "";
            tankname = "";
            uid = 0;
            posScreen.Clear();
            posRaw.Clear();
            visible_on_minimap = false;
            visible_on_screen = false;
            visible_on_screen_last_ticks = 0;
            speed = 0;
            direction = 0;
            cameraDirection = 0;
            health = 1000;
            pos_raw_history.Clear();

        }
        public string username = ""; // bbsang, duck_german, etc.s
        public string tankname = ""; // kv-1s, is, etc.
        public int uid; // an ??internal?? id
        public bool posScreenUpdated;
        public List<Tuple<Point, DateTime>> pos_raw_history = new List<Tuple<Point, DateTime>>();
        public Point posScreen = new Point();
        private Point _posRaw = new Point();
        public double speedMinimapBase
        {
            get
            {
                int len = pos_raw_history.Count;
                try
                {
                    Tuple<Point, DateTime> last = pos_raw_history[len - 1];
                    for (int i = 0; i < len; i++)
                    {
                        Tuple<Point, DateTime> now = pos_raw_history[i];
                        if ((last.Item2 - now.Item2).Seconds < 1)
                        {
                            double meterPerSecond = TBMath.distance(last.Item1, now.Item1) / (last.Item2 - now.Item2).Seconds * TankBot.getInstance().mapSacle;
                            return meterPerSecond * 3.6; // km per hour
                        }
                    }
                }
                catch
                {
                    return 0;
                }
                return 0;
            }
        }
        public Point posRaw
        {
            get
            {
                return _posRaw;
            }
            set
            {
                _posRaw = value;
                pos_raw_history.Add(new Tuple<Point, DateTime>(value, DateTime.Now));
            }
        }
        public bool visible_on_screen;
        public long visible_on_screen_last_ticks;
        public bool visible_on_minimap;
        public double speed;
        public int health = 1000;
        public Point pos
        {
            get { return new Point((posRaw.x + 105) / 21 + 1, (posRaw.y + 105) / 21 + 1); }
        }
        // north(up_minimap) consider 0 degree
        // east(right) consider 90 degree
        // wese(left) consider -90 degree
        // south(down) is -180 as well as 180

        // vehicle head direction
        public bool directionUpdated;
        public double direction;
        // camera's direction
        public bool cameraDirectionUpdated;
        public double cameraDirection;
        public string icon;

    }

    class TBMath
    {
        internal static double sqr(double x)
        {
            return x * x;
        }
        internal static double distance(Point point1, Point point2)
        {
            return Math.Sqrt(sqr(point1.x - point2.x) + sqr(point1.y - point2.y));
        }

        internal static double RadianToDegree(double angle)
        {
            return angle * (180.0 / Math.PI);
        }

        internal static double cameraDegreeToEnemy(Vehicle et, Vehicle myTank)
        {
            double degree_diff = TBMath.RadianToDegree(Math.Atan2(et.pos.y - myTank.pos.y, et.pos.x - myTank.pos.x)) - myTank.cameraDirection + 90 + 360;
            while (degree_diff > 180)
                degree_diff -= 360;
            return degree_diff;
        }

        internal static double vehicleDegreeToPoint(Point np, Vehicle myTank)
        {
            double degree_diff = TBMath.RadianToDegree(Math.Atan2(np.y - myTank.pos.y, np.x - myTank.pos.x)) - myTank.direction + 90 + 720;
            while (degree_diff > 180)
                degree_diff -= 360;
            return degree_diff;
        }
    }
    class SniperMode
    {
        public static int sniper_level = 0;

        public static void resetSniperLevel()
        {
            for (int i = 0; i < 10; i++)
            {
                Helper.key_press("n", Helper.KEY_TYPE.PRESS);
                Thread.Sleep(300);
                sniper_level = 0;
            }
            for (int i = 0; i < 10; i++)
            {
                Helper.key_press("m", Helper.KEY_TYPE.PRESS);
                Thread.Sleep(300);
                sniper_level = 0;
            }
        }
        public static void setSniperMode(int level)
        {
            while (sniper_level < level)
            {
                sniper_level++;
                Helper.key_press("n", Helper.KEY_TYPE.PRESS);
                Thread.Sleep(300);
            }
            while (sniper_level > level)
            {
                sniper_level--;
                Helper.key_press("m", Helper.KEY_TYPE.PRESS);
                Thread.Sleep(300);
            }

        }
    }
    class TankAction
    {
        static List<PointINT> tank_carousel = new List<PointINT>();
        static void init_tank_carousel()
        {
            if (tank_carousel.Count == 0)
            {
                tank_carousel.Clear();
                tank_carousel.Add(new PointINT(250, 666)); //1
                tank_carousel.Add(new PointINT(426, 666)); //2
                tank_carousel.Add(new PointINT(591, 666)); //3
                tank_carousel.Add(new PointINT(754, 666)); //4
                tank_carousel.Add(new PointINT(930, 666)); //5
                tank_carousel.Add(new PointINT(1106, 666)); //6
            }
        }
        public static List<int> clickOrder(string filename = "")
        {
            if (filename == "")
                filename = TBConst.config_carousel_path;
            init_tank_carousel();
            List<int> rtn = new List<int>();
            try
            {
                using (StreamReader sr = new StreamReader(filename))
                {
                    string line = sr.ReadToEnd();
                    foreach (string x in line.Split(' '))
                    {
                        string y = x.Trim();
                        if (y != "")
                        {
                            rtn.Add(Convert.ToInt32(y));
                        }
                    }
                }
            }
            catch
            { 
                Helper.LogException("read " + filename + "failed ");
            }

            return rtn;

        }
        /// <summary>
        /// click the i-th tank (start from 1)
        /// </summary>
        /// <param name="i"> # of tank </param>
        public static void clickTank(int i)
        {
            init_tank_carousel();
            PointINT p = tank_carousel[i - 1];


            Helper.MoveCursorTo(p.X, p.Y);
            Thread.Sleep(100);
            Helper.leftClickSlow();
            Thread.Sleep(1000);

        }


        /// <summary>
        /// move to location
        /// slow click the start
        /// take 1 seconds
        /// </summary>
        internal static void clickStart()
        {
            Helper.MoveCursorTo(628, 45);
            Thread.Sleep(1000);
            Helper.leftClickSlow();
        }

        public static void exitToHangar()
        {
            Helper.LogInfo("DIE and exit to hangar");
            //ESC
            Helper.key_press("b", Helper.KEY_TYPE.PRESS);
            Helper.MoveCursorTo(637, 334);
            Thread.Sleep(500);
            Helper.leftClickSlow();
            Thread.Sleep(100);
            Helper.MoveCursorTo(678, 438);
            Thread.Sleep(100);
            Helper.leftClickSlow();
            Thread.Sleep(100);
        }
    }
}
