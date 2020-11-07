using System;

namespace ZXMAK.Engine.Disk
{
	public class WD1793 : IDisposable
	{
		public WD1793()
		{
			this.drive = 0;
			this.fdd = new DiskImage[4];
			for (int i = 0; i < this.fdd.Length; i++)
			{
				this.fdd[i] = new DiskImage(700000UL);
				this.fdd[i].InitFormated();
			}
			this.trkcache = this.fdd[this.drive & 3].CurrentTrack;
			this.wd93_nodelay = false;
		}

		public DiskImage[] FDD
		{
			get
			{
				return this.fdd;
			}
		}

		public void SetReg(RegWD1793 reg, byte value, ulong tact)
		{
			this.process(tact);
			switch (reg)
			{
			case RegWD1793.COMMAND:
				if ((value & 240) == 208)
				{
					this.state = WDSTATE.S_IDLE;
					this.rqs = BETA_STATUS.INTRQ;
					this.status &= ~WD_STATUS.WDS_BUSY;
				}
				else if ((this.status & WD_STATUS.WDS_BUSY) == (WD_STATUS)0)
				{
					this.cmd = value;
					this.next = tact;
					this.status |= WD_STATUS.WDS_BUSY;
					this.rqs = BETA_STATUS.NONE;
					if ((this.cmd & 128) != 0)
					{
						if ((this.status & WD_STATUS.WDS_NOTRDY) != (WD_STATUS)0)
						{
							this.state = WDSTATE.S_IDLE;
							this.rqs = BETA_STATUS.INTRQ;
						}
						else
						{
							if (this.fdd[this.drive].motor > 0UL || this.wd93_nodelay)
							{
								this.fdd[this.drive].motor = this.next + 7000000UL;
							}
							this.state = WDSTATE.S_DELAY_BEFORE_CMD;
						}
					}
					else
					{
						this.state = WDSTATE.S_TYPE1_CMD;
					}
				}
				break;
			case RegWD1793.TRACK:
				this.track = value;
				break;
			case RegWD1793.SECTOR:
				this.sector = value;
				break;
			case RegWD1793.DATA:
				this.data = value;
				this.rqs &= (BETA_STATUS)191;
				this.status &= ~WD_STATUS.WDS_INDEX;
				break;
			case RegWD1793.BETA128:
				this.system = value;
				this.drive = (int)(value & 3);
				this.side = (1 & ~(value >> 4));
				this.fdd[this.drive & 3].HeadSide = this.side;
				this.trkcache = this.fdd[this.drive & 3].CurrentTrack;
				if ((value & 4) == 0)
				{
					this.status = WD_STATUS.WDS_NOTRDY;
					this.rqs = BETA_STATUS.INTRQ;
					this.fdd[this.drive].motor = 0UL;
					this.state = WDSTATE.S_IDLE;
				}
				break;
			default:
				throw new Exception("WD1793.SetReg: Invalid register");
			}
			this.process(tact);
		}

		public byte GetReg(RegWD1793 reg, ulong tact)
		{
			this.process(tact);
			switch (reg)
			{
			case RegWD1793.COMMAND:
				this.rqs &= (BETA_STATUS)127;
				return (byte)this.status;
			case RegWD1793.TRACK:
				return this.track;
			case RegWD1793.SECTOR:
				return this.sector;
			case RegWD1793.DATA:
				this.status &= ~WD_STATUS.WDS_INDEX;
				this.rqs &= (BETA_STATUS)191;
				return this.data;
			case RegWD1793.BETA128:
				return (byte)(this.rqs | (BETA_STATUS)63);
			default:
				throw new Exception("WD1793.GetReg: Invalid register");
			}
		}

