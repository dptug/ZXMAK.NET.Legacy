using System;
using System.Text;

namespace ZXMAK.Logging
{
	public class LogMessage
	{
		public LogMessage(string name, DateTime time, long tick, LogLevel level, string msg, Exception ex)
		{
			this._name = name;
			this._dateTime = time;
			this._tick = tick;
			this._level = level;
			this._message = msg;
			this._exception = ex;
		}

		public string Name
		{
			get
			{
				return this._name;
			}
		}

		public DateTime DateTime
		{
			get
			{
				return this._dateTime;
			}
		}

		public long Tick
		{
			get
			{
				return this._tick;
			}
		}

		public LogLevel Level
		{
			get
			{
				return this._level;
			}
		}

		public string Message
		{
			get
			{
				return this._message;
			}
		}

		public Exception Exception
		{
			get
			{
				return this._exception;
			}
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (this._name != string.Empty)
			{
				stringBuilder.Append(this._name + "\t");
			}
			stringBuilder.Append(this._level.ToString() + "\t");
			stringBuilder.Append(this._dateTime.ToString() + "\t");
			stringBuilder.Append(this._tick.ToString() + "\t");
			if (this._message != null)
			{
				stringBuilder.Append(this._message.ToString() + "\t");
			}
			string exceptionString = this.getExceptionString();
			if (exceptionString != null)
			{
				stringBuilder.Append(exceptionString);
			}
			return stringBuilder.ToString();
		}

		private string getExceptionString()
		{
			if (this._exception != null)
			{
				return this.parseException(0, this._exception);
			}
			return null;
		}

		private string parseException(int tabSize, Exception ex)
		{
			StringBuilder stringBuilder = new StringBuilder();
			string str = new string(' ', tabSize);
			stringBuilder.Append(str + "Type: " + ex.GetType().ToString() + Environment.NewLine);
			stringBuilder.Append(str + "Message: " + ex.Message + Environment.NewLine);
			stringBuilder.Append(str + "Stack trace:" + Environment.NewLine);
			stringBuilder.Append(str + ex.StackTrace + Environment.NewLine);
			if (ex.InnerException != null)
			{
				stringBuilder.Append(str + "======Inner exception======" + Environment.NewLine);
				stringBuilder.Append(this.parseException(tabSize + 4, ex.InnerException));
				stringBuilder.Append(str + "===========================" + Environment.NewLine);
			}
			return stringBuilder.ToString();
		}

		private string _name;

		private DateTime _dateTime;

		private long _tick;

		private LogLevel _level;

		private string _message;

		private Exception _exception;
	}
}
