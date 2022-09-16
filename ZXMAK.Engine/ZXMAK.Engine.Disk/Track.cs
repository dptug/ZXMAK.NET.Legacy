using System;
using System.Collections;

namespace ZXMAK.Engine.Disk;

public class Track
{
	private ulong _trackTime;

	private ulong _byteTime;

	private byte[] _trackImage;

	private byte[] _trackClock;

	private ArrayList _headerList = new ArrayList();

	public bool sf;

	public int RawLength
	{
		get
		{
			if (_trackImage == null)
			{
				return 6400;
			}
			return _trackImage.Length;
		}
	}

	public byte[][] RawImage => new byte[2][] { _trackImage, _trackClock };

	public ulong ByteTime => _byteTime;

	public ArrayList HeaderList => _headerList;

	public Track(ulong trackTime)
	{
		_trackTime = trackTime;
		_byteTime = trackTime / 6400uL;
		if (_byteTime < 1)
		{
			_byteTime = 1uL;
		}
	}

	public void RefreshHeaders()
	{
		_headerList.Clear();
		if (_trackImage == null)
		{
			return;
		}
		for (int i = 0; i < _trackImage.Length - 8; i++)
		{
			if (_trackImage[i] != 161 || _trackImage[i + 1] != 254 || !RawTestClock(i))
			{
				continue;
			}
			SECHDR sECHDR = new SECHDR();
			_headerList.Add(sECHDR);
			sECHDR.idOffset = i + 2;
			sECHDR.idTime = (ulong)sECHDR.idOffset * _byteTime;
			sECHDR.c = _trackImage[sECHDR.idOffset];
			sECHDR.s = _trackImage[sECHDR.idOffset + 1];
			sECHDR.n = _trackImage[sECHDR.idOffset + 2];
			sECHDR.l = _trackImage[sECHDR.idOffset + 3];
			sECHDR.crc1 = (ushort)(_trackImage[i + 6] | (_trackImage[i + 7] << 8));
			sECHDR.c1 = WD1793_CRC(i + 1, 5) == sECHDR.crc1;
			sECHDR.dataOffset = -1;
			sECHDR.datlen = 0;
			if (sECHDR.l > 5)
			{
				continue;
			}
			int num = _trackImage.Length - 8;
			for (int j = i + 8; j < num; j++)
			{
				if (_trackImage[j] != 161 || !RawTestClock(j) || RawTestClock(j + 1))
				{
					continue;
				}
				if (_trackImage[j + 1] == 248 || _trackImage[j + 1] == 251)
				{
					sECHDR.datlen = 128 << (int)sECHDR.l;
					sECHDR.dataOffset = j + 2;
					sECHDR.dataTime = (ulong)sECHDR.dataOffset * _byteTime;
					if (sECHDR.dataOffset + sECHDR.datlen + 2 > _trackImage.Length)
					{
						sECHDR.datlen = _trackImage.Length - sECHDR.dataOffset;
						sECHDR.crc2 = (ushort)(WD1793_CRC(sECHDR.dataOffset - 1, sECHDR.datlen + 1) ^ 0xFFFFu);
						sECHDR.c2 = false;
					}
					else
					{
						sECHDR.crc2 = (ushort)(_trackImage[sECHDR.dataOffset + sECHDR.datlen] | (_trackImage[sECHDR.dataOffset + sECHDR.datlen + 1] << 8));
						sECHDR.c2 = WD1793_CRC(sECHDR.dataOffset - 1, sECHDR.datlen + 1) == sECHDR.crc2;
					}
				}
				break;
			}
		}
	}

	public void AssignImage(byte[] trackImage, byte[] trackClock)
	{
		if (trackImage.Length <= 0 || trackImage.Length / 8 + ((((uint)trackImage.Length & 7u) != 0) ? 1 : 0) != trackClock.Length)
		{
			throw new InvalidOperationException("Invalid track image length!");
		}
		_trackImage = trackImage;
		_trackClock = trackClock;
		_byteTime = _trackTime / (ulong)trackImage.Length;
		if (_byteTime < 1)
		{
			_byteTime = 1uL;
		}
		RefreshHeaders();
	}

