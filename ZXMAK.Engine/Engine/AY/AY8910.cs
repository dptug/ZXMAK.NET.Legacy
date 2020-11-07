using System;

namespace ZXMAK.Engine.AY
{
	public class AY8910
	{
		public byte ADDR_REG
		{
			get
			{
				return this._curReg;
			}
			set
			{
				this._curReg = value;
			}
		}

		public byte DATA_REG
		{
			get
			{
				switch (this._curReg)
				{
				case 0:
					return (byte)(this.FreqA & 255);
				case 1:
					return (byte)(this.FreqA >> 8);
				case 2:
					return (byte)(this.FreqB & 255);
				case 3:
					return (byte)(this.FreqB >> 8);
				case 4:
					return (byte)(this.FreqC & 255);
				case 5:
					return (byte)(this.FreqC >> 8);
				case 6:
					return this.FreqNoise;
				case 7:
					return this.ControlChannels;
				case 8:
					return this.VolumeA;
				case 9:
					return this.VolumeB;
				case 10:
					return this.VolumeC;
				case 11:
					return (byte)(this.FreqBend & 255);
				case 12:
					return (byte)(this.FreqBend >> 8);
				case 13:
					return this.ControlBend;
				case 14:
					this.OnUpdateIRA(this._ira.OutState);
					return this._ira.InState;
				case 15:
					this.OnUpdateIRB(this._irb.OutState);
					return this._irb.InState;
				default:
					return 0;
				}
			}
			set
			{
				if (this._curReg > 7 && this._curReg < 11)
				{
					if ((value & 16) != 0)
					{
						value &= 16;
					}
					else
					{
						value &= 15;
					}
				}
				value &= this.AY_RegMasks[(int)(this._curReg & 15)];
				switch (this._curReg)
				{
				case 0:
					this.FreqA = (ushort)(((this.FreqA & 65280) | (ushort)value));
					return;
				case 1:
					this.FreqA = (ushort)((int)(this.FreqA & 255) | (int)value << 8);
					return;
				case 2:
					this.FreqB = (ushort)(((this.FreqB & 65280) | (ushort)value));
					return;
				case 3:
					this.FreqB = (ushort)((int)(this.FreqB & 255) | (int)value << 8);
					return;
				case 4:
					this.FreqC = (ushort)(((this.FreqC & 65280) | (ushort)value));
					return;
				case 5:
					this.FreqC = (ushort)((int)(this.FreqC & 255) | (int)value << 8);
					return;
				case 6:
					this.FreqNoise = value;
					return;
				case 7:
					this.ControlChannels = value;
					return;
				case 8:
					this.VolumeA = value;
					return;
				case 9:
					this.VolumeB = value;
					return;
				case 10:
					this.VolumeC = value;
					return;
				case 11:
					this.FreqBend = (ushort)(((this.FreqBend & 65280) | (ushort)value));
					return;
				case 12:
					this.FreqBend = (ushort)((int)(this.FreqBend & 255) | (int)value << 8);
					return;
				case 13:
					this.ControlBend = value;
					if (this.FreqBend != 0)
					{
						if ((value & 4) != 0)
						{
							this.BendVolumeIndex = 0;
							this.BendStatus = 1;
							return;
						}
						this.BendVolumeIndex = 31;
						this.BendStatus = 2;
						return;
					}
					break;
				case 14:
					this.OnUpdateIRA(value);
					return;
				case 15:
					this.OnUpdateIRB(value);
					break;
				default:
					return;
				}
			}
		}

		public AY8910(int sampleRate, int frameTactCount)
		{
			this._frameTactCount = frameTactCount;
			this._sampleRate = sampleRate;
			this.samples = new uint[this._sampleRate / 50];
			this.Volume = 9000;
		}

		public event AyUpdatePortDelegate UpdateIRA;

		public event AyUpdatePortDelegate UpdateIRB;

		protected void OnUpdateIRA(byte outState)
		{
			this._ira.OutState = outState;
			if (this.UpdateIRA != null)
			{
				this.UpdateIRA(this, this._ira);
			}
		}

		protected void OnUpdateIRB(byte outState)
		{
			this._irb.OutState = outState;
			if (this.UpdateIRB != null)
			{
				this.UpdateIRB(this, this._irb);
			}
		}

		public void Reset()
		{
			for (int i = 0; i < 16; i++)
			{
				this.ADDR_REG = (byte)i;
				this.DATA_REG = 0;
			}
			this.process(this.samplePos * this._frameTactCount / this.samples.Length);
		}

