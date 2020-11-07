using System;
using System.Collections.Generic;
using ZXMAK.Engine.AY;
using ZXMAK.Engine.Disk;
using ZXMAK.Engine.Loaders;
using ZXMAK.Engine.Tape;
using ZXMAK.Engine.Z80;
using ZXMAK.Logging;

namespace ZXMAK.Engine
{
	public class Pentagon128 : Spectrum, ISpectrum, ISpectrum128K, IAyDevice, IBetaDiskDevice, ITapeDevice
	{
		public byte PortFE
		{
			get
			{
				return this._portFE;
			}
			set
			{
				this._portFE = value;
			}
		}

		public byte[] UlaBuffer
		{
			get
			{
				return this._ulaMemory;
			}
		}

		public int UlaBufferSize
		{
			get
			{
				return 6912;
			}
		}

		public byte Port7FFD
		{
			get
			{
				return this._port7FFD;
			}
			set
			{
				this._port7FFD = value;
			}
		}

		public WD1793 BetaDisk
		{
			get
			{
				return this._betaDisk;
			}
		}

		public bool SEL_TRDOS
		{
			get
			{
				return this._selTRDOS;
			}
			set
			{
				this._selTRDOS = value;
			}
		}

		public TapeDevice Tape
		{
			get
			{
				return this._tape;
			}
		}

		public override Z80CPU CPU
		{
			get
			{
				return this._cpu;
			}
		}

		public AY8910 Sound
		{
			get
			{
				return this._sound;
			}
		}

		public override LoadManager Loader
		{
			get
			{
				return this._loader;
			}
		}

		public override void OnInit()
		{
			this._cpu = new Z80CPU();
			this._cpu.ReadMemory = new OnRDMEM(this.RDMEM);
			this._cpu.WriteMemory = new OnWRMEM(this.WRMEM);
			this._cpu.ReadPort = new OnRDPORT(this.RDPORT);
			this._cpu.WritePort = new OnWRPORT(this.WRPORT);
			this._cpu.OnCycle = new OnCALLBACK(this.processVideoChanges);
			for (int i = 0; i < this._RAMS.Length; i++)
			{
				this._RAMS[i] = new byte[16384];
			}
			this._ulaMemory = this._RAMS[5];
			for (int j = 0; j < this._ROMS.Length; j++)
			{
				this._ROMS[j] = new byte[16384];
			}
			this._betaDisk = new WD1793();
			this._sound = new AY8910(44100, this.GetFrameTactCount());
			this._sound.UpdateIRA += this.ay_UpdateIRA;
			this._tape = new TapeDevice();
			this._loader = new LoadManager(this);
			this._loader.LoadROMS();
			this._betaDisk._spec = this;
			base.SetResolution(320, 240);
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
				this.setImage(this._ROMS[0], data, startIndex, length);
				break;
			case RomName.ROM_48:
				this.setImage(this._ROMS[1], data, startIndex, length);
				break;
			case RomName.ROM_TRDOS:
				this.setImage(this._ROMS[2], data, startIndex, length);
				break;
			default:
				return false;
			}
			base.OnUpdateState();
			return true;
		}

		public override bool SetRamImage(int page, byte[] data, int startIndex, int length)
		{
			if (page >= this._RAMS.Length)
			{
				return false;
			}
			if (length > 16384)
			{
				length = 16384;
			}
			this.setImage(this._RAMS[page], data, startIndex, length);
			base.OnUpdateState();
			return true;
		}

		public override byte[] GetRamImage(int page)
		{
			if (page >= this._RAMS.Length)
			{
				return null;
			}
			byte[] array = new byte[16384];
			this.setImage(array, this._RAMS[page], 0, 16384);
			return array;
		}

		public override int GetRamImagePageCount()
		{
			return this._RAMS.Length;
		}

		public override byte ReadMemory(ushort addr)
		{
			int num = (int)(addr & 49152);
			if (num != 0)
			{
				if (num == 16384)
				{
					return this._RAMS[5][(int)(addr & 16383)];
				}
				if (num != 32768)
				{
					return this._RAMS[(int)(this._port7FFD & 7)][(int)(addr & 16383)];
				}
				return this._RAMS[2][(int)(addr & 16383)];
			}
			else
			{
				if (this._selTRDOS)
				{
					return this._ROMS[2][(int)addr];
				}
				return this._ROMS[(this._port7FFD & 16) >> 4][(int)addr];
			}
		}

