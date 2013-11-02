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
    public enum Status { IN_HANGAR, QUEUING, COUNT_DOWN, PLAYING, DIE, SHOW_BATTLE_RESULTS, RESTARTING };
    public class TankBot
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


        #region definition

        public bool focusTargetHappen;
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
        public bool penetrationPossible { get { return penetration == "green" || penetration == "yellow" || penetration == "orange"; } }
        public bool focusTarget = false;
        public bool focusTargetUpdated;
        public double mapSacle = 100; // 100 meter per 1 box on minimap.
        public int timeLeft = 0;
        string debugMoveForwardCount;
        MapMining mapMining;

        public DateTime aimStartTime;
        private bool routeLoaded;


        /// <summary>
        /// this id would be better if it is user id.
        /// level_6 is first one with aiming scope
        /// change sniper level from 0--8
        /// </summary>
        public double[] mouseHorizonRatio = new double[9];  // degree per pixel mouse move
        public double[] meterPerPixel = new double[9]; // on screen if the icon is 1 pixel away from center, the tank is ?? meter away from center(on same distance circle/panel) 
        public double gDegreeDiff;
        public int cannot_move = 0;



        private Thread aAimingThread;
        private Thread aShootingThread;
        private Thread aMovingThread;
        #endregion


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
            routeLoaded = false;
            timeLeft = 0;

        }



        /// <summary>
        /// check focusTarget status
        /// and check other stuff
        /// and do the click 
        /// </summary>
        private void shootingThread()
        {
            DateTime lastclick = DateTime.Now;
            while (true)
            {
                if (status != Status.PLAYING)
                    break;
                if (focusTarget)
                {
                    Helper.LogDebug("focusTargetHappen set to true");
                    focusTargetHappen = true;
                    focusTarget = false;
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
        Aim aim;
        private void aimingThread()
        {
            Thread.Sleep(2000);
            SniperMode.resetSniperLevel();
            aim = new Aim();

            while (true)
            {
                try
                {
                    aim.aim();
                }
                catch
                {
                    break;
                }
            }


        }



        double cntMoveForward = 0;
        TimeSpan timeMovingForward = new TimeSpan();
        private void movingThread()
        {
            Move aMove = new Move();
            try
            {
                while (true)
                {
                    DateTime before = DateTime.Now;

                    aMove.aMovePiece();

                    timeMovingForward += DateTime.Now - before;
                    cntMoveForward++;
                    debugMoveForwardCount = "MoveForward function call " + cntMoveForward / timeMovingForward.TotalSeconds + "/s totalCnt: " + cntMoveForward;

                }
            }
            catch
            {

            }
        }


        #region route
        public void genNextRoutePoint()
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
        public void loadRouteToFirePos()
        {
            Trajectory t = mapMining.genRouteToFireposTagMap(myTank.pos);
            route.Clear();
            foreach (Point p in t)
            {
                route.Add(p);
            }
            genNextRoutePoint();
        }
        public void loadRouteToEnemyBase()
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
            Helper.LogInfo("load route map name: " + mapName);
            Helper.LogInfo("load route myTank.pos: " + myTank.pos);
            if (TBConst.notMoveTank)
            {
                Helper.LogInfo("load route returns as notMoveTank=true");
                return;
            }
            if (this.routeLoaded)
            {
                Helper.LogInfo("load route returns already loaded");
                return;
            }
            this.routeLoaded = true;
            while (this.mapName == "")
                Thread.Sleep(1000);
            while (myTank.pos.x == 6 && myTank.pos.y == 6)
                Thread.Sleep(1000);
            Thread.Sleep(3000);

            this.mapSacle = MapDef.map_size[mapName];

            mapMining = new MapMining(this.mapName);

            this.enemyBase = mapMining.enemyBase(myTank.pos);

            //if (myTank.vInfo.vClass == VehicleClass.HT)
            //always use a fire route
            if (TBConst.cheatSlaveMode && CheatClient.getInstance().cheatMasterOnOtherSide())
                loadRouteToFirePos();
            else if (TBConst.cheatSlaveMode)
                loadRouteToEnemyBase();
            else if (myTank.vInfo.tier <= 3)
                loadRouteToEnemyBase();
            else
                loadRouteToFirePos(); // HT choose a more far away route
            //else
            //  loadRouteToEnemyBase();
            this.startPos = myTank.pos;
        }
        #endregion

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
        /// <summary>
        /// initial argument based on the sensitivity
        /// </summary>
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

        public bool noAimMove()
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
            try { aShootingThread.Abort(); }
            catch { }
            try { aMovingThread.Abort(); }
            catch { }
            this.Clear();
        }

        public void actionHangar()
        {
            this.Clear();
            if (TBConst.cheatSlaveMode)
            {
                Thread.Sleep(1000);
                return ;
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
        public void actionCountDown()
        {
            Thread.Sleep(1000);
            if (timeLeft > 60)
            {
                status = Status.PLAYING;
                return;
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

        public void startThread()
        {
            Helper.LogInfo("---------TankBot startThread------------");

            //wait for connection establish
            while (XvmComm.getInstance().messageCnt < 100)
                Thread.Sleep(1000);

            initArgsInBattle();
            while (true)
            {
                if (status == Status.IN_HANGAR)
                {
                    actionHangar();
                }
                else if (status == Status.QUEUING || status == Status.COUNT_DOWN)
                {
                    actionCountDown();
                }
                else if (status == Status.PLAYING)
                {
                    actionPlaying();
                }
                else if (status == Status.DIE)
                {
                    actionDie();
                }
                else if (status == Status.SHOW_BATTLE_RESULTS)
                {
                    actionShowBattleResult();
                }
                Thread.Sleep(1000);
            }

        }

        private void actionShowBattleResult()
        {
            Helper.LogInfo("press esc to eliminate battle results");
            Thread.Sleep(5000);
            //ESC
            Helper.bringToFront();
            Helper.keyPress("b", Helper.KEY_TYPE.PRESS);
            Thread.Sleep(1000);
            status = Status.IN_HANGAR;
        }

        private void actionDie()
        {
            if (TBConst.cheatSlaveMode && CheatClient.getInstance().cheatMasterOnOtherSide())
            {
                Helper.LogInfo("DIE and left click to change view");
                Helper.leftClickSlow();
                Thread.Sleep(5000);
                return;
            }
            TankAction.exitToHangar();
        }

        private void actionPlaying()
        {
            if (!TBConst.noAim)
            {
                Helper.LogInfo("start aimingThread");
                aAimingThread = new Thread(new ThreadStart(this.aimingThread));
                aShootingThread = new Thread(new ThreadStart(shootingThread));
                aAimingThread.Start();
                aShootingThread.Start();
            }


            if (!TBConst.notMoveTank)
            {
                loadRoute();

                Helper.LogInfo("start movingThread");
                aMovingThread = new Thread(new ThreadStart(this.movingThread));
                aMovingThread.Start();
            }

            if (!TBConst.noAim)
            {
                aShootingThread.Join();
                Helper.LogInfo("join aAimingHelperThread");
                aAimingThread.Join();
                Helper.LogInfo("join aimingThread");
            }
            if (!TBConst.notMoveTank)
            {

                aMovingThread.Join();
                Helper.LogInfo("join movingThread");
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
            str += "\r\n" + "Mytank Name: " + myTank.tankName + " " + myTank.vInfo.tier + " " + myTank.vInfo.vClass;
            str += "\r\n" + "Health: " + myTank.health;
            str += "\r\n" + "Speed: " + myTank.speed;
            str += "\r\n" + "Focus: " + focusTarget;
            str += "\r\n" + "time_left: " + timeLeft;


            if(aim!=null)
                str += aim.debugString();
            return str;
        }
    }
}


