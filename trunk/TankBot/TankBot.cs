using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Collections;
using System.Windows.Forms;

namespace TankBot
{
    class TankBot
    {
        #region Singleton
        static private TankBot instance = null;
        static public TankBot getInstance()
        {
            if (instance == null)
                instance = new TankBot();
            return instance;
        }
        #endregion

        private TankBot()
        {
            Helper.LogInfo("TankBot initialization");
            if (!CheckSetting.checkSetting())
            {
                Helper.LogInfo("check setting fail");
                Environment.Exit(0);
            }
            status = Status.IN_HANGAR;
            for (int i = 0; i < allyTank.Length; i++)
                allyTank[i] = new Vehicle();
            for (int i = 0; i < enemyTank.Length; i++)
                enemyTank[i] = new Vehicle();

        }
        public enum Status { IN_HANGAR, QUEUING, COUNT_DOWN, PLAYING, DIE, SHOW_BATTLE_RESULTS, RESTARTING };

        const double screen_width = 1280;
        const double screen_height = 768;

        #region definition
        //information about battle/hangar
        private Status _status;
        public Status status
        {
            get
            {
                return _status;
            }
            set
            {
                Helper.LogInfo("status change to " + value);
                _status = value;
            }
        }
        public string mapName = "";
        public Point startPos = new Point();// tank start position on minimap
        public Point enemyBase = new Point();// enemy base's location on minimap
        public Vehicle myTank = new Vehicle(); // my tank 
        public Vehicle[] allyTank = new Vehicle[14]; // 14 ally tank
        public Vehicle[] enemyTank = new Vehicle[15]; // 15 enemy tank
        public List<Point> route = new List<Point>();
        public int nextRoutePoint = 0;
        public string penetration = "";
        public bool focusTarget = false;
        public bool focusTargetUpdated;
        public double mapSacle = 100; // 100 meter per 1 box on minimap.
        public int timeLeft = 0;
        string debugMoveForwardCount;
        MapMining mapMining;

        public DateTime aimStartTime;
        private bool routeloaded;


        /// <summary>
        /// this id would be better if it is user id.
        /// level_6 is first one with aiming scope
        /// change sniper level from 0--8
        /// </summary>
        private double[] mouseHorizonRatio = new double[9];  // degree per pixel mouse move
        private double[] meterPerPixel = new double[9]; // on screen if the icon is 1 pixel away from center, the tank is ?? meter away from center(on same distance circle/panel) 
        public double gDegreeDiff;
        public int cannot_move = 0;



        private Thread aAimingThread;
        private Thread aAimingHelperThread;
        private Thread aMovingThread;
        #endregion

        bool penetrationPossible { get { return penetration == "green" || penetration == "yellow" || penetration == "orange"; } }

        public void Clear()
        {
            Helper.LogDebug("tankbot clear");
            mapName = "";
            startPos.Clear();
            enemyBase.Clear();
            myTank.Clear();
            foreach (Vehicle v in allyTank)
                v.Clear();
            foreach (Vehicle v in enemyTank)
                v.Clear();
            route.Clear();
            nextRoutePoint = 0;
            penetration = "";
            SniperMode.sniper_level = 0;
            mapSacle = 100;
            focusTarget = false;
            timeLeft = 100;
            cannot_move = 0;
            routeloaded = false;
            timeLeft = 0;

        }


        #region aiming

