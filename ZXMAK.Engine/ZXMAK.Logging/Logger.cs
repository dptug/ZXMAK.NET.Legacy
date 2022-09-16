using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace ZXMAK.Logging;

public class Logger
{
	private class LogReceiver : Log
	{
		private string _name;

		public LogReceiver(string name)
		{
			_name = name;
		}

		protected override void LogMessage(LogLevel level, string message, Exception ex)
		{
			log(_name, level, message, ex);
		}
	}

	private static readonly object _syncStart = new object();

	private static bool _isStarted = false;

	private static Hashtable _loggers = new Hashtable();

	private static Queue _logQueue = new Queue();

	private static Thread _logWriteThread = null;

	private static AutoResetEvent _logQueueEvent = new AutoResetEvent(initialState: false);

	private static string _path = null;

	private static bool _append = false;

	private static byte[] _separator = Encoding.UTF8.GetBytes(Environment.NewLine);

	public static void Init(string path, bool append)
	{
		_path = path;
		_append = append;
	}

	public static void Start()
	{
		lock (_syncStart)
		{
			if (!_isStarted)
			{
				if (_path == null)
				{
					_path = getDefaultFilePath();
				}
				_logWriteThread = new Thread(logWriteProc);
				_logWriteThread.Name = "Logger thread";
				_logWriteThread.IsBackground = false;
				_isStarted = true;
				_logWriteThread.Start();
			}
		}
	}

	public static void Finish()
	{
		lock (_syncStart)
		{
			if (_isStarted)
			{
				_isStarted = false;
				_logQueueEvent.Set();
				_logWriteThread.Join();
			}
		}
	}

	public static Log GetLogger()
	{
		return GetLogger(string.Empty);
	}

	public static Log GetLogger(string name)
	{
		lock (_loggers.SyncRoot)
		{
			if (_loggers.ContainsKey(name))
			{
				return _loggers[name] as Log;
			}
			Log log = new LogReceiver(name);
			_loggers.Add(name, log);
			return log;
		}
	}

	private static string getDefaultFilePath()
	{
		return Path.ChangeExtension(Process.GetCurrentProcess().MainModule.FileName, ".log");
	}

	private static void logWriteProc()
	{
		try
		{
			Stream stream = null;
			if (!_append)
			{
				stream = new FileStream(_path, FileMode.Create, FileAccess.Write, FileShare.Read);
			}
			else
			{
				stream = new FileStream(_path, FileMode.Append, FileAccess.Write, FileShare.Read);
				stream.Seek(0L, SeekOrigin.End);
			}
			using (stream)
			{
				while (_isStarted)
				{
					flushQueue(stream);
					if (!_logQueueEvent.WaitOne(1000, exitContext: false))
					{
						stream.Flush();
					}
				}
				flushQueue(stream);
				stream.Flush();
			}
		}
		catch
		{
		}
		_isStarted = false;
	}

	private static void flushQueue(Stream stream)
	{
		lock (_logQueue.SyncRoot)
		{
			while (_logQueue.Count > 0)
			{
				byte[] bytes = Encoding.UTF8.GetBytes((_logQueue.Dequeue() as LogMessage).ToString());
				stream.Write(bytes, 0, bytes.Length);
				stream.Write(_separator, 0, _separator.Length);
			}
		}
	}

	private static void log(string name, LogLevel level, string message, Exception ex)
	{
		lock (_syncStart)
		{
			if (!_isStarted)
			{
				return;
			}
		}
		lock (_logQueue.SyncRoot)
		{
			_logQueue.Enqueue(new LogMessage(name, DateTime.Now, Stopwatch.GetTimestamp(), level, message, ex));
			_logQueueEvent.Set();
		}
	}
}
