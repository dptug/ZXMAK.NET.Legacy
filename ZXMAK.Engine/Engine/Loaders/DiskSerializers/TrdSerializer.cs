using System;
using System.IO;
using ZXMAK.Engine.Disk;

namespace ZXMAK.Engine.Loaders.DiskSerializers
{
	public class TrdSerializer : FormatSerializer
	{
		public TrdSerializer(DiskImage diskImage)
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
				return "TRD disk image";
			}
		}

		public override string FormatExtension
		{
			get
			{
				return "TRD";
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
			this.saveToStream(stream);
		}

		private void loadFromStream(Stream stream)
		{
			int num = (int)stream.Length / 8192;
			if (stream.Length % 8192L > 0L)
			{
				this._diskImage.Init(num + 1, 2);
			}
			else
			{
				this._diskImage.Init(num, 2);
			}
			this._diskImage.format_trdos();
			int num2 = 0;
			while (stream.Position < stream.Length)
			{
				byte[] buffer = new byte[256];
				stream.Read(buffer, 0, 256);
				this._diskImage.writeLogicalSector(num2 >> 13, num2 >> 12 & 1, (num2 >> 8 & 15) + 1, buffer);
				num2 += 256;
			}
		}

		private void saveToStream(Stream stream)
		{
			for (int i = 0; i < 8192 * this._diskImage.CylynderCount; i += 256)
			{
				byte[] buffer = new byte[256];
				this._diskImage.readLogicalSector(i >> 13, i >> 12 & 1, (i >> 8 & 15) + 1, buffer);
				stream.Write(buffer, 0, 256);
			}
		}

		private DiskImage _diskImage;
	}
}