        /// <summary>
        /// move the tank turret when there's no enemy in sight.
        /// </summary>
        private void noAimMoveTankTurret()
        {
            if (TBConst.noMoveTank)
                return;
            double degree_diff = myTank.direction - myTank.cameraDirection;
            int x_move = Convert.ToInt32(degree_diff / mouseHorizonRatio[SniperMode.sniper_level]);
            // remove this line, 
            //while (Math.Abs(x_move) > 4000) x_move /= 2;


            //Console.WriteLine("aiming " + et.username);
            //Console.WriteLine("move cursor " + x_move + " " + y_move);
            Helper.MoveCursor(x_move, 0);
            Thread.Sleep(100);
        }
        /// <summary>
        /// trying to aim the tank et
        /// return true if we get to that position
        /// </summary>
        /// <param name="et"></param>
        /// <param name="arg_x"></param>
        /// <param name="arg_y"></param>
        /// <param name="farAway"></param>
        /// <returns></returns>
        internal void aimToTankArgxArgy(Vehicle et, double arg_x, double arg_y)
        {
            if (et.visible_on_screen == false)
            {
                throw new NotVisibleOnScreenException();
            }

            if (!et.posScreenUpdated) return;
            if (!myTank.directionUpdated) return ;
            if (!myTank.cameraDirectionUpdated) return ;


            // force everything to update to correct 
            Thread.Sleep(50);

            double cameraDirectionBeforeMouseMove = myTank.cameraDirection;

            Point beforeMouseMove = et.posScreen;
            Helper.LogDebug("aimToTank arg_x:" + arg_x + " arg_y:" + arg_y + " direction:" + myTank.direction + " cameraDirection:" + myTank.cameraDirection);

            double degree_diff = 0;

            int x_move = 0, y_move = 0;

            degree_diff = TBMath.cameraDegreeToEnemy(et, myTank);
            // if too far away, just move horizontal.

            if (et.posScreen.x > screen_width / 2 + 400 || et.posScreen.x < screen_width / 2 - 400)
            {
                x_move = Convert.ToInt32(degree_diff / mouseHorizonRatio[SniperMode.sniper_level]);
                
            }
            else // the tank is almost near in horizontal view
            {
                //---------------------------move horizontal 
                degree_diff = (et.posScreen.x - screen_width / 2);
                double dis = TBMath.distance(myTank.pos, et.pos) * mapSacle;
                degree_diff = arg_x + meterPerPixel[SniperMode.sniper_level] * degree_diff / 100 * dis; // meter from center to screen;
                degree_diff = degree_diff / (2 * Math.PI * dis) * 360;
                x_move = Convert.ToInt32(degree_diff / mouseHorizonRatio[SniperMode.sniper_level]);

                //----------------------------------move vertical
                double pixel_diff = 0;
                if (SniperMode.sniper_level >= 6)
                    pixel_diff = (et.posScreen.y - screen_height / 2); // positive then move down  pixel diff on screen
                else
                    pixel_diff = (et.posScreen.y - screen_height / 2 + 58); // fuck this 58 mm
                dis = TBMath.distance(myTank.pos, et.pos) * mapSacle;

                double meter_diff = arg_y + meterPerPixel[SniperMode.sniper_level] * pixel_diff / 100 * dis; // meter from center to screen; arg_y meter's away

                degree_diff = meter_diff / (2 * Math.PI * dis) * 360;

                //Logger("mouse move" + degree_diff / mouse_horizon_ratio);
                y_move = Convert.ToInt32(degree_diff / mouseHorizonRatio[SniperMode.sniper_level]);

                //if(Const.LoggerMode)
                //Trace.WriteLine("dis:  " + dis + " pixel_diff: " + pixel_diff + " meter_diff: " + meter_diff + " degree_diff: " + degree_diff + " y_move: " + y_move);

            }
            while (Math.Abs(x_move) > 8000)
            {
                x_move /= 2;
            }
            while (Math.Abs(y_move) > 8000)
            {
                y_move /= 2;
            }

            //Console.WriteLine("aiming " + et.username);
            //Console.WriteLine("move cursor " + x_move + " " + y_move);
            Helper.MoveCursor(x_move, y_move);

            /*
            et.posScreenUpdated = false;
            myTank.directionUpdated = false;
            myTank.cameraDirectionUpdated = false;
            if (Math.Abs(x_move) + Math.Abs(y_move) > 5)
            {
                while (beforeMouseMove.x == et.posScreen.x && beforeMouseMove.y == et.posScreen.y && et.visible_on_screen)
                    ;
            }
             * */
            Helper.LogDebug("aimToTankFinish " + arg_x + " " + arg_y + " " + myTank.direction + " " + myTank.cameraDirection);

            return;
        }
        double select_x = Double.NaN, select_y = double.NaN;
        internal void tryingUpDownRange(Vehicle v)
        {
            List<Tuple<double, double>> success = new List<Tuple<double, double>>();
            double argx = 0;
            for (double argy = 0.5; argy <= 5; argy += 0.4)
            {
                aimToTankArgxArgy(v, argx, argy);
                waitFocusTargetUpdate();
                if (this.focusTarget)
                    success.Add(new Tuple<double, double>(argx, argy));

            }
            select_x = select_y = 0;
            foreach (Tuple<double, double> t in success)
            {
                select_x += t.Item1;
                select_y += t.Item2;
            }

            if (success.Count == 0)
            {
                select_x = 0;
                select_y = 2.0;
            }
            else
            {
                select_x /= success.Count;
                select_y /= success.Count;
            }
        }

