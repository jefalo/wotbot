using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace TankBot
{
    public class Trajectory : ArrayList
    {
        public bool reversed = false;
        public Trajectory()
        {
        }
        public void add_point(Point p)
        {
            this.Add(p);
        }

        new public Point this[int i]
        {
            get { return (Point)base[i]; }
        }
        public string comment;


    }
    public class MapMining
    {
        public static int d2i(double x)
        {
            return (int)((x - 1) * WIDTH / 10.0);
        }
        public static double i2d(int x)
        {
            return (double)(x * 10.0 / WIDTH + 1);
        }

        #region definition
        public const int WIDTH = 512;
        public const int expand = 3;

        public bool[,] visit = new bool[WIDTH, WIDTH];
        public bool[,] tagMap = new bool[WIDTH, WIDTH];
        public double[,] score = new double[WIDTH, WIDTH];
        public Tuple<int, int>[,] prev = new Tuple<int, int>[WIDTH, WIDTH];


        public List<Trajectory> trajs = new List<Trajectory>();
        private string map_name;
        public List<Point> allPoints = new List<Point>();
        public int[,] heatmap = new int[WIDTH, WIDTH];


        public List<Point> firepos = new List<Point>();
        public List<Point> startPoints = new List<Point>();
        public Color frequentColor;
        #endregion


        /// <summary>
        /// initialize with map name
        /// will load all the trajectory
        /// </summary>
        /// <param name="_map_name"> for example "01_karelia" </param>
        public MapMining(string _map_name)
        {
            Helper.LogInfo("MapMing " + _map_name);
            try
            {
                if (_map_name == "")
                    return;
                this.map_name = _map_name;
                loadTrajectory_obsolete();
                loadFirepos();
                loadTag();
                foreach (Trajectory t in trajs)
                {
                    if (!t.reversed)
                    {
                        startPoints.Add(t[0]);
                    }
                }
                startPoints.Sort();
            }
            catch
            {
                Helper.LogException("map ming init error");
            }
        }
        private void loadFirepos()
        {
            try
            {

                using (StreamReader sr = new StreamReader(TBConst.fireposPath + this.map_name + ".txt"))
                {
                    while (true)
                    {
                        if (sr.EndOfStream)
                            break;
                        double x, y;
                        string s = sr.ReadLine();
                        string[] sp = s.Split(' ');
                        if (sp.Length == 2)
                        {
                            x = Convert.ToDouble(sp[0]);
                            y = Convert.ToDouble(sp[1]);
                            firepos.Add(new Point(x, y));
                        }
                    }
                }
            }
            catch
            {
                Helper.LogException("load fire pos failed");
            }
        }
        private void loadTag()
        {
            try
            {
                Bitmap bmp = new Bitmap(TBConst.tagPath + this.map_name + ".bmp");
                

                // Lock the bitmap's bits.  
                Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
                BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

                // Get the address of the first line.
                IntPtr ptr = bmpData.Scan0;

                // Declare an array to hold the bytes of the bitmap.
                int bytes = bmpData.Stride * bmp.Height;
                byte[] rgbValues = new byte[bytes];
                byte[] r = new byte[bytes / 3];
                byte[] g = new byte[bytes / 3];
                byte[] b = new byte[bytes / 3];

                // Copy the RGB values into the array.
                Marshal.Copy(ptr, rgbValues, 0, bytes);

                int count = 0;
                int stride = bmpData.Stride;

                for (int column = 0; column < bmpData.Height; column++)
                {
                    for (int row = 0; row < bmpData.Width; row++)
                    {
                        b[count] = (byte)(rgbValues[(column * stride) + (row * 3)]);
                        g[count] = (byte)(rgbValues[(column * stride) + (row * 3) + 1]);
                        r[count++] = (byte)(rgbValues[(column * stride) + (row * 3) + 2]);
                    }
                }
                Dictionary<Color, int> d = new Dictionary<Color, int>();
                for (int i = 0; i < r.Length; i++)
                {
                    Color c = Color.FromArgb(r[i], g[i], b[i]);
                    if (!d.ContainsKey(c))
                        d[c] = 0;
                    d[c]++;
                }
                
                frequentColor = Color.FromArgb(0, 0, 0);
                foreach (Color c in d.Keys)
                {
                    if (frequentColor == Color.FromArgb(0, 0, 0))
                        frequentColor = c;
                    if (d[frequentColor] < d[c])
                        frequentColor = c;
                }
                tagMap = new bool[WIDTH, WIDTH];
                for (int i = 0; i < r.Length; i++)
                {
                    if (Color.FromArgb(r[i], g[i], b[i]) == frequentColor)
                        tagMap[i % WIDTH, i / WIDTH] = true;
                    else
                        tagMap[i % WIDTH, i / WIDTH] = false;
                }
                 
                bmp.Dispose();
            }
            catch
            {
                Helper.LogException("load tag failed");
            }
        }
        public void addToHeatmap(int x, int y)
        {
            for (int i = -expand; i <= expand; i++)
                for (int j = -expand; j <= expand; j++)
                {
                    int nx = x + i;
                    int ny = y + j;
                    if (!available(nx, ny))
                        continue;
                    this.heatmap[nx, ny] += expand * expand * 2 - Math.Abs(i) * Math.Abs(i) - Math.Abs(j) * Math.Abs(j);
                }
        }



        /// <summary>
        /// check whether the axis located inside 0--WIDTH range
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private bool available(int x, int y)
        {
            if (x < 0 || x >= WIDTH) return false;
            if (y < 0 || y >= WIDTH) return false;
            return true;
        }

        /// <summary>
        /// add line x0,y0 x1,y1 to the heat map
        /// using **** algorithm to calculat the nodes that should add
        /// expand a node to EXPAND*EXPAND area
        /// </summary>
        /// <param name="x0"></param>
        /// <param name="y0"></param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        public void addToHeatmap(int x0, int y0, int x1, int y1)
        {
            if (!available(x0, y0)) return;


            if (!available(x1, y1)) return;

            int dx = Math.Abs(x1 - x0);
            int dy = Math.Abs(y1 - y0);
            int sx, sy;
            if (x0 < x1) sx = 1; else sx = -1;
            if (y0 < y1) sy = 1; else sy = -1;
            double err = dx - dy;

            while (true)
            {
                addToHeatmap(x0, y0);
                //Console.WriteLine("plot " + x0 + " " + y0);

                //plot(x0,y0)
                if (x0 == x1 && y0 == y1)
                    break;
                double e2 = 2 * err;
                if (e2 > -dy)
                {
                    err = err - dy;
                    x0 = x0 + sx;
                }
                if (x0 == x1 && y0 == y1)
                {
                    addToHeatmap(x0, y0);
                    break;
                }
                if (e2 < dx)
                {
                    err = err + dx;
                    y0 = y0 + sy;
                }
            }

        }

        /// <summary>
        /// add the trajectory to the WIDTH * WIDTH heat map
        /// </summary>
        /// <param name="traj"></param>
        private void addToHeapmap_obsolete(Trajectory traj)
        {
            for (int i = 1; i < traj.Count; i++)
            {
                Point p0 = traj[i - 1];
                Point p1 = traj[i];
                int x0 = d2i(p0.x);
                int y0 = d2i(p0.y);
                int x1 = d2i(p1.x);
                int y1 = d2i(p1.y);
                addToHeatmap(x0, y0, x1, y1);
            }
        }

        /// <summary>
        /// remove the nodes in trajectory in order to for sure 
        /// that the distance between any nodes are larger than threshold
        /// </summary>
        /// <param name="t"></param>
        /// <param name="threshhold"></param>
        /// <returns></returns>
        private Trajectory pruneTrajectory(Trajectory t, double threshhold = 0.1)
        {
            Trajectory xo = new Trajectory();
            foreach (Point p in t)
            {
                if (xo.Count == 0)
                {
                    xo.add_point(p);
                    continue;
                }
                double mindis = 1e10;
                foreach (Point x in xo)
                {
                    mindis = Math.Min(mindis, TBMath.distance(x, p));
                }
                if (mindis > threshhold)
                    xo.add_point(p);
            }
            return xo;
        }






        /// <summary>
        /// get maximum length of the traj in trajs
        /// </summary>
        /// <returns></returns>
        public int maxCount()
        {
            int rtn = 0;
            foreach (Trajectory t in trajs)
            {
                rtn = Math.Max(rtn, t.Count);
            }
            return rtn;
        }

        /// <summary>
        /// get the enemy base's location according to the start location and all other guy's starting location
        /// </summary>
        /// <param name="startPoint"></param>
        /// <returns></returns>
        public Point enemyBase(Point startPoint)
        {
            try
            {
                Point p1 = MapDef.basePos[this.map_name].Item1;
                Point p2 = MapDef.basePos[this.map_name].Item2;
                            
                p1 = TBMath.BigWorldPos2MinimapPos(p1, map_name);
                p2 = TBMath.BigWorldPos2MinimapPos(p2, map_name);
                if (TBMath.distance(startPoint, p1) < TBMath.distance(startPoint, p2))
                    return p2;
                else
                    return p1;
            }
            catch
            {
                throw new EnemyBaseReadException();
            }
            throw new EnemyBaseReadException();
        }

        /// <summary>
        /// get the key point I would like to go 
        /// </summary>
        /// <param name="start"></param>
        /// <returns></returns>
        public Point getFirepos(Point start)
        {
            double mindis = 1e10;
            Point rtn = new Point();
            for (int i = 0; i < firepos.Count; i++)
            {
                if (mindis > TBMath.distance(firepos[i], start))
                {
                    mindis = TBMath.distance(firepos[i], start);
                    rtn = firepos[i];
                }
            }
            return rtn;
        }



        public void BFS(int sx, int sy, int threshhold = 2)
        {
            visit = new bool[WIDTH, WIDTH];
            score = new double[WIDTH, WIDTH];
            prev = new Tuple<int, int>[WIDTH, WIDTH];
            Queue<Tuple<double, Tuple<int, int>>> q = new Queue<Tuple<double, Tuple<int, int>>>();
            q.Enqueue(new Tuple<double, Tuple<int, int>>(1, new Tuple<int, int>(sx, sy)));


            List<int> dx = new List<int>();
            List<int> dy = new List<int>();
            for (int i = -threshhold; i <= threshhold; i++)
                for (int j = -threshhold; j <= threshhold; j++)
                {
                    dx.Add(i);
                    dy.Add(j);
                }
            int loop = 0;
            bool visitTag = false;
            while (q.Count > 0)
            {
                Tuple<double, Tuple<int, int>> min = q.First();

                int x = min.Item2.Item1;
                int y = min.Item2.Item2;
                if (visit[x, y])
                {
                    q.Dequeue();
                    continue;
                }
                loop++;
                //if (loop % 100 == 0)
                


                visit[x, y] = true;
                for (int i = 0; i < dx.Count; i++)
                {
                    int nx = x + dx[i];
                    int ny = y + dy[i];
                    if (nx < 0) continue;
                    if (nx >= WIDTH) continue;
                    if (ny < 0) continue;
                    if (ny >= WIDTH) continue;
                    if (visit[nx, ny])
                        continue;

                    if (tagMap[nx, ny])
                    {
                        visitTag = true;
                    }
                    else
                    {
                        if (visitTag)
                            continue;
                    }
                   
                    double newscore = score[x, y] + 1;
                    double oldscore = score[nx, ny];
                    if (oldscore == 0 || oldscore > newscore)
                    {
                        score[nx, ny] = newscore;
                        prev[nx, ny] = new Tuple<int, int>(x, y);
                        q.Enqueue(new Tuple<double, Tuple<int, int>>(newscore, new Tuple<int, int>(nx, ny)));
                    }
                }
                q.Dequeue();
            }
        }
        private Trajectory backtrace(Point source, Point target)
        {
            int endx = d2i(target.x);
            int endy = d2i(target.y);

            int fx = 0, fy = 0;
            for (int step = 0; ; step++)
            {
                for (int i = -step; i <= step; i++)
                    for (int j = -step; j <= step; j++)
                    {
                        if (endx + i < 0) return new Trajectory();
                        if (endy + j < 0) return new Trajectory();

                        if (endx + i >= WIDTH) return new Trajectory();
                        if (endy + j >= WIDTH) return new Trajectory();

                        if (visit[endx + i, endy + j])
                        {
                            fx = endx + i;
                            fy = endy + j;
                            goto brk;
                        }
                    }
            }
        brk:
            Trajectory t = new Trajectory();
            while (true)
            {
                if (fx == d2i(source.x) && fy == d2i(source.y))
                    break;
                t.add_point(new Point(i2d(fx), i2d(fy)));
                Tuple<int, int> pre = prev[fx, fy];
                fx = pre.Item1;
                fy = pre.Item2;
            }
            Trajectory rt = new Trajectory();
            for (int i = t.Count - 1; i >= 0; i--)
            {
                rt.add_point(t[i]);
            }
            return rt;
        }
        public Trajectory genRouteTagMap(Point source, Point target)
        {

            BFS(d2i(source.x), d2i(source.y), 1);
            return backtrace(source, target);
        }
        public Trajectory genRouteToFireposTagMap(Point source)
        {
            return genRouteTagMap(source, getFirepos(source));
        }





        #region obsolete

        private void loadTrajectory_obsolete()
        {
            using (StreamReader sr = new StreamReader(TBConst.trajectoryPath_obsolete + this.map_name + ".txt"))
            {
                int tot_point = 0;
                while (true)
                {
                    if (sr.EndOfStream)
                        break;
                    Trajectory traj = new Trajectory();
                    string comment = sr.ReadLine(); //replay_id 19 user_id 1686
                    string line = sr.ReadLine(); // 9.8 93.15 9.9 93.0 10.05 92.8 .......
                    string[] sp = line.Split(' ');
                    //for (int i = 0; i < sp.Length; i += 2)
                    {
                        int i = 0;
                        double x = Convert.ToDouble(sp[i]);
                        double y = Convert.ToDouble(sp[i + 1]);
                        x = (x + 105) / 21 + 1;
                        y = (y + 105) / 21 + 1;
                        traj.add_point(new Point(x, y));
                        
                    }
                    traj.comment = comment;
                    traj = pruneTrajectory(traj, 0.05);
                    addToHeapmap_obsolete(traj);
                    traj.reversed = false;
                    trajs.Add(traj);
                    Trajectory rev = new Trajectory();
                    foreach (Point p in traj)
                    {
                        rev.add_point(p);
                    }
                    rev.Reverse();
                    rev.reversed = true;
                    trajs.Add(rev);
                    tot_point += traj.Count * 2;
                }
                
                

            }

        }
        #endregion



        /// <summary>
        /// return whether point p is Tagged as reachable in tagmap
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public bool withinTagMap(Point p)
        {
            int x = d2i(p.x);
            int y = d2i(p.y);
            return tagMap[x, y];
        }
    }
}
