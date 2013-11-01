/*
 * communication between xvm and my bot
 */

using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Net;
namespace TankBot
{
    class RateCount
    {
        private static Dictionary<string, List<DateTime>> d = new Dictionary<string, List<DateTime>>();
        public static void add(string str)
        {
            if (!d.ContainsKey(str))
                d[str] = new List<DateTime>();

            if (d[str].Count > 10000) d[str].Clear();
            d[str].Add(DateTime.Now);
            Helper.LogDebug("RateCount" + str + " " + DateTime.Now);
        }
        public static string toStr()
        {
            string str = "";
            var lst = d.Keys.ToList();
            lst.Sort();
            foreach (var key in lst)
            {
                str += toStr(key, DateTime.Now);
            }
            return str;
        }

        private static string toStr(string key, DateTime max)
        {
            try
            {
                int cnt = 0;
                foreach (DateTime t in d[key])
                {
                    if ((max - t).TotalSeconds < 1)
                    {
                        cnt++;
                    }
                }
                return key + " " + cnt + "\r\n";
            }
            catch
            {
                return "";
            }
        }
    }
    class XvmComm
    {
        #region definition
        static private XvmComm instance = null;
        static public XvmComm getInstance()
        {
            if (instance == null)
                instance = new XvmComm();
            return instance;
        }
        public UdpClient receivingUdpClient;
        public IPEndPoint RemoteIpEndPoint;
        public MainForm mainForm;
        private DateTime lastSocketRead = new DateTime();
        private Thread aThreadTCP;
        private TcpListener tcpListener;
        #endregion

        #region readFile
        private void readMapName()
        {
            try
            {
                using (StreamReader sr = new StreamReader(TBConst.wotRootPath + "map_name.log"))
                {
                    string line = sr.ReadToEnd();
                    if (line.IndexOf("MAP_NAME :") > -1)
                    {
                        int x = line.IndexOf("$");
                        string mapName = line.Substring(x + 1);
                        mapName = mapName.Trim();
                        TankBot.getInstance().mapName = mapName;
                    }
                }
            }
            catch
            {
                Helper.LogException("readMapName got error");
            }
        }
        private void readPlayerPanel(string file_name, Vehicle[] vehicles)
        {

            try
            {
                using (StreamReader sr = new StreamReader(TBConst.wotRootPath + file_name))
                {
                    string line = sr.ReadToEnd();
                    CheatClient.getInstance().sendCheatMessage(line);
                    string[] separator4 = new string[] { "@@@@" };
                    string[] separator2 = new string[] { "@@" };
                    string[] sp = line.Split(separator4, StringSplitOptions.RemoveEmptyEntries);
                    string[] new_sp = new string[sp.Length - 1];
                    Array.Copy(sp, 1, new_sp, 0, sp.Length - 1);

                    foreach (string player_line in new_sp)
                    {
                        string[] p = player_line.Split(separator2, StringSplitOptions.RemoveEmptyEntries);
                        string uid = p[0];
                        string v_name = p[2]; // stug iii
                        string u_name = p[3]; // bbsanghaha

                        bool happen = false;
                        foreach (Vehicle v in vehicles)
                            if (v.username == "" || v.username == u_name)
                            {
                                happen = true;
                                v.username = u_name;
                                v.tankName = v_name;
                                v.uid = Convert.ToInt32(uid);
                                break;
                            }
                        if (!happen)
                        {
                            foreach (Vehicle v in vehicles)
                                v.username = "";

                        }
                    }
                }
            }
            catch
            {
                Helper.LogException("read player panel got exception");
            }
        }
        private void readPlayerPanelSelf(string file_name, Vehicle v)
        {


            try
            {
                using (StreamReader sr = new StreamReader(TBConst.wotRootPath + file_name))
                {
                    string line = sr.ReadToEnd();
                    string[] sp = line.Split('@');
                    v.icon = sp[sp.Length - 1];
                    v.username = sp[sp.Length - 2];
                    v.tankName = sp[sp.Length - 3];
                }
            }
            catch
            {
                Helper.LogException("read self panel exception");
            }
        }
        private void readPlayerPanel()
        {

            try
            {
                readPlayerPanel("player_panel_ally.log", TankBot.getInstance().allyTank);
                readPlayerPanel("player_panel_enemy.log", TankBot.getInstance().enemyTank);
                readPlayerPanelSelf("player_panel_self.log", TankBot.getInstance().myTank);
            }
            catch
            {
                Helper.LogException("read player panel fail");
            }

        }
        #endregion