        internal bool findBestAim(int uid)
        {
            Helper.LogDebug("tryAimTank" + uid);
            if (enemyTank[uid].username == TBConst.cheatMasterUserName)
            {
                Helper.LogDebug("dont aim cheat master");
                return false;
            }
            this.focusTargetHappen = false;
            DateTime start = DateTime.Now;
            while ((DateTime.Now - start).Seconds < 1)
            {
                Random r = new Random();
                aimToTankArgxArgy(enemyTank[uid], r.NextDouble() * 4 - 2, r.NextDouble() * 4);
                if (this.focusTargetHappen)
                {
                    Helper.LogDebug("findBestAim " + uid + " return true");
                    return true;
                }
            }
            Helper.LogDebug("findBestAim " + uid + " return false");
            return false;
        }
        internal int findBestAim()
        {
            List<Tuple<double, int>> dist = new List<Tuple<double, int>>();
            for (int i = 0; i < 15; i++)
            {
                if (enemyTank[i].visible_on_screen)
                {
                    dist.Add(new Tuple<double,int>( TBMath.distance(myTank.pos, enemyTank[i].pos), i));
                }
            }
            dist.Sort();
            Helper.LogDebug("Try find best aim");
            for (int i = 0; i < dist.Count; i++)
            {
                double mindis = dist[0].Item1;
                // only try the one near the minimal distance one.
                if (dist[i].Item1 > mindis + 1)
                    break;
                int tid = dist[i].Item2;
                Helper.LogDebug("choose_tank " + tid);
                SniperMode.setSniperMode(6);
                if (findBestAim(tid))
                {
                    return tid;
                }
            }
            return -1;
        }

        private bool focusTargetHappen;    

        /// <summary>
        /// check focusTarget status
        /// and check other stuff
        /// and do the click 
        /// </summary>
        private void aimingHelperThread()
        {
            DateTime lastclick = DateTime.Now;
            while (true)
            {
                if (status != Status.PLAYING)
                    break;
                if (this.focusTarget)
                {
                    Helper.LogDebug("focusTargetHappen set to true");
                    this.focusTargetHappen = true;
                    this.focusTarget = false;
                }
                Helper.LogDebug(" penetrationPossible " + penetrationPossible);
                Helper.LogDebug(" (DateTime.Now - aimStartTime).Seconds " + (DateTime.Now - aimStartTime).Seconds);
                if (penetrationPossible && myTank.speed == 0 && (DateTime.Now - aimStartTime).Seconds > 5)
                {
                    if ((DateTime.Now - lastclick).Seconds > 0.1)
                    {
                        lastclick = DateTime.Now;
                        Helper.leftClick();
                    }
                }
                Thread.Sleep(1);
            }
        }
        private void aimSpecificTank(Vehicle v)
        {
            Helper.LogInfo("aimSpecificTank");
            DateTime start = DateTime.Now;
            double dis = TBMath.distance(myTank.pos, v.pos) * this.mapSacle;
            if (dis > 100) SniperMode.setSniperMode(8); // x8 sniper
            else if (dis > 50) SniperMode.setSniperMode(7); // x4 sniper
            else SniperMode.setSniperMode(6); // x2 sniper
            this.tryingUpDownRange(v);

            while (true)
            {
                focusTargetHappen = false;
                aimToTankArgxArgy(v, select_x, select_y);
                waitFocusTargetUpdate();
                if (focusTargetHappen == false)
                {
                    Helper.LogInfo("throw CannotAimException");
                    throw new CannotAimException();
                }
                Helper.LogDebug("aiming at tank " + v.tankname + " " + v.username);
            }
            
        }

