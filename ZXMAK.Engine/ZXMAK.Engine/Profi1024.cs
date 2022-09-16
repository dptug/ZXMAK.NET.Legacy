using System;
using ZXMAK.Engine.AY;
using ZXMAK.Engine.Disk;
using ZXMAK.Engine.Loaders;
using ZXMAK.Engine.Z80;
using ZXMAK.Logging;

namespace ZXMAK.Engine;

public class Profi1024 : Spectrum, IBetaDiskDevice, ISpectrum128K
{
	private const int _ulaLineTime = 224;

	private const int _ulaFirstPaperLine = 48;

	private const int _ulaFirstPaperTact = 68;

	private Z80CPU _cpu;

	private LoadManager _loadManager;

	private byte[][] _RAMS = new byte[64][];

	private byte[][] _ROMS = new byte[4][];

	private byte[] _ulaMemory;

	private unsafe uint* _bitmapBufPtr = null;

	private unsafe uint* _soundBufPtr = null;

	private long _keyboardState;

	private WD1793 _betaDisk;

	private Speaker _speaker;

	private AY8910 _sound;

	private bool _selTRDOS;

	private byte _portFE;

	private byte _port7FFD;

	private int _lastFrameTact;

	private int _flashState;

	private int _flashCounter;

	private int[] _ulaLineOffset;

	private int[] _ulaAddrBW;

	private int[] _ulaAddrAT;

	private byte[] _ulaDo;

	private uint[] _ulaInk;

	private uint[] _ulaPaper;

	private byte _ulaFetchBW;

	private byte _ulaFetchAT;

	private uint _ulaFetchInk;

	private uint _ulaFetchPaper;

	private static uint[] _zxpal = new uint[16]
	{
		4278190080u, 4278190272u, 4290772992u, 4290773184u, 4278239232u, 4278239424u, 4290822144u, 4290822336u, 4278190080u, 4278190335u,
		4294901760u, 4294902015u, 4278255360u, 4278255615u, 4294967040u, 4294967295u
	};

	public byte Port7FFD
	{
		get
		{
			return _port7FFD;
		}
		set
		{
			_port7FFD = value;
		}
	}

	public WD1793 BetaDisk => _betaDisk;

	public bool SEL_TRDOS
	{
		get
		{
			return _selTRDOS;
		}
		set
		{
			_selTRDOS = value;
		}
	}

	public override Z80CPU CPU => _cpu;

	public override LoadManager Loader => _loadManager;

	public override long KeyboardState
	{
		get
		{
			return _keyboardState;
		}
		set
		{
			_keyboardState = value;
		}
	}

	public override int MouseX
	{
		get
		{
			return 0;
		}
		set
		{
		}
	}

	public override int MouseY
	{
		get
		{
			return 0;
		}
		set
		{
		}
	}

	public override int MouseButtons
	{
		get
		{
			return 0;
		}
		set
		{
		}
	}

	public override void OnInit()
	{
		_cpu = new Z80CPU();
		_cpu.ReadMemory = RDMEM;
		_cpu.WriteMemory = WRMEM;
		_cpu.ReadPort = RDPORT;
		_cpu.WritePort = WRPORT;
		_cpu.OnCycle = processVideoChanges;
		for (int i = 0; i < _RAMS.Length; i++)
		{
			_RAMS[i] = new byte[16384];
		}
		_ulaMemory = _RAMS[5];
		for (int j = 0; j < _ROMS.Length; j++)
		{
			_ROMS[j] = new byte[16384];
		}
		_betaDisk = new WD1793();
		_speaker = new Speaker(44100, GetFrameTactCount());
		_sound = new AY8910(44100, GetFrameTactCount());
		_loadManager = new LoadManager(this);
		_loadManager.LoadROMS();
		_betaDisk._spec = this;
		SetResolution(320, 240);
	}

	public override int GetFrameTact()
	{
		return (int)(_cpu.Tact % (ulong)GetFrameTactCount());
	}

