using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace TankBot
{
    public class Move
    {
        public TankBot tb;
        public Move()
        {
            tb = TankBot.getInstance();
        }
        internal bool closeToAllyTank()
        {
            double rtn = 1e10;
            for (int i = 0; i < 14; i++)
            {
                if (tb.allyTank[i].visible_on_minimap)
                {
                    double dis = TBMath.distance(tb.allyTank[i].pos, tb.myTank.pos);
                    if (dis < rtn && TBMath.distance(tb.myTank.pos, tb.startPos) > 1)
                        rtn = dis;
                }
            }
            if (TBConst.closeToAllyTankStop)
                return rtn * tb.mapSacle < 10;
            return false;
        }

        internal void moveForward()
        {
            DateTime t = DateTime.Now;
            if (closeToAllyTank())
            {
                if (tb.myTank.speed != 0)
                {
                    Helper.keyPress("d", Helper.KEY_TYPE.UP);
                    Helper.keyPress("a", Helper.KEY_TYPE.UP);
                    Helper.keyPress("s", Helper.KEY_TYPE.PRESS);
                }
                Thread.Sleep(1000);
                return;
            }
            if (tb.cannot_move >= 3)
            {
                tb.status = Status.DIE;
                Helper.LogPersistent("exit due to cannot move " + tb.myTank.tankName + " " + tb.mapName);
                return;
            }

            // seems we can aim to something
            if (tb.penetrationPossible || tb.focusTarget || tb.focusTargetHappen )
            {
                if (tb.myTank.speed != 0)
                {
                    Helper.keyPress("d", Helper.KEY_TYPE.UP);
                    Helper.keyPress("a", Helper.KEY_TYPE.UP);
                    Helper.keyPress("s", Helper.KEY_TYPE.PRESS);
                }
                Thread.Sleep(10000);
                return;
            }

            // reach the end of the route
            if (tb.nextRoutePoint >= tb.route.Count)
            {
                if (tb.myTank.speed != 0)
                {
                    Helper.keyPress("d", Helper.KEY_TYPE.UP);
                    Helper.keyPress("a", Helper.KEY_TYPE.UP);
                    Helper.keyPress("s", Helper.KEY_TYPE.PRESS);
                }
                //check move forward or not

                if (tb.support() >= 1)
                {
                    tb.loadRouteToEnemyBase();

                }
                return;
            }

            // we can move forward now muhaha
            tb.aimStartTime = DateTime.Now;



            Point np = tb.route[tb.nextRoutePoint];
            //0.1 consider as very close
            double dist2nextPoint = TBMath.distance(tb.myTank.pos, np);
            // > 0 need to turn right
            // < 0 need to turn left
            double degreeDiff = TBMath.vehicleDegreeToPoint(np, tb.myTank);
            tb.gDegreeDiff = degreeDiff;

            if (dist2nextPoint < 0.1)
            {

                tb.cannot_move = 0;
                tb.nextRoutePoint++;
                return;
            }

            if (Math.Abs(degreeDiff) > 20 && tb.myTank.speed > 10) // ooops too fast cannot turn
            {
                Helper.keyPress("s", Helper.KEY_TYPE.PRESS);
                return;
            }


            if (tb.myTank.speed == 0 && Math.Abs(degreeDiff) < 10)
            {
                Helper.keyPress("d", Helper.KEY_TYPE.UP);
                Helper.keyPress("a", Helper.KEY_TYPE.UP);
                Helper.keyPress("r", Helper.KEY_TYPE.PRESS);
                Thread.Sleep(2000);
                if (tb.myTank.speed == 0)
                {
                    tb.cannot_move++;
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
        internal void aMovePiece()
        {
            if (tb.noAimMove() && TBMath.distance(tb.myTank.pos, tb.startPos) > 3)
            {
                Helper.keyPress("s", Helper.KEY_TYPE.PRESS);
                Helper.keyPress("s", Helper.KEY_TYPE.PRESS);
                Helper.keyPress("s", Helper.KEY_TYPE.PRESS);
                Helper.keyPress("a", Helper.KEY_TYPE.UP);
                Helper.keyPress("d", Helper.KEY_TYPE.UP);
                throw new CannotMoveForwardException();
            }
            if (tb.status == Status.QUEUING || tb.status == Status.COUNT_DOWN || tb.status == Status.PLAYING)
            {
            }
            else
                throw new StatusChangeException(); 
            moveForward();
        }
    }
}
