using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ZXMAK.Engine.Disk;
using ZXMAK.Logging;
using ZXMAK.Platform;

namespace ZXMAK.Engine.Loaders.DiskSerializers;

public class Td0Serializer : FormatSerializer
{
	private class TD0_MAIN_HEADER
	{
		private byte[] _buffer = new byte[12];

		public byte Ver
		{
			get
			{
				return _buffer[4];
			}
			set
			{
				_buffer[4] = value;
			}
		}

		public byte DiskType
		{
			get
			{
				return _buffer[6];
			}
			set
			{
				_buffer[6] = value;
			}
		}

		public byte Info
		{
			get
			{
				return _buffer[7];
			}
			set
			{
				_buffer[7] = value;
			}
		}

		public byte DataDOS
		{
			get
			{
				return _buffer[8];
			}
			set
			{
				_buffer[8] = value;
			}
		}

		public byte ChkdSides
		{
			get
			{
				return _buffer[9];
			}
			set
			{
				_buffer[9] = value;
			}
		}

		public bool IsAdvandcedCompression => FormatSerializer.getUInt16(_buffer, 0) == 25716;

		public TD0_MAIN_HEADER()
		{
			for (int i = 0; i < _buffer.Length; i++)
			{
				_buffer[i] = 0;
			}
		}

		public static TD0_MAIN_HEADER Deserialize(Stream stream)
		{
			TD0_MAIN_HEADER tD0_MAIN_HEADER = new TD0_MAIN_HEADER();
			stream.Read(tD0_MAIN_HEADER._buffer, 0, tD0_MAIN_HEADER._buffer.Length);
			ushort uInt = FormatSerializer.getUInt16(tD0_MAIN_HEADER._buffer, 0);
			if (uInt != 17492 && uInt != 25716)
			{
				Logger.GetLogger().LogError("TD0 loader: Invalid header ID");
				PlatformFactory.Platform.ShowWarning("Invalid header ID", "TD0 loader");
				return null;
			}
			ushort num = CalculateTD0CRC(tD0_MAIN_HEADER._buffer, 0, tD0_MAIN_HEADER._buffer.Length - 2);
			ushort uInt2 = FormatSerializer.getUInt16(tD0_MAIN_HEADER._buffer, 10);
			if (uInt2 != num)
			{
				Logger.GetLogger().LogWarning("TD0 loader: Main header had bad CRC=0x" + num.ToString("X4") + " (stamp crc=0x" + uInt2.ToString("X4") + ")");
				PlatformFactory.Platform.ShowWarning("Wrong main header CRC", "TD0 loader");
			}
			return tD0_MAIN_HEADER;
		}

		public void Serialize(Stream stream)
		{
			stream.Write(_buffer, 0, _buffer.Length);
		}
	}

	private class TD0_TRACK
	{
		private byte[] _rawData = new byte[4];

		private ArrayList _sectorList = new ArrayList();

		public int SectorCount => _rawData[0];

		public int Cylinder => _rawData[1];

		public int Side => _rawData[2];

		public ArrayList SectorList => _sectorList;

		public static TD0_TRACK Deserialize(Stream stream)
		{
			TD0_TRACK tD0_TRACK = new TD0_TRACK();
			stream.Read(tD0_TRACK._rawData, 0, 4);
			if (tD0_TRACK._rawData[0] != byte.MaxValue)
			{
				ushort num = CalculateTD0CRC(tD0_TRACK._rawData, 0, 3);
				if (tD0_TRACK._rawData[3] != (num & 0xFF))
				{
					Logger.GetLogger().LogWarning("TD0 loader: Track header had bad CRC=0x" + num.ToString("X4") + " (stamp crc=0x" + tD0_TRACK._rawData[3].ToString("X2") + ") [CYL:0x" + tD0_TRACK._rawData[1].ToString("X2") + ";SIDE:" + tD0_TRACK._rawData[2].ToString("X2"));
					PlatformFactory.Platform.ShowWarning("Track header had bad CRC", "TD0 loader");
				}
				new ArrayList(tD0_TRACK.SectorCount);
				for (int i = 0; i < tD0_TRACK.SectorCount; i++)
				{
					tD0_TRACK._sectorList.Add(TD0_SECTOR.Deserialize(stream));
				}
			}
			return tD0_TRACK;
		}
	}

