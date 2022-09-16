using System;
using System.IO;

namespace ZipLib.Zip;

public class ZipEntry : ICloneable
{
	[Flags]
	private enum Known : byte
	{
		None = 0,
		Size = 1,
		CompressedSize = 2,
		Crc = 4,
		Time = 8,
		ExternalAttributes = 0x10
	}

	private Known known;

	private int externalFileAttributes = -1;

	private ushort versionMadeBy;

	private string name;

	private ulong size;

	private ulong compressedSize;

	private ushort versionToExtract;

	private uint crc;

	private uint dosTime;

	private CompressionMethod method = CompressionMethod.Deflated;

	private byte[] extra;

	private string comment;

	private int flags;

	private long zipFileIndex = -1L;

	private long offset;

	private bool forceZip64_;

	private byte cryptoCheckValue_;

	public bool HasCrc => (known & Known.Crc) != 0;

	public bool IsCrypted
	{
		get
		{
			return (flags & 1) != 0;
		}
		set
		{
			if (value)
			{
				flags |= 1;
			}
			else
			{
				flags &= -2;
			}
		}
	}

	public bool IsUnicodeText
	{
		get
		{
			return (flags & 0x800) != 0;
		}
		set
		{
			if (value)
			{
				flags |= 2048;
			}
			else
			{
				flags &= -2049;
			}
		}
	}

	internal byte CryptoCheckValue
	{
		get
		{
			return cryptoCheckValue_;
		}
		set
		{
			cryptoCheckValue_ = value;
		}
	}

	public int Flags
	{
		get
		{
			return flags;
		}
		set
		{
			flags = value;
		}
	}

	public long ZipFileIndex
	{
		get
		{
			return zipFileIndex;
		}
		set
		{
			zipFileIndex = value;
		}
	}

	public long Offset
	{
		get
		{
			return offset;
		}
		set
		{
			offset = value;
		}
	}

	public int ExternalFileAttributes
	{
		get
		{
			if ((known & Known.ExternalAttributes) == 0)
			{
				return -1;
			}
			return externalFileAttributes;
		}
		set
		{
			externalFileAttributes = value;
			known |= Known.ExternalAttributes;
		}
	}

	public int VersionMadeBy => versionMadeBy & 0xFF;

	public int HostSystem
	{
		get
		{
			return (versionMadeBy >> 8) & 0xFF;
		}
		set
		{
			versionMadeBy &= 255;
			versionMadeBy |= (ushort)((value & 0xFF) << 8);
		}
	}

	public int Version
	{
		get
		{
			if (versionToExtract != 0)
			{
				return versionToExtract;
			}
			int result = 10;
			if (LocalHeaderRequiresZip64)
			{
				result = 45;
			}
			else if (CompressionMethod.Deflated == method)
			{
				result = 20;
			}
			else if (IsDirectory)
			{
				result = 20;
			}
			else if (IsCrypted)
			{
				result = 20;
			}
			else if (HasDosAttributes(8))
			{
				result = 11;
			}
			return result;
		}
	}

	public bool CanDecompress
	{
		get
		{
			if (Version <= 45 && (Version == 10 || Version == 11 || Version == 20 || Version == 45))
			{
				return IsCompressionMethodSupported();
			}
			return false;
		}
	}

	public bool LocalHeaderRequiresZip64
	{
		get
		{
			bool flag = forceZip64_;
			if (!flag)
			{
				ulong num = compressedSize;
				if (versionToExtract == 0 && IsCrypted)
				{
					num += 12;
				}
				flag = (size >= uint.MaxValue || num >= uint.MaxValue) && (versionToExtract == 0 || versionToExtract >= 45);
			}
			return flag;
		}
	}

	public bool CentralHeaderRequiresZip64
	{
		get
		{
			if (!LocalHeaderRequiresZip64)
			{
				return offset >= uint.MaxValue;
			}
			return true;
		}
	}

