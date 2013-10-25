using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace TankBot
{
    class Monitor
    {
        private static DateTime statusLastChangedTime;
        private static TankBot.Status oldStatus;
        public static void startThread()
        {
            TankBot tb = TankBot.getInstance();

            while (true)
            {
                if (tb.status != oldStatus)
                {
                    oldStatus = tb.status;
                    statusLastChangedTime = DateTime.Now;
                }
                Thread.Sleep(1000);
                if ((DateTime.Now - statusLastChangedTime).TotalMinutes > TBConst.noActionRestartMinutes)
                {
                    restart();
                }
            }
        }

        private static void restart()
        {
            Process.Start("restart.bat");
        }
    }
}
