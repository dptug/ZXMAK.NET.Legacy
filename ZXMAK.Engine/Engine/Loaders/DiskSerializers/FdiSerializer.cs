using System;
using System.Collections;
using System.IO;
using ZXMAK.Engine.Disk;
using ZXMAK.Platform;

namespace ZXMAK.Engine.Loaders.DiskSerializers
{
	public class FdiSerializer : FormatSerializer
	{
		public FdiSerializer(DiskImage diskImage)
		{
			this._diskImage = diskImage;
		}

		public override string FormatGroup
		{
			get
			{
				return "Disk images";
			}
		}

		public override string FormatName
		{
			get
			{
				return "FDI disk image";
			}
		}

		public override string FormatExtension
		{
			get
			{
				return "FDI";
			}
		}

		public override bool CanDeserialize
		{
			get
			{
				return true;
			}
		}

		public override void Deserialize(Stream stream)
		{
			if (this._diskImage.Present)
			{
				this._diskImage.Eject();
			}
			stream.Seek(0L, SeekOrigin.Begin);
			this.loadData(stream);
			this._diskImage.Init(this._cylynderImages.Count, this._sideCount);
			for (int i = 0; i < this._cylynderImages.Count; i++)
			{
				byte[][][] array = (byte[][][])this._cylynderImages[i];
				for (int j = 0; j < this._diskImage.SideCount; j++)
				{
					this._diskImage.GetTrackImage(i, j).AssignImage(array[j][0], array[j][1]);
				}
			}
			this._diskImage.Insert();
		}

