using System;
using System.IO;

namespace ZipLib.Zip;

internal class ZipHelperStream : Stream
{
	private bool isOwner_;

	private Stream stream_;

	public bool IsStreamOwner
	{
		get
		{
			return isOwner_;
		}
		set
		{
			isOwner_ = value;
		}
	}

	public override bool CanRead => stream_.CanRead;

	public override bool CanSeek => stream_.CanSeek;

	public override bool CanTimeout => stream_.CanTimeout;

	public override long Length => stream_.Length;

	public override long Position
	{
		get
		{
			return stream_.Position;
		}
		set
		{
			stream_.Position = value;
		}
	}

	public override bool CanWrite => stream_.CanWrite;

	public ZipHelperStream(string name)
	{
		stream_ = new FileStream(name, FileMode.Open, FileAccess.ReadWrite);
		isOwner_ = true;
	}

	public ZipHelperStream(Stream stream)
	{
		stream_ = stream;
	}

	public override void Flush()
	{
		stream_.Flush();
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		return stream_.Seek(offset, origin);
	}

	public override void SetLength(long value)
	{
		stream_.SetLength(value);
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		return stream_.Read(buffer, offset, count);
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		stream_.Write(buffer, offset, count);
	}

	public long LocateBlockWithSignature(int signature, long endLocation, int minimumBlockSize, int maximumVariableData)
	{
		long num = endLocation - minimumBlockSize;
		if (num < 0)
		{
			return -1L;
		}
		long num2 = Math.Max(num - maximumVariableData, 0L);
		do
		{
			if (num < num2)
			{
				return -1L;
			}
			Seek(num--, SeekOrigin.Begin);
		}
		while (ReadLEInt() != signature);
		return Position;
	}

	public void WriteZip64EndOfCentralDirectory(long noOfEntries, long sizeEntries, long centralDirOffset)
	{
		long position = stream_.Position;
		WriteLEInt(101075792);
		WriteLELong(44L);
		WriteLEShort(45);
		WriteLEShort(45);
		WriteLEInt(0);
		WriteLEInt(0);
		WriteLELong(noOfEntries);
		WriteLELong(noOfEntries);
		WriteLELong(sizeEntries);
		WriteLELong(centralDirOffset);
		WriteLEInt(117853008);
		WriteLEInt(0);
		WriteLELong(position);
		WriteLEInt(1);
	}

	public void WriteEndOfCentralDirectory(long noOfEntries, long sizeEntries, long startOfCentralDirectory, byte[] comment)
	{
		if (noOfEntries >= 65535 || startOfCentralDirectory >= uint.MaxValue || sizeEntries >= uint.MaxValue)
		{
			WriteZip64EndOfCentralDirectory(noOfEntries, sizeEntries, startOfCentralDirectory);
		}
		WriteLEInt(101010256);
		WriteLEShort(0);
		WriteLEShort(0);
		if (noOfEntries >= 65535)
		{
			WriteLEUshort(ushort.MaxValue);
			WriteLEUshort(ushort.MaxValue);
		}
		else
		{
			WriteLEShort((short)noOfEntries);
			WriteLEShort((short)noOfEntries);
		}
		if (sizeEntries >= uint.MaxValue)
		{
			WriteLEUint(uint.MaxValue);
		}
		else
		{
			WriteLEInt((int)sizeEntries);
		}
		if (startOfCentralDirectory >= uint.MaxValue)
		{
			WriteLEUint(uint.MaxValue);
		}
		else
		{
			WriteLEInt((int)startOfCentralDirectory);
		}
		int num = ((comment != null) ? comment.Length : 0);
		if (num > 65535)
		{
			throw new ZipException($"Comment length({num}) is too long");
		}
		WriteLEShort(num);
		if (num > 0)
		{
			Write(comment, 0, comment.Length);
		}
	}

	public int ReadLEShort()
	{
		return stream_.ReadByte() | (stream_.ReadByte() << 8);
	}

	public int ReadLEInt()
	{
		return ReadLEShort() | (ReadLEShort() << 16);
	}

	public long ReadLELong()
	{
		return ReadLEInt() | ReadLEInt();
	}

	public void WriteLEShort(int value)
	{
		stream_.WriteByte((byte)((uint)value & 0xFFu));
		stream_.WriteByte((byte)((uint)(value >> 8) & 0xFFu));
	}

	public void WriteLEUshort(ushort value)
	{
		stream_.WriteByte((byte)(value & 0xFFu));
		stream_.WriteByte((byte)(value >> 8));
	}

	public void WriteLEInt(int value)
	{
		WriteLEShort(value);
		WriteLEShort(value >> 16);
	}

	public void WriteLEUint(uint value)
	{
		WriteLEUshort((ushort)(value & 0xFFFFu));
		WriteLEUshort((ushort)(value >> 16));
	}

	public void WriteLELong(long value)
	{
		WriteLEInt((int)value);
		WriteLEInt((int)(value >> 32));
	}

	public void WriteLEUlong(ulong value)
	{
		WriteLEUint((uint)(value & 0xFFFFFFFFu));
		WriteLEUint((uint)(value >> 32));
	}

	public override void Close()
	{
		Stream stream = stream_;
		stream_ = null;
		if (isOwner_ && stream != null)
		{
			isOwner_ = false;
			stream.Close();
		}
	}
}
