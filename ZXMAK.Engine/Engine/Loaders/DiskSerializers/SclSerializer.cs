using System;
using System.IO;
using System.Text;
using ZXMAK.Engine.Disk;
using ZXMAK.Platform;

namespace ZXMAK.Engine.Loaders.DiskSerializers
{
	public class SclSerializer : HobetaSerializer
	{
		public SclSerializer(DiskImage diskImage) : base(diskImage)
		{
		}

		public override string FormatName
		{
			get
			{
				return "SCL disk image";
			}
		}

		public override string FormatExtension
		{
			get
			{
				return "SCL";
			}
		}

		public override void Deserialize(Stream stream)
		{
			this.loadFromStream(stream);
		}

		public override void Serialize(Stream stream)
		{
			throw new NotImplementedException("SclFormatSerializer.Serialize is not implemented.");
		}

		private void loadFromStream(Stream stream)
		{
			if (stream.Length < 9L || stream.Length > 651273L)
			{
				PlatformFactory.Platform.ShowWarning("Invalid SCL file size", "SCL loader");
				return;
			}
			byte[] array = new byte[stream.Length];
			stream.Seek(0L, SeekOrigin.Begin);
			stream.Read(array, 0, (int)stream.Length);
			if (Encoding.ASCII.GetString(array, 0, 8) != "SINCLAIR")
			{
				PlatformFactory.Platform.ShowWarning("Corrupted SCL file", "SCL loader");
				return;
			}
			string a = Path.GetExtension(this._diskImage.FileName).ToUpper();
			if (a == string.Empty)
			{
				this._diskImage.InitFormated();
			}
			int i;
			int num = i = 0;
			while (i < (int)array[8])
			{
				num += (int)array[9 + 14 * i + 13];
				i++;
			}
			if (num > 2544)
			{
				byte[] array2 = new byte[256];
				this._diskImage.readLogicalSector(0, 0, 9, array2);
				array2[229] = (byte)num;
				array2[230] = (byte)(num >> 8);
				this._diskImage.writeLogicalSector(0, 0, 9, array2);
			}
			int num2 = (int)(9 + 14 * array[8]);
			for (int j = 0; j < (int)array[8]; j++)
			{
				if (!base.addFile(array, 9 + 14 * j, num2))
				{
					return;
				}
				num2 += (int)array[9 + 14 * j + 13] * 256;
			}
		}
	}
}