        #region process message
        public void update_my_position(string line)
        {
            string x = line.Substring(line.IndexOf("self_location_minimap") + "self_location_minimap".Length);
            x = x.Trim();
            string[] sp = x.Split(' ');
            Vehicle myTank = TankBot.getInstance().myTank;

            myTank.posRaw = new Point(double.Parse(sp[0]), double.Parse(sp[1]));
            myTank.direction = double.Parse(sp[2]);
            myTank.cameraDirection = double.Parse(sp[3]);
            myTank.directionUpdated = true;
            myTank.cameraDirectionUpdated = true;


        }
        public void update_ally_position_minimap(string line)
        {
            foreach (Vehicle v in TankBot.getInstance().allyTank)
                v.visible_on_minimap = false;
            string x = line.Substring(line.IndexOf("ally_location_minimap") + "ally_location_minimap".Length);
            x = x.Trim();
            if (x == "")
                return;
            string[] sp = x.Split(' ');
            for (int i = 0; i < sp.Length; i += 3)
            {
                string uname = sp[i + 2].Trim();
                foreach (Vehicle v in TankBot.getInstance().allyTank)
                {
                    if (v.username == uname || v.username == "")
                    {
                        v.visible_on_minimap = true;
                        v.username = uname;
                        v.posRaw = new Point(double.Parse(sp[i]), double.Parse(sp[i + 1]));
                        break;
                    }
                }
            }
        }
        public void update_enemy_position_minimap(string line)
        {
            // TODO better not put those stuff here
            // TODO actual not exactly PLAYING, should be waiting.
            if (TankBot.getInstance().status == Status.IN_HANGAR)
                TankBot.getInstance().status = Status.COUNT_DOWN;
            //force this one update every 100ms
            clear_enemy_screen();
            foreach (Vehicle v in TankBot.getInstance().enemyTank)
                v.visible_on_minimap = false;

            string x = line.Substring(line.IndexOf("enemy_location_minimap") + "enemy_location_minimap".Length);
            x = x.Trim();
            if (x == "")
                return;
            string[] sp = x.Split(' ');
            for (int i = 0; i < sp.Length; i += 3)
            {
                string uname = sp[i + 2].Trim();
                foreach (Vehicle v in TankBot.getInstance().enemyTank)
                {
                    if (v.username == uname || v.username == "")
                    {
                        v.visible_on_minimap = true;
                        v.username = uname;
                        v.posRaw = new Point(double.Parse(sp[i]), double.Parse(sp[i + 1]));
                        break;
                    }
                }
            }
        }
        private void clear_enemy_screen()
        {
            //Better invoke somewhere else
            foreach (Vehicle v in TankBot.getInstance().enemyTank)
            {
                double span = TimeSpan.FromTicks(DateTime.Now.Ticks - v.visible_on_screen_last_ticks).TotalSeconds;

                if (span > 0.2)
                    v.visible_on_screen = false;
            }
        }
        public void update_enemy_position_screen(string line)
        {

            line = line.Substring(line.IndexOf("enemy_location_screen") + "enemy_location_screen".Length);
            line = line.Trim();
            string[] sp = line.Split(' ');
            string uname = sp[0].Trim();
            if (uname.IndexOf('[') >= 0)
                uname = uname.Substring(0, uname.IndexOf('['));

            foreach (Vehicle v in TankBot.getInstance().enemyTank)
            {
                if (v.username == uname || v.username == "")
                {
                    v.visible_on_screen_last_ticks = DateTime.Now.Ticks;
                    v.username = uname;
                    v.visible_on_screen = true;
                    v.posScreen = new Point(double.Parse(sp[1]), double.Parse(sp[2]));
                    v.posScreenUpdated = true;
                    break;
                }
            }

        }
        public void update_armor_pen(string line)
        {
            if (line.IndexOf("[SNIPER_onSetMarkerType]") >= 0)
                TankBot.getInstance().penetration = line.Substring(line.IndexOf("[SNIPER_onSetMarkerType]") + "[SNIPER_onSetMarkerType]".Length).Trim();
        }
        public void updateBattleTimer(string line)
        {
            Helper.LogDebug("updateBattleTimer " + line);
            string[] sp = line.Split(' ');
            string p = sp[sp.Length - 1];
            TankBot.getInstance().timeLeft = Convert.ToInt32(p);
            Helper.LogDebug("time left: " + TankBot.getInstance().timeLeft);

        }
        public void update_speed(string line)
        {
            string[] sp = line.Split(' ');
            string speed = sp[sp.Length - 1];
            TankBot.getInstance().myTank.speed = Convert.ToInt32(speed);
        }
        private void update_health(string line)
        {
            string[] sp = line.Split(' ');
            string health = sp[sp.Length - 1];
            health = health.Trim();
            Helper.LogInfo("health change" + health);
            if (health == "0")
                TankBot.getInstance().status = Status.DIE;
            TankBot.getInstance().myTank.health = Convert.ToInt32(health);
        }
        /// <summary>
        /// some example
        /// "2013-09-29 18:38:46: [BigWorld_target] 1 GeneralPaps"  1 or 2 depends, not 1 for self team
        /// "2013-09-29 18:38:46: [BigWorld_target] 2 GeneralPaps"
        /// "2013-09-29 18:38:49: [BigWorld_target] 0"
        /// </summary>
        /// <param name="line"></param>
        private void update_bigworld_target(string line)
        {
            string[] sp = line.Split(' ');
            string focus = "";
            for (int i = 0; i < sp.Length; i++)
            {
                if (sp[i].Trim() == "[BigWorld_target]")
                {
                    focus = sp[i + 1];
                    break;
                }
            }
            if (focus != "0")
            {
                TankBot.getInstance().focusTarget = true;
                TankBot.getInstance().focusTargetUpdated = true;
            }
            else
            {
                TankBot.getInstance().focusTarget = false;
                TankBot.getInstance().focusTargetUpdated = true;
            }

        }
        #endregion


