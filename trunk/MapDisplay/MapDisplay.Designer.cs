namespace TankBot
{
    partial class MapDisplay
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.listBoxMaps = new System.Windows.Forms.ListBox();
            this.listBoxFireAt = new System.Windows.Forms.ListBox();
            this.vScrollBar1 = new System.Windows.Forms.VScrollBar();
            this.checkBoxDispSpecificTime = new System.Windows.Forms.CheckBox();
            this.checkBoxHTRoute = new System.Windows.Forms.CheckBox();
            this.checkBoxFireAt = new System.Windows.Forms.CheckBox();
            this.btnSave = new System.Windows.Forms.Button();
            this.listBoxStaringPoint = new System.Windows.Forms.ListBox();
            this.checkBoxReachable = new System.Windows.Forms.CheckBox();
            this.checkBoxHTRoute2 = new System.Windows.Forms.CheckBox();
            this.checkBoxTag = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // listBoxMaps
            // 
            this.listBoxMaps.FormattingEnabled = true;
            this.listBoxMaps.ItemHeight = 12;
            this.listBoxMaps.Location = new System.Drawing.Point(673, 71);
            this.listBoxMaps.Name = "listBoxMaps";
            this.listBoxMaps.Size = new System.Drawing.Size(120, 412);
            this.listBoxMaps.TabIndex = 0;
            this.listBoxMaps.SelectedIndexChanged += new System.EventHandler(this.listBoxMaps_SelectedIndexChanged);
            // 
            // listBoxFireAt
            // 
            this.listBoxFireAt.FormattingEnabled = true;
            this.listBoxFireAt.ItemHeight = 12;
            this.listBoxFireAt.Location = new System.Drawing.Point(841, 71);
            this.listBoxFireAt.Name = "listBoxFireAt";
            this.listBoxFireAt.Size = new System.Drawing.Size(120, 412);
            this.listBoxFireAt.TabIndex = 1;
            this.listBoxFireAt.SelectedIndexChanged += new System.EventHandler(this.listBoxFireAt_SelectedIndexChanged);
            // 
            // vScrollBar1
            // 
            this.vScrollBar1.Location = new System.Drawing.Point(973, 71);
            this.vScrollBar1.Name = "vScrollBar1";
            this.vScrollBar1.Size = new System.Drawing.Size(17, 412);
            this.vScrollBar1.TabIndex = 2;
            this.vScrollBar1.Scroll += new System.Windows.Forms.ScrollEventHandler(this.vScrollBar1_Scroll);
            // 
            // checkBoxDispSpecificTime
            // 
            this.checkBoxDispSpecificTime.AutoSize = true;
            this.checkBoxDispSpecificTime.Location = new System.Drawing.Point(907, 10);
            this.checkBoxDispSpecificTime.Name = "checkBoxDispSpecificTime";
            this.checkBoxDispSpecificTime.Size = new System.Drawing.Size(96, 16);
            this.checkBoxDispSpecificTime.TabIndex = 3;
            this.checkBoxDispSpecificTime.Text = "dispSpecTime";
            this.checkBoxDispSpecificTime.UseVisualStyleBackColor = true;
            this.checkBoxDispSpecificTime.CheckedChanged += new System.EventHandler(this.checkBoxDispSpecificTime_CheckedChanged);
            // 
            // checkBoxHTRoute
            // 
            this.checkBoxHTRoute.AutoSize = true;
            this.checkBoxHTRoute.Location = new System.Drawing.Point(805, 10);
            this.checkBoxHTRoute.Name = "checkBoxHTRoute";
            this.checkBoxHTRoute.Size = new System.Drawing.Size(96, 16);
            this.checkBoxHTRoute.TabIndex = 4;
            this.checkBoxHTRoute.Text = "checkHTRoute";
            this.checkBoxHTRoute.UseVisualStyleBackColor = true;
            this.checkBoxHTRoute.CheckedChanged += new System.EventHandler(this.checkBoxHTRoute_CheckedChanged);
            // 
            // checkBoxFireAt
            // 
            this.checkBoxFireAt.AutoSize = true;
            this.checkBoxFireAt.Location = new System.Drawing.Point(711, 9);
            this.checkBoxFireAt.Name = "checkBoxFireAt";
            this.checkBoxFireAt.Size = new System.Drawing.Size(60, 16);
            this.checkBoxFireAt.TabIndex = 5;
            this.checkBoxFireAt.Text = "fireAt";
            this.checkBoxFireAt.UseVisualStyleBackColor = true;
            this.checkBoxFireAt.CheckedChanged += new System.EventHandler(this.checkBoxFireAt_CheckedChanged);
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(886, 529);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 6;
            this.btnSave.Text = "save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // listBoxStaringPoint
            // 
            this.listBoxStaringPoint.FormattingEnabled = true;
            this.listBoxStaringPoint.ItemHeight = 12;
            this.listBoxStaringPoint.Location = new System.Drawing.Point(1028, 71);
            this.listBoxStaringPoint.Name = "listBoxStaringPoint";
            this.listBoxStaringPoint.Size = new System.Drawing.Size(106, 412);
            this.listBoxStaringPoint.TabIndex = 7;
            this.listBoxStaringPoint.SelectedIndexChanged += new System.EventHandler(this.listBoxStaringPoint_SelectedIndexChanged);
            // 
            // checkBoxReachable
            // 
            this.checkBoxReachable.AutoSize = true;
            this.checkBoxReachable.Location = new System.Drawing.Point(1028, 9);
            this.checkBoxReachable.Name = "checkBoxReachable";
            this.checkBoxReachable.Size = new System.Drawing.Size(78, 16);
            this.checkBoxReachable.TabIndex = 8;
            this.checkBoxReachable.Text = "Reachable";
            this.checkBoxReachable.UseVisualStyleBackColor = true;
            this.checkBoxReachable.CheckedChanged += new System.EventHandler(this.checkBoxReachable_CheckedChanged);
            // 
            // checkBoxHTRoute2
            // 
            this.checkBoxHTRoute2.AutoSize = true;
            this.checkBoxHTRoute2.Location = new System.Drawing.Point(805, 33);
            this.checkBoxHTRoute2.Name = "checkBoxHTRoute2";
            this.checkBoxHTRoute2.Size = new System.Drawing.Size(120, 16);
            this.checkBoxHTRoute2.TabIndex = 9;
            this.checkBoxHTRoute2.Text = "checkBoxHTRoute2";
            this.checkBoxHTRoute2.UseVisualStyleBackColor = true;
            this.checkBoxHTRoute2.CheckedChanged += new System.EventHandler(this.checkBoxHTRoute2_CheckedChanged);
            // 
            // checkBoxTag
            // 
            this.checkBoxTag.AutoSize = true;
            this.checkBoxTag.Location = new System.Drawing.Point(950, 33);
            this.checkBoxTag.Name = "checkBoxTag";
            this.checkBoxTag.Size = new System.Drawing.Size(90, 16);
            this.checkBoxTag.TabIndex = 10;
            this.checkBoxTag.Text = "checkBoxTag";
            this.checkBoxTag.UseVisualStyleBackColor = true;
            this.checkBoxTag.CheckedChanged += new System.EventHandler(this.checkBoxTag_CheckedChanged);
            // 
            // MapDisplay
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1154, 679);
            this.Controls.Add(this.checkBoxTag);
            this.Controls.Add(this.checkBoxHTRoute2);
            this.Controls.Add(this.checkBoxReachable);
            this.Controls.Add(this.listBoxStaringPoint);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.checkBoxFireAt);
            this.Controls.Add(this.checkBoxHTRoute);
            this.Controls.Add(this.checkBoxDispSpecificTime);
            this.Controls.Add(this.vScrollBar1);
            this.Controls.Add(this.listBoxFireAt);
            this.Controls.Add(this.listBoxMaps);
            this.Name = "MapDisplay";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.MapDisplay_MouseDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox listBoxMaps;
        private System.Windows.Forms.ListBox listBoxFireAt;
        private System.Windows.Forms.VScrollBar vScrollBar1;
        private System.Windows.Forms.CheckBox checkBoxDispSpecificTime;
        private System.Windows.Forms.CheckBox checkBoxHTRoute;
        private System.Windows.Forms.CheckBox checkBoxFireAt;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.ListBox listBoxStaringPoint;
        private System.Windows.Forms.CheckBox checkBoxReachable;
        private System.Windows.Forms.CheckBox checkBoxHTRoute2;
        private System.Windows.Forms.CheckBox checkBoxTag;





    }
}

