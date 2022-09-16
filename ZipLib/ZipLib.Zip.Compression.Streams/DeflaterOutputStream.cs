using System;
using System.IO;
using ZipLib.Checksums;

namespace ZipLib.Zip.Compression.Streams;

public class DeflaterOutputStream : Stream
{
	private string password;

	private uint[] keys;

	private byte[] buffer_;

	protected Deflater def;

	protected Stream baseOutputStream;

	private bool isClosed;

	private bool isStreamOwner = true;

	public bool IsStreamOwner
	{
		get
		{
			return isStreamOwner;
		}
		set
		{
			isStreamOwner = value;
		}
	}

	public bool CanPatchEntries => baseOutputStream.CanSeek;

	public string Password
	{
		get
		{
			return password;
		}
		set
		{
			if (value != null && value.Length == 0)
			{
				password = null;
			}
			else
			{
				password = value;
			}
		}
	}

	public override bool CanRead => false;

	public override bool CanSeek => false;

	public override bool CanWrite => baseOutputStream.CanWrite;

	public override long Length => baseOutputStream.Length;

	public override long Position
	{
		get
		{
			return baseOutputStream.Position;
		}
		set
		{
			throw new NotSupportedException("Position property not supported");
		}
	}

	public DeflaterOutputStream(Stream baseOutputStream)
		: this(baseOutputStream, new Deflater(), 512)
	{
	}

	public DeflaterOutputStream(Stream baseOutputStream, Deflater deflater)
		: this(baseOutputStream, deflater, 512)
	{
	}

	public DeflaterOutputStream(Stream baseOutputStream, Deflater deflater, int bufferSize)
	{
		if (baseOutputStream == null)
		{
			throw new ArgumentNullException("baseOutputStream");
		}
		if (!baseOutputStream.CanWrite)
		{
			throw new ArgumentException("Must support writing", "baseOutputStream");
		}
		if (deflater == null)
		{
			throw new ArgumentNullException("deflater");
		}
		if (bufferSize <= 0)
		{
			throw new ArgumentOutOfRangeException("bufferSize");
		}
		this.baseOutputStream = baseOutputStream;
		buffer_ = new byte[bufferSize];
		def = deflater;
	}

	public virtual void Finish()
	{
		def.Finish();
		while (!def.IsFinished)
		{
			int num = def.Deflate(buffer_, 0, buffer_.Length);
			if (num <= 0)
			{
				break;
			}
			if (keys != null)
			{
				EncryptBlock(buffer_, 0, num);
			}
			baseOutputStream.Write(buffer_, 0, num);
		}
		if (!def.IsFinished)
		{
			throw new SharpZipBaseException("Can't deflate all input?");
		}
		baseOutputStream.Flush();
		keys = null;
	}

	protected byte EncryptByte()
	{
		uint num = (keys[2] & 0xFFFFu) | 2u;
		return (byte)(num * (num ^ 1) >> 8);
	}

	protected void EncryptBlock(byte[] buffer, int offset, int length)
	{
		for (int i = offset; i < offset + length; i++)
		{
			byte ch = buffer[i];
			buffer[i] ^= EncryptByte();
			UpdateKeys(ch);
		}
	}

	protected void InitializePassword(string password)
	{
		keys = new uint[3] { 305419896u, 591751049u, 878082192u };
		for (int i = 0; i < password.Length; i++)
		{
			UpdateKeys((byte)password[i]);
		}
	}

	protected void UpdateKeys(byte ch)
	{
		keys[0] = Crc32.ComputeCrc32(keys[0], ch);
		keys[1] = keys[1] + (byte)keys[0];
		keys[1] = keys[1] * 134775813 + 1;
		keys[2] = Crc32.ComputeCrc32(keys[2], (byte)(keys[1] >> 24));
	}

	protected void Deflate()
	{
		while (!def.IsNeedingInput)
		{
			int num = def.Deflate(buffer_, 0, buffer_.Length);
			if (num <= 0)
			{
				break;
			}
			if (keys != null)
			{
				EncryptBlock(buffer_, 0, num);
			}
			baseOutputStream.Write(buffer_, 0, num);
		}
		if (!def.IsNeedingInput)
		{
			throw new SharpZipBaseException("DeflaterOutputStream can't deflate all input?");
		}
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		throw new NotSupportedException("DeflaterOutputStream Seek not supported");
	}

	public override void SetLength(long value)
	{
		throw new NotSupportedException("DeflaterOutputStream SetLength not supported");
	}

	public override int ReadByte()
	{
		throw new NotSupportedException("DeflaterOutputStream ReadByte not supported");
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		throw new NotSupportedException("DeflaterOutputStream Read not supported");
	}

	public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
	{
		throw new NotSupportedException("DeflaterOutputStream BeginRead not currently supported");
	}

	public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
	{
		throw new NotSupportedException("BeginWrite is not supported");
	}

	public override void Flush()
	{
		def.Flush();
		Deflate();
		baseOutputStream.Flush();
	}

	public override void Close()
	{
		if (!isClosed)
		{
			isClosed = true;
			Finish();
			if (isStreamOwner)
			{
				baseOutputStream.Close();
			}
		}
	}

	public override void WriteByte(byte value)
	{
		Write(new byte[1] { value }, 0, 1);
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		def.SetInput(buffer, offset, count);
		Deflate();
	}
}
