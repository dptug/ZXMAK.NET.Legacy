﻿using System;
using System.Collections;
using System.IO;
using ZipLib.Checksums;
using ZipLib.Zip.Compression;
using ZipLib.Zip.Compression.Streams;

namespace ZipLib.Zip
{
	public class ZipOutputStream : DeflaterOutputStream
	{
		public ZipOutputStream(Stream baseOutputStream) : base(baseOutputStream, new Deflater(-1, true))
		{
		}

		public bool IsFinished
		{
			get
			{
				return this.entries == null;
			}
		}

		public void SetComment(string comment)
		{
			byte[] array = ZipConstants.ConvertToArray(comment);
			if (array.Length > 65535)
			{
				throw new ArgumentOutOfRangeException("comment");
			}
			this.zipComment = array;
		}

		public void SetLevel(int level)
		{
			this.defaultCompressionLevel = level;
			this.def.SetLevel(level);
		}

		public int GetLevel()
		{
			return this.def.GetLevel();
		}

		private UseZip64 UseZip64
		{
			get
			{
				return this.useZip64_;
			}
			set
			{
				this.useZip64_ = value;
			}
		}

		private void WriteLeShort(int value)
		{
			this.baseOutputStream.WriteByte((byte)(value & 255));
			this.baseOutputStream.WriteByte((byte)(value >> 8 & 255));
		}

		private void WriteLeInt(int value)
		{
			this.WriteLeShort(value);
			this.WriteLeShort(value >> 16);
		}

		private void WriteLeLong(long value)
		{
			this.WriteLeInt((int)value);
			this.WriteLeInt((int)(value >> 32));
		}