		public string DumpState()
		{
			return string.Concat(new string[]
			{
				"CMD: 0x",
				this.cmd.ToString("X2"),
				"\nSTATUS: ",
				this.status.ToString(),
				"\nTRK: 0x",
				this.track.ToString("X2"),
				"\nSEC: 0x",
				this.sector.ToString("X2"),
				"\nDATA: 0x",
				this.data.ToString("X2"),
				"\n--------------------------\nbeta: ",
				this.rqs.ToString(),
				"\nsystem: 0x",
				this.system.ToString("X2"),
				"\nstate: ",
				this.state.ToString(),
				"\nstate2: ",
				this.state2.ToString(),
				"\ndrive: ",
				this.drive.ToString(),
				"\nside: ",
				this.side.ToString(),
				"\ntime: ",
				this.time.ToString(),
				"\nnext: ",
				this.next.ToString(),
				"\ntshift: ",
				this.tshift.ToString(),
				"\nrwptr: ",
				this.rwptr.ToString(),
				"\nrwlen: ",
				this.rwlen.ToString(),
				"\n--------------------------CYLS: ",
				this.fdd[this.drive & 3].CylynderCount.ToString(),
				"\nHEAD: ",
				this.fdd[this.drive & 3].HeadCylynder.ToString(),
				"/",
				this.fdd[this.drive & 3].HeadSide.ToString(),
				"\nREADY: ",
				this.fdd[this.drive & 3].IsREADY.ToString(),
				"\nTR00: ",
				this.fdd[this.drive & 3].IsTRK00.ToString(),
				"\n--------------------------"
			});
		}