	[Flags]
	private enum SectorFlags
	{
		DuplicatedWithinTrack = 1,
		BadCrc = 2,
		DeletedData = 4,
		NoDataBlockDOS = 0x10,
		NoDataBlock = 0x20,
		NoAddressBlock = 0x40
	}

	private class TD0_SECTOR : Sector
	{
		private byte[] _admark;

		private byte[] _data;

		public override bool AdPresent => (Td0Flags & SectorFlags.NoAddressBlock) == 0;

		public override bool DataPresent => (Td0Flags & (SectorFlags.NoDataBlockDOS | SectorFlags.NoDataBlock)) == 0;

		public override bool DataDeleteMark => (Td0Flags & SectorFlags.DeletedData) != 0;

		public override byte[] Data => _data;

		public override byte C => _admark[0];

		public override byte H => _admark[1];

		public override byte R => _admark[2];

		public override byte N => _admark[3];

		public SectorFlags Td0Flags => (SectorFlags)_admark[4];

		private TD0_SECTOR()
		{
		}

		public static TD0_SECTOR Deserialize(Stream stream)
		{
			TD0_SECTOR tD0_SECTOR = new TD0_SECTOR();
			byte[] array = new byte[6];
			stream.Read(array, 0, 6);
			tD0_SECTOR._admark = array;
			byte[] array2 = new byte[2];
			stream.Read(array2, 0, 2);
			byte[] array3 = new byte[FormatSerializer.getUInt16(array2, 0)];
			stream.Read(array3, 0, array3.Length);
			tD0_SECTOR._data = unpackData(array3);
			ushort num = CalculateTD0CRC(tD0_SECTOR._data, 0, tD0_SECTOR._data.Length);
			if (array[5] != (num & 0xFF))
			{
				Logger.GetLogger().LogWarning("TD0 loader: Sector data had bad CRC=0x" + num.ToString("X4") + " (stamp crc=0x" + array[5].ToString("X2") + ") [C:" + tD0_SECTOR.C.ToString("X2") + ";H:" + tD0_SECTOR.H.ToString("X2") + ";R:" + tD0_SECTOR.R.ToString("X2") + ";N:" + tD0_SECTOR.N.ToString("X2"));
				PlatformFactory.Platform.ShowWarning("Sector data had bad CRC", "TD0 loader");
			}
			tD0_SECTOR.SetAdCrc(valid: true);
			tD0_SECTOR.SetDataCrc((tD0_SECTOR.Td0Flags & SectorFlags.BadCrc) == 0);
			return tD0_SECTOR;
		}

		private static byte[] unpackData(byte[] buffer)
		{
			List<byte> list = new List<byte>();
			switch (buffer[0])
			{
			case 0:
			{
				for (int k = 1; k < buffer.Length; k++)
				{
					list.Add(buffer[k]);
				}
				break;
			}
			case 1:
			{
				int num2 = FormatSerializer.getUInt16(buffer, 1);
				for (int l = 0; l < num2; l++)
				{
					list.Add(buffer[3]);
					list.Add(buffer[4]);
				}
				break;
			}
			case 2:
			{
				int num = 1;
				do
				{
					switch (buffer[num++])
					{
					case 0:
					{
						int num2 = buffer[num++];
						for (int j = 0; j < num2; j++)
						{
							list.Add(buffer[num++]);
						}
						break;
					}
					case 1:
					{
						int num2 = buffer[num++];
						for (int i = 0; i < num2; i++)
						{
							list.Add(buffer[num]);
							list.Add(buffer[num + 1]);
						}
						num += 2;
						break;
					}
					default:
						PlatformFactory.Platform.ShowWarning("Unknown sector encoding!", "TD0 loader");
						num = buffer.Length;
						break;
					}
				}
				while (num < buffer.Length);
				break;
			}
			}
			return list.ToArray();
		}
	}

	private DiskImage _diskImage;