		public void PutNextEntry(ZipEntry entry)
		{
			if (entry == null)
			{
				throw new ArgumentNullException("entry");
			}
			if (this.entries == null)
			{
				throw new InvalidOperationException("ZipOutputStream was finished");
			}
			if (this.curEntry != null)
			{
				this.CloseEntry();
			}
			if (this.entries.Count == 2147483647)
			{
				throw new ZipException("Too many entries for Zip file");
			}
			CompressionMethod compressionMethod = entry.CompressionMethod;
			int level = this.defaultCompressionLevel;
			entry.Flags &= 2048;
			this.patchEntryHeader = false;
			bool flag = true;
			if (compressionMethod == CompressionMethod.Stored)
			{
				entry.Flags &= -9;
				if (entry.CompressedSize >= 0L)
				{
					if (entry.Size < 0L)
					{
						entry.Size = entry.CompressedSize;
					}
					else if (entry.Size != entry.CompressedSize)
					{
						throw new ZipException("Method STORED, but compressed size != size");
					}
				}
				else if (entry.Size >= 0L)
				{
					entry.CompressedSize = entry.Size;
				}
				if (entry.Size < 0L || entry.Crc < 0L)
				{
					if (base.CanPatchEntries)
					{
						flag = false;
					}
					else
					{
						compressionMethod = CompressionMethod.Deflated;
						level = 0;
					}
				}
			}
			if (compressionMethod == CompressionMethod.Deflated)
			{
				if (entry.Size == 0L)
				{
					entry.CompressedSize = entry.Size;
					entry.Crc = 0L;
					compressionMethod = CompressionMethod.Stored;
				}
				else if (entry.CompressedSize < 0L || entry.Size < 0L || entry.Crc < 0L)
				{
					flag = false;
				}
			}
			if (!flag)
			{
				if (!base.CanPatchEntries)
				{
					entry.Flags |= 8;
				}
				else
				{
					this.patchEntryHeader = true;
				}
			}
			if (base.Password != null)
			{
				entry.IsCrypted = true;
				if (entry.Crc < 0L)
				{
					entry.Flags |= 8;
				}
			}
			entry.Offset = this.offset;
			entry.CompressionMethod = compressionMethod;
			this.curMethod = compressionMethod;
			if (this.useZip64_ == UseZip64.On || (entry.Size < 0L && this.useZip64_ == UseZip64.Dynamic))
			{
				entry.ForceZip64();
			}
			this.WriteLeInt(67324752);
			this.WriteLeShort(entry.Version);
			this.WriteLeShort(entry.Flags);
			this.WriteLeShort((int)((byte)compressionMethod));
			this.WriteLeInt((int)entry.DosTime);
			if (flag)
			{
				this.WriteLeInt((int)entry.Crc);
				if (entry.LocalHeaderRequiresZip64)
				{
					this.WriteLeInt(-1);
					this.WriteLeInt(-1);
				}
				else
				{
					this.WriteLeInt(entry.IsCrypted ? ((int)entry.CompressedSize + 12) : ((int)entry.CompressedSize));
					this.WriteLeInt((int)entry.Size);
				}
			}
			else
			{
				if (this.patchEntryHeader)
				{
					this.crcPatchPos = this.baseOutputStream.Position;
				}
				this.WriteLeInt(0);
				if (this.patchEntryHeader)
				{
					this.sizePatchPos = this.baseOutputStream.Position;
				}
				if (entry.LocalHeaderRequiresZip64)
				{
					this.WriteLeInt(-1);
					this.WriteLeInt(-1);
				}
				else
				{
					this.WriteLeInt(0);
					this.WriteLeInt(0);
				}
			}
			byte[] array = ZipConstants.ConvertToArray(entry.Flags, entry.Name);
			if (array.Length > 65535)
			{
				throw new ZipException("Entry name too long.");
			}
			ZipExtraData zipExtraData = new ZipExtraData(entry.ExtraData);
			if (entry.LocalHeaderRequiresZip64)
			{
				zipExtraData.StartNewEntry();
				zipExtraData.AddLeLong(-1L);
				zipExtraData.AddLeLong(-1L);
				zipExtraData.AddNewEntry(1);
				if (!zipExtraData.Find(1))
				{
					throw new ZipException("Internal error cant find extra data");
				}
				if (this.patchEntryHeader)
				{
					this.sizePatchPos = (long)zipExtraData.CurrentReadIndex;
				}
			}
			else
			{
				zipExtraData.Delete(1);
			}
			byte[] entryData = zipExtraData.GetEntryData();
			this.WriteLeShort(array.Length);
			this.WriteLeShort(entryData.Length);
			if (array.Length > 0)
			{
				this.baseOutputStream.Write(array, 0, array.Length);
			}
			if (entry.LocalHeaderRequiresZip64 && this.patchEntryHeader)
			{
				this.sizePatchPos += this.baseOutputStream.Position;
			}
			if (entryData.Length > 0)
			{
				this.baseOutputStream.Write(entryData, 0, entryData.Length);
			}
			this.offset += (long)(30 + array.Length + entryData.Length);
			this.curEntry = entry;
			this.crc.Reset();
			if (compressionMethod == CompressionMethod.Deflated)
			{
				this.def.Reset();
				this.def.SetLevel(level);
			}
			this.size = 0L;
			if (entry.IsCrypted)
			{
				if (entry.Crc < 0L)
				{
					this.WriteEncryptionHeader(entry.DosTime << 16);
					return;
				}
				this.WriteEncryptionHeader(entry.Crc);
			}
		}