        private void waitFocusTargetUpdate()
        {
            //Helper.LogInfo("wait focus update in");
            for (int i = 0; i < 5; i++)
            {
                focusTargetUpdated = false;
                while (!focusTargetUpdated)
                {
                    Thread.Sleep(1);
                    //Helper.LogInfo("focus updated @i " + i + " " + focusTargetUpdated);
                }
            }
            //Helper.LogInfo("wait focus update out");
        }
        private bool noEnemyOnScreen()
        {
            for (int i = 0; i < 15; i++)
            {
                if (enemyTank[i].visible_on_screen == true)
                    return false;
            }
            Helper.LogDebug("no enemy on screen");
            return true;
        }
        private void aimingThread()
        {
            int bestAim = -1;
            Thread.Sleep(2000);
            SniperMode.resetSniperLevel();
            while (true)
            {
                if (status != Status.PLAYING)
                    break;
                try
                {
                    // we are going to aim something
                    if (this.focusTargetHappen)
                    {
                        this.focusTargetHappen = false;
                        if (bestAim >= 0)
                            aimSpecificTank(enemyTank[bestAim]);
                            
                    }
                    else
                    {
                        bestAim = findBestAim();
                        if(noEnemyOnScreen())
                        {
                            SniperMode.setSniperMode(5);
                            noAimMoveTankTurret();
                        }
                    }
                    
                }
                catch (NotVisibleOnScreenException)
                {
                    Helper.LogDebug("NotVisibleOnScreenException catch");
                    bestAim = -1;
                }
                catch (CannotAimException)
                {
                    Helper.LogDebug("CannotAimException catch");
                    bestAim = -1;
                }

            }

        }


        #endregion