        public int messageCnt = 0;
        public void processMessage(string message)
        {
            
            messageCnt++;
Helper.LogMessage(message);
            //if (message.IndexOf("BigWorld_Vehicle_Position") >= 0)
            
            List<string> keywords = new List<string>();
            keywords.Add("self_location_minimap");
            keywords.Add("ally_location_minimap");
            keywords.Add("enemy_location_screen");
            keywords.Add("SNIPER");
            keywords.Add("DamagePanel_speed");
            keywords.Add("BigWorld_target");
            keywords.Add("BattleTimer");
            foreach (string keyword in keywords)
            {
                if (message.IndexOf(keyword) >= 0)
                {
                    Helper.LogDebug("socket read add keyword message:" + message + " keyword: " + keyword);
                    RateCount.add(keyword);
                }
            }

            if (message.IndexOf("self_location_minimap") >= 0)
                CheatClient.getInstance().sendCheatMessage(message);
            if (message.IndexOf("BigWorld_Vehicle_Position") >= 0)
                CheatClient.getInstance().sendCheatMessage(message);
            if (message.IndexOf("ally_location_minimap") >= 0)
                CheatClient.getInstance().sendCheatMessage(message);


            if (message.IndexOf("self_location_minimap") >= 0)
            {
                lastSocketRead = DateTime.Now;
                update_my_position(message);
                //CheatClient.getInstance().sendCheatMessage(message);
            }

            if (message.IndexOf("ally_location_minimap") >= 0)
            {
                update_ally_position_minimap(message);
            }
            if (message.IndexOf("enemy_location_minimap") >= 0)
                update_enemy_position_minimap(message);
            if (message.IndexOf("enemy_location_screen") >= 0)
                update_enemy_position_screen(message);

            if (message.IndexOf("[SNIPER") >= 0)
                update_armor_pen(message);
            if (message.IndexOf("[DamagePanel_speed]") >= 0)
                update_speed(message);
            if (message.IndexOf("BigWorld_target") >= 0)
                update_bigworld_target(message);

            if (message.IndexOf("View loaded: hangar") >= 0)
                TankBot.getInstance().status = Status.IN_HANGAR;
            if (message.IndexOf("View loaded: battleResults") >= 0)
                TankBot.getInstance().status = Status.SHOW_BATTLE_RESULTS;


            if (message.IndexOf("DamagePanel_health") >= 0)
                update_health(message);

            if (message.IndexOf("BattleTimer") >= 0)
                updateBattleTimer(message);


        }




