using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ZXMAK.Engine.Tape;
using ZXMAK.Platform;

namespace ZXMAK.Engine.Loaders.TapeSerializers
{
	public class TzxSerializer : FormatSerializer
	{
		public TzxSerializer(TapeDevice tape)
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
				return "TZX image";
			}
		}

		public override string FormatExtension
		{
			get
			{
				return "TZX";
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
			byte[] array = new byte[stream.Length];
			stream.Read(array, 0, array.Length);
			if (Encoding.ASCII.GetString(array, 0, 7) != "ZXTape!" || array[7] != 26)
			{
				PlatformFactory.Platform.ShowWarning("Invalid TZX file, identifier not found! ", "TZX loader");
				return;
			}
			int i = 0;
			int num = 0;
			int num2 = 0;
			while (i < array.Length)
			{
				byte b = array[i++];
				TapeBlock tapeBlock;
				switch (b)
				{
				case 16:
				{
					tapeBlock = new TapeBlock();
					int num3 = (int)FormatSerializer.getUInt16(array, i + 2);
					int num4 = (int)FormatSerializer.getUInt16(array, i);
					i += 4;
					tapeBlock.Description = TapSerializer.getBlockDescription(array, i, num3);
					tapeBlock.Periods = TapSerializer.getBlockPeriods(array, i, num3, 2168, 667, 735, 855, 1710, (array[i] < 4) ? 8064 : 3220, num4, 8);
					i += num3;
					this._tape.Blocks.Add(tapeBlock);
					continue;
				}
				case 17:
				{
					tapeBlock = new TapeBlock();
					int num3 = 16777215 & FormatSerializer.getInt32(array, i + 15);
					tapeBlock.Description = TapSerializer.getBlockDescription(array, i + 18, num3);
					tapeBlock.Periods = TapSerializer.getBlockPeriods(array, i + 18, num3, (int)FormatSerializer.getUInt16(array, i), (int)FormatSerializer.getUInt16(array, i + 2), (int)FormatSerializer.getUInt16(array, i + 4), (int)FormatSerializer.getUInt16(array, i + 6), (int)FormatSerializer.getUInt16(array, i + 8), (int)FormatSerializer.getUInt16(array, i + 10), (int)FormatSerializer.getUInt16(array, i + 13), (int)array[i + 12]);
					i += num3 + 18;
					this._tape.Blocks.Add(tapeBlock);
					continue;
				}
				case 18:
				{
					tapeBlock = new TapeBlock();
					tapeBlock.Description = "Pure Tone";
					int num5 = (int)FormatSerializer.getUInt16(array, i);
					int num6 = (int)FormatSerializer.getUInt16(array, i + 2);
					tapeBlock.Periods = new List<int>(num6);
					for (int j = 0; j < num6; j++)
					{
						tapeBlock.Periods.Add(num5);
					}
					i += 4;
					this._tape.Blocks.Add(tapeBlock);
					continue;
				}
				case 19:
				{
					tapeBlock = new TapeBlock();
					tapeBlock.Description = "Pulse sequence";
					int num6 = (int)array[i++];
					tapeBlock.Periods = new List<int>(num6);
					int j = 0;
					while (j < num6)
					{
						tapeBlock.Periods.Add((int)FormatSerializer.getUInt16(array, i));
						j++;
						i += 2;
					}
					this._tape.Blocks.Add(tapeBlock);
					continue;
				}
				case 20:
				{
					tapeBlock = new TapeBlock();
					tapeBlock.Description = "Pure Data Block";
					int num3 = 16777215 & FormatSerializer.getInt32(array, i + 7);
					tapeBlock.Periods = TapSerializer.getBlockPeriods(array, i + 10, num3, 0, 0, 0, (int)FormatSerializer.getUInt16(array, i), (int)FormatSerializer.getUInt16(array, i + 2), -1, (int)FormatSerializer.getUInt16(array, i + 5), (int)array[i + 4]);
					i += num3 + 10;
					this._tape.Blocks.Add(tapeBlock);
					continue;
				}
				case 21:
				{
					tapeBlock = new TapeBlock();
					tapeBlock.Description = "Direct Recording";
					int num3 = 16777215 & FormatSerializer.getInt32(array, i + 5);
					int @uint = (int)FormatSerializer.getUInt16(array, i);
					int num4 = (int)FormatSerializer.getUInt16(array, i + 2);
					int num7 = (int)array[i + 4];
					i += 8;
					int num5 = 0;
					int num6 = 0;
					int j;
					for (j = 0; j < num3; j++)
					{
						for (int num8 = 128; num8 != 0; num8 >>= 1)
						{
							if ((((int)array[i + j] ^ num5) & num8) != 0)
							{
								num6++;
								num5 ^= -1;
							}
						}
					}
					int num9 = 0;
					num5 = 0;
					tapeBlock.Periods = new List<int>(num6 + 2);
					j = 1;
					while (j < num3)
					{
						for (int num8 = 128; num8 != 0; num8 >>= 1)
						{
							num9 += @uint;
							if ((((int)array[i] ^ num5) & num8) != 0)
							{
								tapeBlock.Periods.Add(num9);
								num5 ^= -1;
								num9 = 0;
							}
						}
						j++;
						i++;
					}
					for (int num8 = 128; num8 != (int)((byte)(128 >> num7)); num8 >>= 1)
					{
						num9 += @uint;
						if ((((int)array[i] ^ num5) & num8) != 0)
						{
							tapeBlock.Periods.Add(num9);
							num5 ^= -1;
							num9 = 0;
						}
					}
					i++;
					tapeBlock.Periods.Add(num9);
					if (num4 != 0)
					{
						tapeBlock.Periods.Add(num4 * 3500);
					}
					this._tape.Blocks.Add(tapeBlock);
					continue;
				}
				case 22:
				case 23:
				case 24:
				case 25:
				case 26:
				case 27:
				case 28:
				case 29:
				case 30:
				case 31:
				case 41:
				case 43:
				case 44:
				case 45:
				case 46:
				case 47:
					break;
				case 32:
				{
					tapeBlock = new TapeBlock();
					int num4 = (int)FormatSerializer.getUInt16(array, i);
					tapeBlock.Description = ((num4 != 0) ? ("[Pause " + num4 + " ms]") : "[Stop the Tape]");
					tapeBlock.Periods = new List<int>(2);
					i += 2;
					if (num4 == 0)
					{
						tapeBlock.Command = TapeCommand.STOP_THE_TAPE;
						tapeBlock.Periods.Add(3500);
						num4 = -1;
					}
					else
					{
						num4 *= 3500;
					}
					tapeBlock.Periods.Add(num4);
					this._tape.Blocks.Add(tapeBlock);
					continue;
				}
				case 33:
				{
					tapeBlock = new TapeBlock();
					int num6 = (int)array[i++];
					tapeBlock.Description = "[GROUP: " + Encoding.ASCII.GetString(array, i, num6) + "]";
					tapeBlock.Command = TapeCommand.BEGIN_GROUP;
					i += num6;
					tapeBlock.Periods = new List<int>();
					this._tape.Blocks.Add(tapeBlock);
					continue;
				}
				case 34:
					tapeBlock = new TapeBlock();
					tapeBlock.Description = "[END GROUP]";
					tapeBlock.Command = TapeCommand.END_GROUP;
					tapeBlock.Periods = new List<int>();
					this._tape.Blocks.Add(tapeBlock);
					continue;
				case 35:
					tapeBlock = new TapeBlock();
					tapeBlock.Description = "[JUMP TO BLOCK " + FormatSerializer.getUInt16(array, i) + "]";
					tapeBlock.Periods = new List<int>();
					i += 2;
					this._tape.Blocks.Add(tapeBlock);
					continue;
				case 36:
					num = (int)FormatSerializer.getUInt16(array, i);
					num2 = this._tape.Blocks.Count;
					i += 2;
					continue;
				case 37:
					if (num != 0)
					{
						int num3 = this._tape.Blocks.Count - num2;
						for (int k = 0; k < num3; k++)
						{
							this._tape.Blocks.Add(this._tape.Blocks[num + k]);
						}
						num = 0;
						continue;
					}
					continue;
				case 38:
					tapeBlock = new TapeBlock();
					tapeBlock.Description = "[CALL SEQUENCE]";
					tapeBlock.Periods = new List<int>();
					i += (int)(2 + 2 * FormatSerializer.getUInt16(array, i));
					this._tape.Blocks.Add(tapeBlock);
					continue;
				case 39:
					tapeBlock = new TapeBlock();
					tapeBlock.Description = "[RETURN SEQUENCE]";
					tapeBlock.Periods = new List<int>();
					this._tape.Blocks.Add(tapeBlock);
					continue;
				case 40:
					tapeBlock = new TapeBlock();
					tapeBlock.Description = "[SELECT BLOCK]";
					tapeBlock.Periods = new List<int>();
					i += (int)(2 + FormatSerializer.getUInt16(array, i));
					this._tape.Blocks.Add(tapeBlock);
					continue;
				case 42:
					tapeBlock = new TapeBlock();
					tapeBlock.Description = "[Stop tape if in 48K mode]";
					tapeBlock.Command = TapeCommand.STOP_THE_TAPE_48K;
					tapeBlock.Periods = new List<int>();
					i += (int)(4 + FormatSerializer.getUInt16(array, i));
					this._tape.Blocks.Add(tapeBlock);
					continue;
				case 48:
				{
					tapeBlock = new TapeBlock();
					int num6 = (int)array[i++];
					tapeBlock.Description = "[" + Encoding.ASCII.GetString(array, i, num6) + "]";
					tapeBlock.Periods = new List<int>();
					i += num6;
					this._tape.Blocks.Add(tapeBlock);
					continue;
				}
				case 49:
				{
					tapeBlock = new TapeBlock();
					i++;
					int num6 = (int)array[i++];
					tapeBlock.Description = "[Message: " + Encoding.ASCII.GetString(array, i, num6) + "]";
					tapeBlock.Command = TapeCommand.SHOW_MESSAGE;
					tapeBlock.Periods = new List<int>();
					i += num6;
					this._tape.Blocks.Add(tapeBlock);
					continue;
				}
				case 50:
				{
					tapeBlock = new TapeBlock();
					tapeBlock.Description = "Archive info";
					tapeBlock.Periods = new List<int>();
					int num10 = i + 3;
					for (int j = 0; j < (int)array[i + 2]; j++)
					{
						byte b2 = array[num10++];
						string arg;
						switch (b2)
						{
						case 0:
							arg = "Full title";
							break;
						case 1:
							arg = "Publisher";
							break;
						case 2:
							arg = "Author";
							break;
						case 3:
							arg = "Year";
							break;
						case 4:
							arg = "Language";
							break;
						case 5:
							arg = "Type";
							break;
						case 6:
							arg = "Price";
							break;
						case 7:
							arg = "Protection";
							break;
						case 8:
							arg = "Origin";
							break;
						default:
							if (b2 != 255)
							{
								arg = "info";
							}
							else
							{
								arg = "Comment";
							}
							break;
						}
						int num3 = (int)array[num10++];
						tapeBlock.Description = string.Format("{0}: {1}", arg, Encoding.ASCII.GetString(array, num10, num3));
						num10 += num3;
					}
					i += (int)(2 + FormatSerializer.getUInt16(array, i));
					this._tape.Blocks.Add(tapeBlock);
					continue;
				}
				case 51:
				{
					tapeBlock = new TapeBlock();
					int num6 = (int)array[i++];
					tapeBlock.Description = "[HARDWARE TYPE]";
					tapeBlock.Periods = new List<int>();
					i += 3 * num6;
					this._tape.Blocks.Add(tapeBlock);
					continue;
				}
				case 52:
					tapeBlock = new TapeBlock();
					tapeBlock.Description = "[EMULATION INFO]";
					tapeBlock.Periods = new List<int>();
					i += 8;
					this._tape.Blocks.Add(tapeBlock);
					continue;
				case 53:
					tapeBlock = new TapeBlock();
					tapeBlock.Description = "[CUSTOM INFO - " + Encoding.ASCII.GetString(array, i, 10) + "]";
					i += 10;
					tapeBlock.Periods = new List<int>();
					i += (int)(2 + FormatSerializer.getUInt16(array, i));
					this._tape.Blocks.Add(tapeBlock);
					continue;
				default:
					if (b == 64)
					{
						tapeBlock = new TapeBlock();
						tapeBlock.Description = "[SNAPSHOT - ";
						if (array[i] == 0)
						{
							TapeBlock tapeBlock2 = tapeBlock;
							tapeBlock2.Description += ".Z80]";
						}
						else if (array[i] == 1)
						{
							TapeBlock tapeBlock3 = tapeBlock;
							tapeBlock3.Description += ".SNA]";
						}
						else
						{
							TapeBlock tapeBlock4 = tapeBlock;
							tapeBlock4.Description += "???]";
						}
						i++;
						int num3 = (int)array[i] | (int)array[i + 1] << 8 | (int)array[i + 2] << 16;
						i += 3;
						tapeBlock.Periods = new List<int>();
						i += num3;
						this._tape.Blocks.Add(tapeBlock);
						continue;
					}
					if (b == 90)
					{
						i += 9;
						continue;
					}
					break;
				}
				tapeBlock = new TapeBlock();
				tapeBlock.Description = "[UNKNOWN BLOCK 0x" + array[i - 1].ToString("X2") + "]";
				tapeBlock.Periods = new List<int>();
				i += (FormatSerializer.getInt32(array, i) & 16777215);
				i += 4;
				this._tape.Blocks.Add(tapeBlock);
			}
		}

		private TapeDevice _tape;
	}
}