        #region move
        internal bool closeToAllyTank()
        {
            double rtn = 1e10;
            for (int i = 0; i < 14; i++)
            {
                if (allyTank[i].visible_on_minimap)
                {
                    double dis = TBMath.distance(allyTank[i].pos, myTank.pos);
                    if (dis < rtn && TBMath.distance(myTank.pos, startPos) > 1)
                        rtn = dis;
                }
            }
            if (TBConst.closeToAllyTankStop)
                return rtn * mapSacle < 10;
            return false;
        }
        public int support()
        {
            double dis2enemy = TBMath.distance(myTank.pos, enemyBase);
            int rtn = 0;
            for (int i = 0; i < 14; i++)
            {
                if (allyTank[i].visible_on_minimap)
                {

                    double dis = TBMath.distance(allyTank[i].pos, myTank.pos);
                    if (dis > 2)
                        continue;
                    dis = TBMath.distance(allyTank[i].pos, enemyBase);
                    if (dis < dis2enemy)
                        rtn++;
                }
            }
            return rtn;
        }
        internal void moveForward()
        {

            DateTime t = DateTime.Now;
            if (closeToAllyTank())
            {
                if (myTank.speed != 0)
                {
                    Helper.keyPress("d", Helper.KEY_TYPE.UP);
                    Helper.keyPress("a", Helper.KEY_TYPE.UP);
                    Helper.keyPress("s", Helper.KEY_TYPE.PRESS);
                }
                Thread.Sleep(1000);
                return;
            }
            if (this.cannot_move >= 3)
            {
                status = Status.DIE;
                return;
            }

            // seems we can aim to something
            if (penetrationPossible || focusTarget)
            {
                if (myTank.speed != 0)
                {
                    Helper.keyPress("d", Helper.KEY_TYPE.UP);
                    Helper.keyPress("a", Helper.KEY_TYPE.UP);
                    Helper.keyPress("s", Helper.KEY_TYPE.PRESS);
                }
                Thread.Sleep(10000);
                return;
            }

            // reach the end of the route
            if (nextRoutePoint >= route.Count)
            {
                if (myTank.speed != 0)
                {
                    Helper.keyPress("d", Helper.KEY_TYPE.UP);
                    Helper.keyPress("a", Helper.KEY_TYPE.UP);
                    Helper.keyPress("s", Helper.KEY_TYPE.PRESS);
                }
                //check move forward or not

                if (support() >= 1)
                {
                    loadRouteToEnemyBase();
                    
                }
                return;
            }

            // we can move forward now muhaha
            aimStartTime = DateTime.Now;



            Point np = route[nextRoutePoint];
            //0.1 consider as very close
            double dist2nextPoint = TBMath.distance(myTank.pos, np);
            // > 0 need to turn right
            // < 0 need to turn left
            double degreeDiff = TBMath.vehicleDegreeToPoint(np, myTank);
            this.gDegreeDiff = degreeDiff;

            if (dist2nextPoint < 0.1)
            {

                this.cannot_move = 0;
                nextRoutePoint++;
                return;
            }

            if (Math.Abs(degreeDiff) > 20 && this.myTank.speed > 10) // ooops too fast cannot turn
            {
                Helper.keyPress("s", Helper.KEY_TYPE.PRESS);
                return;
            }


            if (myTank.speed == 0 && Math.Abs(degreeDiff) < 10)
            {
                Helper.keyPress("d", Helper.KEY_TYPE.UP);
                Helper.keyPress("a", Helper.KEY_TYPE.UP);
                Helper.keyPress("r", Helper.KEY_TYPE.PRESS);
                Thread.Sleep(2000);
                if (myTank.speed == 0)
                {
                    this.cannot_move++;
                    string turn = "";
                    Random rnd = new Random();
                    if (rnd.Next(2) == 1)
                        turn = "a";
                    else
                        turn = "d";

                    Helper.keyPress("a", Helper.KEY_TYPE.UP);
                    Helper.keyPress("d", Helper.KEY_TYPE.UP);


                    Helper.keyPress("f", Helper.KEY_TYPE.PRESS);
                    Helper.keyPress("f", Helper.KEY_TYPE.PRESS);
                    Thread.Sleep(rnd.Next(1000, 5000));
                    Helper.keyPress(turn, Helper.KEY_TYPE.DOWN);
                    Thread.Sleep(rnd.Next(0, 2000));
                    Helper.keyPress(turn, Helper.KEY_TYPE.UP);
                    Helper.keyPress("r", Helper.KEY_TYPE.PRESS);
                    Helper.keyPress("r", Helper.KEY_TYPE.PRESS);
                    Helper.keyPress("r", Helper.KEY_TYPE.PRESS);
                    Thread.Sleep(rnd.Next(3000, 5000));

                }
            }
            if (Math.Abs(degreeDiff) < 10)
            {
                Helper.keyPress("d", Helper.KEY_TYPE.UP);
                Helper.keyPress("a", Helper.KEY_TYPE.UP);
                Helper.keyPress("r", Helper.KEY_TYPE.PRESS);
                return;
            }



            if (degreeDiff > -10 && degreeDiff < 10)
            {
                Helper.keyPress("d", Helper.KEY_TYPE.UP);
                Helper.keyPress("a", Helper.KEY_TYPE.UP);
                return;
            }
            if (degreeDiff < -10)
            {
                Helper.keyPress("d", Helper.KEY_TYPE.UP);
                Helper.keyPress("a", Helper.KEY_TYPE.DOWN);
                return;
            }
            if (degreeDiff > 10)
            {
                Helper.keyPress("a", Helper.KEY_TYPE.UP);
                Helper.keyPress("d", Helper.KEY_TYPE.DOWN);
                return;
            }




        }
        TimeSpan timeMovingForward = new TimeSpan();
        double cntMoveForward = 0;
        private void movingThread()
        {
            while (true)
            {
                if (noAimMove() && TBMath.distance(myTank.pos, startPos) > 3)
                {
                    Helper.keyPress("s", Helper.KEY_TYPE.PRESS);
                    Helper.keyPress("s", Helper.KEY_TYPE.PRESS);
                    Helper.keyPress("s", Helper.KEY_TYPE.PRESS);
                    Helper.keyPress("a", Helper.KEY_TYPE.UP);
                    Helper.keyPress("d", Helper.KEY_TYPE.UP);
                    break;
                }
                if (status == Status.QUEUING || status == Status.COUNT_DOWN || status == Status.PLAYING)
                {
                }
                else
                    break;
                moveForward();
                cntMoveForward++;
                debugMoveForwardCount = "MoveForward function call " + cntMoveForward /  timeMovingForward.TotalSeconds +"/s totalCnt: " + cntMoveForward;

            }
        }
        #endregion


