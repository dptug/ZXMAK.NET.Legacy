using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using ZXMAK.Engine.Tape;
using ZXMAK.Platform;

namespace ZXMAK.Engine.Loaders.TapeSerializers;

public class CswSerializer : FormatSerializer
{
	private TapeDevice _tape;

	public override string FormatGroup => "Tape images";

	public override string FormatName => "CSW image";

	public override string FormatExtension => "CSW";

	public override bool CanDeserialize => true;

	public CswSerializer(TapeDevice tape)
	{
		_tape = tape;
	}

	public override void Deserialize(Stream stream)
	{
		List<int> list = new List<int>();
		byte[] array = new byte[52];
		stream.Read(array, 0, 32);
		if (Encoding.ASCII.GetString(array, 0, 22) != "Compressed Square Wave")
		{
			PlatformFactory.Platform.ShowWarning("Invalid CSW file, identifier not found! ", "CSW loader");
			return;
		}
		if (array[23] > 2)
		{
			PlatformFactory.Platform.ShowWarning("Format CSW V" + array[23] + "." + array[24] + " not supported!", "CSW loader");
			return;
		}
		if (array[23] == 2)
		{
			stream.Read(array, 32, 20);
			byte[] array2 = new byte[array[35]];
			stream.Read(array2, 0, array2.Length);
		}
		int num = ((array[23] == 2) ? BitConverter.ToInt32(array, 25) : BitConverter.ToUInt16(array, 25));
		int num2 = ((array[23] == 2) ? array[33] : array[27]);
		byte[] array3 = ((array[23] != 2) ? new byte[stream.Length - 32] : new byte[BitConverter.ToInt32(array, 29)]);
		switch (num2)
		{
		case 1:
			stream.Read(array3, 0, array3.Length);
			break;
		case 2:
			csw2_uncompress(stream, array3);
			break;
		default:
			PlatformFactory.Platform.ShowWarning("Unknown compression type!", "CSW loader");
			return;
		}
		int num3 = (int)(_tape.Z80FQ / (ulong)num);
		int num4 = 0;
		while (num4 < array3.Length)
		{
			int num5 = array3[num4++] * num3;
			if (num5 == 0)
			{
				num5 = BitConverter.ToInt32(array3, num4) / num3;
				num4 += 4;
			}
			list.Add(num5);
		}
		list.Add((int)(_tape.Z80FQ / 10uL));
		TapeBlock tapeBlock = new TapeBlock();
		tapeBlock.Description = "CSW tape image";
		tapeBlock.Periods = list;
		_tape.Blocks.Add(tapeBlock);
	}

	private int csw2_uncompress(Stream stream, byte[] buffer)
	{
		stream.ReadByte();
		stream.ReadByte();
		DeflateStream deflateStream = new DeflateStream(stream, CompressionMode.Decompress, leaveOpen: false);
		return deflateStream.Read(buffer, 0, buffer.Length);
	}
}