		public void process(int tact)
		{
			if (tact > this._frameTactCount)
			{
				tact = this._frameTactCount;
			}
			this.GenSignalAY(this.samples.Length * tact / this._frameTactCount);
		}

		public uint[] flush()
		{
			this.process(this._frameTactCount);
			this.samplePos = 0;
			return this.samples;
		}

		public int Volume
		{
			set
			{
				ushort[] ym_tab = this.YM_tab;
				for (int i = 0; i < 32; i++)
				{
					this.YM_VolTableA[i] = (uint)((long)((int)ym_tab[i] * value / 65535) * (long)((ulong)this.mixerPreset[0]) / 100L + ((long)((int)ym_tab[i] * value / 65535) * (long)((ulong)this.mixerPreset[1]) / 100L << 16));
					this.YM_VolTableB[i] = (uint)((long)((int)ym_tab[i] * value / 65535) * (long)((ulong)this.mixerPreset[2]) / 100L + ((long)((int)ym_tab[i] * value / 65535) * (long)((ulong)this.mixerPreset[3]) / 100L << 16));
					this.YM_VolTableC[i] = (uint)((long)((int)ym_tab[i] * value / 65535) * (long)((ulong)this.mixerPreset[4]) / 100L + ((long)((int)ym_tab[i] * value / 65535) * (long)((ulong)this.mixerPreset[5]) / 100L << 16));
				}
			}
		}

		private void GenSignalAY(int toEndPtr)
		{
			lock (this)
			{
				if (toEndPtr > 882)
				{
					toEndPtr = 882;
				}
				while (this.samplePos < toEndPtr)
				{
					this.OutputNoiseABC &= (byte)((this.ControlChannels & 56) >> 3 ^ 7);
					this.MixLineABC = ((byte)((this.OutputABC & ((this.ControlChannels & 7) ^ 7)) ^ this.OutputNoiseABC));
					byte b;
					if ((this.MixLineABC & 1) == 0)
					{
						b = (byte)((this.VolumeA & 31) << 1);
						if ((b & 32) != 0)
						{
							b = (byte)this.BendVolumeIndex;
						}
						else
						{
							b += 1;
						}
					}
					else
					{
						b = 0;
					}
					byte b2;
					if ((this.MixLineABC & 2) == 0)
					{
						b2 = (byte)((this.VolumeB & 31) << 1);
						if ((b2 & 32) != 0)
						{
							b2 = (byte)this.BendVolumeIndex;
						}
						else
						{
							b2 += 1;
						}
					}
					else
					{
						b2 = 0;
					}
					byte b3;
					if ((this.MixLineABC & 4) == 0)
					{
						b3 = (byte)((this.VolumeC & 31) << 1);
						if ((b3 & 32) != 0)
						{
							b3 = (byte)this.BendVolumeIndex;
						}
						else
						{
							b3 += 1;
						}
					}
					else
					{
						b3 = 0;
					}
					this.samples[this.samplePos] = this.YM_VolTableC[(int)b3] + this.YM_VolTableB[(int)b2] + this.YM_VolTableA[(int)b];
					for (int i = 0; i < 5; i++)
					{
						if ((this.CounterNoise += 1) >= this.FreqNoise)
						{
							this.CounterNoise = 0;
							this.NoiseVal = (((this.NoiseVal >> 16 ^ this.NoiseVal >> 13) & 1U) ^ (this.NoiseVal << 1) + 1U);
							this.OutputNoiseABC = (byte)(this.NoiseVal >> 16 & 1U);
							this.OutputNoiseABC |= (byte)((int)this.OutputNoiseABC << 1 | (int)this.OutputNoiseABC << 2);
						}
						if ((this.CounterA += 1) >= this.FreqA)
						{
							this.CounterA = 0;
							this.OutputABC ^= 1;
						}
						if ((this.CounterB += 1) >= this.FreqB)
						{
							this.CounterB = 0;
							this.OutputABC ^= 2;
						}
						if ((this.CounterC += 1) >= this.FreqC)
						{
							this.CounterC = 0;
							this.OutputABC ^= 4;
						}
						if ((this._CounterBend += 1U) >= (uint)this.FreqBend)
						{
							this._CounterBend = 0U;
							this.ChangeEnvelope();
						}
					}
					this.samplePos++;
				}
			}
		}