        //display informationi on panel
        private void displayInfoThread()
        {
            while (true)
            {
                Thread.Sleep(1000);
                TankBot tb = TankBot.getInstance();

                string v_log = "";
                foreach (Vehicle vv in TankBot.getInstance().enemyTank)
                {
                    //v_log += vv.tankname + " " + vv.speedMinimapBase + "\r\n";
                    v_log += vv.username + " ";

                }
                string str = "";
                str += TankBot.getInstance().mapName;
                if (TBConst.cheatSlaveMode)
                    str += "\r\n" + "cheatMasterOnOtherSide: " + CheatClient.getInstance().cheatMasterOnOtherSide();
                if (TBConst.cheatSlaveMode)
                    str += "\r\n" + "cheatMasterOnSameSide: " + CheatClient.getInstance().cheatMasterOnSameSide();
                str += "\r\n" + "messageCount: " + this.messageCnt;
                str += "\r\n" + "aiming_circle: " + (DateTime.Now - tb.aimStartTime).TotalSeconds;
                str += "\r\n" + tb.debugString();
                str += "\r\n";
                str += "\r\n" + RateCount.toStr();
                str += "\r\n";
                str += "\r\n" + v_log;
                mainForm.setText(str);
                this.mainForm.paint();


            }
        }

        TcpClient client;
        private void tcpThread()
        {
            tcpListener = new TcpListener(IPAddress.Any, TBConst.botPort);

            tcpListener.Start();
            while (true)
            {
                client = tcpListener.AcceptTcpClient();

                Helper.LogInfo("connection established from: " + ((IPEndPoint) client.Client.RemoteEndPoint).Address);
                NetworkStream clientStream = client.GetStream();
                byte[] msg = new byte[4096];
                int bytesRead;
                string message = "";

                int cc = 0;
                while (true)
                {
                    cc++;
                    
                    try
                    {
                        bytesRead = clientStream.Read(msg, 0, 4096);
                    }
                    catch
                    {
                    
                        break;
                    }
                    if (bytesRead == 0)
                        break;

                    ASCIIEncoding encoder = new ASCIIEncoding();
                    message = message + encoder.GetString(msg, 0, bytesRead);
                    if (message.EndsWith("<EOF>"))
                    {
                        message = message.Substring(0, message.Length - "<EOF>".Length);
                        string[] eofSeparator = { "<EOF>" };
                        string[] sp = message.Split(eofSeparator, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string s in sp)
                        {
                            
                            int i;
                            for (i = 0; i < s.Length; i++)
                                if (s[i] == ' ')
                                    break;

                            this.processMessage(s.Substring(i + 1, s.Length - i - 1));
                        }
                        message = "";
                    }
                }

            }
        }
        public void startThread()
        {
            Helper.LogInfo("xvm communication start thread");
            lastSocketRead = DateTime.Now;

            this.aThreadTCP = new Thread(new ThreadStart(tcpThread));
            this.aThreadTCP.Start();
            this.aDisplayInfo = new Thread(new ThreadStart(displayInfoThread));
            this.aDisplayInfo.Start();

            while (true)
            {
                readMapName();
                readPlayerPanel();
                Thread.Sleep(1000);
            }
        }

        internal void abortThread()
        {
            try { client.Close(); }
            catch { }
            try { this.aDisplayInfo.Abort(); }
            catch { }
            try { aThreadTCP.Abort(); }
            catch { }
        }


        public Thread aDisplayInfo { get; set; }
    }
}
