using System;
using System.Collections;

namespace ZXMAK.Engine.Disk
{
	public class DiskImage
	{
		public DiskImage(ulong rotateTime)
		{
			this._rotateTime = rotateTime;
			this._indexTime = rotateTime / 50UL;
			this._nullTrack = new Track(rotateTime);
		}

		public bool Present
		{
			get
			{
				return this._present;
			}
		}

		public event SaveDiskDelegate SaveDisk;

		protected void OnSaveDisk()
		{
			if (this.SaveDisk != null)
			{
				this.SaveDisk(this);
			}
		}

		public string FileName
		{
			get
			{
				return this._fileName;
			}
			set
			{
				this._fileName = value;
			}
		}

		public int CylynderCount
		{
			get
			{
				return this._cylynderList.Count;
			}
		}

		public int SideCount
		{
			get
			{
				return this._sideCount;
			}
		}

		public int HeadSide
		{
			get
			{
				return this._headSide;
			}
			set
			{
				this._headSide = (value & 1);
			}
		}

		public int HeadCylynder
		{
			get
			{
				return this._headCylynder;
			}
			set
			{
				if (value >= this._cylynderList.Count)
				{
					value = this._cylynderList.Count - 1;
				}
				if (value < 0)
				{
					value = 0;
				}
				this._headCylynder = value;
			}
		}

		public Track CurrentTrack
		{
			get
			{
				if (this._headCylynder >= this._cylynderList.Count || this._headSide >= this._sideCount)
				{
					return this._nullTrack;
				}
				return ((Track[])this._cylynderList[this._headCylynder])[this._headSide];
			}
		}

		public void Init(int cylynderCount, int sideCount)
		{
			this._cylynderList.Clear();
			this._sideCount = sideCount;
			for (int i = 0; i < cylynderCount; i++)
			{
				Track[] array = new Track[this._sideCount];
				for (int j = 0; j < array.Length; j++)
				{
					array[j] = new Track(this._rotateTime);
					byte[] array2 = new byte[6400];
					byte[] trackClock = new byte[array2.Length / 8 + (((array2.Length & 7) != 0) ? 1 : 0)];
					array[j].AssignImage(array2, trackClock);
				}
				this._cylynderList.Add(array);
			}
			this._modifyFlag = ModifyFlag.None;
		}

		public void InitFormated()
		{
			this.Init(80, 2);
			this.format_trdos();
		}

		public void Eject()
		{
			if (this._modifyFlag != ModifyFlag.None)
			{
				this.OnSaveDisk();
			}
			this._cylynderList.Clear();
			this._fileName = "";
			this._writeProtect = false;
			this._present = false;
		}

		public void Insert()
		{
			if (this._cylynderList.Count > 0)
			{
				this._present = true;
			}
		}

		public bool IsREADY
		{
			get
			{
				return this._cylynderList.Count > 0;
			}
		}

		public bool IsTRK00
		{
			get
			{
				return this._headCylynder == 0;
			}
		}

		public bool IsINDEX(ulong time)
		{
			return time % this._rotateTime < this._indexTime;
		}

		public bool IsWP
		{
			get
			{
				return this._writeProtect;
			}
			set
			{
				this._writeProtect = value;
			}
		}

		public ModifyFlag ModifyFlag
		{
			get
			{
				return this._modifyFlag;
			}
			set
			{
				this._modifyFlag = value;
			}
		}

