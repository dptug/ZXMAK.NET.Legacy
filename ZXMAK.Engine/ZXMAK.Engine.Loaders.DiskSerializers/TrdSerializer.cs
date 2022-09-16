using System.IO;
using ZXMAK.Engine.Disk;

namespace ZXMAK.Engine.Loaders.DiskSerializers;

public class TrdSerializer : FormatSerializer
{
	private DiskImage _diskImage;

	public override string FormatGroup => "Disk images";

	public override string FormatName => "TRD disk image";

	public override string FormatExtension => "TRD";

	public override bool CanDeserialize => true;

	public override bool CanSerialize => true;

	public TrdSerializer(DiskImage diskImage)
	{
		_diskImage = diskImage;
	}

	public override void Deserialize(Stream stream)
	{
		if (_diskImage.Present)
		{
			_diskImage.Eject();
		}
		loadFromStream(stream);
		_diskImage.Insert();
	}

	public override void Serialize(Stream stream)
	{
		saveToStream(stream);
	}

	private void loadFromStream(Stream stream)
	{
		int num = (int)stream.Length / 8192;
		if (stream.Length % 8192 > 0)
		{
			_diskImage.Init(num + 1, 2);
		}
		else
		{
			_diskImage.Init(num, 2);
		}
		_diskImage.format_trdos();
		int num2 = 0;
		while (stream.Position < stream.Length)
		{
			byte[] buffer = new byte[256];
			stream.Read(buffer, 0, 256);
			_diskImage.writeLogicalSector(num2 >> 13, (num2 >> 12) & 1, ((num2 >> 8) & 0xF) + 1, buffer);
			num2 += 256;
		}
	}

	private void saveToStream(Stream stream)
	{
		for (int i = 0; i < 8192 * _diskImage.CylynderCount; i += 256)
		{
			byte[] buffer = new byte[256];
			_diskImage.readLogicalSector(i >> 13, (i >> 12) & 1, ((i >> 8) & 0xF) + 1, buffer);
			stream.Write(buffer, 0, 256);
		}
	}
}
