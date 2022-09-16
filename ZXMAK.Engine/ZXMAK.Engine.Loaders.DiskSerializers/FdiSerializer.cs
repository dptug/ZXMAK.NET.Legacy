using System.Collections;
using System.IO;
using ZXMAK.Engine.Disk;
using ZXMAK.Platform;

namespace ZXMAK.Engine.Loaders.DiskSerializers;

public class FdiSerializer : FormatSerializer
{
	private class SectorHeader
	{
		public byte C;

		public byte H;

		public byte R;

		public byte N;

		public byte Flags;

		public int DataOffset;

		public byte[] DataArray;

		public bool crcOk;
	}

	private DiskImage _diskImage;

	private bool _writeProtect;

	private string _description = string.Empty;

	private ArrayList _cylynderImages = new ArrayList();

	private int _sideCount;

	public override string FormatGroup => "Disk images";

	public override string FormatName => "FDI disk image";

	public override string FormatExtension => "FDI";

	public override bool CanDeserialize => true;

	public FdiSerializer(DiskImage diskImage)
	{
		_diskImage = diskImage;
	}

	public override void Deserialize(Stream stream)
	{
		if (_diskImage.Present)
		{
			_diskImage.Eject();
		}
		stream.Seek(0L, SeekOrigin.Begin);
		loadData(stream);
		_diskImage.Init(_cylynderImages.Count, _sideCount);
		for (int i = 0; i < _cylynderImages.Count; i++)
		{
			byte[][][] array = (byte[][][])_cylynderImages[i];
			for (int j = 0; j < _diskImage.SideCount; j++)
			{
				_diskImage.GetTrackImage(i, j).AssignImage(array[j][0], array[j][1]);
			}
		}
		_diskImage.Insert();
	}