	public override int GetFrameTactCount()
	{
		return 69888;
	}

	protected override void Reset()
	{
		_port7FFD = 0;
		_ulaMemory = _RAMS[5];
		base.Reset();
	}

	public override bool SetRomImage(RomName romName, byte[] data, int startIndex, int length)
	{
		if (length > 16384)
		{
			length = 16384;
		}
		switch (romName)
		{
		case RomName.ROM_128:
			setImage(_ROMS[0], data, startIndex, length);
			break;
		case RomName.ROM_48:
			setImage(_ROMS[1], data, startIndex, length);
			break;
		case RomName.ROM_TRDOS:
			setImage(_ROMS[2], data, startIndex, length);
			break;
		case RomName.ROM_PROFI:
			setImage(_ROMS[3], data, startIndex, length);
			break;
		default:
			return false;
		}
		OnUpdateState();
		return true;
	}

	public override bool SetRamImage(int page, byte[] data, int startIndex, int length)
	{
		if (page >= _RAMS.Length)
		{
			return false;
		}
		if (length > 16384)
		{
			length = 16384;
		}
		setImage(_RAMS[page], data, startIndex, length);
		OnUpdateState();
		return true;
	}

	public override byte[] GetRamImage(int page)
	{
		if (page >= _RAMS.Length)
		{
			return null;
		}
		byte[] array = new byte[16384];
		setImage(array, _RAMS[page], 0, 16384);
		return array;
	}

	public override int GetRamImagePageCount()
	{
		return _RAMS.Length;
	}

	public override byte ReadMemory(ushort addr)
	{
		return 0;
	}

	public override void WriteMemory(ushort addr, byte value)
	{
	}

	public override void AddBreakpoint(ushort addr)
	{
	}

	public override void RemoveBreakpoint(ushort addr)
	{
	}

	public override ushort[] GetBreakpointList()
	{
		return new ushort[0];
	}

	public override bool CheckBreakpoint(ushort addr)
	{
		return false;
	}

	public override void ClearBreakpoints()
	{
	}

	public unsafe override void ExecuteFrame(IntPtr videoPtr, IntPtr soundPtr)
	{
		_bitmapBufPtr = (uint*)(void*)videoPtr;
		_soundBufPtr = (uint*)(void*)soundPtr;
		try
		{
			if (IsRunning)
			{
				fetchVideo(_bitmapBufPtr, 0, GetFrameTact(), ref _ulaFetchBW, ref _ulaFetchAT, ref _ulaFetchInk, ref _ulaFetchPaper);
				ulong num = _cpu.Tact - (ulong)GetFrameTact();
				while (_cpu.Tact - num <= (ulong)GetFrameTactCount())
				{
					_cpu.ExecCycle();
				}
				flushFrame();
				return;
			}
			DrawScreen(videoPtr);
			if (soundPtr != IntPtr.Zero)
			{
				for (int i = 0; i < 3528; i++)
				{
					((byte*)(void*)soundPtr)[i] = 0;
				}
			}
		}
		finally
		{
			_bitmapBufPtr = null;
			_soundBufPtr = null;
		}
	}

	public unsafe override void DrawScreen(IntPtr videoPtr)
	{
		if (!(videoPtr == IntPtr.Zero))
		{
			byte ulaFetchBW = 0;
			byte ulaFetchAT = 0;
			uint ulaFetchInk = 0u;
			uint ulaFetchPaper = 0u;
			fetchVideo((uint*)(void*)videoPtr, 0, GetFrameTactCount(), ref ulaFetchBW, ref ulaFetchAT, ref ulaFetchInk, ref ulaFetchPaper);
		}
	}

	private void setImage(byte[] dst, byte[] src, int startIndex, int length)
	{
		for (int i = 0; i < length; i++)
		{
			dst[i] = src[startIndex + i];
		}
	}