		public void CloseEntry()
		{
			if (this.curEntry == null)
			{
				throw new InvalidOperationException("No open entry");
			}
			if (this.curMethod == CompressionMethod.Deflated)
			{
				base.Finish();
			}
			long num = (this.curMethod == CompressionMethod.Deflated) ? this.def.TotalOut : this.size;
			if (this.curEntry.Size < 0L)
			{
				this.curEntry.Size = this.size;
			}
			else if (this.curEntry.Size != this.size)
			{
				throw new ZipException(string.Concat(new object[]
				{
					"size was ",
					this.size,
					", but I expected ",
					this.curEntry.Size
				}));
			}
			if (this.curEntry.CompressedSize < 0L)
			{
				this.curEntry.CompressedSize = num;
			}
			else if (this.curEntry.CompressedSize != num)
			{
				throw new ZipException(string.Concat(new object[]
				{
					"compressed size was ",
					num,
					", but I expected ",
					this.curEntry.CompressedSize
				}));
			}
			if (this.curEntry.Crc < 0L)
			{
				this.curEntry.Crc = this.crc.Value;
			}
			else if (this.curEntry.Crc != this.crc.Value)
			{
				throw new ZipException(string.Concat(new object[]
				{
					"crc was ",
					this.crc.Value,
					", but I expected ",
					this.curEntry.Crc
				}));
			}
			this.offset += num;
			if (this.curEntry.IsCrypted)
			{
				this.curEntry.CompressedSize += 12L;
			}
			if (this.patchEntryHeader)
			{
				this.patchEntryHeader = false;
				long position = this.baseOutputStream.Position;
				this.baseOutputStream.Seek(this.crcPatchPos, SeekOrigin.Begin);
				this.WriteLeInt((int)this.curEntry.Crc);
				if (this.curEntry.LocalHeaderRequiresZip64)
				{
					this.baseOutputStream.Seek(this.sizePatchPos, SeekOrigin.Begin);
					this.WriteLeLong(this.curEntry.Size);
					this.WriteLeLong(this.curEntry.CompressedSize);
				}
				else
				{
					this.WriteLeInt((int)this.curEntry.CompressedSize);
					this.WriteLeInt((int)this.curEntry.Size);
				}
				this.baseOutputStream.Seek(position, SeekOrigin.Begin);
			}
			if ((this.curEntry.Flags & 8) != 0)
			{
				this.WriteLeInt(134695760);
				this.WriteLeInt((int)this.curEntry.Crc);
				if (this.curEntry.LocalHeaderRequiresZip64)
				{
					this.WriteLeLong(this.curEntry.CompressedSize);
					this.WriteLeLong(this.curEntry.Size);
					this.offset += 24L;
				}
				else
				{
					this.WriteLeInt((int)this.curEntry.CompressedSize);
					this.WriteLeInt((int)this.curEntry.Size);
					this.offset += 16L;
				}
			}
			this.entries.Add(this.curEntry);
			this.curEntry = null;
		}

		private void WriteEncryptionHeader(long crcValue)
		{
			this.offset += 12L;
			base.InitializePassword(base.Password);
			byte[] array = new byte[12];
			Random random = new Random();
			random.NextBytes(array);
			array[11] = (byte)(crcValue >> 24);
			base.EncryptBlock(array, 0, array.Length);
			this.baseOutputStream.Write(array, 0, array.Length);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			if (this.curEntry == null)
			{
				throw new InvalidOperationException("No open entry.");
			}
			if (buffer == null)
			{
				throw new ArgumentNullException("buffer");
			}
			if (offset < 0)
			{
				throw new ArgumentOutOfRangeException("offset", "Cannot be negative");
			}
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException("count", "Cannot be negative");
			}
			if (buffer.Length - offset < count)
			{
				throw new ArgumentException("Invalid offset/count combination");
			}
			this.crc.Update(buffer, offset, count);
			this.size += (long)count;
			CompressionMethod compressionMethod = this.curMethod;
			if (compressionMethod != CompressionMethod.Stored)
			{
				if (compressionMethod != CompressionMethod.Deflated)
				{
					return;
				}
				base.Write(buffer, offset, count);
				return;
			}
			else
			{
				if (base.Password != null)
				{
					byte[] array = new byte[count];
					Array.Copy(buffer, offset, array, 0, count);
					base.EncryptBlock(array, 0, count);
					this.baseOutputStream.Write(array, offset, count);
					return;
				}
				this.baseOutputStream.Write(buffer, offset, count);
				return;
			}
		}