		private void process(ulong toTact)
		{
			this.time = toTact;
			if (this.time > this.fdd[this.drive].motor && (this.system & 8) != 0)
			{
				this.fdd[this.drive].motor = 0UL;
			}
			if (this.fdd[this.drive].IsREADY)
			{
				this.status &= ~WD_STATUS.WDS_NOTRDY;
			}
			else
			{
				this.status |= WD_STATUS.WDS_NOTRDY;
			}
			if ((this.cmd & 128) == 0)
			{
				this.status &= ~(WD_STATUS.WDS_INDEX | WD_STATUS.WDS_TRK00);
				if (this.fdd[this.drive].motor > 0UL && (this.system & 8) != 0)
				{
					this.status |= WD_STATUS.WDS_RECORDT;
				}
				if (this.fdd[this.drive].IsTRK00)
				{
					this.status |= WD_STATUS.WDS_TRK00;
				}
				if (this.fdd[this.drive].IsREADY && this.fdd[this.drive].motor > 0UL && (this.time + this.tshift) % 700000UL < 14000UL)
				{
					this.status |= WD_STATUS.WDS_INDEX;
				}
			}
			for (;;)
			{
				switch (this.state)
				{
				case WDSTATE.S_IDLE:
					goto IL_192;
				case WDSTATE.S_WAIT:
					if (this.time < this.next)
					{
						return;
					}
					this.state = this.state2;
					continue;
				case WDSTATE.S_DELAY_BEFORE_CMD:
					if (!this.wd93_nodelay && (this.cmd & 4) != 0)
					{
						this.next += 52500UL;
					}
					this.status = ((this.status | WD_STATUS.WDS_BUSY) & ~(WD_STATUS.WDS_INDEX | WD_STATUS.WDS_TRK00 | WD_STATUS.WDS_NOTFOUND | WD_STATUS.WDS_RECORDT | WD_STATUS.WDS_WRITEP));
					this.state2 = WDSTATE.S_CMD_RW;
					this.state = WDSTATE.S_WAIT;
					continue;
				case WDSTATE.S_CMD_RW:
					if (((this.cmd & 224) == 160 || (this.cmd & 240) == 240) && this.fdd[this.drive].IsWP)
					{
						this.status |= WD_STATUS.WDS_WRITEP;
						this.state = WDSTATE.S_IDLE;
						continue;
					}
					if ((this.cmd & 192) == 128 || (this.cmd & 248) == 192)
					{
						this.end_waiting_am = this.next + 3500000UL;
						this.find_marker();
						continue;
					}
					if ((this.cmd & 248) == 240)
					{
						this.rqs = BETA_STATUS.DRQ;
						this.status |= WD_STATUS.WDS_INDEX;
						this.next += 3UL * this.trkcache.ByteTime;
						this.state2 = WDSTATE.S_WRTRACK;
						this.state = WDSTATE.S_WAIT;
						continue;
					}
					if ((this.cmd & 248) == 224)
					{
						this.load();
						this.rwptr = 0;
						this.rwlen = this.trkcache.RawLength;
						this.state2 = WDSTATE.S_READ;
						this.getindex();
						continue;
					}
					this.state = WDSTATE.S_IDLE;
					continue;
				case WDSTATE.S_FOUND_NEXT_ID:
					if (!this.fdd[this.drive].IsREADY)
					{
						this.end_waiting_am = this.next + 3500000UL;
						this.find_marker();
						continue;
					}
					if (this.next >= this.end_waiting_am)
					{
						this.status |= WD_STATUS.WDS_NOTFOUND;
						this.state = WDSTATE.S_IDLE;
						continue;
					}
					if (this.foundid == -1)
					{
						this.status |= WD_STATUS.WDS_NOTFOUND;
						this.state = WDSTATE.S_IDLE;
						continue;
					}
					this.status &= ~WD_STATUS.WDS_CRCERR;
					this.load();
					if ((this.cmd & 128) == 0)
					{
						if (((SECHDR)this.trkcache.HeaderList[this.foundid]).c != this.track)
						{
							this.find_marker();
							continue;
						}
						if (!((SECHDR)this.trkcache.HeaderList[this.foundid]).c1)
						{
							this.status |= WD_STATUS.WDS_CRCERR;
							this.find_marker();
							continue;
						}
						this.state = WDSTATE.S_IDLE;
						continue;
					}
					else
					{
						if ((this.cmd & 240) == 192)
						{
							this.rwptr = ((SECHDR)this.trkcache.HeaderList[this.foundid]).idOffset;
							this.rwlen = 6;
							this.data = this.trkcache.RawRead(this.rwptr++);
							this.rwlen--;
							this.rqs = BETA_STATUS.DRQ;
							this.status |= WD_STATUS.WDS_INDEX;
							this.next += this.trkcache.ByteTime;
							this.state = WDSTATE.S_WAIT;
							this.state2 = WDSTATE.S_READ;
							continue;
						}
						if (((SECHDR)this.trkcache.HeaderList[this.foundid]).c != this.track || ((SECHDR)this.trkcache.HeaderList[this.foundid]).n != this.sector)
						{
							this.find_marker();
							continue;
						}
						if ((this.cmd & 2) != 0 && ((this.cmd >> 3 ^ (int)((SECHDR)this.trkcache.HeaderList[this.foundid]).s) & 1) != 0)
						{
							this.find_marker();
							continue;
						}
						if (!((SECHDR)this.trkcache.HeaderList[this.foundid]).c1)
						{
							this.status |= WD_STATUS.WDS_CRCERR;
							this.find_marker();
							continue;
						}
						if ((this.cmd & 32) != 0)
						{
							this.rqs = BETA_STATUS.DRQ;
							this.status |= WD_STATUS.WDS_INDEX;
							this.next += this.trkcache.ByteTime * 9UL;
							this.state = WDSTATE.S_WAIT;
							this.state2 = WDSTATE.S_WRSEC;
							continue;
						}
						if (((SECHDR)this.trkcache.HeaderList[this.foundid]).dataOffset < 0)
						{
							this.find_marker();
							continue;
						}
						if (this.trkcache.RawRead(((SECHDR)this.trkcache.HeaderList[this.foundid]).dataOffset - 1) == 248)
						{
							this.status |= WD_STATUS.WDS_RECORDT;
						}
						else
						{
							this.status &= ~WD_STATUS.WDS_RECORDT;
						}
						this.rwptr = ((SECHDR)this.trkcache.HeaderList[this.foundid]).dataOffset;
						this.rwlen = 128 << (int)((SECHDR)this.trkcache.HeaderList[this.foundid]).l;
						ulong num = (ulong)((long)this.trkcache.RawLength * (long)this.trkcache.ByteTime);
						int num2 = (int)((this.next + this.tshift) % num / this.trkcache.ByteTime);
						int dataOffset = ((SECHDR)this.trkcache.HeaderList[this.foundid]).dataOffset;
						int num3 = (dataOffset > num2) ? (dataOffset - num2) : (this.trkcache.RawLength + dataOffset - num2);
						this.next += (ulong)((long)num3 * (long)this.trkcache.ByteTime);
						this.state = WDSTATE.S_WAIT;
						this.state2 = WDSTATE.S_READ;
						continue;
					}
					break;
				case WDSTATE.S_READ:
					if (this.notready())
					{
						continue;
					}
					this.load();
					if (this.rwlen > 0)
					{
						if ((byte)(this.rqs & BETA_STATUS.DRQ) != 0)
						{
							this.status |= WD_STATUS.WDS_TRK00;
						}
						this.data = this.trkcache.RawRead(this.rwptr++);
						this.rwlen--;
						this.rqs = BETA_STATUS.DRQ;
						this.status |= WD_STATUS.WDS_INDEX;
						if (!this.wd93_nodelay)
						{
							this.next += this.trkcache.ByteTime;
						}
						this.state = WDSTATE.S_WAIT;
						this.state2 = WDSTATE.S_READ;
						continue;
					}
					if ((this.cmd & 224) == 128)
					{
						if (!((SECHDR)this.trkcache.HeaderList[this.foundid]).c2)
						{
							this.status |= WD_STATUS.WDS_CRCERR;
						}
						if ((this.cmd & 16) != 0)
						{
							this.sector += 1;
							this.state = WDSTATE.S_CMD_RW;
							continue;
						}
					}
					if ((this.cmd & 240) == 192 && !((SECHDR)this.trkcache.HeaderList[this.foundid]).c1)
					{
						this.status |= WD_STATUS.WDS_CRCERR;
					}
					this.state = WDSTATE.S_IDLE;
					continue;
				case WDSTATE.S_WRSEC:
					this.load();
					if ((byte)(this.rqs & BETA_STATUS.DRQ) != 0)
					{
						this.status |= WD_STATUS.WDS_TRK00;
						this.state = WDSTATE.S_IDLE;
						continue;
					}
					this.fdd[this.drive].ModifyFlag |= ModifyFlag.SectorLevel;
					this.rwptr = ((SECHDR)this.trkcache.HeaderList[this.foundid]).idOffset + 6 + 11 + 11;
					this.rwlen = 0;
					while (this.rwlen < 12)
					{
						this.trkcache.RawWrite(this.rwptr++, 0, false);
						this.rwlen++;
					}
					this.rwlen = 0;
					while (this.rwlen < 3)
					{
						this.trkcache.RawWrite(this.rwptr++, 161, true);
						this.rwlen++;
					}
					this.trkcache.RawWrite(this.rwptr++, (byte)(((this.cmd & 1) != 0) ? 248 : 251), false);
					this.rwlen = 128 << (int)((SECHDR)this.trkcache.HeaderList[this.foundid]).l;
					this.state = WDSTATE.S_WRITE;
					continue;
				case WDSTATE.S_WRITE:
				{
					if (this.notready())
					{
						continue;
					}
					if ((byte)(this.rqs & BETA_STATUS.DRQ) != 0)
					{
						this.status |= WD_STATUS.WDS_TRK00;
						this.data = 0;
					}
					this.trkcache.RawWrite(this.rwptr++, this.data, false);
					this.rwlen--;
					if (this.rwptr == this.trkcache.RawLength)
					{
						this.rwptr = 0;
					}
					this.trkcache.sf = true;
					if (this.rwlen > 0)
					{
						if (!this.wd93_nodelay)
						{
							this.next += this.trkcache.ByteTime;
						}
						this.state = WDSTATE.S_WAIT;
						this.state2 = WDSTATE.S_WRITE;
						this.rqs = BETA_STATUS.DRQ;
						this.status |= WD_STATUS.WDS_INDEX;
						continue;
					}
					int num4 = (128 << (int)((SECHDR)this.trkcache.HeaderList[this.foundid]).l) + 1;
					byte[] array = new byte[2056];
					if (this.rwptr < num4)
					{
						for (int i = 0; i < this.rwptr; i++)
						{
							array[i] = this.trkcache.RawRead(this.trkcache.RawLength - this.rwptr + i);
						}
						for (int j = 0; j < num4 - this.rwptr; j++)
						{
							array[this.rwptr + j] = this.trkcache.RawRead(j);
						}
					}
					else
					{
						for (int k = 0; k < num4; k++)
						{
							array[k] = this.trkcache.RawRead(this.rwptr - num4 + k);
						}
					}
					uint num5 = (uint)WD1793.wd93_crc(array, 0, num4);
					this.trkcache.RawWrite(this.rwptr++, (byte)num5, false);
					this.trkcache.RawWrite(this.rwptr++, (byte)(num5 >> 8), false);
					this.trkcache.RawWrite(this.rwptr, byte.MaxValue, false);
					if ((this.cmd & 16) != 0)
					{
						this.sector += 1;
						this.state = WDSTATE.S_CMD_RW;
						continue;
					}
					this.state = WDSTATE.S_IDLE;
					continue;
				}
				case WDSTATE.S_WRTRACK:
					if ((byte)(this.rqs & BETA_STATUS.DRQ) != 0)
					{
						this.status |= WD_STATUS.WDS_TRK00;
						this.state = WDSTATE.S_IDLE;
						continue;
					}
					this.fdd[this.drive].ModifyFlag |= ModifyFlag.TrackLevel;
					this.state2 = WDSTATE.S_WR_TRACK_DATA;
					this.getindex();
					this.end_waiting_am = this.next + 3500000UL;
					continue;
				case WDSTATE.S_WR_TRACK_DATA:
				{
					if (this.notready())
					{
						continue;
					}
					if ((byte)(this.rqs & BETA_STATUS.DRQ) != 0)
					{
						this.status |= WD_STATUS.WDS_TRK00;
						this.data = 0;
					}
					this.trkcache = this.fdd[this.drive & 3].CurrentTrack;
					this.trkcache.sf = true;
					bool clock = false;
					byte value = this.data;
					uint num6 = 0U;
					if (this.data == 245)
					{
						value = 161;
						clock = true;
						this.start_crc = this.rwptr + 1;
					}
					if (this.data == 246)
					{
						value = 194;
						clock = true;
					}
					if (this.data == 247)
					{
						num6 = (uint)this.trkcache.WD1793_CRC(this.start_crc, this.rwptr - this.start_crc);
						value = (byte)(num6 & 255U);
					}
					this.trkcache.RawWrite(this.rwptr++, value, clock);
					this.rwlen--;
					if (this.data == 247)
					{
						this.trkcache.RawWrite(this.rwptr++, (byte)(num6 >> 8), clock);
						this.rwlen--;
					}
					if (this.rwlen > 0)
					{
						if (!this.wd93_nodelay)
						{
							this.next += this.trkcache.ByteTime;
						}
						this.state2 = WDSTATE.S_WR_TRACK_DATA;
						this.state = WDSTATE.S_WAIT;
						this.rqs = BETA_STATUS.DRQ;
						this.status |= WD_STATUS.WDS_INDEX;
						continue;
					}
					this.state = WDSTATE.S_IDLE;
					continue;
				}
				case WDSTATE.S_TYPE1_CMD:
					this.status = ((this.status | WD_STATUS.WDS_BUSY) & ~(WD_STATUS.WDS_INDEX | WD_STATUS.WDS_CRCERR | WD_STATUS.WDS_NOTFOUND | WD_STATUS.WDS_WRITEP));
					this.rqs = BETA_STATUS.NONE;
					if (this.fdd[this.drive].IsWP)
					{
						this.status |= WD_STATUS.WDS_WRITEP;
					}
					this.fdd[this.drive].motor = this.next + 7000000UL;
					this.state2 = WDSTATE.S_SEEKSTART;
					if ((this.cmd & 224) != 0)
					{
						if ((this.cmd & 64) != 0)
						{
							this.stepdirection = (int)(((this.cmd & 32) != 0) ? -1 : 1);
						}
						this.state2 = WDSTATE.S_STEP;
					}
					if (!this.wd93_nodelay)
					{
						this.next += 3500UL;
					}
					this.state = WDSTATE.S_WAIT;
					continue;
				case WDSTATE.S_STEP:
				{
					if (this.fdd[this.drive].IsTRK00 && (this.cmd & 240) == 0)
					{
						this.track = 0;
						this.state = WDSTATE.S_VERIFY;
						continue;
					}
					if ((this.cmd & 224) == 0 || (this.cmd & 16) != 0)
					{
						this.track = (byte)((int)this.track + this.stepdirection);
					}
					this.fdd[this.drive].HeadCylynder += this.stepdirection;
					if (this.fdd[this.drive].HeadCylynder >= this.fdd[this.drive].CylynderCount - 1)
					{
						this.fdd[this.drive].HeadCylynder = this.fdd[this.drive].CylynderCount - 1;
					}
					this.trkcache = this.fdd[this.drive & 3].CurrentTrack;
					uint[] array2 = new uint[]
					{
						6U,
						12U,
						20U,
						30U
					};
					if (!this.wd93_nodelay)
					{
						this.next += (ulong)(array2[(int)(this.cmd & 3)] * 3500000U / 1000U);
					}
					this.state2 = (((this.cmd & 224) != 0) ? WDSTATE.S_VERIFY : WDSTATE.S_SEEK);
					this.state = WDSTATE.S_WAIT;
					continue;
				}
				case WDSTATE.S_SEEKSTART:
					if ((this.cmd & 16) == 0)
					{
						this.track = byte.MaxValue;
						this.data = 0;
					}
					if (this.data == this.track)
					{
						this.state = WDSTATE.S_VERIFY;
						continue;
					}
					this.stepdirection = ((this.data < this.track) ? -1 : 1);
					this.state = WDSTATE.S_STEP;
					continue;
				case WDSTATE.S_SEEK:
					if (this.data == this.track)
					{
						this.state = WDSTATE.S_VERIFY;
						continue;
					}
					this.stepdirection = ((this.data < this.track) ? -1 : 1);
					this.state = WDSTATE.S_STEP;
					continue;
				case WDSTATE.S_VERIFY:
					if ((this.cmd & 4) == 0)
					{
						this.state = WDSTATE.S_IDLE;
						continue;
					}
					this.end_waiting_am = this.next + 4200000UL;
					this.load();
					this.find_marker();
					continue;
				case WDSTATE.S_RESET:
					if (this.fdd[this.drive].IsTRK00)
					{
						this.state = WDSTATE.S_IDLE;
					}
					else
					{
						this.fdd[this.drive].HeadCylynder--;
						this.trkcache = this.fdd[this.drive & 3].CurrentTrack;
					}
					this.next += 21000UL;
					continue;
				}
				break;
			}
			throw new Exception("WD1793.process - WD1793 in wrong state");
			IL_192:
			this.status &= ~WD_STATUS.WDS_BUSY;
			this.rqs = BETA_STATUS.INTRQ;
		}

