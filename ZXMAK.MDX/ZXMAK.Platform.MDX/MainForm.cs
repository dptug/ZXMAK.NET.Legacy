using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using ZXMAK.Engine;
using ZXMAK.Platform.Windows.Forms;

namespace ZXMAK.Platform.MDX;

public class MainForm : Form, IGameLoopForm
{
	protected const int WM_SYSCHAR = 262;

	private IContainer components;

	private RenderVideo vctl;

	private MainMenu mainMenu;

	private MenuItem menuFile;

	private MenuItem menuFileOpen;

	private MenuItem menuFileSaveAs;

	private MenuItem menuFileEjectDisk;

	private MenuItem menuControl;

	private MenuItem menuControlStart;

	private MenuItem menuControlStop;

	private MenuItem menuTools;

	private MenuItem menuToolsTape;

	private MenuItem menuItem3;

	private MenuItem menuToolsDebugger;

	private MenuItem menuToolsWD1793;

	private MenuItem menuHelp;

	private MenuItem menuHelpAbout;

	private MenuItem menuFileSplitter;

	private MenuItem menuFileExit;

	private GenericPlatform _platform;

	private DirectKeyboard _keyboard;

	private DirectMouse _mouse;

	private DirectSound _sound;

	private bool _fullscreen;

	private Point _location;

	private Size _size;

	private FormBorderStyle _style;

	private bool _topMost;

	public bool Fullscreen
	{
		get
		{
			return _fullscreen;
		}
		set
		{
			if (value != _fullscreen)
			{
				if (value)
				{
					_style = base.FormBorderStyle;
					_topMost = base.TopMost;
					_location = base.Location;
					_size = base.ClientSize;
					base.FormBorderStyle = FormBorderStyle.None;
					base.Location = new Point(0, 0);
					_mouse.StartCapture();
					base.Menu = null;
					base.Size = Screen.PrimaryScreen.Bounds.Size;
					Focus();
				}
				else
				{
					base.Location = _location;
					base.FormBorderStyle = _style;
					_mouse.StopCapture();
					base.Menu = mainMenu;
					base.ClientSize = _size;
				}
				_fullscreen = value;
				vctl.RenderScene();
			}
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		this.components = new System.ComponentModel.Container();
		this.mainMenu = new System.Windows.Forms.MainMenu(this.components);
		this.menuFile = new System.Windows.Forms.MenuItem();
		this.menuFileOpen = new System.Windows.Forms.MenuItem();
		this.menuFileSaveAs = new System.Windows.Forms.MenuItem();
		this.menuFileEjectDisk = new System.Windows.Forms.MenuItem();
		this.menuFileSplitter = new System.Windows.Forms.MenuItem();
		this.menuFileExit = new System.Windows.Forms.MenuItem();
		this.menuControl = new System.Windows.Forms.MenuItem();
		this.menuControlStart = new System.Windows.Forms.MenuItem();
		this.menuControlStop = new System.Windows.Forms.MenuItem();
		this.menuTools = new System.Windows.Forms.MenuItem();
		this.menuToolsTape = new System.Windows.Forms.MenuItem();
		this.menuItem3 = new System.Windows.Forms.MenuItem();
		this.menuToolsDebugger = new System.Windows.Forms.MenuItem();
		this.menuToolsWD1793 = new System.Windows.Forms.MenuItem();
		this.menuHelp = new System.Windows.Forms.MenuItem();
		this.menuHelpAbout = new System.Windows.Forms.MenuItem();
		this.vctl = new ZXMAK.Platform.MDX.RenderVideo();
		base.SuspendLayout();
		this.mainMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[4] { this.menuFile, this.menuControl, this.menuTools, this.menuHelp });
		this.menuFile.Index = 0;
		this.menuFile.MenuItems.AddRange(new System.Windows.Forms.MenuItem[5] { this.menuFileOpen, this.menuFileSaveAs, this.menuFileEjectDisk, this.menuFileSplitter, this.menuFileExit });
		this.menuFile.Text = "File";
		this.menuFileOpen.Index = 0;
		this.menuFileOpen.Text = "Open...";
		this.menuFileOpen.Click += new System.EventHandler(menuFileOpen_Click);
		this.menuFileSaveAs.Index = 1;
		this.menuFileSaveAs.Text = "Save as...";
		this.menuFileSaveAs.Click += new System.EventHandler(menuFileSaveAs_Click);
		this.menuFileEjectDisk.Index = 2;
		this.menuFileEjectDisk.Text = "Eject disk";
		this.menuFileEjectDisk.Click += new System.EventHandler(menuFileEjectDisk_Click);
		this.menuFileSplitter.Index = 3;
		this.menuFileSplitter.Text = "-";
		this.menuFileExit.Index = 4;
		this.menuFileExit.Text = "Exit";
		this.menuFileExit.Click += new System.EventHandler(menuFileExit_Click);
		this.menuControl.Index = 1;
		this.menuControl.MenuItems.AddRange(new System.Windows.Forms.MenuItem[2] { this.menuControlStart, this.menuControlStop });
		this.menuControl.Text = "Control";
		this.menuControlStart.Index = 0;
		this.menuControlStart.Text = "Start";
		this.menuControlStart.Click += new System.EventHandler(menuControlStartStop_Click);
		this.menuControlStop.Index = 1;
		this.menuControlStop.Text = "Stop";
		this.menuControlStop.Click += new System.EventHandler(menuControlStartStop_Click);
		this.menuTools.Index = 2;
		this.menuTools.MenuItems.AddRange(new System.Windows.Forms.MenuItem[4] { this.menuToolsTape, this.menuItem3, this.menuToolsDebugger, this.menuToolsWD1793 });
		this.menuTools.Text = "Tools";
		this.menuToolsTape.Index = 0;
		this.menuToolsTape.Text = "Tape";
		this.menuToolsTape.Click += new System.EventHandler(menuTape_Click);
		this.menuItem3.Index = 1;
		this.menuItem3.Text = "-";
		this.menuToolsDebugger.Index = 2;
		this.menuToolsDebugger.Text = "Debugger";
		this.menuToolsDebugger.Click += new System.EventHandler(menuDebug_Click);
		this.menuToolsWD1793.Index = 3;
		this.menuToolsWD1793.Text = "WD1793";
		this.menuToolsWD1793.Click += new System.EventHandler(menuDbgVG_Click);
		this.menuHelp.Index = 3;
		this.menuHelp.MenuItems.AddRange(new System.Windows.Forms.MenuItem[1] { this.menuHelpAbout });
		this.menuHelp.Text = "Help";
		this.menuHelpAbout.Index = 0;
		this.menuHelpAbout.Text = "About";
		this.menuHelpAbout.Click += new System.EventHandler(menuAbout_Click);
		this.vctl.Dock = System.Windows.Forms.DockStyle.Fill;
		this.vctl.Location = new System.Drawing.Point(0, 0);
		this.vctl.Name = "vctl";
		this.vctl.Size = new System.Drawing.Size(348, 289);
		this.vctl.TabIndex = 0;
		this.vctl.Text = "controlVideo1";
		this.vctl.VideoManager = null;
		this.vctl.MouseMove += new System.Windows.Forms.MouseEventHandler(mainForm_MouseMove);
		this.vctl.MouseClick += new System.Windows.Forms.MouseEventHandler(vctl_MouseClick);
		this.vctl.DrawFrame += new ZXMAK.Platform.MDX.DrawFrameDelegate(vctl_DrawFrame);
		this.AllowDrop = true;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(348, 289);
		base.Controls.Add(this.vctl);
		base.KeyPreview = true;
		base.Menu = this.mainMenu;
		base.Name = "MainForm";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "zxmak.net";
		base.Shown += new System.EventHandler(MainForm_Shown);
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(MainForm_FormClosing);
		base.ResumeLayout(false);
	}

