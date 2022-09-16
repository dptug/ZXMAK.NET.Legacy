using System;
using System.Collections;
using System.IO;
using ZipLib.Checksums;
using ZipLib.Zip.Compression;
using ZipLib.Zip.Compression.Streams;

namespace ZipLib.Zip;

public class ZipOutputStream : DeflaterOutputStream
{
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

	public bool IsFinished => entries == null;

	private UseZip64 UseZip64
	{
		get
		{
			return useZip64_;
		}
		set
		{
			useZip64_ = value;
		}
	}

	public ZipOutputStream(Stream baseOutputStream)
		: base(baseOutputStream, new Deflater(-1, noZlibHeaderOrFooter: true))
	{
	}

	public void SetComment(string comment)
	{
		byte[] array = ZipConstants.ConvertToArray(comment);
		if (array.Length > 65535)
		{
			throw new ArgumentOutOfRangeException("comment");
		}
		zipComment = array;
	}

	public void SetLevel(int level)
	{
		defaultCompressionLevel = level;
		def.SetLevel(level);
	}

	public int GetLevel()
	{
		return def.GetLevel();
	}

	private void WriteLeShort(int value)
	{
		baseOutputStream.WriteByte((byte)((uint)value & 0xFFu));
		baseOutputStream.WriteByte((byte)((uint)(value >> 8) & 0xFFu));
	}

	private void WriteLeInt(int value)
	{
		WriteLeShort(value);
		WriteLeShort(value >> 16);
	}

	private void WriteLeLong(long value)
	{
		WriteLeInt((int)value);
		WriteLeInt((int)(value >> 32));
	}

	public void PutNextEntry(ZipEntry entry)
	{
		if (entry == null)
		{
			throw new ArgumentNullException("entry");
		}
		if (entries == null)
		{
			throw new InvalidOperationException("ZipOutputStream was finished");
		}
		if (curEntry != null)
		{
			CloseEntry();
		}
		if (entries.Count == int.MaxValue)
		{
			throw new ZipException("Too many entries for Zip file");
		}
		CompressionMethod compressionMethod = entry.CompressionMethod;
		int level = defaultCompressionLevel;
		entry.Flags &= 2048;
		patchEntryHeader = false;
		bool flag = true;
		if (compressionMethod == CompressionMethod.Stored)
		{
			entry.Flags &= -9;
			if (entry.CompressedSize >= 0)
			{
				if (entry.Size < 0)
				{
					entry.Size = entry.CompressedSize;
				}
				else if (entry.Size != entry.CompressedSize)
				{
					throw new ZipException("Method STORED, but compressed size != size");
				}
			}
			else if (entry.Size >= 0)
			{
				entry.CompressedSize = entry.Size;
			}
			if (entry.Size < 0 || entry.Crc < 0)
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
			if (entry.Size == 0)
			{
				entry.CompressedSize = entry.Size;
				entry.Crc = 0L;
				compressionMethod = CompressionMethod.Stored;
			}
			else if (entry.CompressedSize < 0 || entry.Size < 0 || entry.Crc < 0)
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
				patchEntryHeader = true;
			}
		}
		if (base.Password != null)
		{
			entry.IsCrypted = true;
			if (entry.Crc < 0)
			{
				entry.Flags |= 8;
			}
		}
		entry.Offset = offset;
		entry.CompressionMethod = compressionMethod;
		curMethod = compressionMethod;
		if (useZip64_ == UseZip64.On || (entry.Size < 0 && useZip64_ == UseZip64.Dynamic))
		{
			entry.ForceZip64();
		}
		WriteLeInt(67324752);
		WriteLeShort(entry.Version);
		WriteLeShort(entry.Flags);
		WriteLeShort((byte)compressionMethod);
		WriteLeInt((int)entry.DosTime);
		if (flag)
		{
			WriteLeInt((int)entry.Crc);
			if (entry.LocalHeaderRequiresZip64)
			{
				WriteLeInt(-1);
				WriteLeInt(-1);
			}
			else
			{
				WriteLeInt((int)(entry.IsCrypted ? ((int)entry.CompressedSize + 12) : entry.CompressedSize));
				WriteLeInt((int)entry.Size);
			}
		}
		else
		{
			if (patchEntryHeader)
			{
				crcPatchPos = baseOutputStream.Position;
			}
			WriteLeInt(0);
			if (patchEntryHeader)
			{
				sizePatchPos = baseOutputStream.Position;
			}
			if (entry.LocalHeaderRequiresZip64)
			{
				WriteLeInt(-1);
				WriteLeInt(-1);
			}
			else
			{
				WriteLeInt(0);
				WriteLeInt(0);
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
			if (patchEntryHeader)
			{
				sizePatchPos = zipExtraData.CurrentReadIndex;
			}
		}
		else
		{
			zipExtraData.Delete(1);
		}
		byte[] entryData = zipExtraData.GetEntryData();
		WriteLeShort(array.Length);
		WriteLeShort(entryData.Length);
		if (array.Length > 0)
		{
			baseOutputStream.Write(array, 0, array.Length);
		}
		if (entry.LocalHeaderRequiresZip64 && patchEntryHeader)
		{
			sizePatchPos += baseOutputStream.Position;
		}
		if (entryData.Length > 0)
		{
			baseOutputStream.Write(entryData, 0, entryData.Length);
		}
		offset += 30 + array.Length + entryData.Length;
		curEntry = entry;
		crc.Reset();
		if (compressionMethod == CompressionMethod.Deflated)
		{
			def.Reset();
			def.SetLevel(level);
		}
		size = 0L;
		if (entry.IsCrypted)
		{
			if (entry.Crc < 0)
			{
				WriteEncryptionHeader(entry.DosTime << 16);
			}
			else
			{
				WriteEncryptionHeader(entry.Crc);
			}
		}
	}

