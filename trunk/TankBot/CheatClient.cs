using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Diagnostics;

namespace TankBot
{
    class CheatClient
    {
        static private CheatClient instance = null;
        static public CheatClient getInstance()
        {
            if (!TBConst.cheatSlaveMode)
                return null;
            if (instance == null)
                instance = new CheatClient();
            return instance;
        }
        public IPEndPoint RemoteIpEndPoint;
        public UdpClient cheatUdp;

        public bool cheatMasterOnOtherSide()
        {
            for (int i = 0; i < 15; i++)
                if (TankBot.getInstance().enemyTank[i].username == TBConst.cheatMasterUserName)
                {
                    //Helper.LogDebug("cheat Master on other side");
                    return true;
                }
            return false;
        }
        public bool cheatMasterOnSameSide()
        {
            for (int i = 0; i < 14; i++)
                if (TankBot.getInstance().allyTank[i].username == TBConst.cheatMasterUserName)
                {
                    //Helper.LogDebug("cheat Master on other side");
                    return true;
                }
            return false;
        }
        DateTime lastTry = DateTime.Now;

        private void reconnect()
        {
            try
            {
                client = new TcpClient();
                IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse(TBConst.cheatServerIp), TBConst.cheatServerPort);
                client.Connect(serverEndPoint);
            }
            catch
            {
            }
        }
        public void sendCheatMessage(string message)
        {
            try
            {
                message = "map_name: " + TankBot.getInstance().mapName + " " + message +"<EOF>";
                NetworkStream stream = client.GetStream();
                stream.Write(Encoding.ASCII.GetBytes(message), 0, message.Length);
                stream.Flush();
            }
            catch
            {
                Helper.LogDebug("send message failed.. trying to reconnect");
                if ((DateTime.Now - lastTry).TotalSeconds > 10)
                {
                    lastTry = DateTime.Now;
                    new Thread(new ThreadStart(reconnect)).Start();

                }
            }
        }

        TcpClient client;
        public void controlMessageRead()
        {
            while (true)
            {
                //Creates a UdpClient for reading incoming data.
                if (cheatUdp == null)
                    cheatUdp = new UdpClient(TBConst.cheatServerPort);

                //Creates an IPEndPoint to record the IP Address and port number of the sender.  
                // The IPEndPoint will allow you to read datagrams sent from any source.
                if (RemoteIpEndPoint == null)
                    RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

                // Blocks until a message returns on this socket from a remote host.
                Byte[] receiveBytes = cheatUdp.Receive(ref RemoteIpEndPoint);

                string returnData = Encoding.ASCII.GetString(receiveBytes);

                string message = returnData.ToString();

                Helper.LogInfo("get cheat message" + message);
                if (message == "startin10s")
                {
                    Thread.Sleep(9000);
                    TankAction.clickStart();
                    TankAction.clickStart();
                    TankAction.clickStart();
                }

                if (message.StartsWith("clicktank"))
                {
                    TankAction.moveCarouselLeft();
                    TankAction.clickTank(Convert.ToInt32("" + message[message.Length - 1]));
                }
                if (message == "die" )
                {

                    if (TankBot.getInstance().status == TankBot.Status.DIE)
                    {
                        TankAction.exitToHangar();
                    }
                    if (TankBot.getInstance().status == TankBot.Status.PLAYING)
                    {
                        TankBot.getInstance().status = TankBot.Status.DIE;
                        TankAction.exitToHangar();
                    }
                }
            }

        }
        internal void startThread()
        {
            Helper.LogInfo("CheatClient startThread");
            if (!TBConst.cheatSlaveMode)
                return;
            
            new Thread(new ThreadStart(this.controlMessageRead)).Start();
            while (true)
            {
                try
                {
                    client = new TcpClient();

                    IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse(TBConst.cheatServerIp), TBConst.cheatServerPort);

                    client.Connect(serverEndPoint);
                    break;
                }
                catch
                {
                    Helper.LogException("connect to server " + TBConst.cheatServerIp + " " + TBConst.cheatServerPort + " fail" );
                    //wait 10s for next attempt
                    Thread.Sleep(10000);
                }
                    
            }


        }
        internal void abortThread()
        {
            Helper.LogInfo("CheatClient abortThread");
            return;
        }
    }
}


/*

*/