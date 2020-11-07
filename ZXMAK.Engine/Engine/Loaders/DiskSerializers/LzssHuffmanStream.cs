using System;
using System.IO;

namespace ZXMAK.Engine.Loaders.DiskSerializers
{
	public class LzssHuffmanStream : Stream
	{
		public LzssHuffmanStream(Stream stream)
		{
			this._input = stream;
			this.init_Decode();
		}

		public override bool CanRead
		{
			get
			{
				return true;
			}
		}

		public override bool CanSeek
		{
			get
			{
				return false;
			}
		}

		public override bool CanWrite
		{
			get
			{
				return false;
			}
		}

		public override long Length
		{
			get
			{
				throw new InvalidOperationException("Not supported operation");
			}
		}

		public override long Position
		{
			get
			{
				throw new InvalidOperationException("Not supported operation");
			}
			set
			{
				throw new InvalidOperationException("Not supported operation");
			}
		}

		public override void Flush()
		{
			throw new InvalidOperationException("Not supported operation");
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new InvalidOperationException("Not supported operation");
		}

		public override void SetLength(long value)
		{
			throw new InvalidOperationException("Not supported operation");
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			throw new InvalidOperationException("Not supported operation");
		}

		public override int Read(byte[] buffer, int offset, int length)
		{
			return this.Decode(buffer, offset, length);
		}

		private void init_Decode()
		{
			this._bitCounter = 0;
			this._bufcnt = 0;
			this.StartHuff();
			for (int i = 0; i < 4036; i++)
			{
				this.text_buf[i] = 32;
			}
			this._r = 4036;
		}

		private void StartHuff()
		{
			int i;
			for (i = 0; i < 314; i++)
			{
				this.freq[i] = 1;
				this.son[i] = (short)(i + 627);
				this.prnt[i + 627] = (short)i;
			}
			i = 0;
			for (int j = 314; j <= 626; j++)
			{
				this.freq[j] = (ushort)(this.freq[i] + this.freq[i + 1]);
				this.son[j] = (short)i;
				this.prnt[i] = (this.prnt[i + 1] = (short)j);
				i += 2;
			}
			this.freq[627] = ushort.MaxValue;
			this.prnt[626] = 0;
		}

		private int Decode(byte[] buf, int startIndex, int len)
		{
			int i = 0;
			while (i < len)
			{
				if (this._bufcnt == 0)
				{
					short num;
					if ((num = this.DecodeChar(this._input)) < 0)
					{
						return i;
					}
					if (num < 256)
					{
						buf[startIndex++] = (byte)num;
						byte[] array = this.text_buf;
						ushort r;
						this._r = (ushort)((r = this._r) + 1);
						array[(int)r] = (byte)num;
						this._r &= 4095;
						i++;
					}
					else
					{
						short num2;
						if ((num2 = this.DecodePosition(this._input)) < 0)
						{
							return i;
						}
						this._bufpos = (ushort)((this._r - (ushort)num2 - 1 & 4095));
						this._bufcnt = (ushort)(num - 255 + 2);
						this._bufndx = 0;
					}
				}
				else
				{
					while (this._bufndx < this._bufcnt && i < len)
					{
						short num = (short)this.text_buf[(int)(this._bufpos + this._bufndx & 4095)];
						buf[startIndex++] = (byte)num;
						this._bufndx += 1;
						byte[] array2 = this.text_buf;
						ushort r2;
						this._r = (ushort)((r2 = this._r) + 1);
						array2[(int)r2] = (byte)num;
						this._r &= 4095;
						i++;
					}
					if (this._bufndx >= this._bufcnt)
					{
						this._bufndx = (this._bufcnt = 0);
					}
				}
			}
			return i;
		}

		private short DecodeChar(Stream stream)
		{
			ushort num;
			for (num = (ushort)this.son[626]; num < 627; num = (ushort)this.son[(int)num])
			{
				int bit;
				if ((bit = this.GetBit(stream)) < 0)
				{
					return -1;
				}
				num += (ushort)bit;
			}
			num -= 627;
			this.update((int)num);
			return (short)num;
		}

		private short DecodePosition(Stream stream)
		{
			short num;
			if ((num = (short)this.GetByte(stream)) < 0)
			{
				return -1;
			}
			ushort num2 = (ushort)num;
			ushort num3 = (ushort)(LzssHuffmanStream.d_code[(int)num2] << 6);
			ushort num4 = (ushort)LzssHuffmanStream.d_len[(int)num2];
			num4 -= 2;
			for (;;)
			{
				ushort num5 = num4;
				num4 = (ushort)(num5 - 1);
				if (num5 == 0)
				{
					goto Block_3;
				}
				if ((num = (short)this.GetBit(stream)) < 0)
				{
					break;
				}
				num2 = (ushort)(((int)num2 << 1) + (int)num);
			}
			return -1;
			Block_3:
			return (short)(num3 | (num2 & 63));
		}

