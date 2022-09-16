using System;
using System.IO;

namespace ZipLib.Core;

public sealed class StreamUtils
{
	public static void ReadFully(Stream stream, byte[] buffer)
	{
		ReadFully(stream, buffer, 0, buffer.Length);
	}

	public static void ReadFully(Stream stream, byte[] buffer, int offset, int count)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		if (offset < 0 || offset > buffer.Length)
		{
			throw new ArgumentOutOfRangeException("offset");
		}
		if (count < 0 || offset + count > buffer.Length)
		{
			throw new ArgumentOutOfRangeException("count");
		}
		while (count > 0)
		{
			int num = stream.Read(buffer, offset, count);
			if (num <= 0)
			{
				throw new EndOfStreamException();
			}
			offset += num;
			count -= num;
		}
	}

	public static void Copy(Stream source, Stream destination, byte[] buffer)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (destination == null)
		{
			throw new ArgumentNullException("destination");
		}
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		if (buffer.Length < 128)
		{
			throw new ArgumentException("Buffer is too small", "buffer");
		}
		bool flag = true;
		while (flag)
		{
			int num = source.Read(buffer, 0, buffer.Length);
			if (num > 0)
			{
				destination.Write(buffer, 0, num);
				continue;
			}
			destination.Flush();
			flag = false;
		}
	}

	private StreamUtils()
	{
	}
}