		private void find_marker()
		{
			if (this.wd93_nodelay && this.fdd[this.drive].HeadCylynder != (int)this.track)
			{
				this.fdd[this.drive].HeadCylynder = (int)this.track;
			}
			this.load();
			this.foundid = -1;
			ulong num = 7000000UL;
			if (this.fdd[this.drive].motor > 0UL && this.fdd[this.drive].IsREADY)
			{
				ulong num2 = (ulong)((long)this.trkcache.RawLength * (long)this.trkcache.ByteTime);
				int num3 = (int)((this.next + this.tshift) % num2 / this.trkcache.ByteTime);
				num = ulong.MaxValue;
				for (int i = 0; i < this.trkcache.HeaderList.Count; i++)
				{
					int idOffset = ((SECHDR)this.trkcache.HeaderList[i]).idOffset;
					int num4 = (idOffset > num3) ? (idOffset - num3) : (this.trkcache.RawLength + idOffset - num3);
					if ((long)num4 < (long)num)
					{
						num = (ulong)((long)num4);
						this.foundid = i;
					}
				}
				if (this.foundid != -1)
				{
					num *= this.trkcache.ByteTime;
				}
				else
				{
					num = 7000000UL;
				}
				if (this.wd93_nodelay && this.foundid != -1)
				{
					int num5 = ((SECHDR)this.trkcache.HeaderList[this.foundid]).idOffset + 2;
					this.tshift = (ulong)(((long)num5 * (long)this.trkcache.ByteTime - (long)(this.next % num2) + (long)num2) % (long)num2);
					num = 100UL;
				}
			}
			this.next += num;
			if (this.fdd[this.drive].IsREADY && this.next > this.end_waiting_am)
			{
				this.next = this.end_waiting_am;
				this.foundid = -1;
			}
			this.state = WDSTATE.S_WAIT;
			this.state2 = WDSTATE.S_FOUND_NEXT_ID;
		}