        #region route
        private void genNextRoutePoint()
        {
            nextRoutePoint = 0;
            Trace.WriteLine("" + myTank.pos.x + " " + myTank.pos.y);
            for (int i = 0; i < route.Count; i++)
            {
                Point p = route[i];
                if (TBMath.distance(route[i], myTank.pos) <
                    TBMath.distance(route[nextRoutePoint], myTank.pos)
                    )
                    nextRoutePoint = i;
            }
            double degree_diff = TBMath.vehicleDegreeToPoint(route[nextRoutePoint], myTank);
            if (Math.Abs(degree_diff) > 90)
                nextRoutePoint++;
        }
        private void loadRouteToFirePos()
        {
            Trajectory t = mapMining.genRouteToFireposTagMap(myTank.pos);
            route.Clear();
            foreach (Point p in t)
            {
                route.Add(p);
            }
            genNextRoutePoint();
        }
        private void loadRouteToEnemyBase()
        {
            Trajectory t = mapMining.genRouteTagMap(myTank.pos, enemyBase);
            route.Clear();
            foreach (Point p in t)
            {
                route.Add(p);
            }
            genNextRoutePoint();
        }
        /// <summary>
        /// this load should be execute when down counting, i.e. when we know all the information about my location, ally team member, enemy team member; 
        /// because the fucking algorithm runs slow.... 
        /// </summary>
        private void loadRoute()
        {
            Helper.LogInfo("load route");
            if (TBConst.noMoveTank)
            {
                Helper.LogInfo("load route returns noMoveTank");
                return;
            }
            if (this.routeloaded)
            {
                Helper.LogInfo("load route returns already loaded");
                return;
            }
            this.routeloaded = true;
            while (this.mapName == "")
                Thread.Sleep(1000);
            while(myTank.pos.x==6   && myTank.pos.y==6)
                Thread.Sleep(1000);
            Thread.Sleep(3000);
            Helper.LogInfo("loadroute may name " + mapName);
            Helper.LogInfo("loadroute my tank pos " + myTank.pos);

            mapName = this.mapName.Trim();
            this.mapSacle = MapDef.map_size[mapName];

            mapMining = new MapMining(this.mapName);
            
            this.enemyBase = mapMining.enemyBase(myTank.pos);

            //if (myTank.vInfo.vClass == VehicleClass.HT)
            //always use a fire route
            if (TBConst.cheatSlaveMode && CheatClient.getInstance().cheatMasterOnOtherSide())
                loadRouteToFirePos();
            else if (TBConst.cheatSlaveMode)
                loadRouteToEnemyBase();
            else
                loadRouteToFirePos(); // HT choose a more far away route
            //else
              //  loadRouteToEnemyBase();
            this.startPos = myTank.pos;
        }
        #endregion



        private void initArgsInBattle()
        {
            this.Clear();
            //for(int i=0; i<8; i++)
            //generate_arguments(i);

            mouseHorizonRatio[0] = 0.0413449417970914;
            mouseHorizonRatio[1] = mouseHorizonRatio[0];
            mouseHorizonRatio[2] = mouseHorizonRatio[0];
            mouseHorizonRatio[3] = mouseHorizonRatio[0];
            mouseHorizonRatio[4] = mouseHorizonRatio[0];
            mouseHorizonRatio[5] = mouseHorizonRatio[0];


            meterPerPixel[0] = 0.181995463740876;
            meterPerPixel[1] = 0.174721947322232;
            meterPerPixel[2] = 0.167231465529456;
            meterPerPixel[3] = 0.160001291946903;
            meterPerPixel[4] = 0.152397002754927;
            meterPerPixel[5] = 0.14801988592115;


            // this is hard code parameter 
            mouseHorizonRatio[6] = 0.02103390909437;
            mouseHorizonRatio[7] = mouseHorizonRatio[6] / 2;
            mouseHorizonRatio[8] = mouseHorizonRatio[6] / 4;

            meterPerPixel[6] = 0.0690675193529361; // at 100 meter 
            meterPerPixel[7] = meterPerPixel[6] / 2;
            meterPerPixel[8] = meterPerPixel[6] / 4;
            return;

        }

