using System;
using ZXMAK.Configuration;
using ZXMAK.Engine;
using ZXMAK.Logging;

namespace ZXMAK.Platform
{
	public abstract class GenericPlatform
	{
		public Config Config
		{
			get
			{
				return this._config;
			}
		}

		public VideoManager VideoManager
		{
			get
			{
				return this._videoManager;
			}
		}

		public Spectrum Spectrum
		{
			get
			{
				return this._spectrum;
			}
		}

		public virtual void Init(Config config)
		{
			this._config = config;
			string a;
			if ((a = this.Config.Model.ToUpper()) != null)
			{
				if (a == "PROFI1024")
				{
					this._spectrum = new Profi1024();
					goto IL_4F;
				}
				if (!(a == "PENTAGON128"))
				{
				}
			}
			this._spectrum = new Pentagon128();
			IL_4F:
			this._spectrum.Init(this);
			this._spectrum.DoReset();
		}

		public void Run()
		{
			Logger.GetLogger().LogMessage(base.GetType().ToString() + " started...");
			try
			{
				this.Running();
			}
			catch (Exception ex)
			{
				Logger.GetLogger().LogError(ex);
				this.ShowFatalError(ex);
			}
			finally
			{
				Logger.GetLogger().LogMessage(base.GetType().ToString() + " finished...");
			}
		}

		protected abstract void Running();

		public abstract void ShowFatalError(Exception ex);

		public abstract void ShowWarning(string message, string title);

		public abstract void ShowNotification(string message, string title);

		public abstract void SetCaption(string text);

		public abstract QueryResult QueryDialog(string message, string title, QueryButtons buttons);

		public void OnStartup()
		{
			foreach (string text in this._config.StartupImageList)
			{
				if (this.Spectrum.Loader.CheckCanOpenFileName(text))
				{
					this.Spectrum.Loader.OpenFileName(text, this.Config.StartupDiskWriteProtect, true);
				}
				else
				{
					this.ShowWarning("Can't open file!\n" + text + "\n\nHelp:\n/D - start in debug mode\n/W - open file without write protect\n<filename> - file to open", "Error!");
				}
			}
			if (!this._config.StartupOpenDebugger)
			{
				this._spectrum.IsRunning = true;
				return;
			}
			this._spectrum.IsRunning = false;
		}

		public void OnShutdown()
		{
			IBetaDiskDevice betaDiskDevice = this.Spectrum as IBetaDiskDevice;
			if (betaDiskDevice != null)
			{
				for (int i = 0; i < betaDiskDevice.BetaDisk.FDD.Length; i++)
				{
					betaDiskDevice.BetaDisk.FDD[i].Eject();
				}
			}
		}

		private Spectrum _spectrum;

		private Config _config;

		private VideoManager _videoManager = new VideoManager();
	}
}
