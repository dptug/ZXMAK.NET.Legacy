using System;
using System.IO;

namespace ZipLib.Zip
{
	public class ZipEntry : ICloneable
	{
		public ZipEntry(string name) : this(name, 0, 45, CompressionMethod.Deflated)
		{
		}

		internal ZipEntry(string name, int versionRequiredToExtract) : this(name, versionRequiredToExtract, 45, CompressionMethod.Deflated)
		{
		}

		internal ZipEntry(string name, int versionRequiredToExtract, int madeByInfo, CompressionMethod method)
		{
			this.externalFileAttributes = -1;
			this.method = CompressionMethod.Deflated;
			this.zipFileIndex = -1L;
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
			this.DateTime = DateTime.Now;
			this.name = name;
			this.versionMadeBy = (ushort)madeByInfo;
			this.versionToExtract = (ushort)versionRequiredToExtract;
			this.method = method;
		}

		[Obsolete("Use Clone instead")]
		public ZipEntry(ZipEntry entry)
		{
			this.externalFileAttributes = -1;
			this.method = CompressionMethod.Deflated;
			this.zipFileIndex = -1L;
			if (entry == null)
			{
				throw new ArgumentNullException("entry");
			}
			this.known = entry.known;
			this.name = entry.name;
			this.size = entry.size;
			this.compressedSize = entry.compressedSize;
			this.crc = entry.crc;
			this.dosTime = entry.dosTime;
			this.method = entry.method;
			this.comment = entry.comment;
			this.versionToExtract = entry.versionToExtract;
			this.versionMadeBy = entry.versionMadeBy;
			this.externalFileAttributes = entry.externalFileAttributes;
			this.flags = entry.flags;
			this.zipFileIndex = entry.zipFileIndex;
			this.offset = entry.offset;
			this.forceZip64_ = entry.forceZip64_;
			if (entry.extra != null)
			{
				this.extra = new byte[entry.extra.Length];
				Array.Copy(entry.extra, 0, this.extra, 0, entry.extra.Length);
			}
		}

		public bool HasCrc
		{
			get
			{
				return (byte)(this.known & ZipEntry.Known.Crc) != 0;
			}
		}

		public bool IsCrypted
		{
			get
			{
				return (this.flags & 1) != 0;
			}
			set
			{
				if (value)
				{
					this.flags |= 1;
					return;
				}
				this.flags &= -2;
			}
		}

		public bool IsUnicodeText
		{
			get
			{
				return (this.flags & 2048) != 0;
			}
			set
			{
				if (value)
				{
					this.flags |= 2048;
					return;
				}
				this.flags &= -2049;
			}
		}

		internal byte CryptoCheckValue
		{
			get
			{
				return this.cryptoCheckValue_;
			}
			set
			{
				this.cryptoCheckValue_ = value;
			}
		}

		public int Flags
		{
			get
			{
				return this.flags;
			}
			set
			{
				this.flags = value;
			}
		}

		public long ZipFileIndex
		{
			get
			{
				return this.zipFileIndex;
			}
			set
			{
				this.zipFileIndex = value;
			}
		}

		public long Offset
		{
			get
			{
				return this.offset;
			}
			set
			{
				this.offset = value;
			}
		}

		public int ExternalFileAttributes
		{
			get
			{
				if ((byte)(this.known & ZipEntry.Known.ExternalAttributes) == 0)
				{
					return -1;
				}
				return this.externalFileAttributes;
			}
			set
			{
				this.externalFileAttributes = value;
				this.known |= ZipEntry.Known.ExternalAttributes;
			}
		}

		public int VersionMadeBy
		{
			get
			{
				return (int)(this.versionMadeBy & 255);
			}
		}

		private bool HasDosAttributes(int attributes)
		{
			bool result = false;
			if ((byte)(this.known & ZipEntry.Known.ExternalAttributes) != 0 && (this.HostSystem == 0 || this.HostSystem == 10) && (this.ExternalFileAttributes & attributes) == attributes)
			{
				result = true;
			}
			return result;
		}