	private static byte[] tbltd0crc = new byte[512]
	{
		0, 0, 160, 151, 225, 185, 65, 46, 99, 229,
		195, 114, 130, 92, 34, 203, 199, 202, 103, 93,
		38, 115, 134, 228, 164, 47, 4, 184, 69, 150,
		229, 1, 47, 3, 143, 148, 206, 186, 110, 45,
		76, 230, 236, 113, 173, 95, 13, 200, 232, 201,
		72, 94, 9, 112, 169, 231, 139, 44, 43, 187,
		106, 149, 202, 2, 94, 6, 254, 145, 191, 191,
		31, 40, 61, 227, 157, 116, 220, 90, 124, 205,
		153, 204, 57, 91, 120, 117, 216, 226, 250, 41,
		90, 190, 27, 144, 187, 7, 113, 5, 209, 146,
		144, 188, 48, 43, 18, 224, 178, 119, 243, 89,
		83, 206, 182, 207, 22, 88, 87, 118, 247, 225,
		213, 42, 117, 189, 52, 147, 148, 4, 188, 12,
		28, 155, 93, 181, 253, 34, 223, 233, 127, 126,
		62, 80, 158, 199, 123, 198, 219, 81, 154, 127,
		58, 232, 24, 35, 184, 180, 249, 154, 89, 13,
		147, 15, 51, 152, 114, 182, 210, 33, 240, 234,
		80, 125, 17, 83, 177, 196, 84, 197, 244, 82,
		181, 124, 21, 235, 55, 32, 151, 183, 214, 153,
		118, 14, 226, 10, 66, 157, 3, 179, 163, 36,
		129, 239, 33, 120, 96, 86, 192, 193, 37, 192,
		133, 87, 196, 121, 100, 238, 70, 37, 230, 178,
		167, 156, 7, 11, 205, 9, 109, 158, 44, 176,
		140, 39, 174, 236, 14, 123, 79, 85, 239, 194,
		10, 195, 170, 84, 235, 122, 75, 237, 105, 38,
		201, 177, 136, 159, 40, 8, 216, 143, 120, 24,
		57, 54, 153, 161, 187, 106, 27, 253, 90, 211,
		250, 68, 31, 69, 191, 210, 254, 252, 94, 107,
		124, 160, 220, 55, 157, 25, 61, 142, 247, 140,
		87, 27, 22, 53, 182, 162, 148, 105, 52, 254,
		117, 208, 213, 71, 48, 70, 144, 209, 209, 255,
		113, 104, 83, 163, 243, 52, 178, 26, 18, 141,
		134, 137, 38, 30, 103, 48, 199, 167, 229, 108,
		69, 251, 4, 213, 164, 66, 65, 67, 225, 212,
		160, 250, 0, 109, 34, 166, 130, 49, 195, 31,
		99, 136, 169, 138, 9, 29, 72, 51, 232, 164,
		202, 111, 106, 248, 43, 214, 139, 65, 110, 64,
		206, 215, 143, 249, 47, 110, 13, 165, 173, 50,
		236, 28, 76, 139, 100, 131, 196, 20, 133, 58,
		37, 173, 7, 102, 167, 241, 230, 223, 70, 72,
		163, 73, 3, 222, 66, 240, 226, 103, 192, 172,
		96, 59, 33, 21, 129, 130, 75, 128, 235, 23,
		170, 57, 10, 174, 40, 101, 136, 242, 201, 220,
		105, 75, 140, 74, 44, 221, 109, 243, 205, 100,
		239, 175, 79, 56, 14, 22, 174, 129, 58, 133,
		154, 18, 219, 60, 123, 171, 89, 96, 249, 247,
		184, 217, 24, 78, 253, 79, 93, 216, 28, 246,
		188, 97, 158, 170, 62, 61, 127, 19, 223, 132,
		21, 134, 181, 17, 244, 63, 84, 168, 118, 99,
		214, 244, 151, 218, 55, 77, 210, 76, 114, 219,
		51, 245, 147, 98, 177, 169, 17, 62, 80, 16,
		240, 135
	};

	public override string FormatGroup => "Disk images";

	public override string FormatName => "TD0 disk image";

	public override string FormatExtension => "TD0";

	public override bool CanDeserialize => true;

	public Td0Serializer(DiskImage diskImage)
	{
		_diskImage = diskImage;
	}