	public void CloseEntry()
	{
		if (curEntry == null)
		{
			throw new InvalidOperationException("No open entry");
		}
		if (curMethod == CompressionMethod.Deflated)
		{
			base.Finish();
		}
		long num = ((curMethod == CompressionMethod.Deflated) ? def.TotalOut : size);
		if (curEntry.Size < 0)
		{
			curEntry.Size = size;
		}
		else if (curEntry.Size != size)
		{
			throw new ZipException("size was " + size + ", but I expected " + curEntry.Size);
		}
		if (curEntry.CompressedSize < 0)
		{
			curEntry.CompressedSize = num;
		}
		else if (curEntry.CompressedSize != num)
		{
			throw new ZipException("compressed size was " + num + ", but I expected " + curEntry.CompressedSize);
		}
		if (curEntry.Crc < 0)
		{
			curEntry.Crc = crc.Value;
		}
		else if (curEntry.Crc != crc.Value)
		{
			throw new ZipException("crc was " + crc.Value + ", but I expected " + curEntry.Crc);
		}
		offset += num;
		if (curEntry.IsCrypted)
		{
			curEntry.CompressedSize += 12L;
		}
		if (patchEntryHeader)
		{
			patchEntryHeader = false;
			long position = baseOutputStream.Position;
			baseOutputStream.Seek(crcPatchPos, SeekOrigin.Begin);
			WriteLeInt((int)curEntry.Crc);
			if (curEntry.LocalHeaderRequiresZip64)
			{
				baseOutputStream.Seek(sizePatchPos, SeekOrigin.Begin);
				WriteLeLong(curEntry.Size);
				WriteLeLong(curEntry.CompressedSize);
			}
			else
			{
				WriteLeInt((int)curEntry.CompressedSize);
				WriteLeInt((int)curEntry.Size);
			}
			baseOutputStream.Seek(position, SeekOrigin.Begin);
		}
		if (((uint)curEntry.Flags & 8u) != 0)
		{
			WriteLeInt(134695760);
			WriteLeInt((int)curEntry.Crc);
			if (curEntry.LocalHeaderRequiresZip64)
			{
				WriteLeLong(curEntry.CompressedSize);
				WriteLeLong(curEntry.Size);
				offset += 24L;
			}
			else
			{
				WriteLeInt((int)curEntry.CompressedSize);
				WriteLeInt((int)curEntry.Size);
				offset += 16L;
			}
		}
		entries.Add(curEntry);
		curEntry = null;
	}