	private byte RDMEM(ushort ADDR, bool M1)
	{
		switch (ADDR & 0xC000)
		{
		case 0:
			if (_selTRDOS)
			{
				return _ROMS[2][ADDR];
			}
			if (M1 && (ADDR & 0xFF00) == 15616 && (_port7FFD & 0x10u) != 0)
			{
				_selTRDOS = true;
				return _ROMS[2][ADDR];
			}
			return _ROMS[(_port7FFD & 0x10) >> 4][ADDR];
		case 16384:
			if (M1)
			{
				_selTRDOS = false;
			}
			return _RAMS[5][ADDR & 0x3FFF];
		case 32768:
			if (M1)
			{
				_selTRDOS = false;
			}
			return _RAMS[2][ADDR & 0x3FFF];
		default:
			if (M1)
			{
				_selTRDOS = false;
			}
			return _RAMS[_port7FFD & 7][ADDR & 0x3FFF];
		}
	}

	private void WRMEM(ushort ADDR, byte value)
	{
		switch (ADDR & 0xC000)
		{
		case 16384:
			if (_ulaMemory == _RAMS[5])
			{
				processVideoChanges();
			}
			_RAMS[5][ADDR & 0x3FFF] = value;
			break;
		case 32768:
			_RAMS[2][ADDR & 0x3FFF] = value;
			break;
		case 49152:
			if (_ulaMemory == _RAMS[_port7FFD & 7])
			{
				processVideoChanges();
			}
			_RAMS[_port7FFD & 7][ADDR & 0x3FFF] = value;
			break;
		}
	}

	private byte RDPORT(ushort ADDR)
	{
		if (_selTRDOS)
		{
			switch (ADDR & 0xE3)
			{
			case 3:
				return _betaDisk.GetReg(RegWD1793.COMMAND, _cpu.Tact);
			case 35:
				return _betaDisk.GetReg(RegWD1793.TRACK, _cpu.Tact);
			case 67:
				return _betaDisk.GetReg(RegWD1793.SECTOR, _cpu.Tact);
			case 99:
				return _betaDisk.GetReg(RegWD1793.DATA, _cpu.Tact);
			case 227:
				return _betaDisk.GetReg(RegWD1793.BETA128, _cpu.Tact);
			}
		}
		if ((ADDR & 0xFF) == 254)
		{
			_portFE &= 191;
			return (byte)((0x1Fu & (uint)scanKbdPort(ADDR)) | (_portFE & 0x40u) | (CPU.FreeBUS & 0xA0u));
		}
		if ((ADDR & 0xFF) == 255)
		{
			return byte.MaxValue;
		}
		if ((ADDR & 0xC0FF) == 49405)
		{
			return _sound.DATA_REG;
		}
		return byte.MaxValue;
	}

	private void WRPORT(ushort ADDR, byte value)
	{
		if (_selTRDOS)
		{
			switch (ADDR & 0xE3)
			{
			case 3:
				_betaDisk.SetReg(RegWD1793.COMMAND, value, _cpu.Tact);
				break;
			case 35:
				_betaDisk.SetReg(RegWD1793.TRACK, value, _cpu.Tact);
				break;
			case 67:
				_betaDisk.SetReg(RegWD1793.SECTOR, value, _cpu.Tact);
				break;
			case 99:
				_betaDisk.SetReg(RegWD1793.DATA, value, _cpu.Tact);
				break;
			case 227:
				_betaDisk.SetReg(RegWD1793.BETA128, value, _cpu.Tact);
				break;
			}
		}
		if ((ADDR & 1) == 0)
		{
			int num = value & 0xBF;
			_speaker.SetPort(GetFrameTact(), num);
			processVideoChanges();
			_portFE = (byte)num;
		}
		if ((ADDR & 0x8002) == 0 && (_port7FFD & 0x20) == 0)
		{
			_port7FFD = value;
			if ((_port7FFD & 8) == 0)
			{
				_ulaMemory = _RAMS[5];
			}
			else
			{
				_ulaMemory = _RAMS[7];
			}
		}
		if ((ADDR & 0xC0FF) == 49405)
		{
			_sound.ADDR_REG = value;
		}
		if ((ADDR & 0xC0FF) == 33021)
		{
			_sound.process(GetFrameTact());
			_sound.DATA_REG = value;
		}
	}

