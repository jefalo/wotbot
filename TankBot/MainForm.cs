using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;

namespace TankBot
{
    public partial class MainForm : Form
    {
        #region Init/Hotkey
        public MainForm()
        {
            InitializeComponent();
        }
     

        private void Form1_Load(object sender, EventArgs e)
        {
            // connect xvm_comm with this form in order to draw/debug 
            XvmComm.getInstance().mainForm = this;
            Helper.mainForm = this;
            Main.mainForm = this;

            this.Text += "  v" + TBConst.version;
            if (!Directory.Exists(TBConst.wotRootPath))
            {
                MessageBox.Show(TBConst.wotRootPath + " not exist");
                Environment.Exit(1);
            }

            if (Screen.AllScreens.Length == 2)
                this.Location = new System.Drawing.Point(-1280, 0);
            Main.init();

            updateSlaveMode();
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Main.abortTankBot();

        }

        #endregion


        #region setText


        delegate void SetTextLogCallback(string text);
        public void appendTextLog(string text)
        {
            if (this.textBoxLog.InvokeRequired)
            {
                SetTextLogCallback d = new SetTextLogCallback(appendTextLog);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.textBoxLog.AppendText( text);
            }
        }
        public void appendTextLog(string text, bool append)
        {
            if (TBConst.releaseMode)
                return;
            if(append)
            {
                appendTextLog("\r\n" + text);
            }
        }

        delegate void SetTextCallback(string text);
        public void setText(string text)
        {
            if (TBConst.releaseMode)
                return;
            if (this.textBox1.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(setText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.textBox1.Text = text;
            }
        }
        #endregion

        #region drawPanel
        private void drawPoint(Point p, Graphics g, Color c, int size = 8)
        {

            float side_length = Math.Min(panel_minimap.Width, panel_minimap.Height);
            float x = (float)p.x;
            float y = (float)p.y;

            x = (x - 1) / 10 * side_length;
            y = (y - 1) / 10 * side_length;

            g.FillEllipse(new SolidBrush(c), x - size / 2, y - size / 2, size, size);
        }
        private void drawVehicle(Vehicle v, Graphics g, Color c)
        {
            drawPoint(v.pos, g, c);
        }
        private void drawDirctionLine(Vehicle v, double direction, Graphics g, Color c, float scale)
        {
            float side_length = Math.Min(panel_minimap.Width, panel_minimap.Height);
            float x = (float)v.pos.x;
            float y = (float)v.pos.y;

            x = (x - 1) / 10 * side_length;
            y = (y - 1) / 10 * side_length;


            float scalex = (float)Math.Sin(Math.PI / 180 * direction) * scale;
            float scaley = -(float)Math.Cos(Math.PI / 180 * direction) * scale;
            g.DrawLine(new Pen(c), new PointF(x, y), new PointF(x + scalex, y + scaley));

        }

        public void paintMinimap()
        {
            Bitmap bitmap, back_graph;
            TankBot tankBot = TankBot.getInstance();
            Graphics g;
            try { back_graph = new Bitmap(TBConst.jpgPath + TankBot.getInstance().mapName + ".jpg"); }
            catch { back_graph = new Bitmap(panel_minimap.Width, panel_minimap.Height); }
            bitmap = new Bitmap(back_graph, panel_minimap.Size);
            g = Graphics.FromImage(bitmap);
            //draw ally
            foreach (Vehicle v in tankBot.allyTank)
                if (v.visible_on_minimap)
                    drawVehicle(v, g, Color.LawnGreen);
            // draw enemy
            foreach (Vehicle v in tankBot.enemyTank)
                if (v.visible_on_minimap)
                    drawVehicle(v, g, Color.Red);
            // draw self
            drawVehicle(tankBot.myTank, g, Color.White);
            // draw 
            drawDirctionLine(tankBot.myTank, tankBot.myTank.cameraDirection, g, Color.Pink, 100);
            drawDirctionLine(tankBot.myTank, tankBot.myTank.direction, g, Color.White, 20);

            foreach (Point t in tankBot.route)
                drawPoint(t, g, Color.Aqua, TBConst.drawRoutePixelSize);
            if (tankBot.nextRoutePoint < tankBot.route.Count)
                drawPoint(tankBot.route[tankBot.nextRoutePoint], g, Color.Aqua, TBConst.nextRouteSize);
            drawPoint(tankBot.enemyBase, g, Color.Gray, TBConst.enemyBaseDrawPointSize);

            panel_minimap.CreateGraphics().DrawImage(bitmap, new PointF(0, 0));
            back_graph.Dispose();
            bitmap.Dispose();
            g.Dispose();
        }

        public void paint()
        {
            if (TBConst.releaseMode)
                return;
            try
            {
                paintMinimap();
            }
            catch (Exception)
            {
            }
        }

        #endregion

        private void btnStart_Click(object sender, EventArgs e)
        {
            btnStart.Enabled = false;
            Main.startTankBot();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            Main.abortTankBot();
        }
        private void initSlaveMode()
        {
            if (TBConst.cheatSlaveMode)
                checkSlaveMode.Checked = true;
            else
                checkSlaveMode.Checked = false;
        }
        private void updateSlaveMode()
        {
            if (checkSlaveMode.Checked)
                TBConst.cheatSlaveMode = true;
            else
                TBConst.cheatSlaveMode = false;
            Helper.LogInfo("cheat slave mode: " + TBConst.cheatSlaveMode);
        }
        private void checkSlaveMode_CheckedChanged(object sender, EventArgs e)
        {
            updateSlaveMode();
        }



    }
}
