using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;

namespace TankBot
{
    class Main
    {
        static Thread threadTankbot, threadXvmComm, threadCheatClient, helperThread;
        static bool started = false;
        static public void startTankBotThread()
        {
            //start key mapping
            Helper.LogInfo("root_path " + TBConst.wotRootPath);
            Process.Start(TBConst.ahkPath);
            TankAction.startWOT();

            //move the window to a proper location
            if (TBConst.RealBattle)
            {
                TankAction.initWOTArgs();
                threadTankbot = new Thread(new ThreadStart(TankBot.getInstance().startThread));
                threadTankbot.Start();
            }
            threadXvmComm = new Thread(new ThreadStart(XvmComm.getInstance().startThread));
            threadXvmComm.Start();

            if (TBConst.cheatSlaveMode)
            {
                threadCheatClient = new Thread(new ThreadStart(CheatClient.getInstance().startThread));
                threadCheatClient.Start();
            }
            helperThread = new Thread(new ThreadStart(Helper.updateForegroundPID));
            helperThread.Start();
        }
        static Thread threadStartTankBot;
        static public void startTankBot()
        {
            if (started)
            {
                MessageBox.Show("already started");
                Helper.LogInfo("start tank bot");
                return;
            }
            started = true;
            threadStartTankBot = new Thread(new ThreadStart(startTankBotThread));
            threadStartTankBot.Start();

        }
        static public void abortTankBot()
        {
            if (threadTankbot != null)
            {
                TankBot.getInstance().abortThread();
                threadTankbot.Abort();
            }
            if (threadXvmComm != null)
            {
                XvmComm.getInstance().abortThread();
                threadXvmComm.Abort();
            }
            if (threadCheatClient != null)
            {
                CheatClient.getInstance().abortThread();
                threadCheatClient.Abort();
            }
            try
            {
                threadStartTankBot.Abort();
            }
            catch
            {

            }
            if (helperThread != null)
                helperThread.Abort();
            // kill auto hot key process
            Process[] proc = Process.GetProcessesByName("key_map");
            foreach (Process p in proc)
                p.Kill();
            hotkeyF2.Unregister();
            hotkeyF3.Unregister();
            Environment.Exit(0);
        }

        internal static void init()
        {            // generate two hot key
            hotkeyF2 = new Hotkey();
            hotkeyF2.KeyCode = Keys.F2;
            hotkeyF2.Pressed += delegate { Main.startTankBot(); };
            if (hotkeyF2.Register(mainForm) == false)
            {
                MessageBox.Show("F2 reg fail");
                Environment.Exit(0);
            }
            hotkeyF3 = new Hotkey();
            hotkeyF3.KeyCode = Keys.F3;
            hotkeyF3.Pressed += delegate { Main.abortTankBot(); };
            if (hotkeyF3.Register(mainForm) == false)
            {
                MessageBox.Show("F3 reg fail");
                Environment.Exit(0);
            }
        }
        static Hotkey hotkeyF3, hotkeyF2;
        public static Form mainForm;
    }
}
