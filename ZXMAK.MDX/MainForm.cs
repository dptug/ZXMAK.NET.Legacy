using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using ZXMAK.Engine;
using ZXMAK.Platform.Windows.Forms;

namespace ZXMAK.Platform.MDX
{
	public partial class MainForm : Form, IGameLoopForm
	{
		public MainForm()
		{
			base.SetStyle(ControlStyles.Opaque | ControlStyles.AllPaintingInWmPaint, true);
			this.InitializeComponent();
			base.ClientSize = new Size(640, 480);
		}

		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);
			this._mouse = new DirectMouse(this);
			this._keyboard = new DirectKeyboard(this);
			this._sound = new DirectSound(this, -1, 44100, 16, 2, 3528, 4);
		}

		protected override void OnHandleDestroyed(EventArgs e)
		{
			if (this._keyboard != null)
			{
				this._keyboard.Dispose();
			}
			if (this._mouse != null)
			{
				this._mouse.Dispose();
			}
			base.OnHandleDestroyed(e);
		}

		public void UpdateState()
		{
			this.vctl.UpdateScene();
		}

		public void RenderFrame()
		{
			this.vctl.RenderScene();
		}

		public void Init(GenericPlatform platform)
		{
			this._platform = platform;
			this.vctl.VideoManager = this._platform.VideoManager;
			this.vctl.AntiAlias = this._platform.Config.AntiAlias;
			this._platform.Spectrum.Breakpoint -= this.spectrum_OnBreakpoint;
			this._platform.Spectrum.MaxTactExceed -= this.spectrum_OnMaxTactExceed;
			this._platform.Spectrum.Breakpoint += this.spectrum_OnBreakpoint;
			this._platform.Spectrum.MaxTactExceed += this.spectrum_OnMaxTactExceed;
		}

		private void MainForm_Shown(object sender, EventArgs e)
		{
			this.Fullscreen = this._platform.Config.FullScreen;
			this._platform.OnStartup();
		}

		private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			this._platform.OnShutdown();
			this._platform.Spectrum.Breakpoint -= this.spectrum_OnBreakpoint;
			this._platform.Spectrum.MaxTactExceed -= this.spectrum_OnMaxTactExceed;
		}

		public bool Fullscreen
		{
			get
			{
				return this._fullscreen;
			}
			set
			{
				if (value != this._fullscreen)
				{
					if (value)
					{
						this._style = base.FormBorderStyle;
						this._topMost = base.TopMost;
						this._location = base.Location;
						this._size = base.ClientSize;
						base.FormBorderStyle = FormBorderStyle.None;
						base.Location = new Point(0, 0);
						this._mouse.StartCapture();
						base.Menu = null;
						base.Size = Screen.PrimaryScreen.Bounds.Size;
						base.Focus();
					}
					else
					{
						base.Location = this._location;
						base.FormBorderStyle = this._style;
						this._mouse.StopCapture();
						base.Menu = this.mainMenu;
						base.ClientSize = this._size;
					}
					this._fullscreen = value;
					this.vctl.RenderScene();
				}
			}
		}

		private void mainForm_MouseMove(object sender, MouseEventArgs e)
		{
			if (this.Fullscreen)
			{
				if (base.Menu != null && e.Y > 1)
				{
					base.Menu = null;
					return;
				}
				if (e.Y <= SystemInformation.MenuHeight)
				{
					base.Menu = this.mainMenu;
				}
			}
		}

		private void spectrum_OnBreakpoint(object sender, EventArgs e)
		{
			this.Fullscreen = false;
		}

		private void spectrum_OnMaxTactExceed(object sender, MaxTactExceedEventArgs args)
		{
			string message = args.Tact.ToString() + " tacts executed,\nbut operation not complete!\n\nAre you sure to continue?";
			args.Cancel = (this._platform.QueryDialog(message, "Warning", QueryButtons.YesNo) != QueryResult.Yes);
			Application.DoEvents();
		}

		private void vctl_MouseClick(object sender, MouseEventArgs e)
		{
			if (this.vctl.Focused && this._mouse != null)
			{
				this._mouse.StartCapture();
			}
		}

		private unsafe void vctl_DrawFrame(object sender, IntPtr videoptr)
		{
			if (this.vctl.FrameRate > 50 && this._sound.QueueLoadState >= 50)
			{
				int num = 50 / (this.vctl.FrameRate - 50);
				if (this.vctl.FrameCounter % num == 0)
				{
					return;
				}
			}
			this._keyboard.Scan();
			this._platform.Spectrum.KeyboardState = this._keyboard.State;
			this._mouse.Scan();
			this._platform.Spectrum.MouseX += this._mouse.DeltaX;
			this._platform.Spectrum.MouseY += this._mouse.DeltaY;
			this._platform.Spectrum.MouseButtons = this._mouse.Buttons;
			byte[] array = this._sound.LockBuffer();
			if (array != null)
			{
				try
				{
					try
					{
						fixed (byte* ptr = array)
						{
							this._platform.Spectrum.ExecuteFrame(videoptr, (IntPtr)((void*)ptr));
						}
					}
					finally
					{
						byte* ptr = null;
					}
				}
				finally
				{
					this._sound.UnlockBuffer(array);
				}
			}
		}

		protected override void WndProc(ref Message m)
		{
			if (m.Msg == 262 && (int)m.WParam == 13)
			{
				return;
			}
			base.WndProc(ref m);
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			if (e.Alt && e.Control && this._mouse != null)
			{
				this._mouse.StopCapture();
			}
			ITapeDevice tapeDevice = this._platform.Spectrum as ITapeDevice;
			Keys keyCode = e.KeyCode;
			if (keyCode != Keys.Return)
			{
				switch (keyCode)
				{
				case Keys.F3:
					this._platform.Spectrum.DoReset();
					return;
				case Keys.F4:
				case Keys.F6:
				case Keys.F10:
				case Keys.F11:
					break;
				case Keys.F5:
					this._platform.Spectrum.IsRunning = false;
					return;
				case Keys.F7:
					if (tapeDevice != null)
					{
						tapeDevice.Tape.Rewind(this._platform.Spectrum.CPU.Tact);
						return;
					}
					break;
				case Keys.F8:
					if (tapeDevice != null)
					{
						if (tapeDevice.Tape.IsPlay)
						{
							tapeDevice.Tape.Stop(this._platform.Spectrum.CPU.Tact);
							return;
						}
						tapeDevice.Tape.Play(this._platform.Spectrum.CPU.Tact);
						return;
					}
					break;
				case Keys.F9:
					this._platform.Spectrum.IsRunning = true;
					return;
				case Keys.F12:
				{
					string text = Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName), "boot.zip");
					if (File.Exists(text) && this._platform.Spectrum.Loader.CheckCanOpenFileName(text))
					{
						this._platform.Spectrum.Loader.OpenFileName(text, true, true);
					}
					break;
				}
				default:
					return;
				}
				return;
			}
			if (e.Alt)
			{
				this.Fullscreen = !this.Fullscreen;
			}
			e.Handled = true;
		}

		private void menuFileOpen_Click(object sender, EventArgs e)
		{
			bool isRunning = this._platform.Spectrum.IsRunning;
			try
			{
				this._platform.Spectrum.IsRunning = false;
				OpenFileDialog openFileDialog = new OpenFileDialog();
				openFileDialog.InitialDirectory = ".";
				openFileDialog.SupportMultiDottedExtensions = true;
				openFileDialog.Title = "Open...";
				openFileDialog.Filter = this._platform.Spectrum.Loader.GetOpenExtFilter();
				openFileDialog.DefaultExt = "";
				openFileDialog.FileName = "";
				openFileDialog.ShowReadOnly = true;
				openFileDialog.ReadOnlyChecked = true;
				openFileDialog.CheckFileExists = true;
				openFileDialog.FileOk += this.loadDialog_FileOk;
				if (openFileDialog.ShowDialog() == DialogResult.OK)
				{
					if (this._platform.Spectrum.Loader.CheckCanOpenFileName(openFileDialog.FileName))
					{
						this._platform.Spectrum.Loader.OpenFileName(openFileDialog.FileName, openFileDialog.ReadOnlyChecked, true);
					}
					else
					{
						this._platform.ShowWarning("Unrecognized filetype!", "Error");
					}
				}
			}
			finally
			{
				this._platform.Spectrum.IsRunning = isRunning;
			}
		}

		private void menuFileSaveAs_Click(object sender, EventArgs e)
		{
			bool isRunning = this._platform.Spectrum.IsRunning;
			try
			{
				this._platform.Spectrum.IsRunning = false;
				SaveFileDialog saveFileDialog = new SaveFileDialog();
				saveFileDialog.InitialDirectory = ".";
				saveFileDialog.SupportMultiDottedExtensions = true;
				saveFileDialog.Title = "Save...";
				saveFileDialog.Filter = this._platform.Spectrum.Loader.GetSaveExtFilter();
				saveFileDialog.DefaultExt = this._platform.Spectrum.Loader.GetDefaultExtension();
				saveFileDialog.FileName = "";
				saveFileDialog.OverwritePrompt = true;
				if (saveFileDialog.ShowDialog() == DialogResult.OK)
				{
					if (this._platform.Spectrum.Loader.CheckCanSaveFileName(saveFileDialog.FileName))
					{
						this._platform.Spectrum.Loader.SaveFileName(saveFileDialog.FileName);
					}
					else
					{
						this._platform.ShowWarning("Unrecognized filetype!", "Error");
					}
				}
			}
			finally
			{
				this._platform.Spectrum.IsRunning = isRunning;
			}
		}

		private void loadDialog_FileOk(object sender, CancelEventArgs e)
		{
			OpenFileDialog openFileDialog = sender as OpenFileDialog;
			if (openFileDialog == null)
			{
				return;
			}
			e.Cancel = !this._platform.Spectrum.Loader.CheckCanOpenFileName(openFileDialog.FileName);
		}

		private void menuDbgVG_Click(object sender, EventArgs e)
		{
			new dbgWD1793(this._platform.Spectrum).Show();
		}

		private void menuDebug_Click(object sender, EventArgs e)
		{
			FormCPU instance = FormCPU.GetInstance(this._platform.Spectrum);
			instance.Show();
			instance.Focus();
		}

		private void menuFileEjectDisk_Click(object sender, EventArgs e)
		{
			IBetaDiskDevice betaDiskDevice = this._platform.Spectrum as IBetaDiskDevice;
			if (betaDiskDevice != null)
			{
				betaDiskDevice.BetaDisk.FDD[0].Eject();
			}
		}

		private void menuAbout_Click(object sender, EventArgs e)
		{
			using (About about = new About())
			{
				about.ShowDialog();
			}
		}

		private void menuTape_Click(object sender, EventArgs e)
		{
			TapeForm.GetInstance(this._platform.Spectrum).Show();
		}

		protected override void OnDragOver(DragEventArgs drgevent)
		{
			if (drgevent.Data.GetDataPresent(DataFormats.FileDrop))
			{
				drgevent.Effect = DragDropEffects.None;
				string[] array = drgevent.Data.GetData(DataFormats.FileDrop) as string[];
				if (array == null)
				{
					return;
				}
				foreach (string fileName in array)
				{
					if (this._platform.Spectrum.Loader.CheckCanOpenFileName(fileName))
					{
						drgevent.Effect = DragDropEffects.Copy;
					}
				}
			}
			base.OnDragOver(drgevent);
		}

		protected override void OnDragDrop(DragEventArgs drgevent)
		{
			if (drgevent.Data.GetDataPresent(DataFormats.FileDrop))
			{
				string[] array = drgevent.Data.GetData(DataFormats.FileDrop) as string[];
				if (array != null)
				{
					foreach (string text in array)
					{
						if (this._platform.Spectrum.Loader.CheckCanOpenFileName(text))
						{
							this._platform.Spectrum.Loader.OpenFileName(text, true, true);
						}
						else
						{
							this._platform.ShowWarning("Unrecognized filetype!\n" + text, "Warning");
						}
					}
				}
			}
			base.OnDragDrop(drgevent);
		}

		private void menuFileExit_Click(object sender, EventArgs e)
		{
			base.Close();
		}

		private void menuControlStartStop_Click(object sender, EventArgs e)
		{
			if (((MenuItem)sender).Name == "menuControlStart")
			{
				this._platform.Spectrum.IsRunning = true;
				return;
			}
			if (((MenuItem)sender).Name == "menuControlStop")
			{
				this._platform.Spectrum.IsRunning = false;
			}
		}

		void IGameLoopForm.Show()
		{
			base.Show();
		}

		bool IGameLoopForm.Created
		{
			get
			{
				return base.Created;
			}
		}

		protected const int WM_SYSCHAR = 262;

		private GenericPlatform _platform;

		private DirectKeyboard _keyboard;

		private DirectMouse _mouse;

		private DirectSound _sound;

		private bool _fullscreen;

		private Point _location;

		private Size _size;

		private FormBorderStyle _style;

		private bool _topMost;
	}
}
