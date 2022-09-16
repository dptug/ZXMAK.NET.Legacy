using System.IO;
using ZXMAK.Engine.Disk;
using ZXMAK.Platform;

namespace ZXMAK.Engine.Loaders.DiskSerializers;

public class HobetaSerializer : FormatSerializer
{
	protected DiskImage _diskImage;

	public override string FormatGroup => "Disk images";

	public override string FormatName => "Hobeta disk image";

	public override string FormatExtension => "$";

	public override bool CanDeserialize => true;

	public HobetaSerializer(DiskImage diskImage)
	{
		_diskImage = diskImage;
	}

	public override void Deserialize(Stream stream)
	{
		loadFromStream(stream);
	}

	private void loadFromStream(Stream stream)
	{
		if (stream.Length < 15)
		{
			PlatformFactory.Platform.ShowWarning("Invalid HOBETA file size", "HOBETA loader");
			return;
		}
		byte[] array = new byte[stream.Length];
		stream.Seek(0L, SeekOrigin.Begin);
		stream.Read(array, 0, (int)stream.Length);
		if (array[14] * 256 + 17 != array.Length || array[13] != 0 || array[14] == 0)
		{
			PlatformFactory.Platform.ShowWarning("Corrupt HOBETA file!", "HOBETA loader");
			return;
		}
		string text = Path.GetExtension(_diskImage.FileName).ToUpper();
		if (text == string.Empty)
		{
			_diskImage.InitFormated();
		}
		array[13] = array[14];
		addFile(array, 0, 17);
	}

	protected bool addFile(byte[] buf, int hdrIndex, int dataIndex)
	{
		byte[] array = new byte[256];
		_diskImage.readLogicalSector(0, 0, 9, array);
		int num = buf[hdrIndex + 13];
		int num2 = array[228] * 16;
		byte[] array2 = new byte[256];
		_diskImage.readLogicalSector(0, 0, 1 + num2 / 256, array2);
		if ((array[229] | (array[230] << 8)) < num)
		{
			return false;
		}
		for (int i = 0; i < 14; i++)
		{
			array2[(num2 & 0xFF) + i] = buf[hdrIndex + i];
		}
		ushort num3 = (ushort)(array[225] | (array[226] << 8));
		array2[(num2 & 0xFF) + 14] = (byte)num3;
		array2[(num2 & 0xFF) + 15] = (byte)(num3 >> 8);
		_diskImage.writeLogicalSector(0, 0, 1 + num2 / 256, array2);
		num2 = array[225] + 16 * array[226];
		array[225] = (byte)((uint)(num2 + num) & 0xFu);
		array[226] = (byte)(num2 + num >> 4);
		array[228]++;
		num3 = (ushort)(array[229] | (array[230] << 8));
		num3 = (ushort)(num3 - (ushort)num);
		array[229] = (byte)num3;
		array[230] = (byte)(num3 >> 8);
		_diskImage.writeLogicalSector(0, 0, 9, array);
		int num4 = 0;
		while (num4 < num)
		{
			for (int j = 0; j < 256; j++)
			{
				array[j] = buf[dataIndex + num4 * 256 + j];
			}
			_diskImage.writeLogicalSector(num2 / 32, (num2 / 16) & 1, (num2 & 0xF) + 1, array);
			num4++;
			num2++;
		}
		return true;
	}
}
