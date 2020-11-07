using System;

namespace ZXMAK.Engine.AY
{
	public class AyPortState
	{
		public byte OutState
		{
			get
			{
				return this._outState;
			}
			set
			{
				this._oldOutState = this._outState;
				this._outState = value;
			}
		}

		public byte OldOutState
		{
			get
			{
				return this._oldOutState;
			}
		}

		public byte InState
		{
			get
			{
				return this._inState;
			}
			set
			{
				this._inState = value;
			}
		}

		public AyPortState(byte value)
		{
			this._outState = value;
			this._oldOutState = value;
			this._inState = value;
		}

		private byte _outState;

		private byte _oldOutState;

		private byte _inState;
	}
}
