using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace ZXMAK.Logging
{
	public class Logger
	{
		public static void Init(string path, bool append)
		{
			Logger._path = path;
			Logger._append = append;
		}

		public static void Start()
		{
			lock (Logger._syncStart)
			{
				if (!Logger._isStarted)
				{
					if (Logger._path == null)
					{
						Logger._path = Logger.getDefaultFilePath();
					}
					Logger._logWriteThread = new Thread(new ThreadStart(Logger.logWriteProc));
					Logger._logWriteThread.Name = "Logger thread";
					Logger._logWriteThread.IsBackground = false;
					Logger._isStarted = true;
					Logger._logWriteThread.Start();
				}
			}
		}

		public static void Finish()
		{
			lock (Logger._syncStart)
			{
				if (Logger._isStarted)
				{
					Logger._isStarted = false;
					Logger._logQueueEvent.Set();
					Logger._logWriteThread.Join();
				}
			}
		}

		public static Log GetLogger()
		{
			return Logger.GetLogger(string.Empty);
		}

		public static Log GetLogger(string name)
		{
			Log result;
			lock (Logger._loggers.SyncRoot)
			{
				if (Logger._loggers.ContainsKey(name))
				{
					result = (Logger._loggers[name] as Log);
				}
				else
				{
					Log log = new Logger.LogReceiver(name);
					Logger._loggers.Add(name, log);
					result = log;
				}
			}
			return result;
		}

		private static string getDefaultFilePath()
		{
			return Path.ChangeExtension(Process.GetCurrentProcess().MainModule.FileName, ".log");
		}

		private static void logWriteProc()
		{
			try
			{
				Stream stream;
				if (!Logger._append)
				{
					stream = new FileStream(Logger._path, FileMode.Create, FileAccess.Write, FileShare.Read);
				}
				else
				{
					stream = new FileStream(Logger._path, FileMode.Append, FileAccess.Write, FileShare.Read);
					stream.Seek(0L, SeekOrigin.End);
				}
				using (stream)
				{
					while (Logger._isStarted)
					{
						Logger.flushQueue(stream);
						if (!Logger._logQueueEvent.WaitOne(1000, false))
						{
							stream.Flush();
						}
					}
					Logger.flushQueue(stream);
					stream.Flush();
				}
			}
			catch
			{
			}
			Logger._isStarted = false;
		}

		private static void flushQueue(Stream stream)
		{
			lock (Logger._logQueue.SyncRoot)
			{
				while (Logger._logQueue.Count > 0)
				{
					byte[] bytes = Encoding.UTF8.GetBytes((Logger._logQueue.Dequeue() as LogMessage).ToString());
					stream.Write(bytes, 0, bytes.Length);
					stream.Write(Logger._separator, 0, Logger._separator.Length);
				}
			}
		}

		private static void log(string name, LogLevel level, string message, Exception ex)
		{
			lock (Logger._syncStart)
			{
				if (!Logger._isStarted)
				{
					return;
				}
			}
			lock (Logger._logQueue.SyncRoot)
			{
				Logger._logQueue.Enqueue(new LogMessage(name, DateTime.Now, Stopwatch.GetTimestamp(), level, message, ex));
				Logger._logQueueEvent.Set();
			}
		}

		private static readonly object _syncStart = new object();

		private static bool _isStarted = false;

		private static Hashtable _loggers = new Hashtable();

		private static Queue _logQueue = new Queue();

		private static Thread _logWriteThread = null;

		private static AutoResetEvent _logQueueEvent = new AutoResetEvent(false);

		private static string _path = null;

		private static bool _append = false;

		private static byte[] _separator = Encoding.UTF8.GetBytes(Environment.NewLine);

		private class LogReceiver : Log
		{
			public LogReceiver(string name)
			{
				this._name = name;
			}

			protected override void LogMessage(LogLevel level, string message, Exception ex)
			{
				Logger.log(this._name, level, message, ex);
			}

			private string _name;
		}
	}
}
