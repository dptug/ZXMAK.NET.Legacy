using System;
using System.IO;
using System.Text;
using ZXMAK.Engine.Disk;
using ZXMAK.Platform;

namespace ZXMAK.Engine.Loaders.DiskSerializers;

public class SclSerializer : HobetaSerializer
{
	public override string FormatName => "SCL disk image";

	public override string FormatExtension => "SCL";

	public SclSerializer(DiskImage diskImage)
		: base(diskImage)
	{
	}

	public override void Deserialize(Stream stream)
	{
		loadFromStream(stream);
	}

	public override void Serialize(Stream stream)
	{
		throw new NotImplementedException("SclFormatSerializer.Serialize is not implemented.");
	}

	private void loadFromStream(Stream stream)
	{
		if (stream.Length < 9 || stream.Length > 651273)
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
		string text = Path.GetExtension(_diskImage.FileName).ToUpper();
		if (text == string.Empty)
		{
			_diskImage.InitFormated();
		}
		int num;
		for (int i = (num = 0); i < array[8]; i++)
		{
			num += array[9 + 14 * i + 13];
		}
		if (num > 2544)
		{
			byte[] array2 = new byte[256];
			_diskImage.readLogicalSector(0, 0, 9, array2);
			array2[229] = (byte)num;
			array2[230] = (byte)(num >> 8);
			_diskImage.writeLogicalSector(0, 0, 9, array2);
		}
		int num2 = 9 + 14 * array[8];
		for (int j = 0; j < array[8] && addFile(array, 9 + 14 * j, num2); j++)
		{
			num2 += array[9 + 14 * j + 13] * 256;
		}
	}
}
