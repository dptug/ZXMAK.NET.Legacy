using System;
using System.Collections;
using System.Diagnostics;
using System.IO;

namespace ZXMAK.Configuration
{
	public class Config
	{
		public bool StartupOpenDebugger
		{
			get
			{
				return this._startupOpenDebugger;
			}
		}

		public bool StartupDiskWriteProtect
		{
			get
			{
				return this._startupDiskWriteProtect;
			}
		}

		public string[] StartupImageList
		{
			get
			{
				return this._startupImageList;
			}
		}

		public int VideoMode
		{
			get
			{
				return this._videoMode;
			}
		}

		public bool FullScreen
		{
			get
			{
				return this._fullScreen;
			}
		}

		public bool AntiAlias
		{
			get
			{
				return this._antiAlias;
			}
		}

		public bool Help
		{
			get
			{
				return this._help;
			}
		}

		public ulong MaxStepOverTactCount
		{
			get
			{
				return 17920000UL;
			}
		}

		public string LogFileName
		{
			get
			{
				if (this._logFileName == null)
				{
					return Path.ChangeExtension(Process.GetCurrentProcess().MainModule.FileName, ".log");
				}
				return this._logFileName;
			}
		}

		public bool LogAppend
		{
			get
			{
				return this._logAppend;
			}
		}

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
					this._help = true;
				}
				else if (text.StartsWith("/VM"))
				{
					this._videoMode = Convert.ToInt32(text.Remove(0, 3));
				}
				else if (text == "/AA")
				{
					this._antiAlias = true;
				}
				else if (text == "/AA-")
				{
					this._antiAlias = false;
				}
				else if (text == "/F")
				{
					this._fullScreen = true;
				}
				else if (text == "/D")
				{
					this._startupOpenDebugger = true;
				}
				else if (text == "/W")
				{
					this._startupDiskWriteProtect = false;
				}
				else if (text.StartsWith("/M:") || text.StartsWith("/MODEL:"))
				{
					this.Model = text.Substring(text.IndexOf(":") + 1, text.Length - (text.IndexOf(":") + 1));
				}
				else if (File.Exists(args[i]))
				{
					arrayList.Add(args[i]);
				}
			}
			this._startupImageList = (string[])arrayList.ToArray(typeof(string));
		}

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
	}
}