	private void loadData(Stream stream)
	{
		_cylynderImages.Clear();
		_sideCount = 0;
		if (stream.Length < 14)
		{
			PlatformFactory.Platform.ShowWarning("Corrupted disk image!", "FDI loader");
			return;
		}
		byte[] array = new byte[14];
		stream.Read(array, 0, 14);
		if (array[0] != 70 || array[1] != 68 || array[2] != 73)
		{
			PlatformFactory.Platform.ShowWarning("Invalid FDI file!", "FDI loader");
			return;
		}
		_writeProtect = array[3] != 0;
		int num = array[4] | (array[5] << 8);
		_sideCount = array[6] | (array[7] << 8);
		_ = array[8];
		_ = array[9];
		int num2 = array[10] | (array[11] << 8);
		int num3 = array[12] | (array[13] << 8);
		if (num3 > 0)
		{
			byte[] buffer = new byte[num3];
			stream.Read(buffer, 0, num3);
		}
		ArrayList arrayList = new ArrayList();
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < _sideCount; j++)
			{
				arrayList.Add(readTrackHeader(stream));
			}
		}
		for (int k = 0; k < num; k++)
		{
			byte[][][] array2 = new byte[_sideCount][][];
			_cylynderImages.Add(array2);
			for (int l = 0; l < _sideCount; l++)
			{
				ArrayList arrayList2 = (ArrayList)arrayList[k * _sideCount + l];
				for (int m = 0; m < arrayList2.Count; m++)
				{
					SectorHeader sectorHeader = (SectorHeader)arrayList2[m];
					if ((sectorHeader.Flags & 0x40) == 0)
					{
						int num4 = 128 << (int)sectorHeader.N;
						sectorHeader.crcOk = (sectorHeader.Flags & 0x1F) != 0;
						sectorHeader.DataArray = new byte[num4];
						stream.Seek(num2 + sectorHeader.DataOffset, SeekOrigin.Begin);
						stream.Read(sectorHeader.DataArray, 0, num4);
					}
				}
				array2[l] = generateTrackImage(arrayList2);
			}
		}
	}

	private byte[][] generateTrackImage(ArrayList sectorHeaderList)
	{
		byte[][] array = new byte[2][];
		int num = 6250;
		int count = sectorHeaderList.Count;
		int num2 = 0;
		for (int i = 0; i < count; i++)
		{
			SectorHeader sectorHeader = (SectorHeader)sectorHeaderList[i];
			num2 += 8;
			int num3 = 128 << (int)sectorHeader.N;
			if ((sectorHeader.Flags & 0x40u) != 0)
			{
				num3 = 0;
			}
			else
			{
				num2 += 4;
			}
			num2 += num3;
		}
		int num4 = num - (num2 + count * 5);
		int num5 = 1;
		int num6 = 1;
		int num7 = 1;
		int num8 = 1;
		int num9 = 1;
		num4 -= num6 + num7 + num8 + num9;
		if (num4 < 0)
		{
			num += -num4;
			num4 = 0;
		}
		while (num4 > 0)
		{
			if (num4 >= count * 2 && num9 < 12)
			{
				num9++;
				num4 -= count * 2;
			}
			if (num4 < count)
			{
				break;
			}
			if (num6 < 10)
			{
				num6++;
				num4 -= count;
			}
			if (num4 < count)
			{
				break;
			}
			if (num7 < 22)
			{
				num7++;
				num4 -= count;
			}
			if (num4 < count)
			{
				break;
			}
			if (num8 < 60)
			{
				num8++;
				num4 -= count;
			}
			if (num4 < count || (num9 >= 12 && num6 >= 10 && num7 >= 22 && num8 >= 60))
			{
				break;
			}
		}
		if (num4 > count * 2 + 10)
		{
			num5++;
			num4 -= count;
		}
		if (num4 > count * 2 + 9)
		{
			num5++;
		}
		if (num4 < 0)
		{
			num += -num4;
			num4 = 0;
		}
		array[0] = new byte[num];
		array[1] = new byte[array[0].Length / 8 + ((((uint)array[0].Length & 7u) != 0) ? 1 : 0)];
		int num10 = 0;
		for (int j = 0; j < count; j++)
		{
			SectorHeader sectorHeader2 = (SectorHeader)sectorHeaderList[j];
			for (int k = 0; k < num6; k++)
			{
				array[0][num10] = 78;
				array[1][num10 / 8] &= (byte)(~(1 << (num10 & 7)));
				num10++;
			}
			for (int k = 0; k < num9; k++)
			{
				array[0][num10] = 0;
				array[1][num10 / 8] &= (byte)(~(1 << (num10 & 7)));
				num10++;
			}
			int num11 = num10;
			for (int k = 0; k < num5; k++)
			{
				array[0][num10] = 161;
				array[1][num10 / 8] |= (byte)(1 << (num10 & 7));
				num10++;
			}
			array[0][num10] = 254;
			array[1][num10 / 8] &= (byte)(~(1 << (num10 & 7)));
			num10++;
			array[0][num10] = sectorHeader2.C;
			array[1][num10 / 8] &= (byte)(~(1 << (num10 & 7)));
			num10++;
			array[0][num10] = sectorHeader2.H;
			array[1][num10 / 8] &= (byte)(~(1 << (num10 & 7)));
			num10++;
			array[0][num10] = sectorHeader2.R;
			array[1][num10 / 8] &= (byte)(~(1 << (num10 & 7)));
			num10++;
			array[0][num10] = sectorHeader2.N;
			array[1][num10 / 8] &= (byte)(~(1 << (num10 & 7)));
			num10++;
			ushort num12 = WD1793_CRC(array[0], num11, num10 - num11);
			array[0][num10] = (byte)num12;
			array[1][num10 / 8] &= (byte)(~(1 << (num10 & 7)));
			num10++;
			array[0][num10] = (byte)(num12 >> 8);
			array[1][num10 / 8] &= (byte)(~(1 << (num10 & 7)));
			num10++;
			for (int k = 0; k < num7; k++)
			{
				array[0][num10] = 78;
				array[1][num10 / 8] &= (byte)(~(1 << (num10 & 7)));
				num10++;
			}
			for (int k = 0; k < num9; k++)
			{
				array[0][num10] = 0;
				array[1][num10 / 8] &= (byte)(~(1 << (num10 & 7)));
				num10++;
			}
			byte flags = sectorHeader2.Flags;
			if ((flags & 0x40) == 0)
			{
				num11 = num10;
				for (int k = 0; k < num5; k++)
				{
					array[0][num10] = 161;
					array[1][num10 / 8] |= (byte)(1 << (num10 & 7));
					num10++;
				}
				if ((flags & 0x80u) != 0)
				{
					array[0][num10] = 248;
				}
				else
				{
					array[0][num10] = 251;
				}
				array[1][num10 / 8] &= (byte)(~(1 << (num10 & 7)));
				num10++;
				int num13 = 128 << (int)sectorHeader2.N;
				for (int k = 0; k < num13; k++)
				{
					array[0][num10] = sectorHeader2.DataArray[k];
					array[1][num10 / 8] &= (byte)(~(1 << (num10 & 7)));
					num10++;
				}
				num12 = WD1793_CRC(array[0], num11, num10 - num11);
				if ((flags & 0x3F) == 0)
				{
					num12 = (ushort)(num12 ^ 0xFFFFu);
				}
				array[0][num10] = (byte)num12;
				array[1][num10 / 8] &= (byte)(~(1 << (num10 & 7)));
				num10++;
				array[0][num10] = (byte)(num12 >> 8);
				array[1][num10 / 8] &= (byte)(~(1 << (num10 & 7)));
				num10++;
			}
			for (int k = 0; k < num8; k++)
			{
				array[0][num10] = 78;
				array[1][num10 / 8] &= (byte)(~(1 << (num10 & 7)));
				num10++;
			}
		}
		for (int l = num10; l < array[0].Length; l++)
		{
			array[0][num10] = 78;
			array[1][num10 / 8] &= (byte)(~(1 << (num10 & 7)));
			num10++;
		}
		return array;
	}

	private ArrayList readTrackHeader(Stream f)
	{
		byte[] array = new byte[7];
		ArrayList arrayList = new ArrayList();
		f.Read(array, 0, 4);
		int num = array[0] | (array[1] << 8) | (array[2] << 16) | (array[3] << 24);
		f.Read(array, 0, 2);
		f.Read(array, 0, 1);
		int num2 = array[0];
		for (int i = 0; i < num2; i++)
		{
			f.Read(array, 0, 7);
			SectorHeader sectorHeader = new SectorHeader();
			sectorHeader.C = array[0];
			sectorHeader.H = array[1];
			sectorHeader.R = array[2];
			sectorHeader.N = array[3];
			sectorHeader.Flags = array[4];
			sectorHeader.DataOffset = num + (array[5] | (array[6] << 8));
			arrayList.Add(sectorHeader);
		}
		return arrayList;
	}

	private static ushort WD1793_CRC(byte[] data, int startIndex, int size)
	{
		ushort num = ushort.MaxValue;
		while (size-- > 0)
		{
			num = (ushort)(num ^ (ushort)(data[startIndex++] << 8));
			for (int i = 0; i < 8; i++)
			{
				num = (((num & 0x8000) == 0) ? ((ushort)(num << 1)) : ((ushort)((uint)(num << 1) ^ 0x1021u)));
			}
		}
		return (ushort)((num >> 8) | (num << 8));
	}
}
