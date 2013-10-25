using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Diagnostics;

namespace TankBot
{
    class CheckSetting
    {
        internal static bool checkSetting()
        {
            Helper.LogInfo("checking preferences.xml setting");
            string fileName= Path.Combine( Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Wargaming.net\WorldOfTanks\preferences.xml");
            
            Dictionary<string, string> d=new Dictionary<string,string>();
            d["sniperMode"] = "0.147700";
            d["arcadeMode"] = "0.288641";
            d["strategicMode"] = "1.000000";
            
            using (XmlReader reader = XmlReader.Create(fileName))
            {
                string mode ="";
                while (reader.Read())
                {
                    if (reader.Name =="sniperMode" || reader.Name =="arcadeMode" || reader.Name == "strategicMode" )
                        mode = reader.Name;
                    if (reader.Name == "sensitivity")
                    {
                        string x = reader.ReadElementContentAsString();
                        x=x.Trim();
                        if (d[mode] != x)
                            return false;
                    }
                }
            }
            return true;
        }
    }
}
