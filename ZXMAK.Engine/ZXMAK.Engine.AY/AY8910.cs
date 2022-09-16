namespace ZXMAK.Engine.AY;

public class AY8910
{
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

	private uint NoiseVal = 65535u;

	private byte[] AY_RegMasks = new byte[16]
	{
		255, 15, 255, 15, 255, 15, 31, 255, 31, 31,
		31, 255, 255, 15, 255, 255
	};

	private int samplePos;

	public uint[] samples;

	private uint[] mixerPreset = new uint[6] { 90u, 15u, 60u, 60u, 15u, 90u };

	private uint[] YM_VolTableA = new uint[32];

	private uint[] YM_VolTableB = new uint[32];

	private uint[] YM_VolTableC = new uint[32];

	private ushort[] YM_tab = new ushort[32]
	{
		0, 0, 248, 450, 670, 826, 1010, 1239, 1552, 1919,
		2314, 2626, 3131, 3778, 4407, 5031, 5968, 7161, 8415, 9622,
		11421, 13689, 15957, 18280, 21759, 26148, 30523, 34879, 41434, 49404,
		57492, 65535
	};

	public byte ADDR_REG
	{
		get
		{
			return _curReg;
		}
		set
		{
			_curReg = value;
		}
	}

	public byte DATA_REG
	{
		get
		{
			switch (_curReg)
			{
			case 0:
				return (byte)(FreqA & 0xFFu);
			case 1:
				return (byte)(FreqA >> 8);
			case 2:
				return (byte)(FreqB & 0xFFu);
			case 3:
				return (byte)(FreqB >> 8);
			case 4:
				return (byte)(FreqC & 0xFFu);
			case 5:
				return (byte)(FreqC >> 8);
			case 6:
				return FreqNoise;
			case 7:
				return ControlChannels;
			case 8:
				return VolumeA;
			case 9:
				return VolumeB;
			case 10:
				return VolumeC;
			case 11:
				return (byte)(FreqBend & 0xFFu);
			case 12:
				return (byte)(FreqBend >> 8);
			case 13:
				return ControlBend;
			case 14:
				OnUpdateIRA(_ira.OutState);
				return _ira.InState;
			case 15:
				OnUpdateIRB(_irb.OutState);
				return _irb.InState;
			default:
				return 0;
			}
		}
		set
		{
			if (_curReg > 7 && _curReg < 11)
			{
				value = (((value & 0x10) == 0) ? ((byte)(value & 0xFu)) : ((byte)(value & 0x10u)));
			}
			value = (byte)(value & AY_RegMasks[_curReg & 0xF]);
			switch (_curReg)
			{
			case 0:
				FreqA = (ushort)((FreqA & 0xFF00u) | value);
				break;
			case 1:
				FreqA = (ushort)((FreqA & 0xFFu) | (uint)(value << 8));
				break;
			case 2:
				FreqB = (ushort)((FreqB & 0xFF00u) | value);
				break;
			case 3:
				FreqB = (ushort)((FreqB & 0xFFu) | (uint)(value << 8));
				break;
			case 4:
				FreqC = (ushort)((FreqC & 0xFF00u) | value);
				break;
			case 5:
				FreqC = (ushort)((FreqC & 0xFFu) | (uint)(value << 8));
				break;
			case 6:
				FreqNoise = value;
				break;
			case 7:
				ControlChannels = value;
				break;
			case 8:
				VolumeA = value;
				break;
			case 9:
				VolumeB = value;
				break;
			case 10:
				VolumeC = value;
				break;
			case 11:
				FreqBend = (ushort)((FreqBend & 0xFF00u) | value);
				break;
			case 12:
				FreqBend = (ushort)((FreqBend & 0xFFu) | (uint)(value << 8));
				break;
			case 13:
				ControlBend = value;
				if (FreqBend != 0)
				{
					if ((value & 4u) != 0)
					{
						BendVolumeIndex = 0;
						BendStatus = 1;
					}
					else
					{
						BendVolumeIndex = 31;
						BendStatus = 2;
					}
				}
				break;
			case 14:
				OnUpdateIRA(value);
				break;
			case 15:
				OnUpdateIRB(value);
				break;
			}
		}
	}

	public int Volume
	{
		set
		{
			ushort[] yM_tab = YM_tab;
			for (int i = 0; i < 32; i++)
			{
				YM_VolTableA[i] = (uint)(yM_tab[i] * value / 65535 * mixerPreset[0] / 100 + (yM_tab[i] * value / 65535 * mixerPreset[1] / 100 << 16));
				YM_VolTableB[i] = (uint)(yM_tab[i] * value / 65535 * mixerPreset[2] / 100 + (yM_tab[i] * value / 65535 * mixerPreset[3] / 100 << 16));
				YM_VolTableC[i] = (uint)(yM_tab[i] * value / 65535 * mixerPreset[4] / 100 + (yM_tab[i] * value / 65535 * mixerPreset[5] / 100 << 16));
			}
		}
	}

	public event AyUpdatePortDelegate UpdateIRA;

	public event AyUpdatePortDelegate UpdateIRB;

	public AY8910(int sampleRate, int frameTactCount)
	{
		_frameTactCount = frameTactCount;
		_sampleRate = sampleRate;
		samples = new uint[_sampleRate / 50];
		Volume = 9000;
	}

	protected void OnUpdateIRA(byte outState)
	{
		_ira.OutState = outState;
		if (this.UpdateIRA != null)
		{
			this.UpdateIRA(this, _ira);
		}
	}

