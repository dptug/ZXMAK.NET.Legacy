using System;
using System.IO;

namespace ZXMAK.Engine.Loaders.DiskSerializers;

public class LzssHuffmanStream : Stream
{
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

	private static byte[] d_code = new byte[256]
	{
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 1, 1, 1, 1, 1, 1, 1, 1,
		1, 1, 1, 1, 1, 1, 1, 1, 2, 2,
		2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
		2, 2, 2, 2, 3, 3, 3, 3, 3, 3,
		3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
		4, 4, 4, 4, 4, 4, 4, 4, 5, 5,
		5, 5, 5, 5, 5, 5, 6, 6, 6, 6,
		6, 6, 6, 6, 7, 7, 7, 7, 7, 7,
		7, 7, 8, 8, 8, 8, 8, 8, 8, 8,
		9, 9, 9, 9, 9, 9, 9, 9, 10, 10,
		10, 10, 10, 10, 10, 10, 11, 11, 11, 11,
		11, 11, 11, 11, 12, 12, 12, 12, 13, 13,
		13, 13, 14, 14, 14, 14, 15, 15, 15, 15,
		16, 16, 16, 16, 17, 17, 17, 17, 18, 18,
		18, 18, 19, 19, 19, 19, 20, 20, 20, 20,
		21, 21, 21, 21, 22, 22, 22, 22, 23, 23,
		23, 23, 24, 24, 25, 25, 26, 26, 27, 27,
		28, 28, 29, 29, 30, 30, 31, 31, 32, 32,
		33, 33, 34, 34, 35, 35, 36, 36, 37, 37,
		38, 38, 39, 39, 40, 40, 41, 41, 42, 42,
		43, 43, 44, 44, 45, 45, 46, 46, 47, 47,
		48, 49, 50, 51, 52, 53, 54, 55, 56, 57,
		58, 59, 60, 61, 62, 63
	};

	private static byte[] d_len = new byte[256]
	{
		3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
		3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
		3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
		3, 3, 4, 4, 4, 4, 4, 4, 4, 4,
		4, 4, 4, 4, 4, 4, 4, 4, 4, 4,
		4, 4, 4, 4, 4, 4, 4, 4, 4, 4,
		4, 4, 4, 4, 4, 4, 4, 4, 4, 4,
		4, 4, 4, 4, 4, 4, 4, 4, 4, 4,
		5, 5, 5, 5, 5, 5, 5, 5, 5, 5,
		5, 5, 5, 5, 5, 5, 5, 5, 5, 5,
		5, 5, 5, 5, 5, 5, 5, 5, 5, 5,
		5, 5, 5, 5, 5, 5, 5, 5, 5, 5,
		5, 5, 5, 5, 5, 5, 5, 5, 5, 5,
		5, 5, 5, 5, 5, 5, 5, 5, 5, 5,
		5, 5, 5, 5, 6, 6, 6, 6, 6, 6,
		6, 6, 6, 6, 6, 6, 6, 6, 6, 6,
		6, 6, 6, 6, 6, 6, 6, 6, 6, 6,
		6, 6, 6, 6, 6, 6, 6, 6, 6, 6,
		6, 6, 6, 6, 6, 6, 6, 6, 6, 6,
		6, 6, 7, 7, 7, 7, 7, 7, 7, 7,
		7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
		7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
		7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
		7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
		8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
		8, 8, 8, 8, 8, 8
	};

	public override bool CanRead => true;

	public override bool CanSeek => false;

	public override bool CanWrite => false;

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

	public LzssHuffmanStream(Stream stream)
	{
		_input = stream;
		init_Decode();
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
		return Decode(buffer, offset, length);
	}

	private void init_Decode()
	{
		_bitCounter = 0;
		_bufcnt = 0;
		StartHuff();
		for (int i = 0; i < 4036; i++)
		{
			text_buf[i] = 32;
		}
		_r = 4036;
	}

	private void StartHuff()
	{
		int i;
		for (i = 0; i < 314; i++)
		{
			freq[i] = 1;
			son[i] = (short)(i + 627);
			prnt[i + 627] = (short)i;
		}
		i = 0;
		for (int j = 314; j <= 626; j++)
		{
			freq[j] = (ushort)(freq[i] + freq[i + 1]);
			son[j] = (short)i;
			prnt[i] = (prnt[i + 1] = (short)j);
			i += 2;
		}
		freq[627] = ushort.MaxValue;
		prnt[626] = 0;
	}