		public override void Finish()
		{
			if (this.entries == null)
			{
				return;
			}
			if (this.curEntry != null)
			{
				this.CloseEntry();
			}
			long noOfEntries = (long)this.entries.Count;
			long num = 0L;
			foreach (object obj in this.entries)
			{
				ZipEntry zipEntry = (ZipEntry)obj;
				this.WriteLeInt(33639248);
				this.WriteLeShort(45);
				this.WriteLeShort(zipEntry.Version);
				this.WriteLeShort(zipEntry.Flags);
				this.WriteLeShort((int)((short)zipEntry.CompressionMethod));
				this.WriteLeInt((int)zipEntry.DosTime);
				this.WriteLeInt((int)zipEntry.Crc);
				if (zipEntry.IsZip64Forced() || zipEntry.CompressedSize >= (long)((ulong)-1))
				{
					this.WriteLeInt(-1);
				}
				else
				{
					this.WriteLeInt((int)zipEntry.CompressedSize);
				}
				if (zipEntry.IsZip64Forced() || zipEntry.Size >= (long)((ulong)-1))
				{
					this.WriteLeInt(-1);
				}
				else
				{
					this.WriteLeInt((int)zipEntry.Size);
				}
				byte[] array = ZipConstants.ConvertToArray(zipEntry.Flags, zipEntry.Name);
				if (array.Length > 65535)
				{
					throw new ZipException("Name too long.");
				}
				ZipExtraData zipExtraData = new ZipExtraData(zipEntry.ExtraData);
				if (zipEntry.CentralHeaderRequiresZip64)
				{
					zipExtraData.StartNewEntry();
					if (zipEntry.IsZip64Forced() || zipEntry.Size >= (long)((ulong)-1))
					{
						zipExtraData.AddLeLong(zipEntry.Size);
					}
					if (zipEntry.IsZip64Forced() || zipEntry.CompressedSize >= (long)((ulong)-1))
					{
						zipExtraData.AddLeLong(zipEntry.CompressedSize);
					}
					if (zipEntry.Offset >= (long)((ulong)-1))
					{
						zipExtraData.AddLeLong(zipEntry.Offset);
					}
					zipExtraData.AddNewEntry(1);
				}
				else
				{
					zipExtraData.Delete(1);
				}
				byte[] entryData = zipExtraData.GetEntryData();
				byte[] array2 = (zipEntry.Comment != null) ? ZipConstants.ConvertToArray(zipEntry.Flags, zipEntry.Comment) : new byte[0];
				if (array2.Length > 65535)
				{
					throw new ZipException("Comment too long.");
				}
				this.WriteLeShort(array.Length);
				this.WriteLeShort(entryData.Length);
				this.WriteLeShort(array2.Length);
				this.WriteLeShort(0);
				this.WriteLeShort(0);
				if (zipEntry.ExternalFileAttributes != -1)
				{
					this.WriteLeInt(zipEntry.ExternalFileAttributes);
				}
				else if (zipEntry.IsDirectory)
				{
					this.WriteLeInt(16);
				}
				else
				{
					this.WriteLeInt(0);
				}
				if (zipEntry.Offset >= (long)((ulong)-1))
				{
					this.WriteLeInt(-1);
				}
				else
				{
					this.WriteLeInt((int)zipEntry.Offset);
				}
				if (array.Length > 0)
				{
					this.baseOutputStream.Write(array, 0, array.Length);
				}
				if (entryData.Length > 0)
				{
					this.baseOutputStream.Write(entryData, 0, entryData.Length);
				}
				if (array2.Length > 0)
				{
					this.baseOutputStream.Write(array2, 0, array2.Length);
				}
				num += (long)(46 + array.Length + entryData.Length + array2.Length);
			}
			using (ZipHelperStream zipHelperStream = new ZipHelperStream(this.baseOutputStream))
			{
				zipHelperStream.WriteEndOfCentralDirectory(noOfEntries, num, this.offset, this.zipComment);
			}
			this.entries = null;
		}

		private ArrayList entries = new ArrayList();

		private Crc32 crc = new Crc32();

		private ZipEntry curEntry;

		private int defaultCompressionLevel = -1;

		private CompressionMethod curMethod = CompressionMethod.Deflated;

		private long size;

		private long offset;

		private byte[] zipComment = new byte[0];

		private bool patchEntryHeader;

		private long crcPatchPos = -1L;

		private long sizePatchPos = -1L;

		private UseZip64 useZip64_;
	}
}