	private int scanKbdPort(ushort port)
	{
		byte b = 31;
		int num = 256;
		int num2 = 0;
		while (num2 < 8)
		{
			if ((port & num) == 0)
			{
				b = (byte)(b & (byte)(((_keyboardState >> num2 * 5) ^ 0x1F) & 0x1F));
			}
			num2++;
			num <<= 1;
		}
		return b;
	}

	protected override void VideoParamsChanged(VideoManager sender, VideoParams value)
	{
		if (value == null || value.LinePitch < 320 || value.Width < 320 || value.Height < 240 || value.BPP != 32)
		{
			Logger.GetLogger().LogError("Profi.VideoParamsChanged: Unsupported params!");
			throw new ArgumentOutOfRangeException("Unsupported VideoFrameParams");
		}
		fillUlaTables(GetFrameTactCount(), value);
	}

	private unsafe void flushFrame()
	{
		if (_soundBufPtr != null)
		{
			mixSound(_soundBufPtr, _speaker.FlushFrame(), _sound.flush());
		}
		processVideoChanges();
		_lastFrameTact = 0;
		_flashCounter++;
		if (_flashCounter > 24)
		{
			_flashState ^= 256;
			_flashCounter = 0;
		}
	}

	private unsafe void processVideoChanges()
	{
		int num = GetFrameTact();
		if (num < 32)
		{
			_cpu.INT = true;
		}
		else
		{
			_cpu.INT = false;
		}
		if (num < _lastFrameTact)
		{
			num = GetFrameTactCount();
		}
		fetchVideo(_bitmapBufPtr, _lastFrameTact, num, ref _ulaFetchBW, ref _ulaFetchAT, ref _ulaFetchInk, ref _ulaFetchPaper);
		_lastFrameTact = num;
	}

	private unsafe void mixSound(uint* dst, uint[] src1, uint[] src2)
	{
		for (int i = 0; i < 882; i++)
		{
			uint num = src1[i];
			uint num2 = src2[i];
			_soundBufPtr[i] = (((num >> 16) + (num2 >> 16)) / 2u << 16) | (((num & 0xFFFF) + (num2 & 0xFFFF)) / 2u);
		}
	}

	private void fillUlaTables(int MaxTakt, VideoParams videoParams)
	{
		_ulaLineOffset = new int[MaxTakt];
		_ulaAddrBW = new int[MaxTakt];
		_ulaAddrAT = new int[MaxTakt];
		_ulaDo = new byte[MaxTakt];
		int num = 0;
		for (num = 0; num < MaxTakt; num++)
		{
			int num2 = num / 224;
			int num3 = num % 224;
			if (num2 >= 24 && num2 < 264 && num3 >= 52 && num3 < 212)
			{
				if (num2 >= 48 && num2 < 240 && num3 >= 68 && num3 < 196)
				{
					_ulaDo[num] = 3;
					if ((num3 & 3) == 3)
					{
						_ulaDo[num] = 4;
						int num4 = num3 + 1 - 68;
						int num5 = num2 - 48;
						num4 >>= 2;
						int num6 = num4 | (num5 >> 3 << 5);
						int num7 = num4 | (num5 << 5);
						_ulaAddrBW[num] = (num7 & 0x181F) | ((num7 & 0x700) >> 3) | ((num7 & 0xE0) << 3);
						_ulaAddrAT[num] = 6144 + num6;
					}
				}
				else if (num2 >= 48 && num2 < 240 && num3 == 67)
				{
					_ulaDo[num] = 2;
					int num8 = num3 + 1 - 68;
					int num9 = num2 - 48;
					num8 >>= 2;
					int num10 = num8 | (num9 >> 3 << 5);
					int num11 = num8 | (num9 << 5);
					_ulaAddrBW[num] = (num11 & 0x181F) | ((num11 & 0x700) >> 3) | ((num11 & 0xE0) << 3);
					_ulaAddrAT[num] = 6144 + num10;
				}
				else
				{
					_ulaDo[num] = 1;
				}
				int num12 = num2 - 24;
				int num13 = (num3 - 52) * 2;
				_ulaLineOffset[num] = num12 * videoParams.LinePitch + num13;
			}
			else
			{
				_ulaDo[num] = 0;
			}
		}
		_ulaInk = new uint[512];
		_ulaPaper = new uint[512];
		for (int i = 0; i < 256; i++)
		{
			_ulaInk[i] = _zxpal[(i & 7) + ((i & 0x40) >> 3)];
			_ulaPaper[i] = _zxpal[((i >> 3) & 7) + ((i & 0x40) >> 3)];
			if (((uint)i & 0x80u) != 0)
			{
				_ulaInk[i + 256] = _zxpal[((i >> 3) & 7) + ((i & 0x40) >> 3)];
				_ulaPaper[i + 256] = _zxpal[(i & 7) + ((i & 0x40) >> 3)];
			}
			else
			{
				_ulaInk[i + 256] = _zxpal[(i & 7) + ((i & 0x40) >> 3)];
				_ulaPaper[i + 256] = _zxpal[((i >> 3) & 7) + ((i & 0x40) >> 3)];
			}
		}
	}