	protected void OnUpdateIRB(byte outState)
	{
		_irb.OutState = outState;
		if (this.UpdateIRB != null)
		{
			this.UpdateIRB(this, _irb);
		}
	}

	public void Reset()
	{
		for (int i = 0; i < 16; i++)
		{
			ADDR_REG = (byte)i;
			DATA_REG = 0;
		}
		process(samplePos * _frameTactCount / samples.Length);
	}

	public void process(int tact)
	{
		if (tact > _frameTactCount)
		{
			tact = _frameTactCount;
		}
		GenSignalAY(samples.Length * tact / _frameTactCount);
	}

	public uint[] flush()
	{
		process(_frameTactCount);
		samplePos = 0;
		return samples;
	}

	private void GenSignalAY(int toEndPtr)
	{
		byte b = 0;
		byte b2 = 0;
		byte b3 = 0;
		lock (this)
		{
			if (toEndPtr > 882)
			{
				toEndPtr = 882;
			}
			while (samplePos < toEndPtr)
			{
				OutputNoiseABC &= (byte)(((ControlChannels & 0x38) >> 3) ^ 7);
				MixLineABC = (byte)((OutputABC & ((ControlChannels & 7u) ^ 7u)) ^ OutputNoiseABC);
				if ((MixLineABC & 1) == 0)
				{
					b = (byte)((VolumeA & 0x1F) << 1);
					b = (((b & 0x20) == 0) ? ((byte)(b + 1)) : ((byte)BendVolumeIndex));
				}
				else
				{
					b = 0;
				}
				if ((MixLineABC & 2) == 0)
				{
					b2 = (byte)((VolumeB & 0x1F) << 1);
					b2 = (((b2 & 0x20) == 0) ? ((byte)(b2 + 1)) : ((byte)BendVolumeIndex));
				}
				else
				{
					b2 = 0;
				}
				if ((MixLineABC & 4) == 0)
				{
					b3 = (byte)((VolumeC & 0x1F) << 1);
					b3 = (((b3 & 0x20) == 0) ? ((byte)(b3 + 1)) : ((byte)BendVolumeIndex));
				}
				else
				{
					b3 = 0;
				}
				samples[samplePos] = YM_VolTableC[b3] + YM_VolTableB[b2] + YM_VolTableA[b];
				for (int i = 0; i < 5; i++)
				{
					if (++CounterNoise >= FreqNoise)
					{
						CounterNoise = 0;
						NoiseVal = (((NoiseVal >> 16) ^ (NoiseVal >> 13)) & 1u) ^ ((NoiseVal << 1) + 1);
						OutputNoiseABC = (byte)((NoiseVal >> 16) & 1u);
						OutputNoiseABC |= (byte)((OutputNoiseABC << 1) | (OutputNoiseABC << 2));
					}
					if (++CounterA >= FreqA)
					{
						CounterA = 0;
						OutputABC ^= 1;
					}
					if (++CounterB >= FreqB)
					{
						CounterB = 0;
						OutputABC ^= 2;
					}
					if (++CounterC >= FreqC)
					{
						CounterC = 0;
						OutputABC ^= 4;
					}
					if (++_CounterBend >= FreqBend)
					{
						_CounterBend = 0u;
						ChangeEnvelope();
					}
				}
				samplePos++;
			}
		}
	}

	private void ChangeEnvelope()
	{
		if (BendStatus == 0)
		{
			return;
		}
		if (BendStatus == 1)
		{
			if (++BendVolumeIndex >= 31)
			{
				switch (ControlBend & 0xF)
				{
				case 0:
					env_DD();
					break;
				case 1:
					env_DD();
					break;
				case 2:
					env_DD();
					break;
				case 3:
					env_DD();
					break;
				case 4:
					env_DD();
					break;
				case 5:
					env_DD();
					break;
				case 6:
					env_DD();
					break;
				case 7:
					env_DD();
					break;
				case 8:
					env_UD();
					break;
				case 9:
					env_DD();
					break;
				case 10:
					env_UD();
					break;
				case 11:
					env_UU();
					break;
				case 12:
					env_DU();
					break;
				case 13:
					env_UU();
					break;
				case 14:
					env_UD();
					break;
				case 15:
					env_DD();
					break;
				}
			}
		}
		else if (--BendVolumeIndex < 0)
		{
			switch (ControlBend & 0xF)
			{
			case 0:
				env_DD();
				break;
			case 1:
				env_UU();
				break;
			case 2:
				env_DD();
				break;
			case 3:
				env_DD();
				break;
			case 4:
				env_DD();
				break;
			case 5:
				env_DD();
				break;
			case 6:
				env_DD();
				break;
			case 7:
				env_DD();
				break;
			case 8:
				env_UD();
				break;
			case 9:
				env_DD();
				break;
			case 10:
				env_DU();
				break;
			case 11:
				env_UU();
				break;
			case 12:
				env_DU();
				break;
			case 13:
				env_UU();
				break;
			case 14:
				env_DU();
				break;
			case 15:
				env_DD();
				break;
			}
		}
	}

	private void env_DU()
	{
		BendStatus = 1;
		BendVolumeIndex = 0;
	}

	private void env_UU()
	{
		BendStatus = 0;
		BendVolumeIndex = 31;
	}

	private void env_UD()
	{
		BendStatus = 2;
		BendVolumeIndex = 31;
	}

	private void env_DD()
	{
		BendStatus = 0;
		BendVolumeIndex = 0;
	}
}