	public long DosTime
	{
		get
		{
			if ((known & Known.Time) == 0)
			{
				return 0L;
			}
			return dosTime;
		}
		set
		{
			dosTime = (uint)value;
			known |= Known.Time;
		}
	}

	public DateTime DateTime
	{
		get
		{
			if (dosTime == 0)
			{
				return DateTime.Now;
			}
			uint second = 2 * (dosTime & 0x1F);
			uint minute = (dosTime >> 5) & 0x3Fu;
			uint hour = (dosTime >> 11) & 0x1Fu;
			uint day = (dosTime >> 16) & 0x1Fu;
			uint month = (dosTime >> 21) & 0xFu;
			uint year = ((dosTime >> 25) & 0x7F) + 1980;
			return new DateTime((int)year, (int)month, (int)day, (int)hour, (int)minute, (int)second);
		}
		set
		{
			DosTime = (uint)((((value.Year - 1980) & 0x7F) << 25) | (value.Month << 21) | (value.Day << 16) | (value.Hour << 11) | (value.Minute << 5) | (int)((uint)value.Second >> 1));
		}
	}

	public string Name => name;

	public long Size
	{
		get
		{
			if ((known & Known.Size) == 0)
			{
				return -1L;
			}
			return (long)size;
		}
		set
		{
			size = (ulong)value;
			known |= Known.Size;
		}
	}

	public long CompressedSize
	{
		get
		{
			if ((known & Known.CompressedSize) == 0)
			{
				return -1L;
			}
			return (long)compressedSize;
		}
		set
		{
			compressedSize = (ulong)value;
			known |= Known.CompressedSize;
		}
	}

	public long Crc
	{
		get
		{
			if ((known & Known.Crc) == 0)
			{
				return -1L;
			}
			return (long)crc & 0xFFFFFFFFL;
		}
		set
		{
			if ((crc & -4294967296L) != 0)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			crc = (uint)value;
			known |= Known.Crc;
		}
	}

	public CompressionMethod CompressionMethod
	{
		get
		{
			return method;
		}
		set
		{
			if (!IsCompressionMethodSupported(value))
			{
				throw new NotSupportedException("Compression method not supported");
			}
			method = value;
		}
	}

	public byte[] ExtraData
	{
		get
		{
			return extra;
		}
		set
		{
			if (value == null)
			{
				extra = null;
				return;
			}
			if (value.Length > 65535)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			extra = new byte[value.Length];
			Array.Copy(value, 0, extra, 0, value.Length);
		}
	}

	public string Comment
	{
		get
		{
			return comment;
		}
		set
		{
			if (value != null && value.Length > 65535)
			{
				throw new ArgumentOutOfRangeException("value", "cannot exceed 65535");
			}
			comment = value;
		}
	}

	public bool IsDirectory
	{
		get
		{
			int length = name.Length;
			return (length > 0 && (name[length - 1] == '/' || name[length - 1] == '\\')) || HasDosAttributes(16);
		}
	}

	public bool IsFile
	{
		get
		{
			if (!IsDirectory)
			{
				return !HasDosAttributes(8);
			}
			return false;
		}
	}

	public ZipEntry(string name)
		: this(name, 0, 45, CompressionMethod.Deflated)
	{
	}

	internal ZipEntry(string name, int versionRequiredToExtract)
		: this(name, versionRequiredToExtract, 45, CompressionMethod.Deflated)
	{
	}

	internal ZipEntry(string name, int versionRequiredToExtract, int madeByInfo, CompressionMethod method)
	{
		if (name == null)
		{
			throw new ArgumentNullException("ZipEntry name");
		}
		if (name.Length > 65535)
		{
			throw new ArgumentException("Name is too long", "name");
		}
		if (versionRequiredToExtract != 0 && versionRequiredToExtract < 10)
		{
			throw new ArgumentOutOfRangeException("versionRequiredToExtract");
		}
		DateTime = DateTime.Now;
		this.name = name;
		versionMadeBy = (ushort)madeByInfo;
		versionToExtract = (ushort)versionRequiredToExtract;
		this.method = method;
	}

