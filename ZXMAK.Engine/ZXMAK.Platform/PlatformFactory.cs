using System;
using ZXMAK.Configuration;
using ZXMAK.Engine;
using ZXMAK.Logging;

namespace ZXMAK.Platform;

public class PlatformFactory
{
	private static GenericPlatform _platform;

	public static GenericPlatform Platform => _platform;

	public static void Execute(string[] args, GenericPlatform platform)
	{
		Config config = new Config();
		new VideoManager();
		config.Load();
		config.ParseCommandLine(args);
		Logger.Init(config.LogFileName, config.LogAppend);
		Logger.Start();
		try
		{
			_platform = platform;
			platform.Init(config);
			platform.Run();
		}
		catch (Exception ex)
		{
			Logger.GetLogger().LogError(ex);
			platform.ShowFatalError(ex);
		}
		finally
		{
			Logger.Finish();
		}
	}
}
