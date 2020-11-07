namespace ZXMAK.Platform.Windows.Forms
{
	public partial class TapeForm : global::System.Windows.Forms.Form
	{
		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			this.components = new global::System.ComponentModel.Container();
			this.toolBar = new global::System.Windows.Forms.ToolStrip();
			this.toolButtonRewind = new global::System.Windows.Forms.ToolStripButton();
			this.toolButtonPrev = new global::System.Windows.Forms.ToolStripButton();
			this.toolButtonPlay = new global::System.Windows.Forms.ToolStripButton();
			this.toolButtonNext = new global::System.Windows.Forms.ToolStripButton();
			this.toolProgressBar = new global::System.Windows.Forms.ToolStripProgressBar();
			this.panelList = new global::System.Windows.Forms.Panel();
			this.blockList = new global::System.Windows.Forms.ListBox();
			this.timerProgress = new global::System.Windows.Forms.Timer(this.components);
			this.toolBar.SuspendLayout();
			this.panelList.SuspendLayout();
			base.SuspendLayout();
			this.toolBar.Items.AddRange(new global::System.Windows.Forms.ToolStripItem[]
			{
				this.toolButtonRewind,
				this.toolButtonPrev,
				this.toolButtonPlay,
				this.toolButtonNext,
				this.toolProgressBar
			});
			this.toolBar.Location = new global::System.Drawing.Point(0, 0);
			this.toolBar.Name = "toolBar";
			this.toolBar.Size = new global::System.Drawing.Size(292, 25);
			this.toolBar.TabIndex = 2;
			this.toolBar.Text = "toolBar";
			this.toolButtonRewind.DisplayStyle = global::System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolButtonRewind.Image = global::ZXMAK.Platform.Windows.Forms.Properties.Resources.RewindIcon;
			this.toolButtonRewind.ImageTransparentColor = global::System.Drawing.Color.Magenta;
			this.toolButtonRewind.Name = "toolButtonRewind";
			this.toolButtonRewind.Size = new global::System.Drawing.Size(23, 22);
			this.toolButtonRewind.Text = "Rewind";
			this.toolButtonRewind.Click += new global::System.EventHandler(this.toolButtonRewind_Click);
			this.toolButtonPrev.DisplayStyle = global::System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolButtonPrev.Image = global::ZXMAK.Platform.Windows.Forms.Properties.Resources.PrevIcon;
			this.toolButtonPrev.ImageTransparentColor = global::System.Drawing.Color.Magenta;
			this.toolButtonPrev.Name = "toolButtonPrev";
			this.toolButtonPrev.Size = new global::System.Drawing.Size(23, 22);
			this.toolButtonPrev.Text = "Previous block";
			this.toolButtonPrev.Click += new global::System.EventHandler(this.toolButtonPrev_Click);
			this.toolButtonPlay.DisplayStyle = global::System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolButtonPlay.Image = global::ZXMAK.Platform.Windows.Forms.Properties.Resources.PlayIcon;
			this.toolButtonPlay.ImageTransparentColor = global::System.Drawing.Color.Magenta;
			this.toolButtonPlay.Name = "toolButtonPlay";
			this.toolButtonPlay.Size = new global::System.Drawing.Size(23, 22);
			this.toolButtonPlay.Text = "Play/Stop";
			this.toolButtonPlay.Click += new global::System.EventHandler(this.toolButtonPlay_Click);
			this.toolButtonNext.DisplayStyle = global::System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolButtonNext.Image = global::ZXMAK.Platform.Windows.Forms.Properties.Resources.NextIcon;
			this.toolButtonNext.ImageTransparentColor = global::System.Drawing.Color.Magenta;
			this.toolButtonNext.Name = "toolButtonNext";
			this.toolButtonNext.Size = new global::System.Drawing.Size(23, 22);
			this.toolButtonNext.Text = "Next block";
			this.toolButtonNext.Click += new global::System.EventHandler(this.toolButtonNext_Click);
			this.toolProgressBar.Name = "toolProgressBar";
			this.toolProgressBar.Size = new global::System.Drawing.Size(170, 22);
			this.toolProgressBar.Step = 1;
			this.toolProgressBar.ToolTipText = "Loading progress";
			this.toolProgressBar.Value = 50;
			this.panelList.Controls.Add(this.blockList);
			this.panelList.Dock = global::System.Windows.Forms.DockStyle.Fill;
			this.panelList.Location = new global::System.Drawing.Point(0, 25);
			this.panelList.Name = "panelList";
			this.panelList.Size = new global::System.Drawing.Size(292, 237);
			this.panelList.TabIndex = 3;
			this.blockList.Dock = global::System.Windows.Forms.DockStyle.Fill;
			this.blockList.Font = new global::System.Drawing.Font("Microsoft Sans Serif", 9.75f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 204);
			this.blockList.FormattingEnabled = true;
			this.blockList.IntegralHeight = false;
			this.blockList.ItemHeight = 16;
			this.blockList.Location = new global::System.Drawing.Point(0, 0);
			this.blockList.Name = "blockList";
			this.blockList.Size = new global::System.Drawing.Size(292, 237);
			this.blockList.TabIndex = 0;
			this.blockList.DoubleClick += new global::System.EventHandler(this.blockList_DoubleClick);
			this.blockList.Click += new global::System.EventHandler(this.blockList_Click);
			this.timerProgress.Enabled = true;
			this.timerProgress.Interval = 200;
			this.timerProgress.Tick += new global::System.EventHandler(this.timerProgress_Tick);
			base.AutoScaleDimensions = new global::System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new global::System.Drawing.Size(292, 262);
			base.Controls.Add(this.panelList);
			base.Controls.Add(this.toolBar);
			base.Name = "TapeForm";
			this.Text = "Tape";
			base.FormClosed += new global::System.Windows.Forms.FormClosedEventHandler(this.TapeForm_FormClosed);
			this.toolBar.ResumeLayout(false);
			this.toolBar.PerformLayout();
			this.panelList.ResumeLayout(false);
			base.ResumeLayout(false);
			base.PerformLayout();
		}

		private global::System.ComponentModel.IContainer components;

		private global::System.Windows.Forms.ToolStrip toolBar;

		private global::System.Windows.Forms.Panel panelList;

		private global::System.Windows.Forms.ListBox blockList;

		private global::System.Windows.Forms.ToolStripButton toolButtonRewind;

		private global::System.Windows.Forms.ToolStripButton toolButtonPrev;

		private global::System.Windows.Forms.ToolStripButton toolButtonPlay;

		private global::System.Windows.Forms.ToolStripButton toolButtonNext;

		private global::System.Windows.Forms.ToolStripProgressBar toolProgressBar;

		private global::System.Windows.Forms.Timer timerProgress;
	}
}