	public MainForm()
	{
		SetStyle(ControlStyles.Opaque | ControlStyles.AllPaintingInWmPaint, value: true);
		InitializeComponent();
		base.ClientSize = new Size(640, 480);
	}

	protected override void OnHandleCreated(EventArgs e)
	{
		base.OnHandleCreated(e);
		_mouse = new DirectMouse(this);
		_keyboard = new DirectKeyboard(this);
		_sound = new DirectSound(this, -1, 44100, 16, 2, 3528, 4);
	}

	protected override void OnHandleDestroyed(EventArgs e)
	{
		if (_keyboard != null)
		{
			_keyboard.Dispose();
		}
		if (_mouse != null)
		{
			_mouse.Dispose();
		}
		base.OnHandleDestroyed(e);
	}

	public void UpdateState()
	{
		vctl.UpdateScene();
	}

	public void RenderFrame()
	{
		vctl.RenderScene();
	}

	public void Init(GenericPlatform platform)
	{
		_platform = platform;
		vctl.VideoManager = _platform.VideoManager;
		vctl.AntiAlias = _platform.Config.AntiAlias;
		_platform.Spectrum.Breakpoint -= spectrum_OnBreakpoint;
		_platform.Spectrum.MaxTactExceed -= spectrum_OnMaxTactExceed;
		_platform.Spectrum.Breakpoint += spectrum_OnBreakpoint;
		_platform.Spectrum.MaxTactExceed += spectrum_OnMaxTactExceed;
	}

