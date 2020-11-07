namespace ZXMAK.Platform.MDX
{
	public partial class MainForm : global::System.Windows.Forms.Form, global::ZXMAK.Platform.Windows.Forms.IGameLoopForm
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
			this.mainMenu = new global::System.Windows.Forms.MainMenu(this.components);
			this.menuFile = new global::System.Windows.Forms.MenuItem();
			this.menuFileOpen = new global::System.Windows.Forms.MenuItem();
			this.menuFileSaveAs = new global::System.Windows.Forms.MenuItem();
			this.menuFileEjectDisk = new global::System.Windows.Forms.MenuItem();
			this.menuFileSplitter = new global::System.Windows.Forms.MenuItem();
			this.menuFileExit = new global::System.Windows.Forms.MenuItem();
			this.menuControl = new global::System.Windows.Forms.MenuItem();
			this.menuControlStart = new global::System.Windows.Forms.MenuItem();
			this.menuControlStop = new global::System.Windows.Forms.MenuItem();
			this.menuTools = new global::System.Windows.Forms.MenuItem();
			this.menuToolsTape = new global::System.Windows.Forms.MenuItem();
			this.menuItem3 = new global::System.Windows.Forms.MenuItem();
			this.menuToolsDebugger = new global::System.Windows.Forms.MenuItem();
			this.menuToolsWD1793 = new global::System.Windows.Forms.MenuItem();
			this.menuHelp = new global::System.Windows.Forms.MenuItem();
			this.menuHelpAbout = new global::System.Windows.Forms.MenuItem();
			this.vctl = new global::ZXMAK.Platform.MDX.RenderVideo();
			base.SuspendLayout();
			this.mainMenu.MenuItems.AddRange(new global::System.Windows.Forms.MenuItem[]
			{
				this.menuFile,
				this.menuControl,
				this.menuTools,
				this.menuHelp
			});
			this.menuFile.Index = 0;
			this.menuFile.MenuItems.AddRange(new global::System.Windows.Forms.MenuItem[]
			{
				this.menuFileOpen,
				this.menuFileSaveAs,
				this.menuFileEjectDisk,
				this.menuFileSplitter,
				this.menuFileExit
			});
			this.menuFile.Text = "File";
			this.menuFileOpen.Index = 0;
			this.menuFileOpen.Text = "Open...";
			this.menuFileOpen.Click += new global::System.EventHandler(this.menuFileOpen_Click);
			this.menuFileSaveAs.Index = 1;
			this.menuFileSaveAs.Text = "Save as...";
			this.menuFileSaveAs.Click += new global::System.EventHandler(this.menuFileSaveAs_Click);
			this.menuFileEjectDisk.Index = 2;
			this.menuFileEjectDisk.Text = "Eject disk";
			this.menuFileEjectDisk.Click += new global::System.EventHandler(this.menuFileEjectDisk_Click);
			this.menuFileSplitter.Index = 3;
			this.menuFileSplitter.Text = "-";
			this.menuFileExit.Index = 4;
			this.menuFileExit.Text = "Exit";
			this.menuFileExit.Click += new global::System.EventHandler(this.menuFileExit_Click);
			this.menuControl.Index = 1;
			this.menuControl.MenuItems.AddRange(new global::System.Windows.Forms.MenuItem[]
			{
				this.menuControlStart,
				this.menuControlStop
			});
			this.menuControl.Text = "Control";
			this.menuControlStart.Index = 0;
			this.menuControlStart.Text = "Start";
			this.menuControlStart.Click += new global::System.EventHandler(this.menuControlStartStop_Click);
			this.menuControlStop.Index = 1;
			this.menuControlStop.Text = "Stop";
			this.menuControlStop.Click += new global::System.EventHandler(this.menuControlStartStop_Click);
			this.menuTools.Index = 2;
			this.menuTools.MenuItems.AddRange(new global::System.Windows.Forms.MenuItem[]
			{
				this.menuToolsTape,
				this.menuItem3,
				this.menuToolsDebugger,
				this.menuToolsWD1793
			});
			this.menuTools.Text = "Tools";
			this.menuToolsTape.Index = 0;
			this.menuToolsTape.Text = "Tape";
			this.menuToolsTape.Click += new global::System.EventHandler(this.menuTape_Click);
			this.menuItem3.Index = 1;
			this.menuItem3.Text = "-";
			this.menuToolsDebugger.Index = 2;
			this.menuToolsDebugger.Text = "Debugger";
			this.menuToolsDebugger.Click += new global::System.EventHandler(this.menuDebug_Click);
			this.menuToolsWD1793.Index = 3;
			this.menuToolsWD1793.Text = "WD1793";
			this.menuToolsWD1793.Click += new global::System.EventHandler(this.menuDbgVG_Click);
			this.menuHelp.Index = 3;
			this.menuHelp.MenuItems.AddRange(new global::System.Windows.Forms.MenuItem[]
			{
				this.menuHelpAbout
			});
			this.menuHelp.Text = "Help";
			this.menuHelpAbout.Index = 0;
			this.menuHelpAbout.Text = "About";
			this.menuHelpAbout.Click += new global::System.EventHandler(this.menuAbout_Click);
			this.vctl.Dock = global::System.Windows.Forms.DockStyle.Fill;
			this.vctl.Location = new global::System.Drawing.Point(0, 0);
			this.vctl.Name = "vctl";
			this.vctl.Size = new global::System.Drawing.Size(348, 289);
			this.vctl.TabIndex = 0;
			this.vctl.Text = "controlVideo1";
			this.vctl.VideoManager = null;
			this.vctl.MouseMove += new global::System.Windows.Forms.MouseEventHandler(this.mainForm_MouseMove);
			this.vctl.MouseClick += new global::System.Windows.Forms.MouseEventHandler(this.vctl_MouseClick);
			this.vctl.DrawFrame += new global::ZXMAK.Platform.MDX.DrawFrameDelegate(this.vctl_DrawFrame);
			this.AllowDrop = true;
			base.AutoScaleDimensions = new global::System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new global::System.Drawing.Size(348, 289);
			base.Controls.Add(this.vctl);
			base.KeyPreview = true;
			base.Menu = this.mainMenu;
			base.Name = "MainForm";
			base.StartPosition = global::System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "zxmak.net";
			base.Shown += new global::System.EventHandler(this.MainForm_Shown);
			base.FormClosing += new global::System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
			base.ResumeLayout(false);
		}

		private global::System.ComponentModel.IContainer components;

		private global::ZXMAK.Platform.MDX.RenderVideo vctl;

		private global::System.Windows.Forms.MainMenu mainMenu;

		private global::System.Windows.Forms.MenuItem menuFile;

		private global::System.Windows.Forms.MenuItem menuFileOpen;

		private global::System.Windows.Forms.MenuItem menuFileSaveAs;

		private global::System.Windows.Forms.MenuItem menuFileEjectDisk;

		private global::System.Windows.Forms.MenuItem menuControl;

		private global::System.Windows.Forms.MenuItem menuControlStart;

		private global::System.Windows.Forms.MenuItem menuControlStop;

		private global::System.Windows.Forms.MenuItem menuTools;

		private global::System.Windows.Forms.MenuItem menuToolsTape;

		private global::System.Windows.Forms.MenuItem menuItem3;

		private global::System.Windows.Forms.MenuItem menuToolsDebugger;

		private global::System.Windows.Forms.MenuItem menuToolsWD1793;

		private global::System.Windows.Forms.MenuItem menuHelp;

		private global::System.Windows.Forms.MenuItem menuHelpAbout;

		private global::System.Windows.Forms.MenuItem menuFileSplitter;

		private global::System.Windows.Forms.MenuItem menuFileExit;
	}
}
