using System;
using System.Collections;
using System.Diagnostics;
using System.IO;

namespace ZXMAK.Configuration;

public class Config
{
	private string _logFileName;

	private bool _logAppend;

	private bool _startupOpenDebugger;

	private bool _startupDiskWriteProtect = true;

	private string[] _startupImageList = new string[0];

	private int _videoMode;

	private bool _fullScreen;

	private bool _antiAlias = true;

	private bool _help;

	public bool TapeInSoundEnable = true;

	public bool TapeOutSoundEnable = true;

	public string Model = "PENTAGON128";

	public bool StartupOpenDebugger => _startupOpenDebugger;

	public bool StartupDiskWriteProtect => _startupDiskWriteProtect;

	public string[] StartupImageList => _startupImageList;

	public int VideoMode => _videoMode;

	public bool FullScreen => _fullScreen;

	public bool AntiAlias => _antiAlias;

	public bool Help => _help;

	public ulong MaxStepOverTactCount => 17920000uL;

	public string LogFileName
	{
		get
		{
			if (_logFileName == null)
			{
				return Path.ChangeExtension(Process.GetCurrentProcess().MainModule.FileName, ".log");
			}
			return _logFileName;
		}
	}

	public bool LogAppend => _logAppend;

	public void Load()
	{
	}

	public void ParseCommandLine(string[] args)
	{
		ArrayList arrayList = new ArrayList();
		for (int i = 0; i < args.Length; i++)
		{
			string text = args[i].ToUpper();
			if (text.StartsWith("/?") || text.StartsWith("--HELP") || text.StartsWith("-H"))
			{
				_help = true;
				continue;
			}
			if (text.StartsWith("/VM"))
			{
				_videoMode = Convert.ToInt32(text.Remove(0, 3));
				continue;
			}
			switch (text)
			{
			case "/AA":
				_antiAlias = true;
				continue;
			case "/AA-":
				_antiAlias = false;
				continue;
			case "/F":
				_fullScreen = true;
				continue;
			case "/D":
				_startupOpenDebugger = true;
				continue;
			case "/W":
				_startupDiskWriteProtect = false;
				continue;
			}
			if (text.StartsWith("/M:") || text.StartsWith("/MODEL:"))
			{
				Model = text.Substring(text.IndexOf(":") + 1, text.Length - (text.IndexOf(":") + 1));
			}
			else if (File.Exists(args[i]))
			{
				arrayList.Add(args[i]);
			}
		}
		_startupImageList = (string[])arrayList.ToArray(typeof(string));
	}
}
