using System;
using System.Text;

namespace ZXMAK.Logging;

public class LogMessage
{
	private string _name;

	private DateTime _dateTime;

	private long _tick;

	private LogLevel _level;

	private string _message;

	private Exception _exception;

	public string Name => _name;

	public DateTime DateTime => _dateTime;

	public long Tick => _tick;

	public LogLevel Level => _level;

	public string Message => _message;

	public Exception Exception => _exception;

	public LogMessage(string name, DateTime time, long tick, LogLevel level, string msg, Exception ex)
	{
		_name = name;
		_dateTime = time;
		_tick = tick;
		_level = level;
		_message = msg;
		_exception = ex;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (_name != string.Empty)
		{
			stringBuilder.Append(_name + "\t");
		}
		stringBuilder.Append(_level.ToString() + "\t");
		stringBuilder.Append(_dateTime.ToString() + "\t");
		stringBuilder.Append(_tick + "\t");
		if (_message != null)
		{
			stringBuilder.Append(_message.ToString() + "\t");
		}
		string exceptionString = getExceptionString();
		if (exceptionString != null)
		{
			stringBuilder.Append(exceptionString);
		}
		return stringBuilder.ToString();
	}

	private string getExceptionString()
	{
		if (_exception != null)
		{
			return parseException(0, _exception);
		}
		return null;
	}

	private string parseException(int tabSize, Exception ex)
	{
		StringBuilder stringBuilder = new StringBuilder();
		string text = new string(' ', tabSize);
		stringBuilder.Append(text + "Type: " + ex.GetType().ToString() + Environment.NewLine);
		stringBuilder.Append(text + "Message: " + ex.Message + Environment.NewLine);
		stringBuilder.Append(text + "Stack trace:" + Environment.NewLine);
		stringBuilder.Append(text + ex.StackTrace + Environment.NewLine);
		if (ex.InnerException != null)
		{
			stringBuilder.Append(text + "======Inner exception======" + Environment.NewLine);
			stringBuilder.Append(parseException(tabSize + 4, ex.InnerException));
			stringBuilder.Append(text + "===========================" + Environment.NewLine);
		}
		return stringBuilder.ToString();
	}
}
