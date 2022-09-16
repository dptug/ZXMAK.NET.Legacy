using System.Collections.Generic;
using System.IO;
using System.Text;
using ZXMAK.Engine.Tape;
using ZXMAK.Platform;

namespace ZXMAK.Engine.Loaders.TapeSerializers;

public class TzxSerializer : FormatSerializer
{
	private TapeDevice _tape;

	public override string FormatGroup => "Tape images";

	public override string FormatName => "TZX image";

	public override string FormatExtension => "TZX";

	public override bool CanDeserialize => true;

	public TzxSerializer(TapeDevice tape)
	{
		_tape = tape;
	}

	public override void Deserialize(Stream stream)
	{
		byte[] array = new byte[stream.Length];
		stream.Read(array, 0, array.Length);
		if (Encoding.ASCII.GetString(array, 0, 7) != "ZXTape!" || array[7] != 26)
		{
			PlatformFactory.Platform.ShowWarning("Invalid TZX file, identifier not found! ", "TZX loader");
			return;
		}
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		TapeBlock tapeBlock = null;
		while (num < array.Length)
		{
			switch (array[num++])
			{
			case 16:
			{
				tapeBlock = new TapeBlock();
				int num4 = FormatSerializer.getUInt16(array, num + 2);
				int uInt = FormatSerializer.getUInt16(array, num);
				num += 4;
				tapeBlock.Description = TapSerializer.getBlockDescription(array, num, num4);
				tapeBlock.Periods = TapSerializer.getBlockPeriods(array, num, num4, 2168, 667, 735, 855, 1710, (array[num] < 4) ? 8064 : 3220, uInt, 8);
				num += num4;
				_tape.Blocks.Add(tapeBlock);
				tapeBlock = null;
				break;
			}
			case 17:
			{
				tapeBlock = new TapeBlock();
				int num4 = 0xFFFFFF & FormatSerializer.getInt32(array, num + 15);
				tapeBlock.Description = TapSerializer.getBlockDescription(array, num + 18, num4);
				tapeBlock.Periods = TapSerializer.getBlockPeriods(array, num + 18, num4, FormatSerializer.getUInt16(array, num), FormatSerializer.getUInt16(array, num + 2), FormatSerializer.getUInt16(array, num + 4), FormatSerializer.getUInt16(array, num + 6), FormatSerializer.getUInt16(array, num + 8), FormatSerializer.getUInt16(array, num + 10), FormatSerializer.getUInt16(array, num + 13), array[num + 12]);
				num += num4 + 18;
				_tape.Blocks.Add(tapeBlock);
				tapeBlock = null;
				break;
			}
			case 18:
			{
				tapeBlock = new TapeBlock();
				tapeBlock.Description = "Pure Tone";
				int uInt2 = FormatSerializer.getUInt16(array, num);
				int num5 = FormatSerializer.getUInt16(array, num + 2);
				tapeBlock.Periods = new List<int>(num5);
				for (int i = 0; i < num5; i++)
				{
					tapeBlock.Periods.Add(uInt2);
				}
				num += 4;
				_tape.Blocks.Add(tapeBlock);
				tapeBlock = null;
				break;
			}
			case 19:
			{
				tapeBlock = new TapeBlock();
				tapeBlock.Description = "Pulse sequence";
				int num5 = array[num++];
				tapeBlock.Periods = new List<int>(num5);
				int i = 0;
				while (i < num5)
				{
					tapeBlock.Periods.Add(FormatSerializer.getUInt16(array, num));
					i++;
					num += 2;
				}
				_tape.Blocks.Add(tapeBlock);
				tapeBlock = null;
				break;
			}
			case 20:
			{
				tapeBlock = new TapeBlock();
				tapeBlock.Description = "Pure Data Block";
				int num4 = 0xFFFFFF & FormatSerializer.getInt32(array, num + 7);
				tapeBlock.Periods = TapSerializer.getBlockPeriods(array, num + 10, num4, 0, 0, 0, FormatSerializer.getUInt16(array, num), FormatSerializer.getUInt16(array, num + 2), -1, FormatSerializer.getUInt16(array, num + 5), array[num + 4]);
				num += num4 + 10;
				_tape.Blocks.Add(tapeBlock);
				break;
			}
			case 21:
			{
				tapeBlock = new TapeBlock();
				tapeBlock.Description = "Direct Recording";
				int num4 = 0xFFFFFF & FormatSerializer.getInt32(array, num + 5);
				int uInt3 = FormatSerializer.getUInt16(array, num);
				int uInt = FormatSerializer.getUInt16(array, num + 2);
				int num7 = array[num + 4];
				num += 8;
				int uInt2 = 0;
				int num5 = 0;
				int i;
				for (i = 0; i < num4; i++)
				{
					for (int num8 = 128; num8 != 0; num8 >>= 1)
					{
						if (((array[num + i] ^ uInt2) & num8) != 0)
						{
							num5++;
							uInt2 ^= -1;
						}
					}
				}
				int num9 = 0;
				uInt2 = 0;
				tapeBlock.Periods = new List<int>(num5 + 2);
				i = 1;
				while (i < num4)
				{
					for (int num8 = 128; num8 != 0; num8 >>= 1)
					{
						num9 += uInt3;
						if (((array[num] ^ uInt2) & num8) != 0)
						{
							tapeBlock.Periods.Add(num9);
							uInt2 ^= -1;
							num9 = 0;
						}
					}
					i++;
					num++;
				}
				for (int num8 = 128; num8 != (byte)(128 >> num7); num8 >>= 1)
				{
					num9 += uInt3;
					if (((array[num] ^ uInt2) & num8) != 0)
					{
						tapeBlock.Periods.Add(num9);
						uInt2 ^= -1;
						num9 = 0;
					}
				}
				num++;
				tapeBlock.Periods.Add(num9);
				if (uInt != 0)
				{
					tapeBlock.Periods.Add(uInt * 3500);
				}
				_tape.Blocks.Add(tapeBlock);
				break;
			}
			case 32:
			{
				tapeBlock = new TapeBlock();
				int uInt = FormatSerializer.getUInt16(array, num);
				tapeBlock.Description = ((uInt != 0) ? ("[Pause " + uInt + " ms]") : "[Stop the Tape]");
				tapeBlock.Periods = new List<int>(2);
				num += 2;
				if (uInt == 0)
				{
					tapeBlock.Command = TapeCommand.STOP_THE_TAPE;
					tapeBlock.Periods.Add(3500);
					uInt = -1;
				}
				else
				{
					uInt *= 3500;
				}
				tapeBlock.Periods.Add(uInt);
				_tape.Blocks.Add(tapeBlock);
				break;
			}
			case 33:
			{
				tapeBlock = new TapeBlock();
				int num5 = array[num++];
				tapeBlock.Description = "[GROUP: " + Encoding.ASCII.GetString(array, num, num5) + "]";
				tapeBlock.Command = TapeCommand.BEGIN_GROUP;
				num += num5;
				tapeBlock.Periods = new List<int>();
				_tape.Blocks.Add(tapeBlock);
				break;
			}
			case 34:
				tapeBlock = new TapeBlock();
				tapeBlock.Description = "[END GROUP]";
				tapeBlock.Command = TapeCommand.END_GROUP;
				tapeBlock.Periods = new List<int>();
				_tape.Blocks.Add(tapeBlock);
				break;
			case 35:
				tapeBlock = new TapeBlock();
				tapeBlock.Description = "[JUMP TO BLOCK " + FormatSerializer.getUInt16(array, num) + "]";
				tapeBlock.Periods = new List<int>();
				num += 2;
				_tape.Blocks.Add(tapeBlock);
				break;
			case 36:
				num2 = FormatSerializer.getUInt16(array, num);
				num3 = _tape.Blocks.Count;
				num += 2;
				break;
			case 37:
				if (num2 != 0)
				{
					int num4 = _tape.Blocks.Count - num3;
					for (int j = 0; j < num4; j++)
					{
						_tape.Blocks.Add(_tape.Blocks[num2 + j]);
					}
					num2 = 0;
				}
				break;
			case 38:
				tapeBlock = new TapeBlock();
				tapeBlock.Description = "[CALL SEQUENCE]";
				tapeBlock.Periods = new List<int>();
				num += 2 + 2 * FormatSerializer.getUInt16(array, num);
				_tape.Blocks.Add(tapeBlock);
				break;
			case 39:
				tapeBlock = new TapeBlock();
				tapeBlock.Description = "[RETURN SEQUENCE]";
				tapeBlock.Periods = new List<int>();
				_tape.Blocks.Add(tapeBlock);
				break;
			case 40:
				tapeBlock = new TapeBlock();
				tapeBlock.Description = "[SELECT BLOCK]";
				tapeBlock.Periods = new List<int>();
				num += 2 + FormatSerializer.getUInt16(array, num);
				_tape.Blocks.Add(tapeBlock);
				break;
			case 42:
				tapeBlock = new TapeBlock();
				tapeBlock.Description = "[Stop tape if in 48K mode]";
				tapeBlock.Command = TapeCommand.STOP_THE_TAPE_48K;
				tapeBlock.Periods = new List<int>();
				num += 4 + FormatSerializer.getUInt16(array, num);
				_tape.Blocks.Add(tapeBlock);
				break;
			case 48:
			{
				tapeBlock = new TapeBlock();
				int num5 = array[num++];
				tapeBlock.Description = "[" + Encoding.ASCII.GetString(array, num, num5) + "]";
				tapeBlock.Periods = new List<int>();
				num += num5;
				_tape.Blocks.Add(tapeBlock);
				break;
			}
			case 49:
			{
				tapeBlock = new TapeBlock();
				num++;
				int num5 = array[num++];
				tapeBlock.Description = "[Message: " + Encoding.ASCII.GetString(array, num, num5) + "]";
				tapeBlock.Command = TapeCommand.SHOW_MESSAGE;
				tapeBlock.Periods = new List<int>();
				num += num5;
				_tape.Blocks.Add(tapeBlock);
				break;
			}
			case 50:
			{
				tapeBlock = new TapeBlock();
				tapeBlock.Description = "Archive info";
				tapeBlock.Periods = new List<int>();
				int num6 = num + 3;
				for (int i = 0; i < array[num + 2]; i++)
				{
					string arg = array[num6++] switch
					{
						0 => "Full title", 
						1 => "Publisher", 
						2 => "Author", 
						3 => "Year", 
						4 => "Language", 
						5 => "Type", 
						6 => "Price", 
						7 => "Protection", 
						8 => "Origin", 
						byte.MaxValue => "Comment", 
						_ => "info", 
					};
					int num4 = array[num6++];
					tapeBlock.Description = $"{arg}: {Encoding.ASCII.GetString(array, num6, num4)}";
					num6 += num4;
				}
				num += 2 + FormatSerializer.getUInt16(array, num);
				_tape.Blocks.Add(tapeBlock);
				break;
			}
			case 51:
			{
				tapeBlock = new TapeBlock();
				int num5 = array[num++];
				tapeBlock.Description = "[HARDWARE TYPE]";
				tapeBlock.Periods = new List<int>();
				num += 3 * num5;
				_tape.Blocks.Add(tapeBlock);
				break;
			}
			case 52:
				tapeBlock = new TapeBlock();
				tapeBlock.Description = "[EMULATION INFO]";
				tapeBlock.Periods = new List<int>();
				num += 8;
				_tape.Blocks.Add(tapeBlock);
				break;
			case 53:
				tapeBlock = new TapeBlock();
				tapeBlock.Description = "[CUSTOM INFO - " + Encoding.ASCII.GetString(array, num, 10) + "]";
				num += 10;
				tapeBlock.Periods = new List<int>();
				num += 2 + FormatSerializer.getUInt16(array, num);
				_tape.Blocks.Add(tapeBlock);
				break;
			case 64:
			{
				tapeBlock = new TapeBlock();
				tapeBlock.Description = "[SNAPSHOT - ";
				if (array[num] == 0)
				{
					tapeBlock.Description += ".Z80]";
				}
				else if (array[num] == 1)
				{
					tapeBlock.Description += ".SNA]";
				}
				else
				{
					tapeBlock.Description += "???]";
				}
				num++;
				int num4 = array[num] | (array[num + 1] << 8) | (array[num + 2] << 16);
				num += 3;
				tapeBlock.Periods = new List<int>();
				num += num4;
				_tape.Blocks.Add(tapeBlock);
				break;
			}
			case 90:
				num += 9;
				break;
			default:
				tapeBlock = new TapeBlock();
				tapeBlock.Description = "[UNKNOWN BLOCK 0x" + array[num - 1].ToString("X2") + "]";
				tapeBlock.Periods = new List<int>();
				num += FormatSerializer.getInt32(array, num) & 0xFFFFFF;
				num += 4;
				_tape.Blocks.Add(tapeBlock);
				break;
			}
		}
	}
}