	public void AssignSectors(ArrayList sectorList)
	{
		byte[][] array = new byte[2][];
		int num = 6250;
		int count = sectorList.Count;
		int num2 = 0;
		foreach (Sector sector3 in sectorList)
		{
			num2 += sector3.GetAdBlockSize();
			num2 += sector3.GetDataBlockSize();
		}
		int num3 = num - (num2 + count * 5);
		int num4 = 1;
		int num5 = 1;
		int num6 = 1;
		int num7 = 1;
		num3 -= num4 + num5 + num6 + num7;
		if (num3 < 0)
		{
			num += -num3;
			num3 = 0;
		}
		while (num3 > 0)
		{
			if (num3 >= count * 2 && num7 < 12)
			{
				num7++;
				num3 -= count * 2;
			}
			if (num3 < count)
			{
				break;
			}
			if (num4 < 10)
			{
				num4++;
				num3 -= count;
			}
			if (num3 < count)
			{
				break;
			}
			if (num5 < 22)
			{
				num5++;
				num3 -= count;
			}
			if (num3 < count)
			{
				break;
			}
			if (num6 < 60)
			{
				num6++;
				num3 -= count;
			}
			if (num3 < count || (num7 >= 12 && num4 >= 10 && num5 >= 22 && num6 >= 60))
			{
				break;
			}
		}
		if (num3 < 0)
		{
			num += -num3;
			num3 = 0;
		}
		array[0] = new byte[num];
		array[1] = new byte[array[0].Length / 8 + ((((uint)array[0].Length & 7u) != 0) ? 1 : 0)];
		int num8 = 0;
		foreach (Sector sector4 in sectorList)
		{
			for (int i = 0; i < num4; i++)
			{
				array[0][num8] = 78;
				array[1][num8 / 8] &= (byte)(~(1 << (num8 & 7)));
				num8++;
			}
			for (int i = 0; i < num7; i++)
			{
				array[0][num8] = 0;
				array[1][num8 / 8] &= (byte)(~(1 << (num8 & 7)));
				num8++;
			}
			if (sector4.AdPresent)
			{
				byte[][] array2 = sector4.CreateAdBlock();
				for (int i = 0; i < sector4.GetAdBlockSize(); i++)
				{
					array[0][num8] = array2[0][i];
					if ((array2[1][i / 8] & (1 << (i & 7))) != 0)
					{
						array[1][num8 / 8] |= (byte)(1 << (num8 & 7));
					}
					else
					{
						array[1][num8 / 8] &= (byte)(~(1 << (num8 & 7)));
					}
					num8++;
				}
			}
			for (int i = 0; i < num5; i++)
			{
				array[0][num8] = 78;
				array[1][num8 / 8] &= (byte)(~(1 << (num8 & 7)));
				num8++;
			}
			for (int i = 0; i < num7; i++)
			{
				array[0][num8] = 0;
				array[1][num8 / 8] &= (byte)(~(1 << (num8 & 7)));
				num8++;
			}
			if (sector4.DataPresent)
			{
				byte[][] array3 = sector4.CreateDataBlock();
				for (int i = 0; i < sector4.GetDataBlockSize(); i++)
				{
					array[0][num8] = array3[0][i];
					if ((array3[1][i / 8] & (1 << (i & 7))) != 0)
					{
						array[1][num8 / 8] |= (byte)(1 << (num8 & 7));
					}
					else
					{
						array[1][num8 / 8] &= (byte)(~(1 << (num8 & 7)));
					}
					num8++;
				}
			}
			for (int i = 0; i < num6; i++)
			{
				array[0][num8] = 78;
				array[1][num8 / 8] &= (byte)(~(1 << (num8 & 7)));
				num8++;
			}
		}
		for (int j = num8; j < array[0].Length; j++)
		{
			array[0][num8] = 78;
			array[1][num8 / 8] &= (byte)(~(1 << (num8 & 7)));
			num8++;
		}
		AssignImage(array[0], array[1]);
	}

	public bool RawTestClock(int pos)
	{
		if (_trackClock == null)
		{
			return false;
		}
		return (_trackClock[pos / 8] & (1 << (pos & 7))) != 0;
	}

	public void RawWrite(int pos, byte value, bool clock)
	{
		if (_trackImage != null)
		{
			_trackImage[pos] = value;
			if (clock)
			{
				_trackClock[pos / 8] |= (byte)(1 << (pos & 7));
			}
			else
			{
				_trackClock[pos / 8] &= (byte)(~(1 << (pos & 7)));
			}
		}
	}

	public byte RawRead(int pos)
	{
		if (_trackImage == null)
		{
			return 0;
		}
		return _trackImage[pos];
	}

	public ushort WD1793_CRC(int startIndex, int size)
	{
		if (_trackImage == null)
		{
			return 0;
		}
		uint num = 52660u;
		while (size-- > 0)
		{
			num ^= (uint)(_trackImage[startIndex++] << 8);
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