	private unsafe void fetchVideo(uint* bitmapBufPtr, int startTact, int endTact, ref byte ulaFetchBW, ref byte ulaFetchAT, ref uint ulaFetchInk, ref uint ulaFetchPaper)
	{
		if (_bitmapBufPtr == null || _ulaDo == null)
		{
			return;
		}
		if (endTact > GetFrameTactCount())
		{
			endTact = GetFrameTactCount();
		}
		if (startTact > GetFrameTactCount())
		{
			startTact = GetFrameTactCount();
		}
		if (startTact < 5376)
		{
			startTact = 5376;
		}
		for (int i = startTact; i < endTact; i++)
		{
			switch (_ulaDo[i])
			{
			case 1:
				bitmapBufPtr[_ulaLineOffset[i]] = _zxpal[_portFE & 7];
				bitmapBufPtr[_ulaLineOffset[i] + 1] = _zxpal[_portFE & 7];
				break;
			case 2:
				bitmapBufPtr[_ulaLineOffset[i]] = _zxpal[_portFE & 7];
				bitmapBufPtr[_ulaLineOffset[i] + 1] = _zxpal[_portFE & 7];
				ulaFetchBW = _ulaMemory[_ulaAddrBW[i]];
				ulaFetchAT = _ulaMemory[_ulaAddrAT[i]];
				ulaFetchInk = _ulaInk[ulaFetchAT + _flashState];
				ulaFetchPaper = _ulaPaper[ulaFetchAT + _flashState];
				break;
			case 3:
				bitmapBufPtr[_ulaLineOffset[i]] = (((ulaFetchBW & 0x80u) != 0) ? ulaFetchInk : ulaFetchPaper);
				bitmapBufPtr[_ulaLineOffset[i] + 1] = (((ulaFetchBW & 0x40u) != 0) ? ulaFetchInk : ulaFetchPaper);
				ulaFetchBW <<= 2;
				break;
			case 4:
				bitmapBufPtr[_ulaLineOffset[i]] = (((ulaFetchBW & 0x80u) != 0) ? ulaFetchInk : ulaFetchPaper);
				bitmapBufPtr[_ulaLineOffset[i] + 1] = (((ulaFetchBW & 0x40u) != 0) ? ulaFetchInk : ulaFetchPaper);
				ulaFetchBW <<= 2;
				ulaFetchBW = _ulaMemory[_ulaAddrBW[i]];
				ulaFetchAT = _ulaMemory[_ulaAddrAT[i]];
				ulaFetchInk = _ulaInk[ulaFetchAT + _flashState];
				ulaFetchPaper = _ulaPaper[ulaFetchAT + _flashState];
				break;
			}
		}
	}
}