	[Obsolete("Use Clone instead")]
	public ZipEntry(ZipEntry entry)
	{
		if (entry == null)
		{
			throw new ArgumentNullException("entry");
		}
		known = entry.known;
		name = entry.name;
		size = entry.size;
		compressedSize = entry.compressedSize;
		crc = entry.crc;
		dosTime = entry.dosTime;
		method = entry.method;
		comment = entry.comment;
		versionToExtract = entry.versionToExtract;
		versionMadeBy = entry.versionMadeBy;
		externalFileAttributes = entry.externalFileAttributes;
		flags = entry.flags;
		zipFileIndex = entry.zipFileIndex;
		offset = entry.offset;
		forceZip64_ = entry.forceZip64_;
		if (entry.extra != null)
		{
			extra = new byte[entry.extra.Length];
			Array.Copy(entry.extra, 0, extra, 0, entry.extra.Length);
		}
	}

	private bool HasDosAttributes(int attributes)
	{
		bool result = false;
		if ((known & Known.ExternalAttributes) != 0 && (HostSystem == 0 || HostSystem == 10) && (ExternalFileAttributes & attributes) == attributes)
		{
			result = true;
		}
		return result;
	}

	public void ForceZip64()
	{
		forceZip64_ = true;
	}

	public bool IsZip64Forced()
	{
		return forceZip64_;
	}

	internal void ProcessExtraData(bool localHeader)
	{
		ZipExtraData zipExtraData = new ZipExtraData(extra);
		if (zipExtraData.Find(1))
		{
			if ((versionToExtract & 0xFF) < 45)
			{
				throw new ZipException("Zip64 Extended information found but version is not valid");
			}
			forceZip64_ = true;
			if (zipExtraData.ValueLength < 4)
			{
				throw new ZipException("Extra data extended Zip64 information length is invalid");
			}
			if (localHeader || size == uint.MaxValue)
			{
				size = (ulong)zipExtraData.ReadLong();
			}
			if (localHeader || compressedSize == uint.MaxValue)
			{
				compressedSize = (ulong)zipExtraData.ReadLong();
			}
		}
		else if ((versionToExtract & 0xFF) >= 45 && (size == uint.MaxValue || compressedSize == uint.MaxValue))
		{
			throw new ZipException("Zip64 Extended information required but is missing.");
		}
		if (zipExtraData.Find(21589))
		{
			int valueLength = zipExtraData.ValueLength;
			int num = zipExtraData.ReadByte();
			if (((uint)num & (true ? 1u : 0u)) != 0 && valueLength >= 5)
			{
				int seconds = zipExtraData.ReadInt();
				DateTime = (new DateTime(1970, 1, 1, 0, 0, 0).ToUniversalTime() + new TimeSpan(0, 0, 0, seconds, 0)).ToLocalTime();
			}
		}
	}

	public bool IsCompressionMethodSupported()
	{
		return IsCompressionMethodSupported(CompressionMethod);
	}

	public object Clone()
	{
		ZipEntry zipEntry = (ZipEntry)MemberwiseClone();
		if (extra != null)
		{
			zipEntry.extra = new byte[extra.Length];
			Array.Copy(zipEntry.extra, 0, extra, 0, extra.Length);
		}
		return zipEntry;
	}

	public override string ToString()
	{
		return name;
	}

	public static bool IsCompressionMethodSupported(CompressionMethod method)
	{
		if (method != CompressionMethod.Deflated)
		{
			return method == CompressionMethod.Stored;
		}
		return true;
	}

	public static string CleanName(string name)
	{
		if (name == null)
		{
			return string.Empty;
		}
		if (Path.IsPathRooted(name))
		{
			name = name.Substring(Path.GetPathRoot(name).Length);
		}
		name = name.Replace("\\", "/");
		while (name.Length > 0 && name[0] == '/')
		{
			name = name.Remove(0, 1);
		}
		return name;
	}
}