	private void MainForm_Shown(object sender, EventArgs e)
	{
		Fullscreen = _platform.Config.FullScreen;
		_platform.OnStartup();
	}

	private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
	{
		_platform.OnShutdown();
		_platform.Spectrum.Breakpoint -= spectrum_OnBreakpoint;
		_platform.Spectrum.MaxTactExceed -= spectrum_OnMaxTactExceed;
	}

	private void mainForm_MouseMove(object sender, MouseEventArgs e)
	{
		if (Fullscreen)
		{
			if (base.Menu != null && e.Y > 1)
			{
				base.Menu = null;
			}
			else if (e.Y <= SystemInformation.MenuHeight)
			{
				base.Menu = mainMenu;
			}
		}
	}

	private void spectrum_OnBreakpoint(object sender, EventArgs e)
	{
		Fullscreen = false;
	}

	private void spectrum_OnMaxTactExceed(object sender, MaxTactExceedEventArgs args)
	{
		string message = args.Tact + " tacts executed,\nbut operation not complete!\n\nAre you sure to continue?";
		args.Cancel = _platform.QueryDialog(message, "Warning", QueryButtons.YesNo) != QueryResult.Yes;
		Application.DoEvents();
	}

	private void vctl_MouseClick(object sender, MouseEventArgs e)
	{
		if (vctl.Focused && _mouse != null)
		{
			_mouse.StartCapture();
		}
	}

	private unsafe void vctl_DrawFrame(object sender, IntPtr videoptr)
	{
		if (vctl.FrameRate > 50 && _sound.QueueLoadState >= 50)
		{
			int num = 50 / (vctl.FrameRate - 50);
			if (vctl.FrameCounter % num == 0)
			{
				return;
			}
		}
		_keyboard.Scan();
		_platform.Spectrum.KeyboardState = _keyboard.State;
		_mouse.Scan();
		_platform.Spectrum.MouseX += _mouse.DeltaX;
		_platform.Spectrum.MouseY += _mouse.DeltaY;
		_platform.Spectrum.MouseButtons = _mouse.Buttons;
		byte[] array = _sound.LockBuffer();
		if (array == null)
		{
			return;
		}
		try
		{
			fixed (byte* ptr = array)
			{
				_platform.Spectrum.ExecuteFrame(videoptr, (IntPtr)ptr);
			}
		}
		finally
		{
			_sound.UnlockBuffer(array);
		}
	}

	protected override void WndProc(ref Message m)
	{
		if (m.Msg != 262 || (int)m.WParam != 13)
		{
			base.WndProc(ref m);
		}
	}

