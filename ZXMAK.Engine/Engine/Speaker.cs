using System;

namespace ZXMAK.Engine
{
	public class Speaker
	{
		public bool TapeInEnable
		{
			get
			{
				return this._tapeInEnable;
			}
			set
			{
				this._tapeInEnable = value;
			}
		}

		public bool TapeOutEnable
		{
			get
			{
				return this._tapeOutEnable;
			}
			set
			{
				this._tapeOutEnable = value;
			}
		}

		public bool BeepOutEnable
		{
			get
			{
				return this._beepOutEnable;
			}
			set
			{
				this._beepOutEnable = value;
			}
		}

		public Speaker(int sampleRate, int frameTactCount)
		{
			this._frameTactCount = frameTactCount;
			this._sampleRate = sampleRate;
			this._frameData = new uint[this._sampleRate / 50];
			this._pos = 0;
			this._lastValue = 0;
		}

		public void SetPort(int frameTact, int value)
		{
			if (this._lastValue != value)
			{
				this.process(frameTact);
			}
			this._lastValue = value;
		}

		public uint[] FlushFrame()
		{
			this.process(this._frameTactCount);
			this._pos = 0;
			return this._frameData;
		}

		private void process(int frameTact)
		{
			int num = this._frameData.Length * frameTact / this._frameTactCount;
			if (num > this._frameData.Length)
			{
				num = this._frameData.Length;
			}
			if (num > this._pos)
			{
				uint num2 = 0U;
				if (this._beepOutEnable && (this._lastValue & 16) != 0)
				{
					num2 += 8191U;
				}
				if (this._tapeOutEnable && (this._lastValue & 8) != 0)
				{
					num2 += 8191U;
				}
				if (this._tapeInEnable && (this._lastValue & 64) != 0)
				{
					num2 += 8191U;
				}
				num2 /= 3U;
				num2 |= num2 << 16;
				while (this._pos < num)
				{
					this._frameData[this._pos] = num2;
					this._pos++;
				}
			}
		}

		private bool _tapeInEnable = true;

		private bool _tapeOutEnable = true;

		private bool _beepOutEnable = true;

		private int _sampleRate;

		private int _frameTactCount;

		private uint[] _frameData;

		private int _pos;

		private int _lastValue;
	}
}
