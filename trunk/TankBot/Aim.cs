using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace TankBot
{
    class Aim
    {
        TankBot tb;
        int bestAim;
        public Aim()
        {
            tb = TankBot.getInstance();
            bestAim = -1;
        }
        /// <summary>
        /// move the tank turret when there's no enemy in sight.
        /// </summary>
        private void noAimMoveTankTurret()
        {
            if (TBConst.notMoveTank)
                return;
            double degree_diff = tb.myTank.direction - tb.myTank.cameraDirection;
            int x_move = Convert.ToInt32(degree_diff / tb.mouseHorizonRatio[SniperMode.sniper_level]);
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
            if (!tb.myTank.directionUpdated) return;
            if (!tb.myTank.cameraDirectionUpdated) return;


            // force everything to update to correct 
            Thread.Sleep(50);

            double cameraDirectionBeforeMouseMove = tb.myTank.cameraDirection;

            Point beforeMouseMove = et.posScreen;
            Helper.LogDebug("aimToTank arg_x:" + arg_x + " arg_y:" + arg_y + " direction:" + tb.myTank.direction + " cameraDirection:" + tb.myTank.cameraDirection);

            double degree_diff = 0;

            int x_move = 0, y_move = 0;

            degree_diff = TBMath.cameraDegreeToEnemy(et, tb.myTank);
            // if too far away, just move horizontal.

            if (et.posScreen.x > TBConst.screen_width / 2 + 400 || et.posScreen.x < TBConst.screen_width / 2 - 400)
            {
                x_move = Convert.ToInt32(degree_diff / tb.mouseHorizonRatio[SniperMode.sniper_level]);

            }
            else // the tank is almost near in horizontal view
            {
                //---------------------------move horizontal 
                degree_diff = (et.posScreen.x - TBConst.screen_width / 2);
                double dis = TBMath.distance(tb.myTank.pos, et.pos) * tb.mapSacle;
                degree_diff = arg_x + tb.meterPerPixel[SniperMode.sniper_level] * degree_diff / 100 * dis; // meter from center to screen;
                degree_diff = degree_diff / (2 * Math.PI * dis) * 360;
                x_move = Convert.ToInt32(degree_diff / tb.mouseHorizonRatio[SniperMode.sniper_level]);

                //----------------------------------move vertical
                double pixel_diff = 0;
                if (SniperMode.sniper_level >= 6)
                    pixel_diff = (et.posScreen.y - TBConst.screen_height / 2); // positive then move down  pixel diff on screen
                else
                    pixel_diff = (et.posScreen.y - TBConst.screen_height / 2 + 58); // fuck this 58 mm
                dis = TBMath.distance(tb.myTank.pos, et.pos) * tb.mapSacle;

                double meter_diff = arg_y + tb.meterPerPixel[SniperMode.sniper_level] * pixel_diff / 100 * dis; // meter from center to screen; arg_y meter's away

                degree_diff = meter_diff / (2 * Math.PI * dis) * 360;

                //Logger("mouse move" + degree_diff / mouse_horizon_ratio);
                y_move = Convert.ToInt32(degree_diff / tb.mouseHorizonRatio[SniperMode.sniper_level]);

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
            Helper.LogDebug("aimToTankFinish " + arg_x + " " + arg_y + " " + tb.myTank.direction + " " + tb.myTank.cameraDirection);

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
                if (tb.focusTarget)
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
            if (tb.enemyTank[uid].username == TBConst.cheatMasterUserName)
            {
                Helper.LogDebug("dont aim cheat master");
                return false;
            }
            tb.focusTargetHappen = false;
            DateTime start = DateTime.Now;
            while ((DateTime.Now - start).Seconds < 1)
            {
                Random r = new Random();
                aimToTankArgxArgy(tb.enemyTank[uid], r.NextDouble() * 4 - 2, r.NextDouble() * 4);
                if (tb.focusTargetHappen)
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
                if (tb.enemyTank[i].visible_on_screen)
                {
                    dist.Add(new Tuple<double, int>(TBMath.distance(tb.myTank.pos, tb.enemyTank[i].pos), i));
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



        private void aimSpecificTank(Vehicle v)
        {
            Helper.LogInfo("aimSpecificTank");
            DateTime start = DateTime.Now;
            double dis = TBMath.distance(tb.myTank.pos, v.pos) * tb.mapSacle;
            if (dis > 100) SniperMode.setSniperMode(8); // x8 sniper
            else if (dis > 50) SniperMode.setSniperMode(7); // x4 sniper
            else SniperMode.setSniperMode(6); // x2 sniper
            this.tryingUpDownRange(v);

            while (true)
            {
                tb.focusTargetHappen = false;
                aimToTankArgxArgy(v, select_x, select_y);
                waitFocusTargetUpdate();
                if (tb.focusTargetHappen == false)
                {
                    Helper.LogInfo("throw CannotAimException");
                    throw new CannotAimException();
                }
                Helper.LogDebug("aiming at tank " + v.tankName + " " + v.username);
            }

        }

        private void waitFocusTargetUpdate()
        {
            //Helper.LogInfo("wait focus update in");
            for (int i = 0; i < 5; i++)
            {
                tb.focusTargetUpdated = false;
                while (!tb.focusTargetUpdated)
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
                if (tb.enemyTank[i].visible_on_screen == true)
                    return false;
            }
            Helper.LogDebug("no enemy on screen");
            return true;
        }

        internal void aim()
        {
            if (tb.status != Status.PLAYING)
                throw new StatusChangeException();
            try
            {
                // we are going to aim something
                if (tb.focusTargetHappen)
                {
                    tb.focusTargetHappen = false;
                    if (bestAim >= 0)
                        aimSpecificTank(tb.enemyTank[bestAim]);

                }
                else
                {
                    bestAim = findBestAim();
                    if (noEnemyOnScreen())
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
}
