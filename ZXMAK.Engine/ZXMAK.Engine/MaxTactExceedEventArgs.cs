using System;

namespace ZXMAK.Engine;

public class MaxTactExceedEventArgs : EventArgs
{
	private bool _cancel;

	private ulong _tact;

	public bool Cancel
	{
		get
		{
			return _cancel;
		}
		set
		{
			_cancel = value;
		}
	}

	public ulong Tact
	{
		get
		{
			return _tact;
		}
		set
		{
			_tact = value;
		}
	}

	public MaxTactExceedEventArgs(bool cancel, ulong tact)
	{
		_cancel = cancel;
		_tact = tact;
	}
}
