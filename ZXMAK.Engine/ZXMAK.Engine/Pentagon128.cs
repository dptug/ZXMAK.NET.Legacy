using System;
using System.Collections.Generic;
using ZXMAK.Engine.AY;
using ZXMAK.Engine.Disk;
using ZXMAK.Engine.Loaders;
using ZXMAK.Engine.Tape;
using ZXMAK.Engine.Z80;
using ZXMAK.Logging;

namespace ZXMAK.Engine;

public class Pentagon128 : Spectrum, ISpectrum, ISpectrum128K, IAyDevice, IBetaDiskDevice, ITapeDevice
{
	private const int _ulaLineTime = 224;

	private const int _ulaFirstPaperLine = 80;

	private const int _ulaFirstPaperTact = 68;

	private Z80CPU _cpu;

	private TapeDevice _tape;

	private WD1793 _betaDisk;

	private AY8910 _sound;

	private LoadManager _loader;

	private byte[][] _RAMS = new byte[8][];

	private byte[][] _ROMS = new byte[4][];

	private byte _portFE;

	private byte _port7FFD;

	private bool _selTRDOS;

	private byte[] _ulaMemory;

	private int _beeperSamplePos;

	private uint[] _beeperSamples = new uint[882];

	private unsafe uint* _bitmapBufPtr = null;

	private unsafe uint* _soundBufPtr = null;

	private long _keyboardState;

	private int _lastFrameTact;

	private int _flashState;

	private int _flashCounter;

	private List<ushort> _breakpoints;

	private int _mouseX;

	private int _mouseY;

	private int _mouseButtons;

	private int _lastAyMouseX;

	private int _lastAyMouseY;

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

	public byte PortFE
	{
		get
		{
			return _portFE;
		}
		set
		{
			_portFE = value;
		}
	}

	public byte[] UlaBuffer => _ulaMemory;

	public int UlaBufferSize => 6912;

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

	public TapeDevice Tape => _tape;

	public override Z80CPU CPU => _cpu;

	public AY8910 Sound => _sound;