		private int GetBit(Stream stream)
		{
			if (this._bitCounter < 1)
			{
				this._bitValue = stream.ReadByte();
				if (this._bitValue < 0)
				{
					return -1;
				}
				this._bitMask = 128;
				this._bitCounter = 8;
			}
			int num = this._bitValue & this._bitMask;
			this._bitMask >>= 1;
			this._bitCounter--;
			if (num == 0)
			{
				return 0;
			}
			return 1;
		}

		private int GetByte(Stream stream)
		{
			int num = 0;
			for (int i = 0; i < 8; i++)
			{
				int bit = this.GetBit(stream);
				if (bit < 0)
				{
					return -1;
				}
				num = (num << 1 | bit);
			}
			return num;
		}

		private void update(int c)
		{
			if (this.freq[626] == 32768)
			{
				this.reconst();
			}
			c = (int)this.prnt[c + 627];
			do
			{
				ushort[] array = this.freq;
				int num = c;
				int i = (int)(array[num] += 1);
				int num2;
				if (i > (int)this.freq[num2 = c + 1])
				{
					while (i > (int)this.freq[++num2])
					{
					}
					num2--;
					this.freq[c] = this.freq[num2];
					this.freq[num2] = (ushort)i;
					int num3 = (int)this.son[c];
					this.prnt[num3] = (short)num2;
					if (num3 < 627)
					{
						this.prnt[num3 + 1] = (short)num2;
					}
					int num4 = (int)this.son[num2];
					this.son[num2] = (short)num3;
					this.prnt[num4] = (short)c;
					if (num4 < 627)
					{
						this.prnt[num4 + 1] = (short)c;
					}
					this.son[c] = (short)num4;
					c = num2;
				}
			}
			while ((c = (int)this.prnt[c]) != 0);
		}

		private void reconst()
		{
			short num = 0;
			short num2;
			for (num2 = 0; num2 < 627; num2 += 1)
			{
				if (this.son[(int)num2] >= 627)
				{
					this.freq[(int)num] = (ushort)((this.freq[(int)num2] + 1) / 2);
					this.son[(int)num] = this.son[(int)num2];
					num += 1;
				}
			}
			num2 = 0;
			for (num = 314; num < 627; num += 1)
			{
				short num3 = (short)(num2 + 1);
				ushort num4 = this.freq[(int)num] = (ushort)(this.freq[(int)num2] + this.freq[(int)num3]);
				num3 = (short)(num - 1);
				while (num4 < this.freq[(int)num3])
				{
					num3 -= 1;
				}
				num3 += 1;
				ushort length = (ushort)((num - num3) * 2);
				LzssHuffmanStream.movmem(this.freq, (int)num3, this.freq, (int)(num3 + 1), (int)length);
				this.freq[(int)num3] = num4;
				LzssHuffmanStream.movmem(this.son, (int)num3, this.son, (int)(num3 + 1), (int)length);
				this.son[(int)num3] = num2;
				num2 += 2;
			}
			for (num2 = 0; num2 < 627; num2 += 1)
			{
				short num3;
				if ((num3 = this.son[(int)num2]) >= 627)
				{
					this.prnt[(int)num3] = num2;
				}
				else
				{
					this.prnt[(int)num3] = (this.prnt[(int)(num3 + 1)] = num2);
				}
			}
		}

		private static void movmem(Array src, int srcIndex, Array dest, int destIndex, int length)
		{
			if (srcIndex > destIndex)
			{
				while (length-- > 0)
				{
					dest.SetValue(src.GetValue(srcIndex++), destIndex++);
				}
				return;
			}
			if (srcIndex < destIndex)
			{
				srcIndex += length;
				destIndex += length;
				while (length-- > 0)
				{
					dest.SetValue(src.GetValue(--srcIndex), --destIndex);
				}
			}
		}

		private const string _invalidCallString = "Not supported operation";

		private const int N = 4096;

		private const int F = 60;

		private const int THRESHOLD = 2;

		private const int NIL = 4096;

		private const int N_CHAR = 314;

		private const int T = 627;

		private const int R = 626;

		private const int MAX_FREQ = 32768;

		private Stream _input;

		private byte[] text_buf = new byte[4155];

		private short[] lson = new short[4097];

		private short[] rson = new short[4353];

		private short[] dad = new short[4097];

		private ushort[] freq = new ushort[628];

		private short[] prnt = new short[941];

		private short[] son = new short[627];

		public ushort _r;

		public ushort _bufcnt;

		public ushort _bufndx;

		public ushort _bufpos;

		private int _bitCounter;

		private int _bitValue;

		private int _bitMask;