		private void ChangeEnvelope()
		{
			if (this.BendStatus == 0)
			{
				return;
			}
			if (this.BendStatus == 1)
			{
				if (++this.BendVolumeIndex >= 31)
				{
					switch (this.ControlBend & 15)
					{
					case 0:
						this.env_DD();
						return;
					case 1:
						this.env_DD();
						return;
					case 2:
						this.env_DD();
						return;
					case 3:
						this.env_DD();
						return;
					case 4:
						this.env_DD();
						return;
					case 5:
						this.env_DD();
						return;
					case 6:
						this.env_DD();
						return;
					case 7:
						this.env_DD();
						return;
					case 8:
						this.env_UD();
						return;
					case 9:
						this.env_DD();
						return;
					case 10:
						this.env_UD();
						return;
					case 11:
						this.env_UU();
						return;
					case 12:
						this.env_DU();
						return;
					case 13:
						this.env_UU();
						return;
					case 14:
						this.env_UD();
						return;
					case 15:
						this.env_DD();
						return;
					default:
						return;
					}
				}
			}
			else if (--this.BendVolumeIndex < 0)
			{
				switch (this.ControlBend & 15)
				{
				case 0:
					this.env_DD();
					return;
				case 1:
					this.env_UU();
					return;
				case 2:
					this.env_DD();
					return;
				case 3:
					this.env_DD();
					return;
				case 4:
					this.env_DD();
					return;
				case 5:
					this.env_DD();
					return;
				case 6:
					this.env_DD();
					return;
				case 7:
					this.env_DD();
					return;
				case 8:
					this.env_UD();
					return;
				case 9:
					this.env_DD();
					return;
				case 10:
					this.env_DU();
					return;
				case 11:
					this.env_UU();
					return;
				case 12:
					this.env_DU();
					return;
				case 13:
					this.env_UU();
					return;
				case 14:
					this.env_DU();
					return;
				case 15:
					this.env_DD();
					break;
				default:
					return;
				}
			}
		}

		private void env_DU()
		{
			this.BendStatus = 1;
			this.BendVolumeIndex = 0;
		}

		private void env_UU()
		{
			this.BendStatus = 0;
			this.BendVolumeIndex = 31;
		}

		private void env_UD()
		{
			this.BendStatus = 2;
			this.BendVolumeIndex = 31;
		}

		private void env_DD()
		{
			this.BendStatus = 0;
			this.BendVolumeIndex = 0;
		}

		private const int SMP_T_RELATION = 5;

		private const int MAX_ENV_VOLTBL = 31;

		private int _sampleRate;

		private int _frameTactCount;

		private ushort FreqA;

		private ushort FreqB;

		private ushort FreqC;

		private byte FreqNoise;

		private byte ControlChannels;

		private byte VolumeA;

		private byte VolumeB;

		private byte VolumeC;

		private ushort FreqBend;

		private byte ControlBend;

		private AyPortState _ira = new AyPortState(byte.MaxValue);

		private AyPortState _irb = new AyPortState(byte.MaxValue);

		private byte _curReg;

		private byte BendStatus;

		private int BendVolumeIndex;

		private uint _CounterBend;

		private byte OutputABC;

		private byte OutputNoiseABC;

		private byte MixLineABC;

		private ushort CounterA;

		private ushort CounterB;

		private ushort CounterC;

		private byte CounterNoise;

		private uint NoiseVal = 65535U;

		private byte[] AY_RegMasks = new byte[]
		{
			byte.MaxValue,
			15,
			byte.MaxValue,
			15,
			byte.MaxValue,
			15,
			31,
			byte.MaxValue,
			31,
			31,
			31,
			byte.MaxValue,
			byte.MaxValue,
			15,
			byte.MaxValue,
			byte.MaxValue
		};

		private int samplePos;

		public uint[] samples;

		private uint[] mixerPreset = new uint[]
		{
			90U,
			15U,
			60U,
			60U,
			15U,
			90U
		};

		private uint[] YM_VolTableA = new uint[32];

		private uint[] YM_VolTableB = new uint[32];

		private uint[] YM_VolTableC = new uint[32];

		private ushort[] YM_tab = new ushort[]
		{
			0,
			0,
			248,
			450,
			670,
			826,
			1010,
			1239,
			1552,
			1919,
			2314,
			2626,
			3131,
			3778,
			4407,
			5031,
			5968,
			7161,
			8415,
			9622,
			11421,
			13689,
			15957,
			18280,
			21759,
			26148,
			30523,
			34879,
			41434,
			49404,
			57492,
			ushort.MaxValue
		};
	}
}
