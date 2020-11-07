using System;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SdlDotNet.Core;
using SdlDotNet.Graphics;
using SdlDotNet.Input;
using ZXMAK.Engine;
using ZXMAK.Logging;
using ZXMAK.Platform.Windows.Forms;

namespace ZXMAK.Platform.SDL
{
	public class Platform : GenericPlatform, IVideoDevice
	{
		public override void SetCaption(string text)
		{
			Video.WindowCaption = text;
		}

		public override void ShowFatalError(Exception ex)
		{
			ExceptionReport.Execute(ex);
		}

		public override void ShowWarning(string message, string title)
		{
			MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
		}

		public override void ShowNotification(string message, string title)
		{
			MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
		}

		public override QueryResult QueryDialog(string message, string title, QueryButtons buttons)
		{
			MessageBoxButtons buttons2 = MessageBoxButtons.OK;
			if (buttons == QueryButtons.YesNo)
			{
				buttons2 = MessageBoxButtons.YesNo;
			}
			DialogResult dialogResult = MessageBox.Show(message, title, buttons2, MessageBoxIcon.Question);
			QueryResult result = QueryResult.Yes;
			if (dialogResult != DialogResult.Yes)
			{
				result = QueryResult.No;
			}
			return result;
		}

		private unsafe void DrawFrame(IntPtr videoptr)
		{
			this._keyboard.Scan();
			base.Spectrum.KeyboardState = this._keyboard.State;
			this._mouse.Scan();
			base.Spectrum.MouseX += this._mouse.DeltaX;
			base.Spectrum.MouseY += this._mouse.DeltaY;
			base.Spectrum.MouseButtons = this._mouse.Buttons;
			if (this._sound != null)
			{
				byte[] array = this._sound.LockBuffer();
				if (array == null)
				{
					return;
				}
				try
				{
					try
					{
						fixed (byte* ptr = array)
						{
							base.Spectrum.ExecuteFrame(videoptr, (IntPtr)((void*)ptr));
						}
					}
					finally
					{
						byte* ptr = null;
					}
					return;
				}
				finally
				{
					this._sound.UnlockBuffer(array);
				}
			}
			base.Spectrum.ExecuteFrame(videoptr, IntPtr.Zero);
		}

		private void KeyDown(object sender, KeyboardEventArgs e)
		{
			ITapeDevice tapeDevice = base.Spectrum as ITapeDevice;
			Key key = e.Key;
			if (key != 13)
			{
				switch (key)
				{
				case 283:
					this.fileSaveAsDialog();
					return;
				case 284:
					base.Spectrum.DoReset();
					return;
				case 285:
					this.fileOpenDialog();
					return;
				case 286:
					base.Spectrum.IsRunning = false;
					return;
				case 287:
					TapeForm.GetInstance(base.Spectrum).Show();
					return;
				case 288:
					if (tapeDevice != null)
					{
						tapeDevice.Tape.Rewind(base.Spectrum.CPU.Tact);
					}
					break;
				case 289:
					if (tapeDevice != null)
					{
						if (tapeDevice.Tape.IsPlay)
						{
							tapeDevice.Tape.Stop(base.Spectrum.CPU.Tact);
							return;
						}
						tapeDevice.Tape.Play(base.Spectrum.CPU.Tact);
						return;
					}
					break;
				case 290:
					base.Spectrum.IsRunning = true;
					return;
				default:
					return;
				}
			}
			else if ((e.Mod & 768) != null)
			{
				this._fullScreen = !this._fullScreen;
				this.UpdateVideoSettings();
				return;
			}
		}

		private void Quit(object sender, QuitEventArgs e)
		{
			Events.QuitApplication();
		}

		private void Tick(object sender, TickEventArgs args)
		{
			this._surface.Lock();
			try
			{
				this.DrawFrame(this._surface.Pixels);
			}
			finally
			{
				this._surface.Unlock();
			}
			if (base.Config.VideoMode > 0)
			{
				this._stretchedSurface.SoftStretch(this._surface, this._destinationRectangle, this._sourceRectangle);
				Video.Screen.Blit(this._stretchedSurface);
			}
			else
			{
				Video.Screen.Blit(this._surface);
			}
			Video.Update();
		}

		private void UpdateVideoSettings()
		{
			this._surfaceVideo = Video.SetVideoMode(this._destinationSize.Width, this._destinationSize.Height, 32, false, false, this._fullScreen, false, true);
		}