		public int HostSystem
		{
			get
			{
				return this.versionMadeBy >> 8 & 255;
			}
			set
			{
				this.versionMadeBy &= 255;
				this.versionMadeBy |= (ushort)((value & 255) << 8);
			}
		}

		public int Version
		{
			get
			{
				if (this.versionToExtract != 0)
				{
					return (int)this.versionToExtract;
				}
				int result = 10;
				if (this.LocalHeaderRequiresZip64)
				{
					result = 45;
				}
				else if (CompressionMethod.Deflated == this.method)
				{
					result = 20;
				}
				else if (this.IsDirectory)
				{
					result = 20;
				}
				else if (this.IsCrypted)
				{
					result = 20;
				}
				else if (this.HasDosAttributes(8))
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
				return this.Version <= 45 && (this.Version == 10 || this.Version == 11 || this.Version == 20 || this.Version == 45) && this.IsCompressionMethodSupported();
			}
		}

		public void ForceZip64()
		{
			this.forceZip64_ = true;
		}

		public bool IsZip64Forced()
		{
			return this.forceZip64_;
		}

		public bool LocalHeaderRequiresZip64
		{
			get
			{
				bool flag = this.forceZip64_;
				if (!flag)
				{
					ulong num = this.compressedSize;
					if (this.versionToExtract == 0 && this.IsCrypted)
					{
						num += 12UL;
					}
					flag = ((this.size >= unchecked((ulong)-1) || num >= unchecked((ulong)-1)) && (this.versionToExtract == 0 || this.versionToExtract >= 45));
				}
				return flag;
			}
		}

		public bool CentralHeaderRequiresZip64
		{
			get
			{
				return this.LocalHeaderRequiresZip64 || this.offset >= unchecked((long)((ulong)-1));
			}
		}

		public long DosTime
		{
			get
			{
				if ((byte)(this.known & ZipEntry.Known.Time) == 0)
				{
					return 0L;
				}
				return (long)((ulong)this.dosTime);
			}
			set
			{
				this.dosTime = (uint)value;
				this.known |= ZipEntry.Known.Time;
			}
		}

		public DateTime DateTime
		{
			get
			{
				if (this.dosTime == 0U)
				{
					return DateTime.Now;
				}
				uint second = 2U * (this.dosTime & 31U);
				uint minute = this.dosTime >> 5 & 63U;
				uint hour = this.dosTime >> 11 & 31U;
				uint day = this.dosTime >> 16 & 31U;
				uint month = this.dosTime >> 21 & 15U;
				uint year = (this.dosTime >> 25 & 127U) + 1980U;
				return new DateTime((int)year, (int)month, (int)day, (int)hour, (int)minute, (int)second);
			}
			set
			{
				this.DosTime = (long)((ulong)((value.Year - 1980 & 127) << 25 | value.Month << 21 | value.Day << 16 | value.Hour << 11 | value.Minute << 5 | (int)((uint)value.Second >> 1)));
			}
		}

		public string Name
		{
			get
			{
				return this.name;
			}
		}

		public long Size
		{
			get
			{
				if ((byte)(this.known & ZipEntry.Known.Size) == 0)
				{
					return -1L;
				}
				return (long)this.size;
			}
			set
			{
				this.size = (ulong)value;
				this.known |= ZipEntry.Known.Size;
			}
		}

		public long CompressedSize
		{
			get
			{
				if ((byte)(this.known & ZipEntry.Known.CompressedSize) == 0)
				{
					return -1L;
				}
				return (long)this.compressedSize;
			}
			set
			{
				this.compressedSize = (ulong)value;
				this.known |= ZipEntry.Known.CompressedSize;
			}
		}

		public long Crc
		{
			get
			{
				if ((byte)(this.known & ZipEntry.Known.Crc) == 0)
				{
					return -1L;
				}
				return (long)((ulong)this.crc & unchecked((ulong)-1));
			}
			set
			{
				if (((ulong)this.crc & 18446744069414584320UL) != 0UL)
				{
					throw new ArgumentOutOfRangeException("value");
				}
				this.crc = (uint)value;
				this.known |= ZipEntry.Known.Crc;
			}
		}

