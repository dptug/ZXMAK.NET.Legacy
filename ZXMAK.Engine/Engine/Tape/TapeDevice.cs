using System;
using System.Collections.Generic;

namespace ZXMAK.Engine.Tape
{
	public class TapeDevice
	{
		public TapeDevice()
		{
			this._Z80FQ = 3500000UL;
		}

		public ulong Z80FQ
		{
			get
			{
				return this._Z80FQ;
			}
		}

		public List<TapeBlock> Blocks
		{
			get
			{
				return this._blocks;
			}
			set
			{
				this._blocks = value;
			}
		}

		public int CurrentBlock
		{
			get
			{
				if (this._blocks.Count > 0)
				{
					return this._index;
				}
				return -1;
			}
			set
			{
				if (this._index >= 0 && this._index < this._blocks.Count)
				{
					this._index = value;
					this._playPosition = 0;
				}
			}
		}

		public event EventHandler TapeStateChanged;

		private void raiseTapeStateChanged()
		{
			if (this.TapeStateChanged != null)
			{
				this.TapeStateChanged(this, new EventArgs());
			}
		}

		public int Position
		{
			get
			{
				if (this._playPosition >= this._blocks[this._index].Periods.Count)
				{
					return 0;
				}
				return this._playPosition;
			}
		}

		private int tape_bit(ulong globalTact)
		{
			int i = (int)(globalTact - this._lastTact);
			if (!this._play)
			{
				this._lastTact = globalTact;
				return -1;
			}
			if (this._index < 0)
			{
				this._play = false;
				this.raiseTapeStateChanged();
				return this._state;
			}
			while (i >= this._waitEdge)
			{
				i -= this._waitEdge;
				this._state ^= -1;
				this._playPosition++;
				if (this._playPosition >= this._blocks[this._index].Periods.Count)
				{
					while (this._playPosition >= this._blocks[this._index].Periods.Count)
					{
						this._playPosition = 0;
						this._index++;
						if (this._index >= this._blocks.Count)
						{
							break;
						}
					}
					if (this._index >= this._blocks.Count)
					{
						this._lastTact = globalTact;
						this._index = 0;
						this._play = false;
						this.raiseTapeStateChanged();
						return this._state;
					}
					this.raiseTapeStateChanged();
				}
				this._waitEdge = this._blocks[this._index].Periods[this._playPosition];
			}
			this._lastTact = globalTact - (ulong)((long)i);
			return this._state;
		}

		public byte GetTapeBit(ulong globalTact)
		{
			return (byte)(this.tape_bit(globalTact) & 64);
		}

		public bool IsPlay
		{
			get
			{
				return this._play;
			}
		}

		public void Reset()
		{
			this._waitEdge = 0;
			this._index = -1;
			if (this._blocks.Count > 0)
			{
				this._index = 0;
			}
			this._playPosition = 0;
			this._play = false;
			this.raiseTapeStateChanged();
		}

		public void Rewind(ulong globalTact)
		{
			this._lastTact = globalTact;
			this._waitEdge = 0;
			this._index = -1;
			if (this._blocks.Count > 0)
			{
				this._index = 0;
			}
			this._playPosition = 0;
			this._play = false;
			this.raiseTapeStateChanged();
		}

		public void Play(ulong globalTact)
		{
			this._lastTact = globalTact;
			if (this._blocks.Count > 0 && this._index >= 0)
			{
				while (this._playPosition >= this._blocks[this._index].Periods.Count)
				{
					this._playPosition = 0;
					this._index++;
					if (this._index >= this._blocks.Count)
					{
						break;
					}
				}
				if (this._index >= this._blocks.Count)
				{
					this._index = -1;
					return;
				}
				this._state ^= -1;
				this._waitEdge = this._blocks[this._index].Periods[this._playPosition];
				this._play = true;
				this.raiseTapeStateChanged();
			}
		}

		public void Stop(ulong globalTact)
		{
			this._lastTact = globalTact;
			this._play = false;
			this.raiseTapeStateChanged();
		}

		private ulong _Z80FQ = 3500000UL;

		private List<TapeBlock> _blocks = new List<TapeBlock>();

		private int _index;

		private int _playPosition;

		private bool _play;

		private ulong _lastTact;

		private int _waitEdge;

		private int _state;
	}
}
