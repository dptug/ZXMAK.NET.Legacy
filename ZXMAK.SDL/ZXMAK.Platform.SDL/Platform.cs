using SdlDotNet.Core;
using SdlDotNet.Graphics;
using SdlDotNet.Input;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ZXMAK.Engine;
using ZXMAK.Logging;
using ZXMAK.Platform.Windows.Forms;

namespace ZXMAK.Platform.SDL
{
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

        public override void SetCaption(string text) => Video.WindowCaption = text;

        public override void ShowFatalError(Exception ex) => ExceptionReport.Execute(ex);

        public override void ShowWarning(string message, string title)
        {
            int num = (int)MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        public override void ShowNotification(string message, string title)
        {
            int num = (int)MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        public override QueryResult QueryDialog(
          string message,
          string title,
          QueryButtons buttons)
        {
            MessageBoxButtons buttons1 = MessageBoxButtons.OK;
            if (buttons == QueryButtons.YesNo)
                buttons1 = MessageBoxButtons.YesNo;
            DialogResult dialogResult = MessageBox.Show(message, title, buttons1, MessageBoxIcon.Question);
            QueryResult queryResult = QueryResult.Yes;
            if (dialogResult != DialogResult.Yes)
                queryResult = QueryResult.No;
            return queryResult;
        }

        private unsafe void DrawFrame(IntPtr videoptr)
        {
            this._keyboard.Scan();
            this.Spectrum.KeyboardState = this._keyboard.State;
            this._mouse.Scan();
            this.Spectrum.MouseX += this._mouse.DeltaX;
            this.Spectrum.MouseY += this._mouse.DeltaY;
            this.Spectrum.MouseButtons = this._mouse.Buttons;
            if (this._sound != null)
            {
                byte[] sndbuf = this._sound.LockBuffer();
                if (sndbuf == null)
                    return;
                try
                {
                    fixed (byte* soundPtr = sndbuf)
                        this.Spectrum.ExecuteFrame(videoptr, (IntPtr)(void*)soundPtr);
                }
                finally
                {
                    this._sound.UnlockBuffer(sndbuf);
                }
            }
            else
                this.Spectrum.ExecuteFrame(videoptr, IntPtr.Zero);
        }

        private void KeyDown(object sender, KeyboardEventArgs e)
        {
            ITapeDevice spectrum = this.Spectrum as ITapeDevice;
            switch (e.Key)
            {
                case Key.Return:
                    if ((e.Mod & ModifierKeys.AltKeys) == ModifierKeys.None)
                        break;
                    this._fullScreen = !this._fullScreen;
                    this.UpdateVideoSettings();
                    break;
                case Key.F2:
                    this.fileSaveAsDialog();
                    break;
                case Key.F3:
                    this.Spectrum.DoReset();
                    break;
                case Key.F4:
                    this.fileOpenDialog();
                    break;
                case Key.F5:
                    this.Spectrum.IsRunning = false;
                    break;
                case Key.F6:
                    TapeForm.GetInstance(this.Spectrum).Show();
                    break;
                case Key.F7:
                    spectrum?.Tape.Rewind(this.Spectrum.CPU.Tact);
                    break;
                case Key.F8:
                    if (spectrum == null)
                        break;
                    if (spectrum.Tape.IsPlay)
                    {
                        spectrum.Tape.Stop(this.Spectrum.CPU.Tact);
                        break;
                    }
                    spectrum.Tape.Play(this.Spectrum.CPU.Tact);
                    break;
                case Key.F9:
                    this.Spectrum.IsRunning = true;
                    break;
            }
        }

        private void Quit(object sender, QuitEventArgs e) => Events.QuitApplication();

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
            if (this.Config.VideoMode > 0)
            {
                this._stretchedSurface.SoftStretch(this._surface, this._destinationRectangle, this._sourceRectangle);
                Video.Screen.Blit((Surface)this._stretchedSurface);
            }
            else
                Video.Screen.Blit((Surface)this._surface);
            Video.Update();
        }

        private void UpdateVideoSettings() => this._surfaceVideo = Video.SetVideoMode(this._destinationSize.Width, this._destinationSize.Height, 32, false, false, this._fullScreen, false, true);

        protected override void Running()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            this._fullScreen = this.Config.FullScreen;
            this._antiAlias = this.Config.AntiAlias;
            Video.WindowIcon();
            Video.WindowCaption = "ZXMAK.NET SDL";
            this._sdlModes = Video.ListModes();
            if (this._sdlModes.Length == 0)
                throw new Exception("Video modes list empty");
            if (this.Config.Help)
            {
                StringBuilder stringBuilder1 = new StringBuilder();
                stringBuilder1.AppendLine("Available video modes (use /vmX):");
                int num1 = 0;
                StringBuilder stringBuilder2 = stringBuilder1;
                int num2 = num1;
                int num3 = num2 + 1;
                string str = num2.ToString();
                stringBuilder2.Append(str);
                stringBuilder1.AppendLine(" - Direct Render");
                foreach (Size sdlMode in this._sdlModes)
                {
                    stringBuilder1.Append(num3++.ToString());
                    stringBuilder1.Append(" - ");
                    stringBuilder1.Append(sdlMode.Width.ToString());
                    stringBuilder1.Append("x");
                    stringBuilder1.AppendLine(sdlMode.Height.ToString());
                }
                PlatformFactory.Platform.ShowNotification(stringBuilder1.ToString(), "Notification");
            }
            else
            {
                if (this._sdlModes.Length < this.Config.VideoMode)
                    throw new Exception("Wrong video mode supplied");
                this.VideoManager.SetVideoDevice((IVideoDevice)this);
                this._mouse = new Mouse();
                this._keyboard = new Keyboard();
                try
                {
                    this._sound = new Audio(44100, (short)3528, 4);
                }
                catch (Exception ex)
                {
                    Logger.GetLogger().LogError(ex);
                }
                Events.Fps = 60;
                Events.Tick += new EventHandler<TickEventArgs>(this.Tick);
                Events.Quit += new EventHandler<QuitEventArgs>(this.Quit);
                Events.KeyboardDown += new EventHandler<KeyboardEventArgs>(this.KeyDown);
                this.OnStartup();
                this.Spectrum.IsRunning = true;
                Events.Run();
                if (this._surface != null)
                {
                    this._surface.Dispose();
                    this._surface = (SurfaceEx)null;
                }
                this.OnShutdown();
            }
        }