		public override void WriteMemory(ushort addr, byte value)
		{
			int num = (int)(addr & 49152);
			if (num != 0)
			{
				if (num == 16384)
				{
					this._RAMS[5][(int)(addr & 16383)] = value;
					return;
				}
				if (num == 32768)
				{
					this._RAMS[2][(int)(addr & 16383)] = value;
					return;
				}
				this._RAMS[(int)(this._port7FFD & 7)][(int)(addr & 16383)] = value;
			}
		}

		public override void AddBreakpoint(ushort addr)
		{
			if (this._breakpoints == null)
			{
				this._breakpoints = new List<ushort>();
			}
			if (!this._breakpoints.Contains(addr))
			{
				this._breakpoints.Add(addr);
			}
		}

		public override void RemoveBreakpoint(ushort addr)
		{
			if (this._breakpoints != null)
			{
				if (this._breakpoints.Contains(addr))
				{
					this._breakpoints.Remove(addr);
				}
				if (this._breakpoints.Count < 1)
				{
					this._breakpoints = null;
				}
			}
		}

		public override ushort[] GetBreakpointList()
		{
			if (this._breakpoints == null)
			{
				return new ushort[0];
			}
			return this._breakpoints.ToArray();
		}

		public override bool CheckBreakpoint(ushort addr)
		{
			return this._breakpoints != null && this._breakpoints.Contains(addr);
		}

		public override void ClearBreakpoints()
		{
			if (this._breakpoints != null)
			{
				this._breakpoints.Clear();
			}
			this._breakpoints = null;
		}

		protected override void VideoParamsChanged(VideoManager sender, VideoParams value)
		{
			if (value == null || value.LinePitch < 320 || value.Width < 320 || value.Height < 240 || value.BPP != 32)
			{
				Logger.GetLogger().LogError("Pentagon128.VideoParamsChanged: Unsupported params!");
				throw new ArgumentOutOfRangeException("Unsupported VideoFrameParams");
			}
			this.fillUlaTables(this.GetFrameTactCount(), value);
		}

		public override long KeyboardState
		{
			get
			{
				return this._keyboardState;
			}
			set
			{
				this._keyboardState = value;
			}
		}

		public override int MouseX
		{
			get
			{
				return this._mouseX;
			}
			set
			{
				this._mouseX = value;
			}
		}

		public override int MouseY
		{
			get
			{
				return this._mouseY;
			}
			set
			{
				this._mouseY = value;
			}
		}

		public override int MouseButtons
		{
			get
			{
				return this._mouseButtons;
			}
			set
			{
				this._mouseButtons = value;
			}
		}