		private void loadData(Stream stream)
		{
			this._cylynderImages.Clear();
			this._sideCount = 0;
			if (stream.Length < 14L)
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
			this._writeProtect = (array[3] != 0);
			int num = (int)array[4] | (int)array[5] << 8;
			this._sideCount = ((int)array[6] | (int)array[7] << 8);
			byte b = array[8];
			byte b2 = array[9];
			int num2 = (int)array[10] | (int)array[11] << 8;
			int num3 = (int)array[12] | (int)array[13] << 8;
			if (num3 > 0)
			{
				byte[] buffer = new byte[num3];
				stream.Read(buffer, 0, num3);
			}
			ArrayList arrayList = new ArrayList();
			for (int i = 0; i < num; i++)
			{
				for (int j = 0; j < this._sideCount; j++)
				{
					arrayList.Add(this.readTrackHeader(stream));
				}
			}
			for (int k = 0; k < num; k++)
			{
				byte[][][] array2 = new byte[this._sideCount][][];
				this._cylynderImages.Add(array2);
				for (int l = 0; l < this._sideCount; l++)
				{
					ArrayList arrayList2 = (ArrayList)arrayList[k * this._sideCount + l];
					for (int m = 0; m < arrayList2.Count; m++)
					{
						FdiSerializer.SectorHeader sectorHeader = (FdiSerializer.SectorHeader)arrayList2[m];
						if ((sectorHeader.Flags & 64) == 0)
						{
							int num4 = 128 << (int)sectorHeader.N;
							sectorHeader.crcOk = ((sectorHeader.Flags & 31) != 0);
							sectorHeader.DataArray = new byte[num4];
							stream.Seek((long)(num2 + sectorHeader.DataOffset), SeekOrigin.Begin);
							stream.Read(sectorHeader.DataArray, 0, num4);
						}
					}
					array2[l] = this.generateTrackImage(arrayList2);
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
				FdiSerializer.SectorHeader sectorHeader = (FdiSerializer.SectorHeader)sectorHeaderList[i];
				num2 += 8;
				int num3 = 128 << (int)sectorHeader.N;
				if ((sectorHeader.Flags & 64) != 0)
				{
					num3 = 0;
				}
				else
				{
					num2 += 4;
				}
				num2 += num3;
			}
			int j = num - (num2 + count * 5);
			int num4 = 1;
			int num5 = 1;
			int num6 = 1;
			int num7 = 1;
			int num8 = 1;
			j -= num5 + num6 + num7 + num8;
			if (j < 0)
			{
				num += -j;
				j = 0;
			}
			while (j > 0)
			{
				if (j >= count * 2 && num8 < 12)
				{
					num8++;
					j -= count * 2;
				}
				if (j < count)
				{
					break;
				}
				if (num5 < 10)
				{
					num5++;
					j -= count;
				}
				if (j < count)
				{
					break;
				}
				if (num6 < 22)
				{
					num6++;
					j -= count;
				}
				if (j < count)
				{
					break;
				}
				if (num7 < 60)
				{
					num7++;
					j -= count;
				}
				if (j < count || (num8 >= 12 && num5 >= 10 && num6 >= 22 && num7 >= 60))
				{
					break;
				}
			}
			if (j > count * 2 + 10)
			{
				num4++;
				j -= count;
			}
			if (j > count * 2 + 9)
			{
				num4++;
			}
			if (j < 0)
			{
				num += -j;
			}
			array[0] = new byte[num];
			array[1] = new byte[array[0].Length / 8 + (((array[0].Length & 7) != 0) ? 1 : 0)];
			int num9 = 0;
			for (int k = 0; k < count; k++)
			{
				FdiSerializer.SectorHeader sectorHeader2 = (FdiSerializer.SectorHeader)sectorHeaderList[k];
				for (int l = 0; l < num5; l++)
				{
					array[0][num9] = 78;
					byte[] array2 = array[1];
					int num10 = num9 / 8;
					array2[num10] &= (byte)(~(byte)(1 << (num9 & 7)));
					num9++;
				}
				for (int l = 0; l < num8; l++)
				{
					array[0][num9] = 0;
					byte[] array3 = array[1];
					int num11 = num9 / 8;
					array3[num11] &= (byte)(~(byte)(1 << (num9 & 7)));
					num9++;
				}
				int num12 = num9;
				for (int l = 0; l < num4; l++)
				{
					array[0][num9] = 161;
					byte[] array4 = array[1];
					int num13 = num9 / 8;
					array4[num13] |= (byte)(1 << (num9 & 7));
					num9++;
				}
				array[0][num9] = 254;
				byte[] array5 = array[1];
				int num14 = num9 / 8;
				array5[num14] &= (byte)(~(byte)(1 << (num9 & 7)));
				num9++;
				array[0][num9] = sectorHeader2.C;
				byte[] array6 = array[1];
				int num15 = num9 / 8;
				array6[num15] &= (byte)(~(byte)(1 << (num9 & 7)));
				num9++;
				array[0][num9] = sectorHeader2.H;
				byte[] array7 = array[1];
				int num16 = num9 / 8;
				array7[num16] &= (byte)(~(byte)(1 << (num9 & 7)));
				num9++;
				array[0][num9] = sectorHeader2.R;
				byte[] array8 = array[1];
				int num17 = num9 / 8;
				array8[num17] &= (byte)(~(byte)(1 << (num9 & 7)));
				num9++;
				array[0][num9] = sectorHeader2.N;
				byte[] array9 = array[1];
				int num18 = num9 / 8;
				array9[num18] &= (byte)(~(byte)(1 << (num9 & 7)));
				num9++;
				ushort num19 = FdiSerializer.WD1793_CRC(array[0], num12, num9 - num12);
				array[0][num9] = (byte)num19;
				byte[] array10 = array[1];
				int num20 = num9 / 8;
				array10[num20] &= (byte)(~(byte)(1 << (num9 & 7)));
				num9++;
				array[0][num9] = (byte)(num19 >> 8);
				byte[] array11 = array[1];
				int num21 = num9 / 8;
				array11[num21] &= (byte)(~(byte)(1 << (num9 & 7)));
				num9++;
				for (int l = 0; l < num6; l++)
				{
					array[0][num9] = 78;
					byte[] array12 = array[1];
					int num22 = num9 / 8;
					array12[num22] &= (byte)(~(byte)(1 << (num9 & 7)));
					num9++;
				}
				for (int l = 0; l < num8; l++)
				{
					array[0][num9] = 0;
					byte[] array13 = array[1];
					int num23 = num9 / 8;
					array13[num23] &= (byte)(~(byte)(1 << (num9 & 7)));
					num9++;
				}
				byte flags = sectorHeader2.Flags;
				if ((flags & 64) == 0)
				{
					num12 = num9;
					for (int l = 0; l < num4; l++)
					{
						array[0][num9] = 161;
						byte[] array14 = array[1];
						int num24 = num9 / 8;
						array14[num24] |= (byte)(1 << (num9 & 7));
						num9++;
					}
					if ((flags & 128) != 0)
					{
						array[0][num9] = 248;
					}
					else
					{
						array[0][num9] = 251;
					}
					byte[] array15 = array[1];
					int num25 = num9 / 8;
					array15[num25] &= (byte)(~(byte)(1 << (num9 & 7)));
					num9++;
					int num26 = 128 << (int)sectorHeader2.N;
					for (int l = 0; l < num26; l++)
					{
						array[0][num9] = sectorHeader2.DataArray[l];
						byte[] array16 = array[1];
						int num27 = num9 / 8;
						array16[num27] &= (byte)(~(byte)(1 << (num9 & 7)));
						num9++;
					}
					num19 = FdiSerializer.WD1793_CRC(array[0], num12, num9 - num12);
					if ((flags & 63) == 0)
					{
						num19 ^= ushort.MaxValue;
					}
					array[0][num9] = (byte)num19;
					byte[] array17 = array[1];
					int num28 = num9 / 8;
					array17[num28] &= (byte)(~(byte)(1 << (num9 & 7)));
					num9++;
					array[0][num9] = (byte)(num19 >> 8);
					byte[] array18 = array[1];
					int num29 = num9 / 8;
					array18[num29] &= (byte)(~(byte)(1 << (num9 & 7)));
					num9++;
				}
				for (int l = 0; l < num7; l++)
				{
					array[0][num9] = 78;
					byte[] array19 = array[1];
					int num30 = num9 / 8;
					array19[num30] &= (byte)(~(byte)(1 << (num9 & 7)));
					num9++;
				}
			}
			for (int m = num9; m < array[0].Length; m++)
			{
				array[0][num9] = 78;
				byte[] array20 = array[1];
				int num31 = num9 / 8;
				array20[num31] &= (byte)(~(byte)(1 << (num9 & 7)));
				num9++;
			}
			return array;
		}

		private ArrayList readTrackHeader(Stream f)
		{
			byte[] array = new byte[7];
			ArrayList arrayList = new ArrayList();
			f.Read(array, 0, 4);
			int num = (int)array[0] | (int)array[1] << 8 | (int)array[2] << 16 | (int)array[3] << 24;
			f.Read(array, 0, 2);
			f.Read(array, 0, 1);
			int num2 = (int)array[0];
			for (int i = 0; i < num2; i++)
			{
				f.Read(array, 0, 7);
				arrayList.Add(new FdiSerializer.SectorHeader
				{
					C = array[0],
					H = array[1],
					R = array[2],
					N = array[3],
					Flags = array[4],
					DataOffset = num + ((int)array[5] | (int)array[6] << 8)
				});
			}
			return arrayList;
		}

		private static ushort WD1793_CRC(byte[] data, int startIndex, int size)
		{
			ushort num = ushort.MaxValue;
			while (size-- > 0)
			{
				num ^= (ushort)(data[startIndex++] << 8);
				for (int i = 0; i < 8; i++)
				{
					if ((num & 32768) != 0)
					{
						num = (ushort)((int)num << 1 ^ 4129);
					}
					else
					{
						num = (ushort)(num << 1);
					}
				}
			}
			return (ushort)(num >> 8 | (int)num << 8);
		}

		private DiskImage _diskImage;

		private bool _writeProtect;

		private string _description = string.Empty;

		private ArrayList _cylynderImages = new ArrayList();

		private int _sideCount;

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
	}
}
