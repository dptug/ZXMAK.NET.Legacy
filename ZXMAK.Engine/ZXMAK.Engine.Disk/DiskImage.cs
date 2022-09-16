using System.Collections;

namespace ZXMAK.Engine.Disk;

public class DiskImage
{
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

	private byte[] il = new byte[16]
	{
		1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
		11, 12, 13, 14, 15, 16
	};

	public bool Present => _present;

	public string FileName
	{
		get
		{
			return _fileName;
		}
		set
		{
			_fileName = value;
		}
	}

	public int CylynderCount => _cylynderList.Count;

	public int SideCount => _sideCount;

	public int HeadSide
	{
		get
		{
			return _headSide;
		}
		set
		{
			_headSide = value & 1;
		}
	}

	public int HeadCylynder
	{
		get
		{
			return _headCylynder;
		}
		set
		{
			if (value >= _cylynderList.Count)
			{
				value = _cylynderList.Count - 1;
			}
			if (value < 0)
			{
				value = 0;
			}
			_headCylynder = value;
		}
	}

	public Track CurrentTrack
	{
		get
		{
			if (_headCylynder >= _cylynderList.Count || _headSide >= _sideCount)
			{
				return _nullTrack;
			}
			return ((Track[])_cylynderList[_headCylynder])[_headSide];
		}
	}

	public bool IsREADY => _cylynderList.Count > 0;

	public bool IsTRK00 => _headCylynder == 0;

	public bool IsWP
	{
		get
		{
			return _writeProtect;
		}
		set
		{
			_writeProtect = value;
		}
	}

	public ModifyFlag ModifyFlag
	{
		get
		{
			return _modifyFlag;
		}
		set
		{
			_modifyFlag = value;
		}
	}

	public event SaveDiskDelegate SaveDisk;

	public DiskImage(ulong rotateTime)
	{
		_rotateTime = rotateTime;
		_indexTime = rotateTime / 50uL;
		_nullTrack = new Track(rotateTime);
	}

	protected void OnSaveDisk()
	{
		if (this.SaveDisk != null)
		{
			this.SaveDisk(this);
		}
	}

	public void Init(int cylynderCount, int sideCount)
	{
		_cylynderList.Clear();
		_sideCount = sideCount;
		for (int i = 0; i < cylynderCount; i++)
		{
			Track[] array = new Track[_sideCount];
			for (int j = 0; j < array.Length; j++)
			{
				array[j] = new Track(_rotateTime);
				byte[] array2 = new byte[6400];
				byte[] trackClock = new byte[array2.Length / 8 + ((((uint)array2.Length & 7u) != 0) ? 1 : 0)];
				array[j].AssignImage(array2, trackClock);
			}
			_cylynderList.Add(array);
		}
		_modifyFlag = ModifyFlag.None;
	}

	public void InitFormated()
	{
		Init(80, 2);
		format_trdos();
	}

	public void Eject()
	{
		if (_modifyFlag != 0)
		{
			OnSaveDisk();
		}
		_cylynderList.Clear();
		_fileName = "";
		_writeProtect = false;
		_present = false;
	}

	public void Insert()
	{
		if (_cylynderList.Count > 0)
		{
			_present = true;
		}
	}

	public bool IsINDEX(ulong time)
	{
		return time % _rotateTime < _indexTime;
	}

	public void format_trdos()
	{
		for (int i = 0; i < _cylynderList.Count; i++)
		{
			Track[] array = (Track[])_cylynderList[i];
			for (int j = 0; j < array.Length; j++)
			{
				ArrayList arrayList = new ArrayList();
				for (int k = 0; k < 16; k++)
				{
					SimpleSector simpleSector = new SimpleSector(i, 0, il[k], 1, new byte[256]);
					for (int l = 0; l < 256; l++)
					{
						simpleSector.Data[l] = 0;
					}
					simpleSector.SetAdCrc(valid: true);
					simpleSector.SetDataCrc(valid: true);
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
		writeLogicalSector(0, 0, 9, array2);
		_modifyFlag = ModifyFlag.None;
	}

	public void writeLogicalSector(int cyl, int side, int sec, byte[] buffer)
	{
		if (cyl < 0 || cyl >= _cylynderList.Count)
		{
			return;
		}
		Track[] array = (Track[])_cylynderList[cyl];
		if (side < 0 || side >= _sideCount)
		{
			return;
		}
		Track track = array[side];
		for (int i = 0; i < track.HeaderList.Count; i++)
		{
			SECHDR sECHDR = (SECHDR)track.HeaderList[i];
			if (sECHDR.n == sec && sECHDR.c == cyl && sECHDR.dataOffset > 0 && sECHDR.datlen >= buffer.Length)
			{
				int num = buffer.Length;
				if (num > sECHDR.datlen)
				{
					num = sECHDR.datlen;
				}
				for (int j = 0; j < num; j++)
				{
					track.RawWrite(sECHDR.dataOffset + j, buffer[j], clock: false);
				}
				ushort num2 = track.WD1793_CRC(sECHDR.dataOffset - 1, sECHDR.datlen + 1);
				track.RawWrite(sECHDR.dataOffset + sECHDR.datlen, (byte)num2, clock: false);
				track.RawWrite(sECHDR.dataOffset + sECHDR.datlen + 1, (byte)(num2 >> 8), clock: false);
				sECHDR.crc2 = num2;
				sECHDR.c2 = true;
				break;
			}
		}
	}

	public void readLogicalSector(int cyl, int side, int sec, byte[] buffer)
	{
		for (int i = 0; i < buffer.Length; i++)
		{
			buffer[i] = 0;
		}
		if (cyl < 0 || cyl >= _cylynderList.Count)
		{
			return;
		}
		Track[] array = (Track[])_cylynderList[cyl];
		if (side < 0 || side >= _sideCount)
		{
			return;
		}
		Track track = array[side];
		for (int j = 0; j < track.HeaderList.Count; j++)
		{
			SECHDR sECHDR = (SECHDR)track.HeaderList[j];
			if (sECHDR.n == sec && sECHDR.c == cyl && sECHDR.dataOffset > 0)
			{
				int num = sECHDR.datlen;
				if (num > buffer.Length)
				{
					num = buffer.Length;
				}
				for (int k = 0; k < num; k++)
				{
					buffer[k] = track.RawRead(sECHDR.dataOffset + k);
				}
				break;
			}
		}
	}

	public Track GetTrackImage(int cyl, int side)
	{
		return ((Track[])_cylynderList[cyl])[side];
	}
}