	protected override void OnKeyDown(KeyEventArgs e)
	{
		if (e.Alt && e.Control && _mouse != null)
		{
			_mouse.StopCapture();
		}
		ITapeDevice tapeDevice = _platform.Spectrum as ITapeDevice;
		switch (e.KeyCode)
		{
		case Keys.Return:
			if (e.Alt)
			{
				Fullscreen = !Fullscreen;
			}
			e.Handled = true;
			break;
		case Keys.F3:
			_platform.Spectrum.DoReset();
			break;
		case Keys.F5:
			_platform.Spectrum.IsRunning = false;
			break;
		case Keys.F9:
			_platform.Spectrum.IsRunning = true;
			break;
		case Keys.F8:
			if (tapeDevice != null)
			{
				if (tapeDevice.Tape.IsPlay)
				{
					tapeDevice.Tape.Stop(_platform.Spectrum.CPU.Tact);
				}
				else
				{
					tapeDevice.Tape.Play(_platform.Spectrum.CPU.Tact);
				}
			}
			break;
		case Keys.F7:
			tapeDevice?.Tape.Rewind(_platform.Spectrum.CPU.Tact);
			break;
		case Keys.F12:
		{
			string text = Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName), "boot.zip");
			if (File.Exists(text) && _platform.Spectrum.Loader.CheckCanOpenFileName(text))
			{
				_platform.Spectrum.Loader.OpenFileName(text, wp: true, x: true);
			}
			break;
		}
		}
	}

	private void menuFileOpen_Click(object sender, EventArgs e)
	{
		bool isRunning = _platform.Spectrum.IsRunning;
		try
		{
			_platform.Spectrum.IsRunning = false;
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.InitialDirectory = ".";
			openFileDialog.SupportMultiDottedExtensions = true;
			openFileDialog.Title = "Open...";
			openFileDialog.Filter = _platform.Spectrum.Loader.GetOpenExtFilter();
			openFileDialog.DefaultExt = "";
			openFileDialog.FileName = "";
			openFileDialog.ShowReadOnly = true;
			openFileDialog.ReadOnlyChecked = true;
			openFileDialog.CheckFileExists = true;
			openFileDialog.FileOk += loadDialog_FileOk;
			if (openFileDialog.ShowDialog() == DialogResult.OK)
			{
				if (_platform.Spectrum.Loader.CheckCanOpenFileName(openFileDialog.FileName))
				{
					_platform.Spectrum.Loader.OpenFileName(openFileDialog.FileName, openFileDialog.ReadOnlyChecked, x: true);
				}
				else
				{
					_platform.ShowWarning("Unrecognized filetype!", "Error");
				}
			}
		}
		finally
		{
			_platform.Spectrum.IsRunning = isRunning;
		}
	}

	private void menuFileSaveAs_Click(object sender, EventArgs e)
	{
		bool isRunning = _platform.Spectrum.IsRunning;
		try
		{
			_platform.Spectrum.IsRunning = false;
			SaveFileDialog saveFileDialog = new SaveFileDialog();
			saveFileDialog.InitialDirectory = ".";
			saveFileDialog.SupportMultiDottedExtensions = true;
			saveFileDialog.Title = "Save...";
			saveFileDialog.Filter = _platform.Spectrum.Loader.GetSaveExtFilter();
			saveFileDialog.DefaultExt = _platform.Spectrum.Loader.GetDefaultExtension();
			saveFileDialog.FileName = "";
			saveFileDialog.OverwritePrompt = true;
			if (saveFileDialog.ShowDialog() == DialogResult.OK)
			{
				if (_platform.Spectrum.Loader.CheckCanSaveFileName(saveFileDialog.FileName))
				{
					_platform.Spectrum.Loader.SaveFileName(saveFileDialog.FileName);
				}
				else
				{
					_platform.ShowWarning("Unrecognized filetype!", "Error");
				}
			}
		}
		finally
		{
			_platform.Spectrum.IsRunning = isRunning;
		}
	}

	private void loadDialog_FileOk(object sender, CancelEventArgs e)
	{
		if (sender is OpenFileDialog openFileDialog)
		{
			e.Cancel = !_platform.Spectrum.Loader.CheckCanOpenFileName(openFileDialog.FileName);
		}
	}

	private void menuDbgVG_Click(object sender, EventArgs e)
	{
		new dbgWD1793(_platform.Spectrum).Show();
	}

	private void menuDebug_Click(object sender, EventArgs e)
	{
		FormCPU instance = FormCPU.GetInstance(_platform.Spectrum);
		instance.Show();
		instance.Focus();
	}

	private void menuFileEjectDisk_Click(object sender, EventArgs e)
	{
		if (_platform.Spectrum is IBetaDiskDevice betaDiskDevice)
		{
			betaDiskDevice.BetaDisk.FDD[0].Eject();
		}
	}

	private void menuAbout_Click(object sender, EventArgs e)
	{
		using About about = new About();
		about.ShowDialog();
	}

	private void menuTape_Click(object sender, EventArgs e)
	{
		TapeForm.GetInstance(_platform.Spectrum).Show();
	}

	protected override void OnDragOver(DragEventArgs drgevent)
	{
		if (drgevent.Data.GetDataPresent(DataFormats.FileDrop))
		{
			drgevent.Effect = DragDropEffects.None;
			if (!(drgevent.Data.GetData(DataFormats.FileDrop) is string[] array))
			{
				return;
			}
			string[] array2 = array;
			foreach (string fileName in array2)
			{
				if (_platform.Spectrum.Loader.CheckCanOpenFileName(fileName))
				{
					drgevent.Effect = DragDropEffects.Copy;
				}
			}
		}
		base.OnDragOver(drgevent);
	}

	protected override void OnDragDrop(DragEventArgs drgevent)
	{
		if (drgevent.Data.GetDataPresent(DataFormats.FileDrop) && drgevent.Data.GetData(DataFormats.FileDrop) is string[] array)
		{
			string[] array2 = array;
			foreach (string text in array2)
			{
				if (_platform.Spectrum.Loader.CheckCanOpenFileName(text))
				{
					_platform.Spectrum.Loader.OpenFileName(text, wp: true, x: true);
				}
				else
				{
					_platform.ShowWarning("Unrecognized filetype!\n" + text, "Warning");
				}
			}
		}
		base.OnDragDrop(drgevent);
	}

	private void menuFileExit_Click(object sender, EventArgs e)
	{
		Close();
	}

	private void menuControlStartStop_Click(object sender, EventArgs e)
	{
		if (((MenuItem)sender).Name == "menuControlStart")
		{
			_platform.Spectrum.IsRunning = true;
		}
		else if (((MenuItem)sender).Name == "menuControlStop")
		{
			_platform.Spectrum.IsRunning = false;
		}
	}

	void IGameLoopForm.Show()
	{
		Show();
	}

    bool IGameLoopForm.Created => this.Created;
}
