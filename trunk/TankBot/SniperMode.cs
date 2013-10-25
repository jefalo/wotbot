using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace TankBot
{

    class SniperMode
    {
        public static int sniper_level = 0;

        public static void resetSniperLevel()
        {
            for (int i = 0; i < 10; i++)
            {
                Helper.wheelUp();
                Thread.Sleep(100);
                sniper_level = 0;
            }
            for (int i = 0; i < 10; i++)
            {
                Helper.wheelDown();
                Thread.Sleep(100);
                sniper_level = 0;
            }
        }
        public static void setSniperMode(int level)
        {
            while (sniper_level < level)
            {
                sniper_level++;
                Helper.wheelUp();
                Thread.Sleep(100);
            }
            while (sniper_level > level)
            {
                sniper_level--;
                Helper.wheelDown();
                Thread.Sleep(100);
            }

        }
    }
}