		private bool notready()
		{
			if (!this.wd93_nodelay || (byte)(this.rqs & BETA_STATUS.DRQ) == 0)
			{
				return false;
			}
			if (this.next > this.end_waiting_am)
			{
				return false;
			}
			this.state2 = this.state;
			this.state = WDSTATE.S_WAIT;
			this.next += this.trkcache.ByteTime;
			return true;
		}

		private void load()
		{
			if (this.trkcache.sf)
			{
				this.trkcache.RefreshHeaders();
			}
			this.trkcache.sf = false;
			this.trkcache = this.fdd[this.drive & 3].CurrentTrack;
		}

		private void getindex()
		{
			ulong num = (ulong)((long)this.trkcache.RawLength * (long)this.trkcache.ByteTime);
			ulong num2 = (this.next + this.tshift) % num;
			if (!this.wd93_nodelay)
			{
				this.next += num - num2;
			}
			this.rwptr = 0;
			this.rwlen = this.trkcache.RawLength;
			this.state = WDSTATE.S_WAIT;
		}

		public void Dispose()
		{
			foreach (DiskImage diskImage in this.fdd)
			{
				diskImage.Eject();
			}
		}

		public static ushort wd93_crc(byte[] data, int startIndex, int size)
		{
			uint num = 52660U;
			while (size-- > 0)
			{
				num ^= (uint)((uint)data[startIndex++] << 8);
				for (int num2 = 8; num2 != 0; num2--)
				{
					if (((num *= 2U) & 65536U) != 0U)
					{
						num ^= 4129U;
					}
				}
			}
			return (ushort)((num & 65280U) >> 8 | (num & 255U) << 8);
		}