		protected override void Running()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			this._fullScreen = base.Config.FullScreen;
			this._antiAlias = base.Config.AntiAlias;
			Video.WindowIcon();
			Video.WindowCaption = "ZXMAK.NET SDL";
			this._sdlModes = Video.ListModes();
			if (this._sdlModes.Length == 0)
			{
				throw new Exception("Video modes list empty");
			}
			if (base.Config.Help)
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine("Available video modes (use /vmX):");
				int num = 0;
				stringBuilder.Append(num++.ToString());
				stringBuilder.AppendLine(" - Direct Render");
				foreach (Size size in this._sdlModes)
				{
					stringBuilder.Append(num++.ToString());
					stringBuilder.Append(" - ");
					stringBuilder.Append(size.Width.ToString());
					stringBuilder.Append("x");
					stringBuilder.AppendLine(size.Height.ToString());
				}
				PlatformFactory.Platform.ShowNotification(stringBuilder.ToString(), "Notification");
				return;
			}
			if (this._sdlModes.Length < base.Config.VideoMode)
			{
				throw new Exception("Wrong video mode supplied");
			}
			base.VideoManager.SetVideoDevice(this);
			this._mouse = new Mouse();
			this._keyboard = new Keyboard();
			try
			{
				this._sound = new Audio(44100, 3528, 4);
			}
			catch (Exception ex)
			{
				Logger.GetLogger().LogError(ex);
			}
			Events.Fps = 60;
			Events.Tick += this.Tick;
			Events.Quit += this.Quit;
			Events.KeyboardDown += this.KeyDown;
			base.OnStartup();
			base.Spectrum.IsRunning = true;
			Events.Run();
			if (this._surface != null)
			{
				this._surface.Dispose();
				this._surface = null;
			}
			base.OnShutdown();
		}

		void IVideoDevice.SetResolution(VideoManager sender, int width, int height)
		{
			if (this._surface != null)
			{
				this._surface.Dispose();
				this._surface = null;
			}
			this._surface = new SurfaceEx(width, height);
			if (base.Config.VideoMode > 0)
			{
				this._destinationSize = this._sdlModes[base.Config.VideoMode - 1];
				this._stretchedSurface = new SurfaceEx(this._destinationSize.Width, this._destinationSize.Height);
				this._sourceRectangle = new Rectangle(0, 0, width, height);
				this._destinationRectangle = new Rectangle(0, 0, this._destinationSize.Width, this._destinationSize.Height);
			}
			else
			{
				this._destinationSize = new Size(width, height);
			}
			this.UpdateVideoSettings();
			base.VideoManager.SetVideoParams(new VideoParams(width, height, width, 32));
		}

		private void fileSaveAsDialog()
		{
			bool isRunning = base.Spectrum.IsRunning;
			try
			{
				base.Spectrum.IsRunning = false;
				SaveFileDialog saveFileDialog = new SaveFileDialog();
				saveFileDialog.InitialDirectory = ".";
				saveFileDialog.SupportMultiDottedExtensions = true;
				saveFileDialog.Title = "Save...";
				string empty = string.Empty;
				saveFileDialog.Filter = base.Spectrum.Loader.GetSaveExtFilter();
				saveFileDialog.DefaultExt = empty;
				saveFileDialog.FileName = "";
				saveFileDialog.OverwritePrompt = true;
				if (saveFileDialog.ShowDialog() == DialogResult.OK)
				{
					if (base.Spectrum.Loader.CheckCanSaveFileName(saveFileDialog.FileName))
					{
						base.Spectrum.Loader.SaveFileName(saveFileDialog.FileName);
					}
					else
					{
						this.ShowWarning("Unrecognized filetype!", "Error");
					}
				}
			}
			finally
			{
				base.Spectrum.IsRunning = isRunning;
			}
		}

		private void fileOpenDialog()
		{
			bool isRunning = base.Spectrum.IsRunning;
			try
			{
				base.Spectrum.IsRunning = false;
				OpenFileDialog openFileDialog = new OpenFileDialog();
				openFileDialog.InitialDirectory = ".";
				openFileDialog.SupportMultiDottedExtensions = true;
				openFileDialog.Title = "Open...";
				openFileDialog.Filter = base.Spectrum.Loader.GetOpenExtFilter();
				openFileDialog.DefaultExt = "";
				openFileDialog.FileName = "";
				openFileDialog.ShowReadOnly = true;
				openFileDialog.ReadOnlyChecked = true;
				openFileDialog.CheckFileExists = true;
				openFileDialog.FileOk += this.loadDialog_FileOk;
				if (openFileDialog.ShowDialog() == DialogResult.OK)
				{
					if (base.Spectrum.Loader.CheckCanOpenFileName(openFileDialog.FileName))
					{
						base.Spectrum.Loader.OpenFileName(openFileDialog.FileName, openFileDialog.ReadOnlyChecked, true);
					}
					else
					{
						this.ShowWarning("Unrecognized filetype!", "Error");
					}
				}
			}
			finally
			{
				base.Spectrum.IsRunning = isRunning;
			}
		}

		private void loadDialog_FileOk(object sender, CancelEventArgs e)
		{
			OpenFileDialog openFileDialog = sender as OpenFileDialog;
			if (openFileDialog == null)
			{
				return;
			}
			e.Cancel = !base.Spectrum.Loader.CheckCanOpenFileName(openFileDialog.FileName);
		}

		private Surface _surfaceVideo;

		private SurfaceEx _surface;

		private SurfaceEx _stretchedSurface;

		private Keyboard _keyboard;

		private Mouse _mouse;

		private Audio _sound;

		private Rectangle _sourceRectangle;

		private Rectangle _destinationRectangle;

		private Size _destinationSize;

		private Size[] _sdlModes;

		private bool _antiAlias = true;

		private bool _fullScreen;
	}
}
