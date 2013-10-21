using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace TankBot
{
    public class TBConst
    {
        public static bool RealBattle = true;
        static string _wotRootPath="";
        public static string wotRootPath
        {
            get
            {
                if (_wotRootPath == "")
                {
                    _wotRootPath = Directory.GetParent(Directory.GetCurrentDirectory()).ToString() +"\\";
                    //Helper.LogInfo("set root path" + _wotRootPath);
                }
                return _wotRootPath;
            }
        }
        public static string dataPath = TBConst.wotRootPath + @"res_mods\Data\";
        public static string ahkPath = TBConst.wotRootPath + @"res_mods\key_map.exe";
        public static string logFile_d = TBConst.wotRootPath + @"tankbot.log";

        public static string logFileMessage_d = TBConst.wotRootPath + @"tankbot_message.log";
        public static string localIP
        {
            get
            {
                IPHostEntry host;
                string localIP = "0.0.0.0";
                host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (IPAddress ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        localIP = ip.ToString();
                    }
                }
                if (localIP == "0.0.0.0")
                    Helper.LogException("cannot obtain local IP");
                return localIP;
            }
        }
        public static string config_carousel_path
        {
            get
            {
                if (File.Exists(TBConst.wotRootPath + @"res_mods\config\" + localIP + @"\tank_carousel_order.ini"))
                    return TBConst.wotRootPath + @"res_mods\config\" + localIP + @"\tank_carousel_order.ini";
                if (File.Exists(TBConst.wotRootPath + @"res_mods\config\tank_carousel_order.ini"))
                    return TBConst.wotRootPath + @"res_mods\config\tank_carousel_order.ini";
                Helper.LogException("config file not exist");
                return "";
                
            }
        }
        public static string trajectoryPath = TBConst.wotRootPath + @"res_mods\trajectory\";
        public static string fireposPath = TBConst.wotRootPath + @"res_mods\firepos\";
        public static string tagPath = TBConst.wotRootPath + @"res_mods\tag\";
        public static double noActionRestartMinutes = 10;

        public static bool LogMessage = false;
        public static bool LogToConsole = false;
        public static bool LogDebug = false;
        public static bool LogInfo = true;
        public static bool closeToAllyTankStop = false;

        public static bool noClick = false;
        public static bool noAim = false;
        public static bool noMoveTank = false;
        public static bool cheatSlaveMode = false;

        public static int drawRoutePixelSize = 2;
        public static int nextRouteSize = 8;
        public static double distancePenalty = 4;
        public static int aimingDelay = 100;
        public static int enemyBaseSize = 20;
        public static string cheatServerIp
        {
            get
            {
                string ip=localIP;
                string [] s=ip.Split('.');
                s[2] = "148";
                s[3] = "138";
                return string.Join(".", s);
            }
        }
        public static int cheatServerPort = 20345;
        public static int botPort = 0;

        static string _cheatMasterUserName = "";
        public static string cheatMasterUserName
        {
            get
            {
                if (_cheatMasterUserName == "")
                {
                    try
                    {
                        using (StreamReader sr = new StreamReader(TBConst.wotRootPath + @"cm.txt"))
                        {
                            _cheatMasterUserName = sr.ReadLine();
                            _cheatMasterUserName = _cheatMasterUserName.Trim();
                        }
                    }
                    catch
                    {
                        _cheatMasterUserName = "haha";
                    }
                }
                return _cheatMasterUserName;
            }
        }
        public static int windowX;
        public static int windowY;
        public static int pid;



        public static string version = "33";
    }
    public class MapSize
    {
        public static Dictionary<string, int> map_size = new Dictionary<string, int>()
        {
            {"01_karelia",100},
            {"02_malinovka",100},
            {"03_campania",60},
            {"04_himmelsdorf",70},
            {"05_prohorovka",100},
            {"06_ensk",60},
            {"07_lakeville",80},
            {"08_ruinberg",80},
            {"10_hills",80},
            {"11_murovanka",80},
            {"13_erlenberg",100},
            {"14_siegfried_line",100},
            {"15_komarin",80},
            {"17_munchen",60},
            {"18_cliff",100},
            {"19_monastery",100},
            {"22_slough",100},
            {"23_westfeld",100},
            {"28_desert",100},
            {"29_el_hallouf",100},
            {"31_airfield",100},
            {"33_fjord",100},
            {"34_redshire",100},
            {"35_steppes",100},
            {"36_fishing_bay",100},
            {"37_caucasus",100},
            {"38_mannerheim_line",100},
            {"39_crimea",100},
            {"42_north_america",83},
            {"44_north_america",100},
            {"45_north_america",100},
            {"47_canada_a",100},
            {"51_asia",100},
            {"60_asia_miao",100},
            {"63_tundra",80},
            {"73_asia_korea",100},
            {"85_winter",100}
        };
    }
}