		public const int Z80FQ = 3500000;

		public const int FDD_RPS = 5;

		private const byte CMD_SEEK_RATE = 3;

		private const byte CMD_SEEK_VERIFY = 4;

		private const byte CMD_SEEK_HEADLOAD = 8;

		private const byte CMD_SEEK_TRKUPD = 16;

		private const byte CMD_SEEK_DIR = 32;

		private const byte CMD_WRITE_DEL = 1;

		private const byte CMD_SIDE_CMP_FLAG = 2;

		private const byte CMD_DELAY = 4;

		private const byte CMD_SIDE = 8;

		private const byte CMD_SIDE_SHIFT = 3;

		private const byte CMD_MULTIPLE = 16;

		public Spectrum _spec;

		private Track trkcache;

		private DiskImage[] fdd;

		private ulong next;

		private ulong time;

		private ulong tshift;

		private byte cmd;

		private WDSTATE state;

		private WDSTATE state2;

		private int drive;

		private int side;

		private int stepdirection = 1;

		private byte system;

		private byte data;

		private byte track;

		private byte sector;

		private BETA_STATUS rqs;

		private WD_STATUS status;

		private ulong end_waiting_am;

		private int foundid;

		private int rwptr;

		private int rwlen;

		private int start_crc;

		private bool wd93_nodelay;
	}
}
