using System;

namespace ZXMAK.Logging
{
	public abstract class Log
	{
		public virtual void LogMessage(string message)
		{
			this.LogMessage(LogLevel.Message, message, null);
		}

		public virtual void LogMessage(Exception ex)
		{
			this.LogMessage(LogLevel.Message, null, ex);
		}

		public virtual void LogMessage(string message, Exception ex)
		{
			this.LogMessage(LogLevel.Message, message, ex);
		}

		public virtual void LogWarning(string message)
		{
			this.LogMessage(LogLevel.Warning, message, null);
		}

		public virtual void LogWarning(Exception ex)
		{
			this.LogMessage(LogLevel.Warning, null, ex);
		}

		public virtual void LogWarning(string message, Exception ex)
		{
			this.LogMessage(LogLevel.Warning, message, ex);
		}

		public virtual void LogError(string message)
		{
			this.LogMessage(LogLevel.Error, message, null);
		}

		public virtual void LogError(Exception ex)
		{
			this.LogMessage(LogLevel.Error, null, ex);
		}

		public virtual void LogError(string message, Exception ex)
		{
			this.LogMessage(LogLevel.Error, message, ex);
		}

		public virtual void LogFatal(string message)
		{
			this.LogMessage(LogLevel.Fatal, message, null);
		}

		public virtual void LogFatal(string message, Exception ex)
		{
			this.LogMessage(LogLevel.Fatal, message, ex);
		}

		public virtual void LogFatal(Exception ex)
		{
			this.LogMessage(LogLevel.Fatal, null, ex);
		}

		public virtual void LogTrace(string message)
		{
			this.LogMessage(LogLevel.Trace, message, null);
		}

		public virtual void LogTrace(Exception ex)
		{
			this.LogMessage(LogLevel.Trace, null, ex);
		}

		public virtual void LogTrace(string message, Exception ex)
		{
			this.LogMessage(LogLevel.Trace, message, ex);
		}

		protected abstract void LogMessage(LogLevel level, string message, Exception ex);
	}
}
