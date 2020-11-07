using System;
using System.IO;
using ZipLib.Checksums;

namespace ZipLib.Zip.Compression.Streams
{
	public class DeflaterOutputStream : Stream
	{
		public DeflaterOutputStream(Stream baseOutputStream) : this(baseOutputStream, new Deflater(), 512)
		{
		}

		public DeflaterOutputStream(Stream baseOutputStream, Deflater deflater) : this(baseOutputStream, deflater, 512)
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
			this.buffer_ = new byte[bufferSize];
			this.def = deflater;
		}

		public virtual void Finish()
		{
			this.def.Finish();
			while (!this.def.IsFinished)
			{
				int num = this.def.Deflate(this.buffer_, 0, this.buffer_.Length);
				if (num <= 0)
				{
					break;
				}
				if (this.keys != null)
				{
					this.EncryptBlock(this.buffer_, 0, num);
				}
				this.baseOutputStream.Write(this.buffer_, 0, num);
			}
			if (!this.def.IsFinished)
			{
				throw new SharpZipBaseException("Can't deflate all input?");
			}
			this.baseOutputStream.Flush();
			this.keys = null;
		}

		public bool IsStreamOwner
		{
			get
			{
				return this.isStreamOwner;
			}
			set
			{
				this.isStreamOwner = value;
			}
		}

		public bool CanPatchEntries
		{
			get
			{
				return this.baseOutputStream.CanSeek;
			}
		}

		public string Password
		{
			get
			{
				return this.password;
			}
			set
			{
				if (value != null && value.Length == 0)
				{
					this.password = null;
					return;
				}
				this.password = value;
			}
		}

		protected byte EncryptByte()
		{
			uint num = (this.keys[2] & 65535U) | 2U;
			return (byte)(num * (num ^ 1U) >> 8);
		}

		protected void EncryptBlock(byte[] buffer, int offset, int length)
		{
			for (int i = offset; i < offset + length; i++)
			{
				byte ch = buffer[i];
				int num = i;
				buffer[num] ^= this.EncryptByte();
				this.UpdateKeys(ch);
			}
		}

		protected void InitializePassword(string password)
		{
			this.keys = new uint[]
			{
				305419896U,
				591751049U,
				878082192U
			};
			for (int i = 0; i < password.Length; i++)
			{
				this.UpdateKeys((byte)password[i]);
			}
		}

		protected void UpdateKeys(byte ch)
		{
			this.keys[0] = Crc32.ComputeCrc32(this.keys[0], ch);
			this.keys[1] = this.keys[1] + (uint)((byte)this.keys[0]);
			this.keys[1] = this.keys[1] * 134775813U + 1U;
			this.keys[2] = Crc32.ComputeCrc32(this.keys[2], (byte)(this.keys[1] >> 24));
		}

		protected void Deflate()
		{
			while (!this.def.IsNeedingInput)
			{
				int num = this.def.Deflate(this.buffer_, 0, this.buffer_.Length);
				if (num <= 0)
				{
					break;
				}
				if (this.keys != null)
				{
					this.EncryptBlock(this.buffer_, 0, num);
				}
				this.baseOutputStream.Write(this.buffer_, 0, num);
			}
			if (!this.def.IsNeedingInput)
			{
				throw new SharpZipBaseException("DeflaterOutputStream can't deflate all input?");
			}
		}

		public override bool CanRead
		{
			get
			{
				return false;
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
				return this.baseOutputStream.CanWrite;
			}
		}

		public override long Length
		{
			get
			{
				return this.baseOutputStream.Length;
			}
		}

		public override long Position
		{
			get
			{
				return this.baseOutputStream.Position;
			}
			set
			{
				throw new NotSupportedException("Position property not supported");
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
			this.def.Flush();
			this.Deflate();
			this.baseOutputStream.Flush();
		}

		public override void Close()
		{
			if (!this.isClosed)
			{
				this.isClosed = true;
				this.Finish();
				if (this.isStreamOwner)
				{
					this.baseOutputStream.Close();
				}
			}
		}

		public override void WriteByte(byte value)
		{
			this.Write(new byte[]
			{
				value
			}, 0, 1);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			this.def.SetInput(buffer, offset, count);
			this.Deflate();
		}

		private string password;

		private uint[] keys;

		private byte[] buffer_;

		protected Deflater def;

		protected Stream baseOutputStream;

		private bool isClosed;

		private bool isStreamOwner = true;
	}
}
