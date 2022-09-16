using System;

namespace ZXMAK.Engine.Disk;

public class WD1793 : IDisposable
{
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

	public DiskImage[] FDD => fdd;

	public WD1793()
	{
		drive = 0;
		fdd = new DiskImage[4];
		for (int i = 0; i < fdd.Length; i++)
		{
			fdd[i] = new DiskImage(700000uL);
			fdd[i].InitFormated();
		}
		trkcache = fdd[drive & 3].CurrentTrack;
		wd93_nodelay = false;
	}

	public void SetReg(RegWD1793 reg, byte value, ulong tact)
	{
		process(tact);
		switch (reg)
		{
		case RegWD1793.COMMAND:
			if ((value & 0xF0) == 208)
			{
				state = WDSTATE.S_IDLE;
				rqs = BETA_STATUS.INTRQ;
				status &= ~WD_STATUS.WDS_BUSY;
			}
			else
			{
				if ((status & WD_STATUS.WDS_BUSY) != 0)
				{
					break;
				}
				cmd = value;
				next = tact;
				status |= WD_STATUS.WDS_BUSY;
				rqs = BETA_STATUS.NONE;
				if ((cmd & 0x80u) != 0)
				{
					if ((status & WD_STATUS.WDS_NOTRDY) != 0)
					{
						state = WDSTATE.S_IDLE;
						rqs = BETA_STATUS.INTRQ;
						break;
					}
					if (fdd[drive].motor != 0 || wd93_nodelay)
					{
						fdd[drive].motor = next + 7000000;
					}
					state = WDSTATE.S_DELAY_BEFORE_CMD;
				}
				else
				{
					state = WDSTATE.S_TYPE1_CMD;
				}
			}
			break;
		case RegWD1793.TRACK:
			track = value;
			break;
		case RegWD1793.SECTOR:
			sector = value;
			break;
		case RegWD1793.DATA:
			data = value;
			rqs &= (BETA_STATUS)191;
			status &= ~WD_STATUS.WDS_INDEX;
			break;
		case RegWD1793.BETA128:
			system = value;
			drive = value & 3;
			side = 1 & ~(value >> 4);
			fdd[drive & 3].HeadSide = side;
			trkcache = fdd[drive & 3].CurrentTrack;
			if ((value & 4) == 0)
			{
				status = WD_STATUS.WDS_NOTRDY;
				rqs = BETA_STATUS.INTRQ;
				fdd[drive].motor = 0uL;
				state = WDSTATE.S_IDLE;
			}
			break;
		default:
			throw new Exception("WD1793.SetReg: Invalid register");
		}
		process(tact);
	}

	public byte GetReg(RegWD1793 reg, ulong tact)
	{
		process(tact);
		switch (reg)
		{
		case RegWD1793.COMMAND:
			rqs &= (BETA_STATUS)127;
			return (byte)status;
		case RegWD1793.TRACK:
			return track;
		case RegWD1793.SECTOR:
			return sector;
		case RegWD1793.DATA:
			status &= ~WD_STATUS.WDS_INDEX;
			rqs &= (BETA_STATUS)191;
			return data;
		case RegWD1793.BETA128:
			return (byte)(rqs | (BETA_STATUS)63);
		default:
			throw new Exception("WD1793.GetReg: Invalid register");
		}
	}

	public string DumpState()
	{
		return "CMD: 0x" + cmd.ToString("X2") + "\nSTATUS: " + status.ToString() + "\nTRK: 0x" + track.ToString("X2") + "\nSEC: 0x" + sector.ToString("X2") + "\nDATA: 0x" + data.ToString("X2") + "\n--------------------------\nbeta: " + rqs.ToString() + "\nsystem: 0x" + system.ToString("X2") + "\nstate: " + state.ToString() + "\nstate2: " + state2.ToString() + "\ndrive: " + drive + "\nside: " + side + "\ntime: " + time + "\nnext: " + next + "\ntshift: " + tshift + "\nrwptr: " + rwptr + "\nrwlen: " + rwlen + "\n--------------------------CYLS: " + fdd[drive & 3].CylynderCount + "\nHEAD: " + fdd[drive & 3].HeadCylynder + "/" + fdd[drive & 3].HeadSide + "\nREADY: " + fdd[drive & 3].IsREADY + "\nTR00: " + fdd[drive & 3].IsTRK00 + "\n--------------------------";
	}