		public CompressionMethod CompressionMethod
		{
			get
			{
				return this.method;
			}
			set
			{
				if (!ZipEntry.IsCompressionMethodSupported(value))
				{
					throw new NotSupportedException("Compression method not supported");
				}
				this.method = value;
			}
		}

		public byte[] ExtraData
		{
			get
			{
				return this.extra;
			}
			set
			{
				if (value == null)
				{
					this.extra = null;
					return;
				}
				if (value.Length > 65535)
				{
					throw new ArgumentOutOfRangeException("value");
				}
				this.extra = new byte[value.Length];
				Array.Copy(value, 0, this.extra, 0, value.Length);
			}
		}

		internal void ProcessExtraData(bool localHeader)
		{
			ZipExtraData zipExtraData = new ZipExtraData(this.extra);
			if (zipExtraData.Find(1))
			{
				if ((this.versionToExtract & 255) < 45)
				{
					throw new ZipException("Zip64 Extended information found but version is not valid");
				}
				this.forceZip64_ = true;
				if (zipExtraData.ValueLength < 4)
				{
					throw new ZipException("Extra data extended Zip64 information length is invalid");
				}
				if (localHeader || this.size == unchecked((ulong)-1))
				{
					this.size = (ulong)zipExtraData.ReadLong();
				}
				if (localHeader || this.compressedSize == unchecked((ulong)-1))
				{
					this.compressedSize = (ulong)zipExtraData.ReadLong();
				}
			}
			else if ((this.versionToExtract & 255) >= 45 && (this.size == unchecked((ulong)-1) || this.compressedSize == unchecked((ulong)-1)))
			{
				throw new ZipException("Zip64 Extended information required but is missing.");
			}
			if (zipExtraData.Find(21589))
			{
				int valueLength = zipExtraData.ValueLength;
				int num = zipExtraData.ReadByte();
				if ((num & 1) != 0 && valueLength >= 5)
				{
					int seconds = zipExtraData.ReadInt();
					this.DateTime = (new DateTime(1970, 1, 1, 0, 0, 0).ToUniversalTime() + new TimeSpan(0, 0, 0, seconds, 0)).ToLocalTime();
				}
			}
		}

		public string Comment
		{
			get
			{
				return this.comment;
			}
			set
			{
				if (value != null && value.Length > 65535)
				{
					throw new ArgumentOutOfRangeException("value", "cannot exceed 65535");
				}
				this.comment = value;
			}
		}

		public bool IsDirectory
		{
			get
			{
				int length = this.name.Length;
				return (length > 0 && (this.name[length - 1] == '/' || this.name[length - 1] == '\\')) || this.HasDosAttributes(16);
			}
		}

		public bool IsFile
		{
			get
			{
				return !this.IsDirectory && !this.HasDosAttributes(8);
			}
		}

		public bool IsCompressionMethodSupported()
		{
			return ZipEntry.IsCompressionMethodSupported(this.CompressionMethod);
		}

		public object Clone()
		{
			ZipEntry zipEntry = (ZipEntry)base.MemberwiseClone();
			if (this.extra != null)
			{
				zipEntry.extra = new byte[this.extra.Length];
				Array.Copy(zipEntry.extra, 0, this.extra, 0, this.extra.Length);
			}
			return zipEntry;
		}

		public override string ToString()
		{
			return this.name;
		}

		public static bool IsCompressionMethodSupported(CompressionMethod method)
		{
			return method == CompressionMethod.Deflated || method == CompressionMethod.Stored;
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

		private ZipEntry.Known known;

		private int externalFileAttributes;

		private ushort versionMadeBy;

		private string name;

		private ulong size;

		private ulong compressedSize;

		private ushort versionToExtract;

		private uint crc;

		private uint dosTime;

		private CompressionMethod method;

		private byte[] extra;

		private string comment;

		private int flags;

		private long zipFileIndex;

		private long offset;

		private bool forceZip64_;

		private byte cryptoCheckValue_;

		[Flags]
		private enum Known : byte
		{
			None = 0,
			Size = 1,
			CompressedSize = 2,
			Crc = 4,
			Time = 8,
			ExternalAttributes = 16
		}
	}
}
