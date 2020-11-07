using System;
using System.IO;
using ZXMAK.Engine.Disk;
using ZXMAK.Platform;

namespace ZXMAK.Engine.Loaders.DiskSerializers
{
	public class HobetaSerializer : FormatSerializer
	{
		public HobetaSerializer(DiskImage diskImage)
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
				return "Hobeta disk image";
			}
		}

		public override string FormatExtension
		{
			get
			{
				return "$";
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
			this.loadFromStream(stream);
		}

		private void loadFromStream(Stream stream)
		{
			if (stream.Length < 15L)
			{
				PlatformFactory.Platform.ShowWarning("Invalid HOBETA file size", "HOBETA loader");
				return;
			}
			byte[] array = new byte[stream.Length];
			stream.Seek(0L, SeekOrigin.Begin);
			stream.Read(array, 0, (int)stream.Length);
			if ((int)array[14] * 256 + 17 != array.Length || array[13] != 0 || array[14] == 0)
			{
				PlatformFactory.Platform.ShowWarning("Corrupt HOBETA file!", "HOBETA loader");
				return;
			}
			string a = Path.GetExtension(this._diskImage.FileName).ToUpper();
			if (a == string.Empty)
			{
				this._diskImage.InitFormated();
			}
			array[13] = array[14];
			this.addFile(array, 0, 17);
		}

		protected bool addFile(byte[] buf, int hdrIndex, int dataIndex)
		{
			byte[] array = new byte[256];
			this._diskImage.readLogicalSector(0, 0, 9, array);
			int num = (int)buf[hdrIndex + 13];
			int num2 = (int)(array[228] * 16);
			byte[] array2 = new byte[256];
			this._diskImage.readLogicalSector(0, 0, 1 + num2 / 256, array2);
			if (((int)array[229] | (int)array[230] << 8) < num)
			{
				return false;
			}
			for (int i = 0; i < 14; i++)
			{
				array2[(num2 & 255) + i] = buf[hdrIndex + i];
			}
			ushort num3 = (ushort)((int)array[225] | (int)array[226] << 8);
			array2[(num2 & 255) + 14] = (byte)num3;
			array2[(num2 & 255) + 15] = (byte)(num3 >> 8);
			this._diskImage.writeLogicalSector(0, 0, 1 + num2 / 256, array2);
			num2 = (int)(array[225] + 16 * array[226]);
			array[225] = (byte)(num2 + num & 15);
			array[226] = (byte)(num2 + num >> 4);
			byte[] array3 = array;
			int num4 = 228;
			array3[num4] += 1;
			num3 = (ushort)((int)array[229] | (int)array[230] << 8);
			num3 -= (ushort)num;
			array[229] = (byte)num3;
			array[230] = (byte)(num3 >> 8);
			this._diskImage.writeLogicalSector(0, 0, 9, array);
			int j = 0;
			while (j < num)
			{
				for (int k = 0; k < 256; k++)
				{
					array[k] = buf[dataIndex + j * 256 + k];
				}
				this._diskImage.writeLogicalSector(num2 / 32, num2 / 16 & 1, (num2 & 15) + 1, array);
				j++;
				num2++;
			}
			return true;
		}

		protected DiskImage _diskImage;
	}
}