		private static byte[] d_code = new byte[]
		{
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			2,
			2,
			2,
			2,
			2,
			2,
			2,
			2,
			2,
			2,
			2,
			2,
			2,
			2,
			2,
			2,
			3,
			3,
			3,
			3,
			3,
			3,
			3,
			3,
			3,
			3,
			3,
			3,
			3,
			3,
			3,
			3,
			4,
			4,
			4,
			4,
			4,
			4,
			4,
			4,
			5,
			5,
			5,
			5,
			5,
			5,
			5,
			5,
			6,
			6,
			6,
			6,
			6,
			6,
			6,
			6,
			7,
			7,
			7,
			7,
			7,
			7,
			7,
			7,
			8,
			8,
			8,
			8,
			8,
			8,
			8,
			8,
			9,
			9,
			9,
			9,
			9,
			9,
			9,
			9,
			10,
			10,
			10,
			10,
			10,
			10,
			10,
			10,
			11,
			11,
			11,
			11,
			11,
			11,
			11,
			11,
			12,
			12,
			12,
			12,
			13,
			13,
			13,
			13,
			14,
			14,
			14,
			14,
			15,
			15,
			15,
			15,
			16,
			16,
			16,
			16,
			17,
			17,
			17,
			17,
			18,
			18,
			18,
			18,
			19,
			19,
			19,
			19,
			20,
			20,
			20,
			20,
			21,
			21,
			21,
			21,
			22,
			22,
			22,
			22,
			23,
			23,
			23,
			23,
			24,
			24,
			25,
			25,
			26,
			26,
			27,
			27,
			28,
			28,
			29,
			29,
			30,
			30,
			31,
			31,
			32,
			32,
			33,
			33,
			34,
			34,
			35,
			35,
			36,
			36,
			37,
			37,
			38,
			38,
			39,
			39,
			40,
			40,
			41,
			41,
			42,
			42,
			43,
			43,
			44,
			44,
			45,
			45,
			46,
			46,
			47,
			47,
			48,
			49,
			50,
			51,
			52,
			53,
			54,
			55,
			56,
			57,
			58,
			59,
			60,
			61,
			62,
			63
		};

		private static byte[] d_len = new byte[]
		{
			3,
			3,
			3,
			3,
			3,
			3,
			3,
			3,
			3,
			3,
			3,
			3,
			3,
			3,
			3,
			3,
			3,
			3,
			3,
			3,
			3,
			3,
			3,
			3,
			3,
			3,
			3,
			3,
			3,
			3,
			3,
			3,
			4,
			4,
			4,
			4,
			4,
			4,
			4,
			4,
			4,
			4,
			4,
			4,
			4,
			4,
			4,
			4,
			4,
			4,
			4,
			4,
			4,
			4,
			4,
			4,
			4,
			4,
			4,
			4,
			4,
			4,
			4,
			4,
			4,
			4,
			4,
			4,
			4,
			4,
			4,
			4,
			4,
			4,
			4,
			4,
			4,
			4,
			4,
			4,
			5,
			5,
			5,
			5,
			5,
			5,
			5,
			5,
			5,
			5,
			5,
			5,
			5,
			5,
			5,
			5,
			5,
			5,
			5,
			5,
			5,
			5,
			5,
			5,
			5,
			5,
			5,
			5,
			5,
			5,
			5,
			5,
			5,
			5,
			5,
			5,
			5,
			5,
			5,
			5,
			5,
			5,
			5,
			5,
			5,
			5,
			5,
			5,
			5,
			5,
			5,
			5,
			5,
			5,
			5,
			5,
			5,
			5,
			5,
			5,
			5,
			5,
			5,
			5,
			6,
			6,
			6,
			6,
			6,
			6,
			6,
			6,
			6,
			6,
			6,
			6,
			6,
			6,
			6,
			6,
			6,
			6,
			6,
			6,
			6,
			6,
			6,
			6,
			6,
			6,
			6,
			6,
			6,
			6,
			6,
			6,
			6,
			6,
			6,
			6,
			6,
			6,
			6,
			6,
			6,
			6,
			6,
			6,
			6,
			6,
			6,
			6,
			7,
			7,
			7,
			7,
			7,
			7,
			7,
			7,
			7,
			7,
			7,
			7,
			7,
			7,
			7,
			7,
			7,
			7,
			7,
			7,
			7,
			7,
			7,
			7,
			7,
			7,
			7,
			7,
			7,
			7,
			7,
			7,
			7,
			7,
			7,
			7,
			7,
			7,
			7,
			7,
			7,
			7,
			7,
			7,
			7,
			7,
			7,
			7,
			8,
			8,
			8,
			8,
			8,
			8,
			8,
			8,
			8,
			8,
			8,
			8,
			8,
			8,
			8,
			8
		};
	}
}