		public void format_trdos()
		{
			for (int i = 0; i < this._cylynderList.Count; i++)
			{
				Track[] array = (Track[])this._cylynderList[i];
				for (int j = 0; j < array.Length; j++)
				{
					ArrayList arrayList = new ArrayList();
					for (int k = 0; k < 16; k++)
					{
						SimpleSector simpleSector = new SimpleSector(i, 0, (int)this.il[k], 1, new byte[256]);
						for (int l = 0; l < 256; l++)
						{
							simpleSector.Data[l] = 0;
						}
						simpleSector.SetAdCrc(true);
						simpleSector.SetDataCrc(true);
						arrayList.Add(simpleSector);
					}
					array[j].AssignSectors(arrayList);
				}
			}
			byte[] array2 = new byte[256];
			for (int m = 0; m < array2.Length; m++)
			{
				array2[m] = 0;
			}
			array2[226] = 1;
			array2[227] = 22;
			array2[229] = 240;
			array2[230] = 9;
			array2[231] = 16;
			for (int n = 234; n <= 242; n++)
			{
				array2[n] = 32;
			}
			array2[245] = 90;
			array2[246] = 88;
			array2[247] = 77;
			array2[248] = 65;
			array2[249] = 75;
			array2[250] = 50;
			array2[251] = 32;
			array2[252] = 32;
			this.writeLogicalSector(0, 0, 9, array2);
			this._modifyFlag = ModifyFlag.None;
		}

		public void writeLogicalSector(int cyl, int side, int sec, byte[] buffer)
		{
			if (cyl < 0 || cyl >= this._cylynderList.Count)
			{
				return;
			}
			Track[] array = (Track[])this._cylynderList[cyl];
			if (side < 0 || side >= this._sideCount)
			{
				return;
			}
			Track track = array[side];
			for (int i = 0; i < track.HeaderList.Count; i++)
			{
				SECHDR sechdr = (SECHDR)track.HeaderList[i];
				if ((int)sechdr.n == sec && (int)sechdr.c == cyl && sechdr.dataOffset > 0 && sechdr.datlen >= buffer.Length)
				{
					int num = buffer.Length;
					if (num > sechdr.datlen)
					{
						num = sechdr.datlen;
					}
					for (int j = 0; j < num; j++)
					{
						track.RawWrite(sechdr.dataOffset + j, buffer[j], false);
					}
					ushort num2 = track.WD1793_CRC(sechdr.dataOffset - 1, sechdr.datlen + 1);
					track.RawWrite(sechdr.dataOffset + sechdr.datlen, (byte)num2, false);
					track.RawWrite(sechdr.dataOffset + sechdr.datlen + 1, (byte)(num2 >> 8), false);
					sechdr.crc2 = num2;
					sechdr.c2 = true;
					return;
				}
			}
		}

		public void readLogicalSector(int cyl, int side, int sec, byte[] buffer)
		{
			for (int i = 0; i < buffer.Length; i++)
			{
				buffer[i] = 0;
			}
			if (cyl < 0 || cyl >= this._cylynderList.Count)
			{
				return;
			}
			Track[] array = (Track[])this._cylynderList[cyl];
			if (side < 0 || side >= this._sideCount)
			{
				return;
			}
			Track track = array[side];
			for (int j = 0; j < track.HeaderList.Count; j++)
			{
				SECHDR sechdr = (SECHDR)track.HeaderList[j];
				if ((int)sechdr.n == sec && (int)sechdr.c == cyl && sechdr.dataOffset > 0)
				{
					int num = sechdr.datlen;
					if (num > buffer.Length)
					{
						num = buffer.Length;
					}
					for (int k = 0; k < num; k++)
					{
						buffer[k] = track.RawRead(sechdr.dataOffset + k);
					}
					return;
				}
			}
		}

		public Track GetTrackImage(int cyl, int side)
		{
			return ((Track[])this._cylynderList[cyl])[side];
		}

		private bool _present;

		private ulong _rotateTime;

		private ulong _indexTime;

		private ArrayList _cylynderList = new ArrayList();

		private int _headSide;

		private int _headCylynder;

		private int _sideCount;

		private bool _writeProtect;

		private Track _nullTrack;

		private ModifyFlag _modifyFlag;

		private string _fileName = string.Empty;

		public string Description = string.Empty;

		public ulong motor;

		private byte[] il = new byte[]
		{
			1,
			2,
			3,
			4,
			5,
			6,
			7,
			8,
			9,
			10,
			11,
			12,
			13,
			14,
			15,
			16
		};
	}
}
