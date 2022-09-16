using System;
using ZXMAK.Configuration;
using ZXMAK.Engine;
using ZXMAK.Logging;

namespace ZXMAK.Platform;

public abstract class GenericPlatform
{
	private Spectrum _spectrum;

	private Config _config;

	private VideoManager _videoManager = new VideoManager();

	public Config Config => _config;

	public VideoManager VideoManager => _videoManager;

	public Spectrum Spectrum => _spectrum;

	public virtual void Init(Config config)
	{
		_config = config;
		switch (Config.Model.ToUpper())
		{
		case "PROFI1024":
			_spectrum = new Profi1024();
			break;
		default:
			_spectrum = new Pentagon128();
			break;
		}
		_spectrum.Init(this);
		_spectrum.DoReset();
	}

	public void Run()
	{
		Logger.GetLogger().LogMessage(GetType().ToString() + " started...");
		try
		{
			Running();
		}
		catch (Exception ex)
		{
			Logger.GetLogger().LogError(ex);
			ShowFatalError(ex);
		}
		finally
		{
			Logger.GetLogger().LogMessage(GetType().ToString() + " finished...");
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
		string[] startupImageList = _config.StartupImageList;
		foreach (string text in startupImageList)
		{
			if (Spectrum.Loader.CheckCanOpenFileName(text))
			{
				Spectrum.Loader.OpenFileName(text, Config.StartupDiskWriteProtect, x: true);
			}
			else
			{
				ShowWarning("Can't open file!\n" + text + "\n\nHelp:\n/D - start in debug mode\n/W - open file without write protect\n<filename> - file to open", "Error!");
			}
		}
		if (!_config.StartupOpenDebugger)
		{
			_spectrum.IsRunning = true;
		}
		else
		{
			_spectrum.IsRunning = false;
		}
	}

	public void OnShutdown()
	{
		if (Spectrum is IBetaDiskDevice betaDiskDevice)
		{
			for (int i = 0; i < betaDiskDevice.BetaDisk.FDD.Length; i++)
			{
				betaDiskDevice.BetaDisk.FDD[i].Eject();
			}
		}
	}
}