	private void process(ulong toTact)
	{
		time = toTact;
		if (time > fdd[drive].motor && (system & 8u) != 0)
		{
			fdd[drive].motor = 0uL;
		}
		if (fdd[drive].IsREADY)
		{
			status &= ~WD_STATUS.WDS_NOTRDY;
		}
		else
		{
			status |= WD_STATUS.WDS_NOTRDY;
		}
		if ((cmd & 0x80) == 0)
		{
			status &= ~(WD_STATUS.WDS_INDEX | WD_STATUS.WDS_TRK00);
			if (fdd[drive].motor != 0 && (system & 8u) != 0)
			{
				status |= WD_STATUS.WDS_RECORDT;
			}
			if (fdd[drive].IsTRK00)
			{
				status |= WD_STATUS.WDS_TRK00;
			}
			if (fdd[drive].IsREADY && fdd[drive].motor != 0 && (time + tshift) % 700000uL < 14000)
			{
				status |= WD_STATUS.WDS_INDEX;
			}
		}
		while (true)
		{
			switch (state)
			{
			case WDSTATE.S_IDLE:
				status &= ~WD_STATUS.WDS_BUSY;
				rqs = BETA_STATUS.INTRQ;
				return;
			case WDSTATE.S_WAIT:
				if (time < next)
				{
					return;
				}
				state = state2;
				break;
			case WDSTATE.S_DELAY_BEFORE_CMD:
				if (!wd93_nodelay && (cmd & 4u) != 0)
				{
					next += 52500uL;
				}
				status = (status | WD_STATUS.WDS_BUSY) & ~(WD_STATUS.WDS_INDEX | WD_STATUS.WDS_TRK00 | WD_STATUS.WDS_NOTFOUND | WD_STATUS.WDS_RECORDT | WD_STATUS.WDS_WRITEP);
				state2 = WDSTATE.S_CMD_RW;
				state = WDSTATE.S_WAIT;
				break;
			case WDSTATE.S_CMD_RW:
				if (((cmd & 0xE0) == 160 || (cmd & 0xF0) == 240) && fdd[drive].IsWP)
				{
					status |= WD_STATUS.WDS_WRITEP;
					state = WDSTATE.S_IDLE;
				}
				else if ((cmd & 0xC0) == 128 || (cmd & 0xF8) == 192)
				{
					end_waiting_am = next + 3500000;
					find_marker();
				}
				else if ((cmd & 0xF8) == 240)
				{
					rqs = BETA_STATUS.DRQ;
					status |= WD_STATUS.WDS_INDEX;
					next += 3 * trkcache.ByteTime;
					state2 = WDSTATE.S_WRTRACK;
					state = WDSTATE.S_WAIT;
				}
				else if ((cmd & 0xF8) == 224)
				{
					load();
					rwptr = 0;
					rwlen = trkcache.RawLength;
					state2 = WDSTATE.S_READ;
					getindex();
				}
				else
				{
					state = WDSTATE.S_IDLE;
				}
				break;
			case WDSTATE.S_FOUND_NEXT_ID:
			{
				if (!fdd[drive].IsREADY)
				{
					end_waiting_am = next + 3500000;
					find_marker();
					break;
				}
				if (next >= end_waiting_am)
				{
					status |= WD_STATUS.WDS_NOTFOUND;
					state = WDSTATE.S_IDLE;
					break;
				}
				if (foundid == -1)
				{
					status |= WD_STATUS.WDS_NOTFOUND;
					state = WDSTATE.S_IDLE;
					break;
				}
				status &= ~WD_STATUS.WDS_CRCERR;
				load();
				if ((cmd & 0x80) == 0)
				{
					if (((SECHDR)trkcache.HeaderList[foundid]).c != track)
					{
						find_marker();
					}
					else if (!((SECHDR)trkcache.HeaderList[foundid]).c1)
					{
						status |= WD_STATUS.WDS_CRCERR;
						find_marker();
					}
					else
					{
						state = WDSTATE.S_IDLE;
					}
					break;
				}
				if ((cmd & 0xF0) == 192)
				{
					rwptr = ((SECHDR)trkcache.HeaderList[foundid]).idOffset;
					rwlen = 6;
					data = trkcache.RawRead(rwptr++);
					rwlen--;
					rqs = BETA_STATUS.DRQ;
					status |= WD_STATUS.WDS_INDEX;
					next += trkcache.ByteTime;
					state = WDSTATE.S_WAIT;
					state2 = WDSTATE.S_READ;
					break;
				}
				if (((SECHDR)trkcache.HeaderList[foundid]).c != track || ((SECHDR)trkcache.HeaderList[foundid]).n != sector)
				{
					find_marker();
					break;
				}
				if ((cmd & 2u) != 0 && ((uint)((cmd >> 3) ^ ((SECHDR)trkcache.HeaderList[foundid]).s) & (true ? 1u : 0u)) != 0)
				{
					find_marker();
					break;
				}
				if (!((SECHDR)trkcache.HeaderList[foundid]).c1)
				{
					status |= WD_STATUS.WDS_CRCERR;
					find_marker();
					break;
				}
				if ((cmd & 0x20u) != 0)
				{
					rqs = BETA_STATUS.DRQ;
					status |= WD_STATUS.WDS_INDEX;
					next += trkcache.ByteTime * 9;
					state = WDSTATE.S_WAIT;
					state2 = WDSTATE.S_WRSEC;
					break;
				}
				if (((SECHDR)trkcache.HeaderList[foundid]).dataOffset < 0)
				{
					find_marker();
					break;
				}
				if (trkcache.RawRead(((SECHDR)trkcache.HeaderList[foundid]).dataOffset - 1) == 248)
				{
					status |= WD_STATUS.WDS_RECORDT;
				}
				else
				{
					status &= ~WD_STATUS.WDS_RECORDT;
				}
				rwptr = ((SECHDR)trkcache.HeaderList[foundid]).dataOffset;
				rwlen = 128 << (int)((SECHDR)trkcache.HeaderList[foundid]).l;
				ulong num = (ulong)trkcache.RawLength * trkcache.ByteTime;
				int num2 = (int)((next + tshift) % num / trkcache.ByteTime);
				int dataOffset = ((SECHDR)trkcache.HeaderList[foundid]).dataOffset;
				int num3 = ((dataOffset > num2) ? (dataOffset - num2) : (trkcache.RawLength + dataOffset - num2));
				next += (ulong)((long)num3 * (long)trkcache.ByteTime);
				state = WDSTATE.S_WAIT;
				state2 = WDSTATE.S_READ;
				break;
			}
			case WDSTATE.S_READ:
				if (notready())
				{
					break;
				}
				load();
				if (rwlen > 0)
				{
					if ((rqs & BETA_STATUS.DRQ) != 0)
					{
						status |= WD_STATUS.WDS_TRK00;
					}
					data = trkcache.RawRead(rwptr++);
					rwlen--;
					rqs = BETA_STATUS.DRQ;
					status |= WD_STATUS.WDS_INDEX;
					if (!wd93_nodelay)
					{
						next += trkcache.ByteTime;
					}
					state = WDSTATE.S_WAIT;
					state2 = WDSTATE.S_READ;
					break;
				}
				if ((cmd & 0xE0) == 128)
				{
					if (!((SECHDR)trkcache.HeaderList[foundid]).c2)
					{
						status |= WD_STATUS.WDS_CRCERR;
					}
					if ((cmd & 0x10u) != 0)
					{
						sector++;
						state = WDSTATE.S_CMD_RW;
						break;
					}
				}
				if ((cmd & 0xF0) == 192 && !((SECHDR)trkcache.HeaderList[foundid]).c1)
				{
					status |= WD_STATUS.WDS_CRCERR;
				}
				state = WDSTATE.S_IDLE;
				break;
			case WDSTATE.S_WRSEC:
				load();
				if ((rqs & BETA_STATUS.DRQ) != 0)
				{
					status |= WD_STATUS.WDS_TRK00;
					state = WDSTATE.S_IDLE;
					break;
				}
				fdd[drive].ModifyFlag |= ModifyFlag.SectorLevel;
				rwptr = ((SECHDR)trkcache.HeaderList[foundid]).idOffset + 6 + 11 + 11;
				for (rwlen = 0; rwlen < 12; rwlen++)
				{
					trkcache.RawWrite(rwptr++, 0, clock: false);
				}
				for (rwlen = 0; rwlen < 3; rwlen++)
				{
					trkcache.RawWrite(rwptr++, 161, clock: true);
				}
				trkcache.RawWrite(rwptr++, (byte)((((uint)cmd & (true ? 1u : 0u)) != 0) ? 248u : 251u), clock: false);
				rwlen = 128 << (int)((SECHDR)trkcache.HeaderList[foundid]).l;
				state = WDSTATE.S_WRITE;
				break;
			case WDSTATE.S_WRITE:
			{
				if (notready())
				{
					break;
				}
				if ((rqs & BETA_STATUS.DRQ) != 0)
				{
					status |= WD_STATUS.WDS_TRK00;
					data = 0;
				}
				trkcache.RawWrite(rwptr++, data, clock: false);
				rwlen--;
				if (rwptr == trkcache.RawLength)
				{
					rwptr = 0;
				}
				trkcache.sf = true;
				if (rwlen > 0)
				{
					if (!wd93_nodelay)
					{
						next += trkcache.ByteTime;
					}
					state = WDSTATE.S_WAIT;
					state2 = WDSTATE.S_WRITE;
					rqs = BETA_STATUS.DRQ;
					status |= WD_STATUS.WDS_INDEX;
					break;
				}
				int num4 = (128 << (int)((SECHDR)trkcache.HeaderList[foundid]).l) + 1;
				byte[] array2 = new byte[2056];
				if (rwptr < num4)
				{
					for (int i = 0; i < rwptr; i++)
					{
						array2[i] = trkcache.RawRead(trkcache.RawLength - rwptr + i);
					}
					for (int j = 0; j < num4 - rwptr; j++)
					{
						array2[rwptr + j] = trkcache.RawRead(j);
					}
				}
				else
				{
					for (int k = 0; k < num4; k++)
					{
						array2[k] = trkcache.RawRead(rwptr - num4 + k);
					}
				}
				uint num5 = wd93_crc(array2, 0, num4);
				trkcache.RawWrite(rwptr++, (byte)num5, clock: false);
				trkcache.RawWrite(rwptr++, (byte)(num5 >> 8), clock: false);
				trkcache.RawWrite(rwptr, byte.MaxValue, clock: false);
				if ((cmd & 0x10u) != 0)
				{
					sector++;
					state = WDSTATE.S_CMD_RW;
				}
				else
				{
					state = WDSTATE.S_IDLE;
				}
				break;
			}
			case WDSTATE.S_WRTRACK:
				if ((rqs & BETA_STATUS.DRQ) != 0)
				{
					status |= WD_STATUS.WDS_TRK00;
					state = WDSTATE.S_IDLE;
					break;
				}
				fdd[drive].ModifyFlag |= ModifyFlag.TrackLevel;
				state2 = WDSTATE.S_WR_TRACK_DATA;
				getindex();
				end_waiting_am = next + 3500000;
				break;
			case WDSTATE.S_WR_TRACK_DATA:
			{
				if (notready())
				{
					break;
				}
				if ((rqs & BETA_STATUS.DRQ) != 0)
				{
					status |= WD_STATUS.WDS_TRK00;
					data = 0;
				}
				trkcache = fdd[drive & 3].CurrentTrack;
				trkcache.sf = true;
				bool clock = false;
				byte value = data;
				uint num6 = 0u;
				if (data == 245)
				{
					value = 161;
					clock = true;
					start_crc = rwptr + 1;
				}
				if (data == 246)
				{
					value = 194;
					clock = true;
				}
				if (data == 247)
				{
					num6 = trkcache.WD1793_CRC(start_crc, rwptr - start_crc);
					value = (byte)(num6 & 0xFFu);
				}
				trkcache.RawWrite(rwptr++, value, clock);
				rwlen--;
				if (data == 247)
				{
					trkcache.RawWrite(rwptr++, (byte)(num6 >> 8), clock);
					rwlen--;
				}
				if (rwlen > 0)
				{
					if (!wd93_nodelay)
					{
						next += trkcache.ByteTime;
					}
					state2 = WDSTATE.S_WR_TRACK_DATA;
					state = WDSTATE.S_WAIT;
					rqs = BETA_STATUS.DRQ;
					status |= WD_STATUS.WDS_INDEX;
				}
				else
				{
					state = WDSTATE.S_IDLE;
				}
				break;
			}
			case WDSTATE.S_TYPE1_CMD:
				status = (status | WD_STATUS.WDS_BUSY) & ~(WD_STATUS.WDS_INDEX | WD_STATUS.WDS_CRCERR | WD_STATUS.WDS_NOTFOUND | WD_STATUS.WDS_WRITEP);
				rqs = BETA_STATUS.NONE;
				if (fdd[drive].IsWP)
				{
					status |= WD_STATUS.WDS_WRITEP;
				}
				fdd[drive].motor = next + 7000000;
				state2 = WDSTATE.S_SEEKSTART;
				if ((cmd & 0xE0u) != 0)
				{
					if ((cmd & 0x40u) != 0)
					{
						stepdirection = (sbyte)(((cmd & 0x20) == 0) ? 1 : (-1));
					}
					state2 = WDSTATE.S_STEP;
				}
				if (!wd93_nodelay)
				{
					next += 3500uL;
				}
				state = WDSTATE.S_WAIT;
				break;
			case WDSTATE.S_STEP:
			{
				if (fdd[drive].IsTRK00 && (cmd & 0xF0) == 0)
				{
					track = 0;
					state = WDSTATE.S_VERIFY;
					break;
				}
				if ((cmd & 0xE0) == 0 || (cmd & 0x10u) != 0)
				{
					track = (byte)(track + stepdirection);
				}
				fdd[drive].HeadCylynder += stepdirection;
				if (fdd[drive].HeadCylynder >= fdd[drive].CylynderCount - 1)
				{
					fdd[drive].HeadCylynder = fdd[drive].CylynderCount - 1;
				}
				trkcache = fdd[drive & 3].CurrentTrack;
				uint[] array = new uint[4] { 6u, 12u, 20u, 30u };
				if (!wd93_nodelay)
				{
					next += array[cmd & 3] * 3500000 / 1000u;
				}
				state2 = (((cmd & 0xE0u) != 0) ? WDSTATE.S_VERIFY : WDSTATE.S_SEEK);
				state = WDSTATE.S_WAIT;
				break;
			}
			case WDSTATE.S_SEEKSTART:
				if ((cmd & 0x10) == 0)
				{
					track = byte.MaxValue;
					data = 0;
				}
				if (data == track)
				{
					state = WDSTATE.S_VERIFY;
					break;
				}
				stepdirection = ((data >= track) ? 1 : (-1));
				state = WDSTATE.S_STEP;
				break;
			case WDSTATE.S_SEEK:
				if (data == track)
				{
					state = WDSTATE.S_VERIFY;
					break;
				}
				stepdirection = ((data >= track) ? 1 : (-1));
				state = WDSTATE.S_STEP;
				break;
			case WDSTATE.S_VERIFY:
				if ((cmd & 4) == 0)
				{
					state = WDSTATE.S_IDLE;
					break;
				}
				end_waiting_am = next + 4200000;
				load();
				find_marker();
				break;
			case WDSTATE.S_RESET:
				if (fdd[drive].IsTRK00)
				{
					state = WDSTATE.S_IDLE;
				}
				else
				{
					fdd[drive].HeadCylynder--;
					trkcache = fdd[drive & 3].CurrentTrack;
				}
				next += 21000uL;
				break;
			default:
				throw new Exception("WD1793.process - WD1793 in wrong state");
			}
		}
	}

