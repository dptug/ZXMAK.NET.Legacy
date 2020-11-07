using System;

namespace ZXMAK.Engine
{
	public class MaxTactExceedEventArgs : EventArgs
	{
		public MaxTactExceedEventArgs(bool cancel, ulong tact)
		{
			this._cancel = cancel;
			this._tact = tact;
		}

		public bool Cancel
		{
			get
			{
				return this._cancel;
			}
			set
			{
				this._cancel = value;
			}
		}

		public ulong Tact
		{
			get
			{
				return this._tact;
			}
			set
			{
				this._tact = value;
			}
		}

		private bool _cancel;

		private ulong _tact;
	}
}
