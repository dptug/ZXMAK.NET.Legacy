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

namespace ZXMAK.Platform.SDL;

public class Platform : GenericPlatform, IVideoDevice
{
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

	public override void SetCaption(string text)
	{
		Video.set_WindowCaption(text);
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
		_keyboard.Scan();
		base.Spectrum.KeyboardState = _keyboard.State;
		_mouse.Scan();
		base.Spectrum.MouseX += _mouse.DeltaX;
		base.Spectrum.MouseY += _mouse.DeltaY;
		base.Spectrum.MouseButtons = _mouse.Buttons;
		if (_sound != null)
		{
			byte[] array = _sound.LockBuffer();
			if (array == null)
			{
				return;
			}
			try
			{
				fixed (byte* ptr = array)
				{
					base.Spectrum.ExecuteFrame(videoptr, (IntPtr)ptr);
					return;
				}
			}
			finally
			{
				_sound.UnlockBuffer(array);
			}
		}
		base.Spectrum.ExecuteFrame(videoptr, IntPtr.Zero);
	}

	private void KeyDown(object sender, KeyboardEventArgs e)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Invalid comparison between Unknown and I4
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Expected I4, but got Unknown
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		ITapeDevice tapeDevice = base.Spectrum as ITapeDevice;
		Key key = e.get_Key();
		if ((int)key != 13)
		{
			switch (key - 283)
			{
			case 0:
				fileSaveAsDialog();
				break;
			case 1:
				base.Spectrum.DoReset();
				break;
			case 2:
				fileOpenDialog();
				break;
			case 3:
				base.Spectrum.IsRunning = false;
				break;
			case 4:
				TapeForm.GetInstance(base.Spectrum).Show();
				break;
			case 7:
				base.Spectrum.IsRunning = true;
				break;
			case 6:
				if (tapeDevice != null)
				{
					if (tapeDevice.Tape.IsPlay)
					{
						tapeDevice.Tape.Stop(base.Spectrum.CPU.Tact);
					}
					else
					{
						tapeDevice.Tape.Play(base.Spectrum.CPU.Tact);
					}
				}
				break;
			case 5:
				tapeDevice?.Tape.Rewind(base.Spectrum.CPU.Tact);
				break;
			}
		}
		else if ((e.get_Mod() & 0x300) != 0)
		{
			_fullScreen = !_fullScreen;
			UpdateVideoSettings();
		}
	}

	private void Quit(object sender, QuitEventArgs e)
	{
		Events.QuitApplication();
	}

	private void Tick(object sender, TickEventArgs args)
	{
		((Surface)_surface).Lock();
		try
		{
			DrawFrame(((Surface)_surface).get_Pixels());
		}
		finally
		{
			((Surface)_surface).Unlock();
		}
		if (base.Config.VideoMode > 0)
		{
			_stretchedSurface.SoftStretch(_surface, _destinationRectangle, _sourceRectangle);
			Video.get_Screen().Blit((Surface)(object)_stretchedSurface);
		}
		else
		{
			Video.get_Screen().Blit((Surface)(object)_surface);
		}
		Video.Update();
	}

	private void UpdateVideoSettings()
	{
		_surfaceVideo = Video.SetVideoMode(_destinationSize.Width, _destinationSize.Height, 32, false, false, _fullScreen, false, true);
	}

	protected override void Running()
	{
		Application.EnableVisualStyles();
		Application.SetCompatibleTextRenderingDefault(defaultValue: false);
		_fullScreen = base.Config.FullScreen;
		_antiAlias = base.Config.AntiAlias;
		Video.WindowIcon();
		Video.set_WindowCaption("ZXMAK.NET SDL");
		_sdlModes = Video.ListModes();
		if (_sdlModes.Length == 0)
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
			Size[] sdlModes = _sdlModes;
			for (int i = 0; i < sdlModes.Length; i++)
			{
				Size size = sdlModes[i];
				stringBuilder.Append(num++.ToString());
				stringBuilder.Append(" - ");
				stringBuilder.Append(size.Width.ToString());
				stringBuilder.Append("x");
				stringBuilder.AppendLine(size.Height.ToString());
			}
			PlatformFactory.Platform.ShowNotification(stringBuilder.ToString(), "Notification");
			return;
		}
		if (_sdlModes.Length < base.Config.VideoMode)
		{
			throw new Exception("Wrong video mode supplied");
		}
		base.VideoManager.SetVideoDevice(this);
		_mouse = new Mouse();
		_keyboard = new Keyboard();
		try
		{
			_sound = new Audio(44100, 3528, 4);
		}
		catch (Exception ex)
		{
			Logger.GetLogger().LogError(ex);
		}
		Events.set_Fps(60);
		Events.add_Tick((EventHandler<TickEventArgs>)Tick);
		Events.add_Quit((EventHandler<QuitEventArgs>)Quit);
		Events.add_KeyboardDown((EventHandler<KeyboardEventArgs>)KeyDown);
		OnStartup();
		base.Spectrum.IsRunning = true;
		Events.Run();
		if (_surface != null)
		{
			((BaseSdlResource)_surface).Dispose();
			_surface = null;
		}
		OnShutdown();
	}

	void IVideoDevice.SetResolution(VideoManager sender, int width, int height)
	{
		if (_surface != null)
		{
			((BaseSdlResource)_surface).Dispose();
			_surface = null;
		}
		_surface = new SurfaceEx(width, height);
		if (base.Config.VideoMode > 0)
		{
			_destinationSize = _sdlModes[base.Config.VideoMode - 1];
			_stretchedSurface = new SurfaceEx(_destinationSize.Width, _destinationSize.Height);
			_sourceRectangle = new Rectangle(0, 0, width, height);
			_destinationRectangle = new Rectangle(0, 0, _destinationSize.Width, _destinationSize.Height);
		}
		else
		{
			_destinationSize = new Size(width, height);
		}
		UpdateVideoSettings();
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
					ShowWarning("Unrecognized filetype!", "Error");
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
			openFileDialog.FileOk += loadDialog_FileOk;
			if (openFileDialog.ShowDialog() == DialogResult.OK)
			{
				if (base.Spectrum.Loader.CheckCanOpenFileName(openFileDialog.FileName))
				{
					base.Spectrum.Loader.OpenFileName(openFileDialog.FileName, openFileDialog.ReadOnlyChecked, x: true);
				}
				else
				{
					ShowWarning("Unrecognized filetype!", "Error");
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
		if (sender is OpenFileDialog openFileDialog)
		{
			e.Cancel = !base.Spectrum.Loader.CheckCanOpenFileName(openFileDialog.FileName);
		}
	}
}