	private void find_marker()
	{
		if (wd93_nodelay && fdd[drive].HeadCylynder != track)
		{
			fdd[drive].HeadCylynder = track;
		}
		load();
		foundid = -1;
		ulong num = 7000000uL;
		if (fdd[drive].motor != 0 && fdd[drive].IsREADY)
		{
			ulong num2 = (ulong)trkcache.RawLength * trkcache.ByteTime;
			int num3 = (int)((next + tshift) % num2 / trkcache.ByteTime);
			num = ulong.MaxValue;
			for (int i = 0; i < trkcache.HeaderList.Count; i++)
			{
				int idOffset = ((SECHDR)trkcache.HeaderList[i]).idOffset;
				int num4 = ((idOffset > num3) ? (idOffset - num3) : (trkcache.RawLength + idOffset - num3));
				if ((ulong)num4 < num)
				{
					num = (ulong)num4;
					foundid = i;
				}
			}
			num = ((foundid == -1) ? 7000000 : (num * trkcache.ByteTime));
			if (wd93_nodelay && foundid != -1)
			{
				int num5 = ((SECHDR)trkcache.HeaderList[foundid]).idOffset + 2;
				tshift = (ulong)((long)num5 * (long)trkcache.ByteTime - (long)(next % num2) + (long)num2) % num2;
				num = 100uL;
			}
		}
		next += num;
		if (fdd[drive].IsREADY && next > end_waiting_am)
		{
			next = end_waiting_am;
			foundid = -1;
		}
		state = WDSTATE.S_WAIT;
		state2 = WDSTATE.S_FOUND_NEXT_ID;
	}

