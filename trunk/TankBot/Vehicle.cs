using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TankBot
{
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
        

        /// <summary>
        /// north(up_minimap) consider 0 degree
        /// east(right) consider 90 degree
        /// wese(left) consider -90 degree
        /// south(down) is -180 as well as 180
        /// vehicle head direction
        /// </summary>
        public bool directionUpdated;
        public double direction;
        // camera's direction
        public bool cameraDirectionUpdated;
        public double cameraDirection;
        public string icon;

    }
}
