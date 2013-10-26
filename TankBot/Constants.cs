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
        public static string version = "49";

        public static bool RealBattle = true;
        static string _wotRootPath = "";
        public static string wotRootPath
        {
            get
            {
                if (_wotRootPath == "")
                {
                    _wotRootPath = Directory.GetParent(Directory.GetCurrentDirectory()).ToString() + "\\";
                    //Helper.LogInfo("set root path" + _wotRootPath);
                }
                return _wotRootPath;
            }
        }
        public static string dataPath = TBConst.wotRootPath + @"res_mods\Data\";
        public static string ahkPath = TBConst.wotRootPath + @"res_mods\key_map.exe";
        public static string logFile = TBConst.wotRootPath + @"tankbot.log";
        public static string basePath = TBConst.wotRootPath + @"res_mods\base\base.txt";

        public static string logFileMessage = TBConst.wotRootPath + @"tankbot_message.log";
        private static string _localIP="0.0.0.0";
        public static string localIP
        {
            get
            {
                if (_localIP != "0.0.0.0")
                    return _localIP;
                IPHostEntry host;
                
                host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (IPAddress ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        if (ip.ToString().StartsWith("192.168"))
                            continue;
                        _localIP = ip.ToString();
                        Helper.LogInfo("local ip: " + localIP);
                    }
                }
                if (localIP == "0.0.0.0")
                    Helper.LogException("cannot obtain local IP");
                return _localIP;
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
        public static string trajectoryPath_obsolete = TBConst.wotRootPath + @"res_mods\trajectory\";
        public static string fireposPath = TBConst.wotRootPath + @"res_mods\firepos\";
        public static string tagPath = TBConst.wotRootPath + @"res_mods\tag\";
        public static string jpgPath = TBConst.wotRootPath + @"res_mods\jpg\";
        public static double noActionRestartMinutes = 10;

        public static bool LogMessage = false;
        public static bool LogToConsole = false;
        public static bool LogDebug = false;
        public static bool LogInfo = true;
        public static bool closeToAllyTankStop = false;

        public static bool noClick = false;
        public static bool noAim = false;
        public static bool noMoveTank = false;
        public static bool cheatSlaveMode = true;
        /// <summary>
        /// in release mode, no output would display
        /// </summary>
        public static bool releaseMode = false;

        public static int drawRoutePixelSize = 2;
        public static int nextRouteSize = 8;
        public static double distancePenalty = 4;
        public static int aimingDelay = 100;
        public static int enemyBaseDrawPointSize = 20;
        public static string cheatServerIp
        {
            get
            {
                string ip = localIP;
                string[] s = ip.Split('.');
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
        public static IntPtr wotHandle;





    }
    public class MapDef
    {
        public static Dictionary<string, Tuple<Point, Point>> boundingBox = new Dictionary<string, Tuple<Point, Point>>()
        {
            {"01_karelia", new Tuple<Point,Point> (new Point(-500.0 ,-500.0 ), new Point(500.0 , 500.0))},
            {"02_malinovka", new Tuple<Point,Point> (new Point(-500.0 ,-500.0 ), new Point(500.0 , 500.0))},
            {"03_campania", new Tuple<Point,Point> (new Point(-300.0 ,-300.0 ), new Point(300.0 , 300.0))},
            {"04_himmelsdorf", new Tuple<Point,Point> (new Point(-300.0 ,-300.0 ), new Point(400.0 , 400.0))},
            {"05_prohorovka", new Tuple<Point,Point> (new Point(-500.0 ,-500.0 ), new Point(500.0 , 500.0))},
            {"06_ensk", new Tuple<Point,Point> (new Point(-300.0 ,-300.0 ), new Point(300.0 , 300.0))},
            {"07_lakeville", new Tuple<Point,Point> (new Point(-400.0 ,-400.0 ), new Point(400.0 , 400.0))},
            {"08_ruinberg", new Tuple<Point,Point> (new Point(-400.0 ,-400.0 ), new Point(400.0 , 400.0))},
            {"10_hills", new Tuple<Point,Point> (new Point(-400.0 ,-400.0 ), new Point(400.0 , 400.0))},
            {"11_murovanka", new Tuple<Point,Point> (new Point(-400.0 ,-400.0 ), new Point(400.0 , 400.0))},
            {"13_erlenberg", new Tuple<Point,Point> (new Point(-500.0 ,-500.0 ), new Point(500.0 , 500.0))},
            {"14_siegfried_line", new Tuple<Point,Point> (new Point(-500.0 ,-500.0 ), new Point(500.0 , 500.0))},
            {"15_komarin", new Tuple<Point,Point> (new Point(-400.0 ,-400.0 ), new Point(400.0 , 400.0))},
            {"17_munchen", new Tuple<Point,Point> (new Point(-300.0 ,-300.0 ), new Point(300.0 , 300.0))},
            {"18_cliff", new Tuple<Point,Point> (new Point(-500.0 ,-500.0 ), new Point(500.0 , 500.0))},
            {"19_monastery", new Tuple<Point,Point> (new Point(-500.0 ,-500.0 ), new Point(500.0 , 500.0))},
            {"22_slough", new Tuple<Point,Point> (new Point(-500.0 ,-500.0 ), new Point(500.0 , 500.0))},
            {"23_westfeld", new Tuple<Point,Point> (new Point(-500.0 ,-500.0 ), new Point(500.0 , 500.0))},
            {"28_desert", new Tuple<Point,Point> (new Point(-500.0 ,-500.0 ), new Point(500.0 , 500.0))},
            {"29_el_hallouf", new Tuple<Point,Point> (new Point(-500.0 ,-500.0 ), new Point(500.0 , 500.0))},
            {"31_airfield", new Tuple<Point,Point> (new Point(-500.0 ,-500.0 ), new Point(500.0 , 500.0))},
            {"33_fjord", new Tuple<Point,Point> (new Point(-500.0 ,-500.0 ), new Point(500.0 , 500.0))},
            {"34_redshire", new Tuple<Point,Point> (new Point(-500.0 ,-500.0 ), new Point(500.0 , 500.0))},
            {"35_steppes", new Tuple<Point,Point> (new Point(-500.0 ,-500.0 ), new Point(500.0 , 500.0))},
            {"36_fishing_bay", new Tuple<Point,Point> (new Point(-500.0 ,-500.0 ), new Point(500.0 , 500.0))},
            {"37_caucasus", new Tuple<Point,Point> (new Point(-500.0 ,-500.0 ), new Point(500.0 , 500.0))},
            {"38_mannerheim_line", new Tuple<Point,Point> (new Point(-500.0 ,-500.0 ), new Point(500.0 , 500.0))},
            {"39_crimea", new Tuple<Point,Point> (new Point(-500.0 ,-500.0 ), new Point(500.0 , 500.0))},
            {"42_north_america", new Tuple<Point,Point> (new Point(-400.0 ,-430.0 ), new Point(430.0 , 400.0))},
            {"44_north_america", new Tuple<Point,Point> (new Point(-500.0 ,-500.0 ), new Point(500.0 , 500.0))},
            {"45_north_america", new Tuple<Point,Point> (new Point(-500.0 ,-500.0 ), new Point(500.0 , 500.0))},
            {"47_canada_a", new Tuple<Point,Point> (new Point(-500.0 ,-500.0 ), new Point(500.0 , 500.0))},
            {"60_asia_miao", new Tuple<Point,Point> (new Point(-500.0 ,-500.0 ), new Point(500.0 , 500.0))},
            {"63_tundra", new Tuple<Point,Point> (new Point(-400.0 ,-400.0 ), new Point(400.0 , 400.0))},
            {"73_asia_korea", new Tuple<Point,Point> (new Point(-500.0 ,-500.0 ), new Point(500.0 , 500.0))},
            {"85_winter", new Tuple<Point,Point> (new Point(-500.0 ,-500.0 ), new Point(500.0 , 500.0))}
        };
        public static Dictionary<string, Tuple<Point, Point>> basePos = new Dictionary<string, Tuple<Point, Point>>()
        {
            {"01_karelia", new Tuple<Point, Point> ( new Point(397.579529 , 402.584381), new Point( -401.340149 , -400.062683))},
            {"02_malinovka", new Tuple<Point, Point> ( new Point(75.599945 , -391.920929), new Point( -372.700012 , 108.119667))},
            {"03_campania", new Tuple<Point, Point> ( new Point(8.663710 , -210.228149), new Point( -0.173965 , 209.417648))},
            {"04_himmelsdorf", new Tuple<Point, Point> ( new Point(2.499999 , -252.600128), new Point( 69.099937 , 348.999969))},
            {"05_prohorovka", new Tuple<Point, Point> ( new Point(-125.100014 , 448.300018), new Point( 51.599991 , -447.000000))},
            {"06_ensk", new Tuple<Point, Point> ( new Point(20.299999 , 249.699997), new Point( 19.099998 , -248.700012))},
            {"07_lakeville", new Tuple<Point, Point> ( new Point(-169.513458 , 319.351288), new Point( -169.513443 , -319.048615))},
            {"08_ruinberg", new Tuple<Point, Point> ( new Point(-66.399994 , 306.100006), new Point( -82.900002 , -290.899994))},
            {"10_hills", new Tuple<Point, Point> ( new Point(175.783722 , -305.848389), new Point( -236.649063 , 329.674500))},
            {"11_murovanka", new Tuple<Point, Point> ( new Point(202.800003 , 296.099976), new Point( -205.000015 , -292.799988))},
            {"13_erlenberg", new Tuple<Point, Point> ( new Point(-146.199997 , -0.100014), new Point( 146.399994 , 0.100015))},
            {"14_siegfried_line", new Tuple<Point, Point> ( new Point(255.799988 , -439.830017), new Point( 283.845886 , 434.601990))},
            {"15_komarin", new Tuple<Point, Point> ( new Point(160.870193 , -303.957031), new Point( -175.401016 , 304.407318))},
            {"17_munchen", new Tuple<Point, Point> ( new Point(-83.649483 , -201.658936), new Point( 61.649 , 242.999))},
            {"18_cliff", new Tuple<Point, Point> ( new Point(-273.786 , -436.601), new Point( -251.606 , 434.591))},
            {"19_monastery", new Tuple<Point, Point> ( new Point(20.061766 , -387.906860), new Point( -0.393303 , 397.375824))},
            {"22_slough", new Tuple<Point, Point> ( new Point(-403.600006 , -424.100006), new Point( 383.300018 , 422.700012))},
            {"23_westfeld", new Tuple<Point, Point> ( new Point(-300.099884 , -339.599976), new Point( 339.399902 , 299.768127))},
            {"28_desert", new Tuple<Point, Point> ( new Point(373.485199 , -178.960999), new Point( -405.038452 , 137.526276))},
            {"29_el_hallouf", new Tuple<Point, Point> ( new Point(299.255981 , 319.405975), new Point( -338.583160 , -319.307434))},
            {"31_airfield", new Tuple<Point, Point> ( new Point(360.648438 , -154.437271), new Point( -324.047974 , -176.182037))},
            {"33_fjord", new Tuple<Point, Point> ( new Point(399.095856 , -42.143173), new Point( -381.328522 , 111.410522))},
            {"34_redshire", new Tuple<Point, Point> ( new Point(368.678406 , -269.466949), new Point( -209.877243 , 368.253113))},
            {"35_steppes", new Tuple<Point, Point> ( new Point(228.220047 , -341.927917), new Point( -88.818558 , 361.861725))},
            {"36_fishing_bay", new Tuple<Point, Point> ( new Point(-84.829124 , 397.810852), new Point( -17.017586 , -396.105652))},
            {"37_caucasus", new Tuple<Point, Point> ( new Point(-376.743073 , 371.289215), new Point( 345.804749 , -399.466736))},
            {"38_mannerheim_line", new Tuple<Point, Point> ( new Point(398.135315 , 293.874969), new Point( -338.180603 , -306.263550))},
            {"39_crimea", new Tuple<Point, Point> ( new Point(106.300003 , -402.543488), new Point( 114.694191 , 350.556519))},
            {"42_north_america", new Tuple<Point, Point> ( new Point(-191.100006 , -315.200012), new Point( 318.000000 , 286.299988))},
            {"44_north_america", new Tuple<Point, Point> ( new Point(-356.987122 , -329.806335), new Point( 300.193024 , 363.934174))},
            {"45_north_america", new Tuple<Point, Point> ( new Point(197.412552 , 356.577179), new Point( -343.146606 , -327.370056))},
            {"47_canada_a", new Tuple<Point, Point> ( new Point(-126.886963 , -305.908661), new Point( 213.120361 , 328.109039))},
            {"60_asia_miao", new Tuple<Point, Point> ( new Point(-46.235470 , 195.650360), new Point( 369.455597 , -65.344101))},
            {"63_tundra", new Tuple<Point, Point> ( new Point(85.432190 , -167.152176), new Point( -72.202568 , 141.256195))},
            {"73_asia_korea", new Tuple<Point, Point> ( new Point(271.300 , 252.7), new Point( -270.591 , -298.420))},
            {"85_winter", new Tuple<Point, Point> ( new Point(209.8 , 353.700), new Point( -171.593 , -308.134))}
        };
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
