using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using ZXMAK.Engine.Tape;
using ZXMAK.Platform;

namespace ZXMAK.Engine.Loaders.TapeSerializers
{
	public class CswSerializer : FormatSerializer
	{
		public CswSerializer(TapeDevice tape)
		{
			this._tape = tape;
		}

		public override string FormatGroup
		{
			get
			{
				return "Tape images";
			}
		}

		public override string FormatName
		{
			get
			{
				return "CSW image";
			}
		}

		public override string FormatExtension
		{
			get
			{
				return "CSW";
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
				PlatformFactory.Platform.ShowWarning(string.Concat(new string[]
				{
					"Format CSW V",
					array[23].ToString(),
					".",
					array[24].ToString(),
					" not supported!"
				}), "CSW loader");
				return;
			}
			if (array[23] == 2)
			{
				stream.Read(array, 32, 20);
				byte[] array2 = new byte[(int)array[35]];
				stream.Read(array2, 0, array2.Length);
			}
			int num = (array[23] == 2) ? BitConverter.ToInt32(array, 25) : ((int)BitConverter.ToUInt16(array, 25));
			int num2 = (int)((array[23] == 2) ? array[33] : array[27]);
			byte[] array3;
			if (array[23] == 2)
			{
				array3 = new byte[BitConverter.ToInt32(array, 29)];
			}
			else
			{
				array3 = new byte[stream.Length - 32L];
			}
			if (num2 == 1)
			{
				stream.Read(array3, 0, array3.Length);
			}
			else
			{
				if (num2 != 2)
				{
					PlatformFactory.Platform.ShowWarning("Unknown compression type!", "CSW loader");
					return;
				}
				this.csw2_uncompress(stream, array3);
			}
			int num3 = (int)(this._tape.Z80FQ / (ulong)((long)num));
			int i = 0;
			while (i < array3.Length)
			{
				int num4 = (int)array3[i++] * num3;
				if (num4 == 0)
				{
					num4 = BitConverter.ToInt32(array3, i) / num3;
					i += 4;
				}
				list.Add(num4);
			}
			list.Add((int)(this._tape.Z80FQ / 10UL));
			TapeBlock tapeBlock = new TapeBlock();
			tapeBlock.Description = "CSW tape image";
			tapeBlock.Periods = list;
			this._tape.Blocks.Add(tapeBlock);
		}

		private int csw2_uncompress(Stream stream, byte[] buffer)
		{
			stream.ReadByte();
			stream.ReadByte();
			DeflateStream deflateStream = new DeflateStream(stream, CompressionMode.Decompress, false);
			return deflateStream.Read(buffer, 0, buffer.Length);
		}

		private TapeDevice _tape;
	}
}