        private bool noAimMove()
        {
            if (TBConst.cheatSlaveMode && CheatClient.getInstance().cheatMasterOnOtherSide())
                return true;
            return false;
        }
        public void abortThread()
        {
            Helper.LogInfo("TankBot abortThread");
            try { aAimingThread.Abort(); }
            catch { }
            try { aAimingHelperThread.Abort(); }
            catch { }
            try { aMovingThread.Abort(); }
            catch { }
            this.Clear();
        }
        public void startThread()
        {
            Helper.LogInfo("---------TankBot startThread------------");

            //for xvm comm to init the argument 
            //Thread.Sleep(1000);
            while (XvmComm.getInstance().messageCnt < 1000)
                ; 
            initArgsInBattle();
            while (true)
            {

                if (status == Status.IN_HANGAR)
                {
                    this.Clear();
                    if (TBConst.cheatSlaveMode)
                    {
                        Thread.Sleep(1000);
                        continue;
                    }
                    Helper.LogInfo("click tank for battle");
                    foreach (int p in TankAction.clickOrder())
                    {
                        //Console.WriteLine("click the tank " + p);
                        TankAction.clickTank(p);
                        TankAction.clickStart();
                    }
                    Thread.Sleep(20000);
                }
                else if (status == Status.QUEUING || status == Status.COUNT_DOWN)
                {
                    if (timeLeft > 60)
                    {
                        status = Status.PLAYING;
                        continue;
                    }
                    Thread.Sleep(5000);
                    this.Clear();
                    Thread.Sleep(5000);
                    loadRoute();
                    while (true)
                    {
                        if (timeLeft > 100 || timeLeft < 1)
                        {
                            status = Status.PLAYING;
                            Thread.Sleep(1000);
                            break;
                        }
                    }
                }
                else if (status == Status.PLAYING)
                {
                    if (!TBConst.noAim)
                    {
                        Helper.LogInfo("start aimingThread");
                        aAimingThread = new Thread(new ThreadStart(this.aimingThread));
                        aAimingHelperThread = new Thread(new ThreadStart(this.aimingHelperThread));
                        aAimingThread.Start();
                        aAimingHelperThread.Start();
                    }


                    if (!TBConst.noMoveTank)
                    {
                        loadRoute();

                        Helper.LogInfo("start movingThread");
                        aMovingThread = new Thread(new ThreadStart(this.movingThread));
                        aMovingThread.Start();
                    }

                    if (!TBConst.noAim)
                    {
                        aAimingHelperThread.Join();
                        Helper.LogInfo("join aAimingHelperThread");
                        aAimingThread.Join();
                        Helper.LogInfo("join aimingThread");
                    }
                    if (!TBConst.noMoveTank)
                    {

                        aMovingThread.Join();
                        Helper.LogInfo("join movingThread");
                    }
                }
                else if (status == Status.DIE)
                {
                    if (TBConst.cheatSlaveMode && CheatClient.getInstance().cheatMasterOnOtherSide())
                    {
                        Helper.LogInfo("DIE and left click to change view");
                        Helper.leftClickSlow();
                        Thread.Sleep(5000);
                        continue;
                    }
                    TankAction.exitToHangar();

                }
                else if (status == Status.SHOW_BATTLE_RESULTS)
                {
                    Helper.LogInfo("press esc to eliminate battle results");
                    Thread.Sleep(5000);
                    //ESC
                    Helper.bringToFront();
                    Helper.keyPress("b", Helper.KEY_TYPE.PRESS);
                    Thread.Sleep(1000);
                    status = Status.IN_HANGAR;
                }
                Thread.Sleep(1000);
            }

        }
        internal string debugString()
        {
            string str = "";
            str += "\r\n" + debugMoveForwardCount;
            str += "\r\n" + "focusTargetUpdated: " + focusTargetUpdated;
            str += "\r\n" + "map_scale: " + mapSacle;
            str += "\r\n" + "support: " + support();
            str += "\r\n" + "cannot_move: " + cannot_move;
            str += "\r\n" + "g_degree_diff: " + gDegreeDiff;
            str += "\r\n" + "Sniper Level: " + SniperMode.sniper_level;
            str += "\r\n" + "Status: " + status;
            str += "\r\n" + "Mytank Name: " + myTank.tankname + " " + myTank.vInfo.tier + " " + myTank.vInfo.vClass;
            str += "\r\n" + "Health: " + myTank.health;
            str += "\r\n" + "Speed: " + myTank.speed;
            str += "\r\n" + "Focus: " + focusTarget;
            str += "\r\n" + "time_left: " + timeLeft;
            return str;
        }



    }
}