		protected byte ReadAyMouseX()
		{
			int num = this._mouseX / 4 - this._lastAyMouseX;
			this._lastAyMouseX = this._mouseX / 4;
			int num2 = 0;
			if ((this._mouseButtons & 1) == 0)
			{
				num2 |= 16;
			}
			if ((this._mouseButtons & 2) == 0)
			{
				num2 |= 32;
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
			return (byte)(num | num2 | 192);
		}

		protected byte ReadAyMouseY()
		{
			int num = this._lastAyMouseY - this._mouseY / 4;
			this._lastAyMouseY = this._mouseY / 4;
			int num2 = 0;
			if ((this._mouseButtons & 1) == 0)
			{
				num2 |= 16;
			}
			if ((this._mouseButtons & 2) == 0)
			{
				num2 |= 32;
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
			return (byte)(num | num2 | 192);
		}

		protected byte ReadKempstonMouseX()
		{
			return (byte)(this._mouseX / 3);
		}

		protected byte ReadKempstonMouseY()
		{
			return (byte)(-this._mouseY / 3);
		}

		private void ay_UpdateIRA(AY8910 sender, AyPortState state)
		{
			if (((state.OutState ^ state.OldOutState) & 64) != 0)
			{
				state.InState = (byte)((((state.OutState & 64) != 0) ? (this.ReadAyMouseY() | 64) : this.ReadAyMouseX()));
			}
		}

		public unsafe override void ExecuteFrame(IntPtr videoPtr, IntPtr soundPtr)
		{
			this._bitmapBufPtr = (uint*)((void*)videoPtr);
			this._soundBufPtr = (uint*)((void*)soundPtr);
			try
			{
				if (this.IsRunning)
				{
					this.fetchVideo(this._bitmapBufPtr, 0, this.GetFrameTact(), ref this._ulaFetchBW, ref this._ulaFetchAT, ref this._ulaFetchInk, ref this._ulaFetchPaper);
					ulong num = this._cpu.Tact - (ulong)((long)this.GetFrameTact());
					while (this._cpu.Tact - num <= (ulong)((long)this.GetFrameTactCount()))
					{
						this._cpu.ExecCycle();
						if (this._tape.IsPlay)
						{
							this._portFE &= 191;
							this._portFE |= (byte)((this._tape.GetTapeBit(this._cpu.Tact) & 64));
							this.processAudioChanges(this.GetFrameTact());
						}
						if (this._breakpoints != null && this.CheckBreakpoint(this._cpu.regs.PC))
						{
							this.IsRunning = false;
							base.OnBreakpoint();
							break;
						}
					}
					this.flushAudio();
					this.flushVideo();
				}
				else
				{
					this.DrawScreen(videoPtr);
					if (soundPtr != IntPtr.Zero)
					{
						for (int i = 0; i < 3528; i++)
						{
							((byte*)((void*)soundPtr))[i] = 0;
						}
					}
				}
			}
			finally
			{
				this._bitmapBufPtr = null;
				this._soundBufPtr = null;
			}
		}

		public unsafe override void DrawScreen(IntPtr videoPtr)
		{
			if (videoPtr == IntPtr.Zero)
			{
				return;
			}
			byte b = 0;
			byte b2 = 0;
			uint num = 0U;
			uint num2 = 0U;
			this.fetchVideo((uint*)((void*)videoPtr), 0, this.GetFrameTactCount(), ref b, ref b2, ref num, ref num2);
		}

		protected override void Reset()
		{
			this._port7FFD = 0;
			this._ulaMemory = this._RAMS[5];
			base.Reset();
			this.Sound.Reset();
		}

		public override int GetFrameTact()
		{
			return (int)(this._cpu.Tact % (ulong)((long)this.GetFrameTactCount()));
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
			int num = (int)(ADDR & 49152);
			if (num != 0)
			{
				if (num == 16384)
				{
					if (M1)
					{
						this._selTRDOS = false;
					}
					return this._RAMS[5][(int)(ADDR & 16383)];
				}
				if (num != 32768)
				{
					if (M1)
					{
						this._selTRDOS = false;
					}
					return this._RAMS[(int)(this._port7FFD & 7)][(int)(ADDR & 16383)];
				}
				if (M1)
				{
					this._selTRDOS = false;
				}
				return this._RAMS[2][(int)(ADDR & 16383)];
			}
			else
			{
				if (this._selTRDOS)
				{
					return this._ROMS[2][(int)ADDR];
				}
				if (M1 && (ADDR & 65280) == 15616 && (this._port7FFD & 16) != 0)
				{
					this._selTRDOS = true;
					return this._ROMS[2][(int)ADDR];
				}
				return this._ROMS[(this._port7FFD & 16) >> 4][(int)ADDR];
			}
		}

		private void WRMEM(ushort ADDR, byte value)
		{
			int num = (int)(ADDR & 49152);
			if (num == 16384)
			{
				if (this._ulaMemory == this._RAMS[5])
				{
					this.processVideoChanges();
				}
				this._RAMS[5][(int)(ADDR & 16383)] = value;
				return;
			}
			if (num == 32768)
			{
				this._RAMS[2][(int)(ADDR & 16383)] = value;
				return;
			}
			if (num != 49152)
			{
				return;
			}
			if (this._ulaMemory == this._RAMS[(int)(this._port7FFD & 7)])
			{
				this.processVideoChanges();
			}
			this._RAMS[(int)(this._port7FFD & 7)][(int)(ADDR & 16383)] = value;
		}

		private byte RDPORT(ushort ADDR)
		{
			if (this._selTRDOS)
			{
				int num = (int)(ADDR & 227);
				if (num <= 35)
				{
					if (num == 3)
					{
						return this._betaDisk.GetReg(RegWD1793.COMMAND, this._cpu.Tact);
					}
					if (num == 35)
					{
						return this._betaDisk.GetReg(RegWD1793.TRACK, this._cpu.Tact);
					}
				}
				else
				{
					if (num == 67)
					{
						return this._betaDisk.GetReg(RegWD1793.SECTOR, this._cpu.Tact);
					}
					if (num == 99)
					{
						return this._betaDisk.GetReg(RegWD1793.DATA, this._cpu.Tact);
					}
					if (num == 227)
					{
						return this._betaDisk.GetReg(RegWD1793.BETA128, this._cpu.Tact);
					}
				}
			}
			if ((ADDR & 255) == 254)
			{
				this._portFE &= 191;
				this._portFE |= (byte)((this._tape.GetTapeBit(this._cpu.Tact) & 64));
				return (byte)((31 & this.scanKbdPort(ADDR)) | (int)(this._portFE & 64) | (int)(this.CPU.FreeBUS & 160));
			}
			if ((ADDR & 255) == 255)
			{
				return byte.MaxValue;
			}
			if ((ADDR & 49407) == 49405)
			{
				return this._sound.DATA_REG;
			}
			if (ADDR == 64223)
			{
				return (byte)(this._mouseButtons ^ 255);
			}
			if (ADDR == 64479)
			{
				return this.ReadKempstonMouseX();
			}
			if (ADDR == 65503)
			{
				return this.ReadKempstonMouseY();
			}
			return byte.MaxValue;
		}

		private void WRPORT(ushort ADDR, byte value)
		{
			if (this._selTRDOS)
			{
				int num = (int)(ADDR & 227);
				if (num <= 35)
				{
					if (num != 3)
					{
						if (num == 35)
						{
							this._betaDisk.SetReg(RegWD1793.TRACK, value, this._cpu.Tact);
						}
					}
					else
					{
						this._betaDisk.SetReg(RegWD1793.COMMAND, value, this._cpu.Tact);
					}
				}
				else if (num != 67)
				{
					if (num != 99)
					{
						if (num == 227)
						{
							this._betaDisk.SetReg(RegWD1793.BETA128, value, this._cpu.Tact);
						}
					}
					else
					{
						this._betaDisk.SetReg(RegWD1793.DATA, value, this._cpu.Tact);
					}
				}
				else
				{
					this._betaDisk.SetReg(RegWD1793.SECTOR, value, this._cpu.Tact);
				}
			}
			if ((ADDR & 1) == 0)
			{
				this.processAudioChanges(this.GetFrameTact());
				this.processVideoChanges();
				this._portFE = value;
				this._portFE &= 191;
				this._portFE |= (byte)((byte)((this._tape.GetTapeBit(this._cpu.Tact) & 64)));
			}
			if ((ADDR & 32770) == 0 && (this._port7FFD & 32) == 0)
			{
				this._port7FFD = value;
				if ((this._port7FFD & 8) == 0)
				{
					this._ulaMemory = this._RAMS[5];
				}
				else
				{
					this._ulaMemory = this._RAMS[7];
				}
			}
			if ((ADDR & 49407) == 49405)
			{
				this._sound.ADDR_REG = value;
			}
			if ((ADDR & 49407) == 33021)
			{
				this._sound.process(this.GetFrameTact());
				this._sound.DATA_REG = value;
			}
		}

		private unsafe void flushAudio()
		{
			this.processAudioChanges(this.GetFrameTactCount());
			this._beeperSamplePos = 0;
			this._sound.flush();
			if (this._soundBufPtr != null)
			{
				for (int i = 0; i < 882; i++)
				{
					uint num = this._beeperSamples[i];
					uint num2 = this._sound.samples[i];
					this._soundBufPtr[i] = (((num >> 16) + (num2 >> 16)) / 2U << 16 | ((num & 65535U) + (num2 & 65535U)) / 2U);
				}
			}
		}

		private void flushVideo()
		{
			this.processVideoChanges();
			this._lastFrameTact = 0;
			this._flashCounter++;
			if (this._flashCounter > 24)
			{
				this._flashState ^= 256;
				this._flashCounter = 0;
			}
		}

		private void processVideoChanges()
		{
			int num = this.GetFrameTact();
			if (num < 32)
			{
				this._cpu.INT = true;
			}
			else
			{
				this._cpu.INT = false;
			}
			if (num < this._lastFrameTact)
			{
				num = this.GetFrameTactCount();
			}
			unsafe { this.fetchVideo(this._bitmapBufPtr, this._lastFrameTact, num, ref this._ulaFetchBW, ref this._ulaFetchAT, ref this._ulaFetchInk, ref this._ulaFetchPaper); }
			this._lastFrameTact = num;
		}

		private void processAudioChanges(int ToTact)
		{
			int num = 882 * ToTact / this.GetFrameTactCount();
			if (num > 882)
			{
				num = 882;
			}
			if (num > this._beeperSamplePos)
			{
				uint num2 = 0U;
				if ((this._portFE & 16) != 0)
				{
					num2 += 8191U;
				}
				if (this.Config.TapeOutSoundEnable && (this._portFE & 8) != 0)
				{
					num2 += 8191U;
				}
				if (this.Config.TapeInSoundEnable && (this._portFE & 64) != 0)
				{
					num2 += 8191U;
				}
				num2 /= 3U;
				num2 |= num2 << 16;
				while (this._beeperSamplePos < num)
				{
					this._beeperSamples[this._beeperSamplePos] = num2;
					this._beeperSamplePos++;
				}
			}
		}

		private int scanKbdPort(ushort port)
		{
			byte b = 31;
			int num = 256;
			int i = 0;
			while (i < 8)
			{
				if (((int)port & num) == 0)
				{
					b &= (byte)((this._keyboardState >> i * 5 ^ 31L) & 31L);
				}
				i++;
				num <<= 1;
			}
			return (int)b;
		}

		private void fillUlaTables(int MaxTakt, VideoParams videoParams)
		{
			this._ulaLineOffset = new int[MaxTakt];
			this._ulaAddrBW = new int[MaxTakt];
			this._ulaAddrAT = new int[MaxTakt];
			this._ulaDo = new byte[MaxTakt];
			int num = 0;
			for (int i = 0; i < 320; i++)
			{
				int j = 0;
				while (j < 224)
				{
					if (i >= 56 && i < 296 && j >= 52 && j < 212)
					{
						if (i >= 80 && i < 272 && j >= 68 && j < 196)
						{
							this._ulaDo[num] = 3;
							if ((j & 3) == 3)
							{
								this._ulaDo[num] = 4;
								int num2 = j + 1 - 68;
								int num3 = i - 80;
								num2 >>= 2;
								int num4 = num2 | num3 >> 3 << 5;
								int num5 = num2 | num3 << 5;
								this._ulaAddrBW[num] = ((num5 & 6175) | (num5 & 1792) >> 3 | (num5 & 224) << 3);
								this._ulaAddrAT[num] = 6144 + num4;
							}
						}
						else if (i >= 80 && i < 272 && j == 67)
						{
							this._ulaDo[num] = 2;
							int num6 = j + 1 - 68;
							int num7 = i - 80;
							num6 >>= 2;
							int num8 = num6 | num7 >> 3 << 5;
							int num9 = num6 | num7 << 5;
							this._ulaAddrBW[num] = ((num9 & 6175) | (num9 & 1792) >> 3 | (num9 & 224) << 3);
							this._ulaAddrAT[num] = 6144 + num8;
						}
						else
						{
							this._ulaDo[num] = 1;
						}
						int num10 = i - 56;
						int num11 = (j - 52) * 2;
						this._ulaLineOffset[num] = num10 * videoParams.LinePitch + num11;
					}
					else
					{
						this._ulaDo[num] = 0;
					}
					j++;
					num++;
				}
			}
			this._ulaInk = new uint[512];
			this._ulaPaper = new uint[512];
			for (int k = 0; k < 256; k++)
			{
				this._ulaInk[k] = Pentagon128._zxpal[(k & 7) + ((k & 64) >> 3)];
				this._ulaPaper[k] = Pentagon128._zxpal[(k >> 3 & 7) + ((k & 64) >> 3)];
				if ((k & 128) != 0)
				{
					this._ulaInk[k + 256] = Pentagon128._zxpal[(k >> 3 & 7) + ((k & 64) >> 3)];
					this._ulaPaper[k + 256] = Pentagon128._zxpal[(k & 7) + ((k & 64) >> 3)];
				}
				else
				{
					this._ulaInk[k + 256] = Pentagon128._zxpal[(k & 7) + ((k & 64) >> 3)];
					this._ulaPaper[k + 256] = Pentagon128._zxpal[(k >> 3 & 7) + ((k & 64) >> 3)];
				}
			}
		}

		private unsafe void fetchVideo(uint* bitmapBufPtr, int startTact, int endTact, ref byte ulaFetchBW, ref byte ulaFetchAT, ref uint ulaFetchInk, ref uint ulaFetchPaper)
		{
			if (this._bitmapBufPtr == null)
			{
				return;
			}
			if (this._ulaDo == null)
			{
				return;
			}
			if (endTact > this.GetFrameTactCount())
			{
				endTact = this.GetFrameTactCount();
			}
			if (startTact > this.GetFrameTactCount())
			{
				startTact = this.GetFrameTactCount();
			}
			if (startTact < 12544)
			{
				startTact = 12544;
			}
			for (int i = startTact; i < endTact; i++)
			{
				switch (this._ulaDo[i])
				{
				case 1:
					bitmapBufPtr[this._ulaLineOffset[i]] = Pentagon128._zxpal[(int)(this._portFE & 7)];
					bitmapBufPtr[this._ulaLineOffset[i] + 1] = Pentagon128._zxpal[(int)(this._portFE & 7)];
					break;
				case 2:
					bitmapBufPtr[this._ulaLineOffset[i]] = Pentagon128._zxpal[(int)(this._portFE & 7)];
					bitmapBufPtr[this._ulaLineOffset[i] + 1] = Pentagon128._zxpal[(int)(this._portFE & 7)];
					ulaFetchBW = this._ulaMemory[this._ulaAddrBW[i]];
					ulaFetchAT = this._ulaMemory[this._ulaAddrAT[i]];
					ulaFetchInk = this._ulaInk[(int)ulaFetchAT + this._flashState];
					ulaFetchPaper = this._ulaPaper[(int)ulaFetchAT + this._flashState];
					break;
				case 3:
					bitmapBufPtr[this._ulaLineOffset[i]] = (((ulaFetchBW & 128) != 0) ? ulaFetchInk : ulaFetchPaper);
					bitmapBufPtr[this._ulaLineOffset[i] + 1] = (((ulaFetchBW & 64) != 0) ? ulaFetchInk : ulaFetchPaper);
					ulaFetchBW = (byte)(ulaFetchBW << 2);
					break;
				case 4:
					bitmapBufPtr[this._ulaLineOffset[i]] = (((ulaFetchBW & 128) != 0) ? ulaFetchInk : ulaFetchPaper);
					bitmapBufPtr[this._ulaLineOffset[i] + 1] = (((ulaFetchBW & 64) != 0) ? ulaFetchInk : ulaFetchPaper);
					ulaFetchBW = (byte)(ulaFetchBW << 2);
					ulaFetchBW = this._ulaMemory[this._ulaAddrBW[i]];
					ulaFetchAT = this._ulaMemory[this._ulaAddrAT[i]];
					ulaFetchInk = this._ulaInk[(int)ulaFetchAT + this._flashState];
					ulaFetchPaper = this._ulaPaper[(int)ulaFetchAT + this._flashState];
					break;
				}
			}
		}

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

		private static uint[] _zxpal = new uint[]
		{
			4278190080U,
			4278190272U,
			4290772992U,
			4290773184U,
			4278239232U,
			4278239424U,
			4290822144U,
			4290822336U,
			4278190080U,
			4278190335U,
			4294901760U,
			4294902015U,
			4278255360U,
			4278255615U,
			4294967040U,
			uint.MaxValue
		};
	}
}