	private int Decode(byte[] buf, int startIndex, int len)
	{
		int num = 0;
		while (num < len)
		{
			if (_bufcnt == 0)
			{
				short num2;
				if ((num2 = DecodeChar(_input)) < 0)
				{
					return num;
				}
				if (num2 < 256)
				{
					buf[startIndex++] = (byte)num2;
					text_buf[_r++] = (byte)num2;
					_r &= 4095;
					num++;
					continue;
				}
				short num3;
				if ((num3 = DecodePosition(_input)) < 0)
				{
					return num;
				}
				_bufpos = (ushort)((uint)(_r - num3 - 1) & 0xFFFu);
				_bufcnt = (ushort)(num2 - 255 + 2);
				_bufndx = 0;
			}
			else
			{
				while (_bufndx < _bufcnt && num < len)
				{
					short num2 = text_buf[(_bufpos + _bufndx) & 0xFFF];
					buf[startIndex++] = (byte)num2;
					_bufndx++;
					text_buf[_r++] = (byte)num2;
					_r &= 4095;
					num++;
				}
				if (_bufndx >= _bufcnt)
				{
					_bufndx = (_bufcnt = 0);
				}
			}
		}
		return num;
	}

	private short DecodeChar(Stream stream)
	{
		ushort num;
		for (num = (ushort)son[626]; num < 627; num = (ushort)son[num])
		{
			int bit;
			if ((bit = GetBit(stream)) < 0)
			{
				return -1;
			}
			num = (ushort)(num + (ushort)bit);
		}
		num = (ushort)(num - 627);
		update(num);
		return (short)num;
	}

	private short DecodePosition(Stream stream)
	{
		short num;
		if ((num = (short)GetByte(stream)) < 0)
		{
			return -1;
		}
		ushort num2 = (ushort)num;
		ushort num3 = (ushort)(d_code[num2] << 6);
		ushort num4 = d_len[num2];
		num4 = (ushort)(num4 - 2);
		while (num4-- != 0)
		{
			if ((num = (short)GetBit(stream)) < 0)
			{
				return -1;
			}
			num2 = (ushort)((num2 << 1) + num);
		}
		return (short)(num3 | (num2 & 0x3F));
	}

	private int GetBit(Stream stream)
	{
		if (_bitCounter < 1)
		{
			_bitValue = stream.ReadByte();
			if (_bitValue < 0)
			{
				return -1;
			}
			_bitMask = 128;
			_bitCounter = 8;
		}
		int num = _bitValue & _bitMask;
		_bitMask >>= 1;
		_bitCounter--;
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
			int bit = GetBit(stream);
			if (bit < 0)
			{
				return -1;
			}
			num = (num << 1) | bit;
		}
		return num;
	}

	private void update(int c)
	{
		if (freq[626] == 32768)
		{
			reconst();
		}
		c = prnt[c + 627];
		do
		{
			int num = ++freq[c];
			int num2;
			if (num > freq[num2 = c + 1])
			{
				while (num > freq[++num2])
				{
				}
				num2--;
				freq[c] = freq[num2];
				freq[num2] = (ushort)num;
				int num3 = son[c];
				prnt[num3] = (short)num2;
				if (num3 < 627)
				{
					prnt[num3 + 1] = (short)num2;
				}
				int num4 = son[num2];
				son[num2] = (short)num3;
				prnt[num4] = (short)c;
				if (num4 < 627)
				{
					prnt[num4 + 1] = (short)c;
				}
				son[c] = (short)num4;
				c = num2;
			}
		}
		while ((c = prnt[c]) != 0);
	}

	private void reconst()
	{
		short num = 0;
		short num2;
		for (num2 = 0; num2 < 627; num2 = (short)(num2 + 1))
		{
			if (son[num2] >= 627)
			{
				freq[num] = (ushort)((freq[num2] + 1) / 2);
				son[num] = son[num2];
				num = (short)(num + 1);
			}
		}
		num2 = 0;
		for (num = 314; num < 627; num = (short)(num + 1))
		{
			short num3 = (short)(num2 + 1);
			ushort num4;
			freq[num] = (num4 = (ushort)(freq[num2] + freq[num3]));
			ushort num5 = num4;
			num3 = (short)(num - 1);
			while (num5 < freq[num3])
			{
				num3 = (short)(num3 - 1);
			}
			num3 = (short)(num3 + 1);
			ushort length = (ushort)((num - num3) * 2);
			movmem(freq, num3, freq, num3 + 1, length);
			freq[num3] = num5;
			movmem(son, num3, son, num3 + 1, length);
			son[num3] = num2;
			num2 = (short)(num2 + 2);
		}
		for (num2 = 0; num2 < 627; num2 = (short)(num2 + 1))
		{
			short num3;
			if ((num3 = son[num2]) >= 627)
			{
				prnt[num3] = num2;
			}
			else
			{
				prnt[num3] = (prnt[num3 + 1] = num2);
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
		}
		else if (srcIndex < destIndex)
		{
			srcIndex += length;
			destIndex += length;
			while (length-- > 0)
			{
				dest.SetValue(src.GetValue(--srcIndex), --destIndex);
			}
		}
	}
}
