using System;
using System.IO;
using System.Text;
using ZXMAK.Engine.Disk;
using ZXMAK.Platform;

namespace ZXMAK.Engine.Loaders.DiskSerializers
{
	public class UdiSerializer : FormatSerializer
	{
		public UdiSerializer(DiskImage diskImage)
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
				return "UDI disk image";
			}
		}

		public override string FormatExtension
		{
			get
			{
				return "UDI";
			}
		}

		public override bool CanDeserialize
		{
			get
			{
				return true;
			}
		}

		public override bool CanSerialize
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
			this.loadFromStream(stream);
			this._diskImage.Insert();
		}

		public override void Serialize(Stream stream)
		{
			byte[] array;
			using (MemoryStream memoryStream = new MemoryStream())
			{
				this.saveToStream(memoryStream);
				array = memoryStream.ToArray();
			}
			stream.Write(array, 0, array.Length);
		}

		private void loadFromStream(Stream stream)
		{
			int num = -1;
			byte[] array = new byte[16];
			stream.Seek(0L, SeekOrigin.Begin);
			stream.Read(array, 0, 16);
			for (int i = 0; i < 16; i++)
			{
				num = this.calcCRC32(num, array[i]);
			}
			if (Encoding.ASCII.GetString(array, 0, 4) != "UDI!")
			{
				PlatformFactory.Platform.ShowWarning("Unknown *.UDI file identifier!", "UDI loader");
				return;
			}
			long num2 = (long)((int)array[4] | (int)array[5] << 8 | (int)array[6] << 16 | (int)array[7] << 24);
			if (stream.Length != num2 + 4L)
			{
				PlatformFactory.Platform.ShowWarning("Corrupt *.UDI file!", "UDI loader");
				return;
			}
			if (array[8] != 0)
			{
				PlatformFactory.Platform.ShowWarning("Unsupported *.UDI format version!", "UDI loader");
				return;
			}
			int num3 = (int)(array[9] + 1);
			int num4 = (int)(array[10] + 1);
			int num5 = (int)array[12] | (int)array[13] << 8 | (int)array[14] << 16 | (int)array[15] << 24;
			this._diskImage.Init(num3, num4);
			if (num5 > 0)
			{
				byte[] array2 = new byte[num5];
				stream.Read(array2, 0, num5);
				for (int j = 0; j < num5; j++)
				{
					num = this.calcCRC32(num, array2[j]);
				}
			}
			for (int k = 0; k < num3; k++)
			{
				for (int l = 0; l < num4; l++)
				{
					stream.Read(array, 0, 3);
					for (int m = 0; m < 3; m++)
					{
						num = this.calcCRC32(num, array[m]);
					}
					byte b = array[0];
					int num6 = (int)array[1] | (int)array[2] << 8;
					byte[][] array3 = new byte[][]
					{
						new byte[num6],
						new byte[num6 / 8 + (((num6 & 7) != 0) ? 1 : 0)]
					};
					stream.Read(array3[0], 0, array3[0].Length);
					for (int n = 0; n < array3[0].Length; n++)
					{
						num = this.calcCRC32(num, array3[0][n]);
					}
					stream.Read(array3[1], 0, array3[1].Length);
					for (int num7 = 0; num7 < array3[1].Length; num7++)
					{
						num = this.calcCRC32(num, array3[1][num7]);
					}
					this._diskImage.GetTrackImage(k, l).AssignImage(array3[0], array3[1]);
				}
			}
			stream.Read(array, 0, 4);
			int num8 = (int)array[0] | (int)array[1] << 8 | (int)array[2] << 16 | (int)array[3] << 24;
			if (num8 != num)
			{
				PlatformFactory.Platform.ShowWarning("CRC ERROR:\nStamp: " + num8.ToString("X8") + "\nReal: " + num.ToString("X8"), "UDI loader");
			}
		}

		private void saveToStream(Stream stream)
		{
			stream.SetLength(0L);
			stream.Seek(0L, SeekOrigin.Begin);
			int num = -1;
			byte[] array = new byte[16];
			array[0] = 85;
			array[1] = 68;
			array[2] = 73;
			array[3] = 33;
			array[8] = 0;
			array[9] = (byte)(this._diskImage.CylynderCount - 1);
			array[10] = (byte)(this._diskImage.SideCount - 1);
			int num2 = 0;
			array[12] = (byte)num2;
			array[13] = (byte)(num2 >> 8);
			array[14] = (byte)(num2 >> 16);
			array[15] = (byte)(num2 >> 24);
			stream.Write(array, 0, 16);
			if (num2 > 0)
			{
				byte[] buffer = new byte[num2];
				stream.Write(buffer, 0, num2);
			}
			for (int i = 0; i < this._diskImage.CylynderCount; i++)
			{
				for (int j = 0; j < this._diskImage.SideCount; j++)
				{
					array[0] = 0;
					array[1] = (byte)this._diskImage.GetTrackImage(i, j).RawLength;
					array[2] = (byte)(this._diskImage.GetTrackImage(i, j).RawLength >> 8);
					stream.Write(array, 0, 3);
					byte[][] rawImage = this._diskImage.GetTrackImage(i, j).RawImage;
					stream.Write(rawImage[0], 0, rawImage[0].Length);
					stream.Write(rawImage[1], 0, rawImage[1].Length);
				}
			}
			long length = stream.Length;
			array[0] = (byte)length;
			array[1] = (byte)(length >> 8);
			array[2] = (byte)(length >> 16);
			array[3] = (byte)(length >> 24);
			stream.Seek(4L, SeekOrigin.Begin);
			stream.Write(array, 0, 4);
			stream.Seek(0L, SeekOrigin.Begin);
			byte[] array2 = new byte[length];
			stream.Read(array2, 0, (int)length);
			int num3 = 0;
			while ((long)num3 < length)
			{
				num = this.calcCRC32(num, array2[num3]);
				num3++;
			}
			array[0] = (byte)num;
			array[1] = (byte)(num >> 8);
			array[2] = (byte)(num >> 16);
			array[3] = (byte)(num >> 24);
			stream.Write(array, 0, 4);
		}

		private int calcCRC32(int CRC, byte value)
		{
			CRC ^= (-1 ^ (int)value);
			int num = 8;
			while (num-- != 0)
			{
				int num2 = -(CRC & 1);
				CRC >>= 1;
				CRC ^= (-306674912 & num2);
			}
			CRC ^= -1;
			return CRC;
		}

		private DiskImage _diskImage;
	}
}