	public override void Deserialize(Stream stream)
	{
		if (_diskImage.Present)
		{
			_diskImage.Eject();
		}
		loadData(stream);
		_diskImage.Insert();
	}

	private bool loadData(Stream stream)
	{
		TD0_MAIN_HEADER tD0_MAIN_HEADER = TD0_MAIN_HEADER.Deserialize(stream);
		if (tD0_MAIN_HEADER == null)
		{
			return false;
		}
		if (tD0_MAIN_HEADER.Ver > 21 || tD0_MAIN_HEADER.Ver < 10)
		{
			PlatformFactory.Platform.ShowWarning("Format version is not supported [0x" + tD0_MAIN_HEADER.Ver.ToString("X2") + "]", "TD0 loader");
			return false;
		}
		if (tD0_MAIN_HEADER.DataDOS != 0)
		{
			PlatformFactory.Platform.ShowWarning("'DOS Allocated sectors were copied' option is not supported!", "TD0 loader");
			return false;
		}
		Stream stream2 = stream;
		if (tD0_MAIN_HEADER.IsAdvandcedCompression)
		{
			if (tD0_MAIN_HEADER.Ver < 20)
			{
				PlatformFactory.Platform.ShowWarning("Old Advanced compression is not implemented!", "TD0 loader");
				return false;
			}
			stream2 = new LzssHuffmanStream(stream);
		}
		string description = string.Empty;
		if ((tD0_MAIN_HEADER.Info & 0x80u) != 0)
		{
			byte[] array = new byte[4];
			stream2.Read(array, 0, 2);
			stream2.Read(array, 2, 2);
			byte[] array2 = new byte[FormatSerializer.getUInt16(array, 2) + 10];
			for (int i = 0; i < 4; i++)
			{
				array2[i] = array[i];
			}
			stream2.Read(array2, 4, 6);
			stream2.Read(array2, 10, array2.Length - 10);
			if (CalculateTD0CRC(array2, 2, 8 + FormatSerializer.getUInt16(array2, 2)) != FormatSerializer.getUInt16(array2, 0))
			{
				PlatformFactory.Platform.ShowWarning("Info crc wrong", "TD0 loader");
			}
			StringBuilder stringBuilder = new StringBuilder();
			int num = 10;
			for (int j = 10; j < array2.Length; j++)
			{
				if (array2[j] == 0 && j > num)
				{
					stringBuilder.Append(Encoding.ASCII.GetString(array2, num, j - num));
					stringBuilder.Append("\n");
					num = j + 1;
				}
			}
			description = stringBuilder.ToString();
		}
		int num2 = -1;
		int num3 = -1;
		ArrayList arrayList = new ArrayList();
		while (true)
		{
			TD0_TRACK tD0_TRACK = TD0_TRACK.Deserialize(stream2);
			if (tD0_TRACK.SectorCount == 255)
			{
				break;
			}
			arrayList.Add(tD0_TRACK);
			if (num2 < tD0_TRACK.Cylinder)
			{
				num2 = tD0_TRACK.Cylinder;
			}
			if (num3 < tD0_TRACK.Side)
			{
				num3 = tD0_TRACK.Side;
			}
		}
		num2++;
		num3++;
		if (num2 < 1 || num3 < 1)
		{
			PlatformFactory.Platform.ShowWarning("Invalid disk structure", "td0");
			return false;
		}
		_diskImage.Init(num2, num3);
		foreach (TD0_TRACK item in arrayList)
		{
			_diskImage.GetTrackImage(item.Cylinder, item.Side).AssignSectors(item.SectorList);
		}
		_diskImage.Description = description;
		return true;
	}

	private static ushort CalculateTD0CRC(byte[] buffer, int startIndex, int length)
	{
		ushort num = 0;
		for (int i = 0; i < length; i++)
		{
			num = (ushort)(num ^ buffer[startIndex++]);
			int num2 = num & 0xFF;
			num = (ushort)(num & 0xFF00u);
			num = (ushort)((num << 8) | (num >> 8));
			num = (ushort)(num ^ FormatSerializer.getUInt16(tbltd0crc, num2 * 2));
		}
		return (ushort)((num << 8) | (num >> 8));
	}
}