	private void WriteEncryptionHeader(long crcValue)
	{
		offset += 12L;
		InitializePassword(base.Password);
		byte[] array = new byte[12];
		Random random = new Random();
		random.NextBytes(array);
		array[11] = (byte)(crcValue >> 24);
		EncryptBlock(array, 0, array.Length);
		baseOutputStream.Write(array, 0, array.Length);
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		if (curEntry == null)
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
		crc.Update(buffer, offset, count);
		size += count;
		switch (curMethod)
		{
		case CompressionMethod.Deflated:
			base.Write(buffer, offset, count);
			break;
		case CompressionMethod.Stored:
			if (base.Password != null)
			{
				byte[] array = new byte[count];
				Array.Copy(buffer, offset, array, 0, count);
				EncryptBlock(array, 0, count);
				baseOutputStream.Write(array, offset, count);
			}
			else
			{
				baseOutputStream.Write(buffer, offset, count);
			}
			break;
		}
	}

	public override void Finish()
	{
		if (entries == null)
		{
			return;
		}
		if (curEntry != null)
		{
			CloseEntry();
		}
		long noOfEntries = entries.Count;
		long num = 0L;
		foreach (ZipEntry entry in entries)
		{
			WriteLeInt(33639248);
			WriteLeShort(45);
			WriteLeShort(entry.Version);
			WriteLeShort(entry.Flags);
			WriteLeShort((short)entry.CompressionMethod);
			WriteLeInt((int)entry.DosTime);
			WriteLeInt((int)entry.Crc);
			if (entry.IsZip64Forced() || entry.CompressedSize >= uint.MaxValue)
			{
				WriteLeInt(-1);
			}
			else
			{
				WriteLeInt((int)entry.CompressedSize);
			}
			if (entry.IsZip64Forced() || entry.Size >= uint.MaxValue)
			{
				WriteLeInt(-1);
			}
			else
			{
				WriteLeInt((int)entry.Size);
			}
			byte[] array = ZipConstants.ConvertToArray(entry.Flags, entry.Name);
			if (array.Length > 65535)
			{
				throw new ZipException("Name too long.");
			}
			ZipExtraData zipExtraData = new ZipExtraData(entry.ExtraData);
			if (entry.CentralHeaderRequiresZip64)
			{
				zipExtraData.StartNewEntry();
				if (entry.IsZip64Forced() || entry.Size >= uint.MaxValue)
				{
					zipExtraData.AddLeLong(entry.Size);
				}
				if (entry.IsZip64Forced() || entry.CompressedSize >= uint.MaxValue)
				{
					zipExtraData.AddLeLong(entry.CompressedSize);
				}
				if (entry.Offset >= uint.MaxValue)
				{
					zipExtraData.AddLeLong(entry.Offset);
				}
				zipExtraData.AddNewEntry(1);
			}
			else
			{
				zipExtraData.Delete(1);
			}
			byte[] entryData = zipExtraData.GetEntryData();
			byte[] array2 = ((entry.Comment != null) ? ZipConstants.ConvertToArray(entry.Flags, entry.Comment) : new byte[0]);
			if (array2.Length > 65535)
			{
				throw new ZipException("Comment too long.");
			}
			WriteLeShort(array.Length);
			WriteLeShort(entryData.Length);
			WriteLeShort(array2.Length);
			WriteLeShort(0);
			WriteLeShort(0);
			if (entry.ExternalFileAttributes != -1)
			{
				WriteLeInt(entry.ExternalFileAttributes);
			}
			else if (entry.IsDirectory)
			{
				WriteLeInt(16);
			}
			else
			{
				WriteLeInt(0);
			}
			if (entry.Offset >= uint.MaxValue)
			{
				WriteLeInt(-1);
			}
			else
			{
				WriteLeInt((int)entry.Offset);
			}
			if (array.Length > 0)
			{
				baseOutputStream.Write(array, 0, array.Length);
			}
			if (entryData.Length > 0)
			{
				baseOutputStream.Write(entryData, 0, entryData.Length);
			}
			if (array2.Length > 0)
			{
				baseOutputStream.Write(array2, 0, array2.Length);
			}
			num += 46 + array.Length + entryData.Length + array2.Length;
		}
		using (ZipHelperStream zipHelperStream = new ZipHelperStream(baseOutputStream))
		{
			zipHelperStream.WriteEndOfCentralDirectory(noOfEntries, num, offset, zipComment);
		}
		entries = null;
	}
}
