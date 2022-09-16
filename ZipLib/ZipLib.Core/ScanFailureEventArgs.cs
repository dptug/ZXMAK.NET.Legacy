using System;

namespace ZipLib.Core;

public class ScanFailureEventArgs
{
	private string name_;

	private Exception exception_;

	private bool continueRunning_;

	public string Name => name_;

	public Exception Exception => exception_;

	public bool ContinueRunning
	{
		get
		{
			return continueRunning_;
		}
		set
		{
			continueRunning_ = value;
		}
	}

	public ScanFailureEventArgs(string name, Exception e)
	{
		name_ = name;
		exception_ = e;
		continueRunning_ = true;
	}
}