	public override LoadManager Loader => _loader;

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
			return _mouseX;
		}
		set
		{
			_mouseX = value;
		}
	}

	public override int MouseY
	{
		get
		{
			return _mouseY;
		}
		set
		{
			_mouseY = value;
		}
	}

	public override int MouseButtons
	{
		get
		{
			return _mouseButtons;
		}
		set
		{
			_mouseButtons = value;
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
		_sound = new AY8910(44100, GetFrameTactCount());
		_sound.UpdateIRA += ay_UpdateIRA;
		_tape = new TapeDevice();
		_loader = new LoadManager(this);
		_loader.LoadROMS();
		_betaDisk._spec = this;
		SetResolution(320, 240);
	}

	public override bool SetRomImage(RomName name, byte[] data, int startIndex, int length)
	{
		if (length > 16384)
		{
			length = 16384;
		}
		switch (name)
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
		switch (addr & 0xC000)
		{
		case 0:
			if (_selTRDOS)
			{
				return _ROMS[2][addr];
			}
			return _ROMS[(_port7FFD & 0x10) >> 4][addr];
		case 16384:
			return _RAMS[5][addr & 0x3FFF];
		case 32768:
			return _RAMS[2][addr & 0x3FFF];
		default:
			return _RAMS[_port7FFD & 7][addr & 0x3FFF];
		}
	}

	public override void WriteMemory(ushort addr, byte value)
	{
		switch (addr & 0xC000)
		{
		case 16384:
			_RAMS[5][addr & 0x3FFF] = value;
			break;
		case 32768:
			_RAMS[2][addr & 0x3FFF] = value;
			break;
		default:
			_RAMS[_port7FFD & 7][addr & 0x3FFF] = value;
			break;
		case 0:
			break;
		}
	}

	public override void AddBreakpoint(ushort addr)
	{
		if (_breakpoints == null)
		{
			_breakpoints = new List<ushort>();
		}
		if (!_breakpoints.Contains(addr))
		{
			_breakpoints.Add(addr);
		}
	}

	public override void RemoveBreakpoint(ushort addr)
	{
		if (_breakpoints != null)
		{
			if (_breakpoints.Contains(addr))
			{
				_breakpoints.Remove(addr);
			}
			if (_breakpoints.Count < 1)
			{
				_breakpoints = null;
			}
		}
	}

	public override ushort[] GetBreakpointList()
	{
		if (_breakpoints == null)
		{
			return new ushort[0];
		}
		return _breakpoints.ToArray();
	}

	public override bool CheckBreakpoint(ushort addr)
	{
		if (_breakpoints != null)
		{
			return _breakpoints.Contains(addr);
		}
		return false;
	}

	public override void ClearBreakpoints()
	{
		if (_breakpoints != null)
		{
			_breakpoints.Clear();
		}
		_breakpoints = null;
	}

	protected override void VideoParamsChanged(VideoManager sender, VideoParams value)
	{
		if (value == null || value.LinePitch < 320 || value.Width < 320 || value.Height < 240 || value.BPP != 32)
		{
			Logger.GetLogger().LogError("Pentagon128.VideoParamsChanged: Unsupported params!");
			throw new ArgumentOutOfRangeException("Unsupported VideoFrameParams");
		}
		fillUlaTables(GetFrameTactCount(), value);
	}

	protected byte ReadAyMouseX()
	{
		int num = _mouseX / 4 - _lastAyMouseX;
		_lastAyMouseX = _mouseX / 4;
		int num2 = 0;
		if ((_mouseButtons & 1) == 0)
		{
			num2 |= 0x10;
		}
		if ((_mouseButtons & 2) == 0)
		{
			num2 |= 0x20;
		}
		num += 8;
		if (num < 0)
		{
			num = 0;
		}
		if (num > 15)
		{
			num = 15;
		}
		return (byte)((uint)(num | num2) | 0xC0u);
	}

	protected byte ReadAyMouseY()
	{
		int num = _lastAyMouseY - _mouseY / 4;
		_lastAyMouseY = _mouseY / 4;
		int num2 = 0;
		if ((_mouseButtons & 1) == 0)
		{
			num2 |= 0x10;
		}
		if ((_mouseButtons & 2) == 0)
		{
			num2 |= 0x20;
		}
		num += 8;
		if (num < 0)
		{
			num = 0;
		}
		if (num > 15)
		{
			num = 15;
		}
		return (byte)((uint)(num | num2) | 0xC0u);
	}

	protected byte ReadKempstonMouseX()
	{
		return (byte)(_mouseX / 3);
	}

	protected byte ReadKempstonMouseY()
	{
		return (byte)(-_mouseY / 3);
	}

	private void ay_UpdateIRA(AY8910 sender, AyPortState state)
	{
		if (((uint)(state.OutState ^ state.OldOutState) & 0x40u) != 0)
		{
			state.InState = (((state.OutState & 0x40u) != 0) ? ((byte)(ReadAyMouseY() | 0x40u)) : ReadAyMouseX());
		}
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
					if (_tape.IsPlay)
					{
						_portFE &= 191;
						_portFE |= (byte)(_tape.GetTapeBit(_cpu.Tact) & 0x40);
						processAudioChanges(GetFrameTact());
					}
					if (_breakpoints != null && CheckBreakpoint(_cpu.regs.PC))
					{
						IsRunning = false;
						OnBreakpoint();
						break;
					}
				}
				flushAudio();
				flushVideo();
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

	protected override void Reset()
	{
		_port7FFD = 0;
		_ulaMemory = _RAMS[5];
		base.Reset();
		Sound.Reset();
	}

	public override int GetFrameTact()
	{
		return (int)(_cpu.Tact % (ulong)GetFrameTactCount());
	}

	public override int GetFrameTactCount()
	{
		return 71680;
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
			_portFE |= (byte)(_tape.GetTapeBit(_cpu.Tact) & 0x40);
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
		return ADDR switch
		{
			64223 => (byte)((uint)_mouseButtons ^ 0xFFu), 
			64479 => ReadKempstonMouseX(), 
			65503 => ReadKempstonMouseY(), 
			_ => byte.MaxValue, 
		};
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
			processAudioChanges(GetFrameTact());
			processVideoChanges();
			_portFE = value;
			_portFE &= 191;
			_portFE |= (byte)(_tape.GetTapeBit(_cpu.Tact) & 0x40);
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

	private unsafe void flushAudio()
	{
		processAudioChanges(GetFrameTactCount());
		_beeperSamplePos = 0;
		_sound.flush();
		if (_soundBufPtr != null)
		{
			for (int i = 0; i < 882; i++)
			{
				uint num = _beeperSamples[i];
				uint num2 = _sound.samples[i];
				_soundBufPtr[i] = (((num >> 16) + (num2 >> 16)) / 2u << 16) | (((num & 0xFFFF) + (num2 & 0xFFFF)) / 2u);
			}
		}
	}

	private void flushVideo()
	{
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

	private void processAudioChanges(int ToTact)
	{
		int num = 882 * ToTact / GetFrameTactCount();
		if (num > 882)
		{
			num = 882;
		}
		if (num > _beeperSamplePos)
		{
			uint num2 = 0u;
			if ((_portFE & 0x10u) != 0)
			{
				num2 += 8191;
			}
			if (Config.TapeOutSoundEnable && (_portFE & 8u) != 0)
			{
				num2 += 8191;
			}
			if (Config.TapeInSoundEnable && (_portFE & 0x40u) != 0)
			{
				num2 += 8191;
			}
			num2 /= 3u;
			num2 |= num2 << 16;
			while (_beeperSamplePos < num)
			{
				_beeperSamples[_beeperSamplePos] = num2;
				_beeperSamplePos++;
			}
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

	private void fillUlaTables(int MaxTakt, VideoParams videoParams)
	{
		_ulaLineOffset = new int[MaxTakt];
		_ulaAddrBW = new int[MaxTakt];
		_ulaAddrAT = new int[MaxTakt];
		_ulaDo = new byte[MaxTakt];
		int num = 0;
		for (int i = 0; i < 320; i++)
		{
			int num2 = 0;
			while (num2 < 224)
			{
				if (i >= 56 && i < 296 && num2 >= 52 && num2 < 212)
				{
					if (i >= 80 && i < 272 && num2 >= 68 && num2 < 196)
					{
						_ulaDo[num] = 3;
						if ((num2 & 3) == 3)
						{
							_ulaDo[num] = 4;
							int num3 = num2 + 1 - 68;
							int num4 = i - 80;
							num3 >>= 2;
							int num5 = num3 | (num4 >> 3 << 5);
							int num6 = num3 | (num4 << 5);
							_ulaAddrBW[num] = (num6 & 0x181F) | ((num6 & 0x700) >> 3) | ((num6 & 0xE0) << 3);
							_ulaAddrAT[num] = 6144 + num5;
						}
					}
					else if (i >= 80 && i < 272 && num2 == 67)
					{
						_ulaDo[num] = 2;
						int num7 = num2 + 1 - 68;
						int num8 = i - 80;
						num7 >>= 2;
						int num9 = num7 | (num8 >> 3 << 5);
						int num10 = num7 | (num8 << 5);
						_ulaAddrBW[num] = (num10 & 0x181F) | ((num10 & 0x700) >> 3) | ((num10 & 0xE0) << 3);
						_ulaAddrAT[num] = 6144 + num9;
					}
					else
					{
						_ulaDo[num] = 1;
					}
					int num11 = i - 56;
					int num12 = (num2 - 52) * 2;
					_ulaLineOffset[num] = num11 * videoParams.LinePitch + num12;
				}
				else
				{
					_ulaDo[num] = 0;
				}
				num2++;
				num++;
			}
		}
		_ulaInk = new uint[512];
		_ulaPaper = new uint[512];
		for (int j = 0; j < 256; j++)
		{
			_ulaInk[j] = _zxpal[(j & 7) + ((j & 0x40) >> 3)];
			_ulaPaper[j] = _zxpal[((j >> 3) & 7) + ((j & 0x40) >> 3)];
			if (((uint)j & 0x80u) != 0)
			{
				_ulaInk[j + 256] = _zxpal[((j >> 3) & 7) + ((j & 0x40) >> 3)];
				_ulaPaper[j + 256] = _zxpal[(j & 7) + ((j & 0x40) >> 3)];
			}
			else
			{
				_ulaInk[j + 256] = _zxpal[(j & 7) + ((j & 0x40) >> 3)];
				_ulaPaper[j + 256] = _zxpal[((j >> 3) & 7) + ((j & 0x40) >> 3)];
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
		if (startTact < 12544)
		{
			startTact = 12544;
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
