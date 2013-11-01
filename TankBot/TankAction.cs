using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace TankBot
{

    class TankAction
    {
        /// <summary>
        /// move the cursor down to carousel and wheel up 
        /// </summary>
        public static void moveCarouselLeft()
        {
            initTankCarousel();
            PointINT p = tank_carousel[0];
            Helper.MoveCursorTo(p.X, p.Y);
            Thread.Sleep(100);
            Helper.wheelUp();
            Thread.Sleep(100);
            Helper.wheelUp();
            Thread.Sleep(100);
            Helper.wheelUp();
            Thread.Sleep(100);

        }


        static List<PointINT> tank_carousel = new List<PointINT>();
        static void initTankCarousel()
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

            Process[] processes = Process.GetProcessesByName("WorldOfTanks");
            if (processes.Length == 0)
            {
                Helper.LogException("There's no open worldoftanks.exe");
                MessageBox.Show("There's no open worldoftanks.exe");
                Environment.Exit(0);
            }
            Process process = processes[0];
            Helper.LogInfo("worldoftanks.exe with pid " + process.Id);
            TBConst.pid = process.Id;
            TBConst.wotHandle = process.MainWindowHandle;
            RECT rct;
            int width;
            int height;
            Helper.GetWindowRect(new HandleRef(null, TBConst.wotHandle), out rct);
            width = rct.Right - rct.Left;
            height = rct.Bottom - rct.Top;

            RECT client;

            Helper.GetClientRect(TBConst.wotHandle, out client);
            Helper.LogInfo("client.right  " + client.Right + " client.bottom " + client.Bottom);
            width += 1280 - client.Right;
            height += 768 - client.Bottom;

            PointINT p = new PointINT(0, 0);
            Helper.ClientToScreen(TBConst.wotHandle, ref p);
            // correct the window size
            if (client.Right != 1280 || client.Bottom != 768)
            {
                Helper.LogInfo("client height and width not correct. correcting it");

                Helper.MoveWindow(TBConst.wotHandle, rct.Left, rct.Top, width, height, false);
            }
            Helper.LogInfo("width " + width + " height " + height);

            Helper.GetClientRect(TBConst.wotHandle, out client);
            if (client.Right != 1280 || client.Bottom != 768)
            {
                Helper.LogException("client Height and Width is not 1280x768");
                //MessageBox.Show("client Height and Width is not 1280x768");
                //Environment.Exit(0);
            }

            Thread.Sleep(20);
            TBConst.windowX = p.X;
            TBConst.windowY = p.Y;

            TBConst.botPort = TBConst.pid % 10000 + 10000;
            Helper.LogInfo("botPort" + TBConst.botPort);
        }

        internal static void startWOT()
        {

            bool wot_alive = false;
            if (Process.GetProcessesByName("WorldOfTanks").Length > 0)
                wot_alive = true;

            if (!wot_alive)
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.WorkingDirectory = TBConst.wotRootPath;
                startInfo.FileName = "WorldOfTanks.exe";
                Process proc = Process.Start(startInfo);
                Thread.Sleep(30000);
                initWOTArgs();
                Thread.Sleep(2000);
                Helper.keyPress("i", Helper.KEY_TYPE.PRESS, true);
                Helper.LogInfo("enter the game");
                
                Thread.Sleep(10000);
                TankBot.getInstance().status = Status.IN_HANGAR;
            }

        }


        public static List<int> clickOrder(string filename = "")
        {
            if (filename == "")
                filename = TBConst.config_carousel_path;
            initTankCarousel();
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
            Helper.LogInfo("TankAction clickTank" + i);
            initTankCarousel();
            PointINT p = tank_carousel[i - 1];
            Helper.MoveCursorTo(p.X, p.Y);
            Thread.Sleep(100);
            Helper.leftClickSlow();
            Thread.Sleep(300);
            Helper.leftClickSlow();
            Thread.Sleep(300);

        }


        /// <summary>
        /// move to location
        /// slow click the start
        /// take 1 seconds
        /// </summary>
        internal static void clickStart()
        {
            Helper.LogInfo("TankAction clickStart");
            Helper.MoveCursorTo(628, 45);
            Thread.Sleep(1000);
            Helper.leftClickSlow();
        }

        public static void exitToHangar()
        {
            Helper.LogInfo("TankAction exit to hangar");
            //ESC
            Helper.keyPress("b", Helper.KEY_TYPE.PRESS);
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