        void IVideoDevice.SetResolution(VideoManager sender, int width, int height)
        {
            if (this._surface != null)
            {
                this._surface.Dispose();
                this._surface = (SurfaceEx)null;
            }
            this._surface = new SurfaceEx(width, height);
            if (this.Config.VideoMode > 0)
            {
                this._destinationSize = this._sdlModes[this.Config.VideoMode - 1];
                this._stretchedSurface = new SurfaceEx(this._destinationSize.Width, this._destinationSize.Height);
                this._sourceRectangle = new Rectangle(0, 0, width, height);
                this._destinationRectangle = new Rectangle(0, 0, this._destinationSize.Width, this._destinationSize.Height);
            }
            else
                this._destinationSize = new Size(width, height);
            this.UpdateVideoSettings();
            this.VideoManager.SetVideoParams(new VideoParams(width, height, width, 32));
        }

        private void fileSaveAsDialog()
        {
            bool isRunning = this.Spectrum.IsRunning;
            try
            {
                this.Spectrum.IsRunning = false;
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.InitialDirectory = ".";
                saveFileDialog.SupportMultiDottedExtensions = true;
                saveFileDialog.Title = "Save...";
                string empty = string.Empty;
                saveFileDialog.Filter = this.Spectrum.Loader.GetSaveExtFilter();
                saveFileDialog.DefaultExt = empty;
                saveFileDialog.FileName = "";
                saveFileDialog.OverwritePrompt = true;
                if (saveFileDialog.ShowDialog() != DialogResult.OK)
                    return;
                if (this.Spectrum.Loader.CheckCanSaveFileName(saveFileDialog.FileName))
                    this.Spectrum.Loader.SaveFileName(saveFileDialog.FileName);
                else
                    this.ShowWarning("Unrecognized filetype!", "Error");
            }
            finally
            {
                this.Spectrum.IsRunning = isRunning;
            }
        }

        private void fileOpenDialog()
        {
            bool isRunning = this.Spectrum.IsRunning;
            try
            {
                this.Spectrum.IsRunning = false;
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.InitialDirectory = ".";
                openFileDialog.SupportMultiDottedExtensions = true;
                openFileDialog.Title = "Open...";
                openFileDialog.Filter = this.Spectrum.Loader.GetOpenExtFilter();
                openFileDialog.DefaultExt = "";
                openFileDialog.FileName = "";
                openFileDialog.ShowReadOnly = true;
                openFileDialog.ReadOnlyChecked = true;
                openFileDialog.CheckFileExists = true;
                openFileDialog.FileOk += new CancelEventHandler(this.loadDialog_FileOk);
                if (openFileDialog.ShowDialog() != DialogResult.OK)
                    return;
                if (this.Spectrum.Loader.CheckCanOpenFileName(openFileDialog.FileName))
                    this.Spectrum.Loader.OpenFileName(openFileDialog.FileName, openFileDialog.ReadOnlyChecked, true);
                else
                    this.ShowWarning("Unrecognized filetype!", "Error");
            }
            finally
            {
                this.Spectrum.IsRunning = isRunning;
            }
        }

        private void loadDialog_FileOk(object sender, CancelEventArgs e)
        {
            if (!(sender is OpenFileDialog openFileDialog))
                return;
            e.Cancel = !this.Spectrum.Loader.CheckCanOpenFileName(openFileDialog.FileName);
        }
    }
}