	private bool notready()
	{
		if (!wd93_nodelay || (rqs & BETA_STATUS.DRQ) == 0)
		{
			return false;
		}
		if (next > end_waiting_am)
		{
			return false;
		}
		state2 = state;
		state = WDSTATE.S_WAIT;
		next += trkcache.ByteTime;
		return true;
	}

	private void load()
	{
		if (trkcache.sf)
		{
			trkcache.RefreshHeaders();
		}
		trkcache.sf = false;
		trkcache = fdd[drive & 3].CurrentTrack;
	}

	private void getindex()
	{
		ulong num = (ulong)trkcache.RawLength * trkcache.ByteTime;
		ulong num2 = (next + tshift) % num;
		if (!wd93_nodelay)
		{
			next += num - num2;
		}
		rwptr = 0;
		rwlen = trkcache.RawLength;
		state = WDSTATE.S_WAIT;
	}

	public void Dispose()
	{
		DiskImage[] array = fdd;
		foreach (DiskImage diskImage in array)
		{
			diskImage.Eject();
		}
	}

	public static ushort wd93_crc(byte[] data, int startIndex, int size)
	{
		uint num = 52660u;
		while (size-- > 0)
		{
			num ^= (uint)(data[startIndex++] << 8);
			for (int num2 = 8; num2 != 0; num2--)
			{
				if (((num *= 2) & 0x10000u) != 0)
				{
					num ^= 0x1021u;
				}
			}
		}
		return (ushort)(((num & 0xFF00) >> 8) | ((num & 0xFF) << 8));
	}
}
