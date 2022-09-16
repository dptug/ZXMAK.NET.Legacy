using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ZXMAK.Engine.Tape;

namespace ZXMAK.Engine.Loaders.TapeSerializers;

public class TapSerializer : FormatSerializer
{
	private TapeDevice _tape;

	public override string FormatGroup => "Tape images";

	public override string FormatName => "TAP image";

	public override string FormatExtension => "TAP";

	public override bool CanDeserialize => true;

	public TapSerializer(TapeDevice tape)
	{
		_tape = tape;
	}

	public override void Deserialize(Stream stream)
	{
		byte[] array = new byte[2];
		while (stream.Position < stream.Length)
		{
			stream.Read(array, 0, 2);
			int num = BitConverter.ToUInt16(array, 0);
			if (num == 0)
			{
				break;
			}
			byte[] array2 = new byte[num];
			stream.Read(array2, 0, num);
			TapeBlock tapeBlock = new TapeBlock();
			tapeBlock.Description = getBlockDescription(array2, 0, array2.Length);
			tapeBlock.Periods = getBlockPeriods(array2, 0, array2.Length, 2168, 667, 735, 855, 1710, (array2[0] < 4) ? 8064 : 3220, 1000, 8);
			_tape.Blocks.Add(tapeBlock);
		}
	}

	public static List<int> getBlockPeriods(byte[] block, int indexOffset, int blockLength, int pilot_t, int s1_t, int s2_t, int zero_t, int one_t, int pilot_len, int pause, int last)
	{
		List<int> list = new List<int>();
		if (pilot_len > 0)
		{
			for (int i = 0; i < pilot_len; i++)
			{
				list.Add(pilot_t);
			}
			list.Add(s1_t);
			list.Add(s2_t);
		}
		int num = indexOffset;
		int num2 = 0;
		while (num2 < blockLength - 1)
		{
			for (byte b = 128; b != 0; b = (byte)(b >> 1))
			{
				list.Add(((block[num] & b) != 0) ? one_t : zero_t);
				list.Add(((block[num] & b) != 0) ? one_t : zero_t);
			}
			num2++;
			num++;
		}
		for (byte b2 = 128; b2 != (byte)(128 >> last); b2 = (byte)(b2 >> 1))
		{
			list.Add(((block[num] & b2) != 0) ? one_t : zero_t);
			list.Add(((block[num] & b2) != 0) ? one_t : zero_t);
		}
		if (pause != 0)
		{
			list.Add(pause * 3500);
		}
		return list;
	}

	public static string getBlockDescription(byte[] block, int indexOffset, int blockLength)
	{
		string empty = string.Empty;
		byte b = 0;
		byte[] array = new byte[10];
		for (int i = 0; i < blockLength; i++)
		{
			b = (byte)(b ^ block[indexOffset + i]);
		}
		if (block[indexOffset] != 0 || blockLength != 19 || (block[indexOffset + 1] != 0 && block[indexOffset + 1] != 3))
		{
			empty = ((block[indexOffset] != byte.MaxValue) ? string.Format("#{0} block, {1} bytes", block[indexOffset].ToString("X2"), blockLength - 2) : $"Data block, {blockLength - 2} bytes");
		}
		else
		{
			for (int j = 0; j < 10; j++)
			{
				array[j] = (byte)((block[indexOffset + j + 2] < 32 || block[indexOffset + j + 2] >= 128) ? 63 : block[indexOffset + j + 2]);
			}
			string @string = Encoding.ASCII.GetString(array, 0, 10);
			string text = ((block[indexOffset + 1] != 0) ? "Bytes" : "Program");
			empty = $"{text}: \"{@string}\" {FormatSerializer.getUInt16(block, indexOffset + 14)},{FormatSerializer.getUInt16(block, indexOffset + 12)}";
		}
		return empty + string.Format(", crc {0}", (b != 0) ? "bad" : "ok");
	}
}
