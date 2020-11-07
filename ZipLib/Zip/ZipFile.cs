﻿using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using ZipLib.Checksums;
using ZipLib.Core;
using ZipLib.Encryption;
using ZipLib.Zip.Compression;
using ZipLib.Zip.Compression.Streams;

namespace ZipLib.Zip
{
	public class ZipFile : IEnumerable, IDisposable
	{
		private void OnKeysRequired(string fileName)
		{
			if (this.KeysRequired != null)
			{
				KeysRequiredEventArgs keysRequiredEventArgs = new KeysRequiredEventArgs(fileName, this.key);
				this.KeysRequired(this, keysRequiredEventArgs);
				this.key = keysRequiredEventArgs.Key;
			}
		}

		private byte[] Key
		{
			get
			{
				return this.key;
			}
			set
			{
				this.key = value;
			}
		}

		public string Password
		{
			set
			{
				if (value == null || value.Length == 0)
				{
					this.key = null;
					return;
				}
				this.key = PkzipClassic.GenerateKeys(Encoding.ASCII.GetBytes(value));
			}
		}

		private bool HaveKeys
		{
			get
			{
				return this.key != null;
			}
		}

		public ZipFile(string name)
		{
			this.name_ = name;
			this.isStreamOwner = false;
			this.baseStream_ = File.OpenRead(name);
			this.isStreamOwner = true;
			try
			{
				this.ReadEntries();
			}
			catch
			{
				this.DisposeInternal(true);
				throw;
			}
		}

		public ZipFile(FileStream file)
		{
			if (file == null)
			{
				throw new ArgumentNullException("file");
			}
			if (!file.CanSeek)
			{
				throw new ArgumentException("Stream is not seekable", "file");
			}
			this.baseStream_ = file;
			this.name_ = file.Name;
			try
			{
				this.ReadEntries();
			}
			catch
			{
				this.DisposeInternal(true);
				throw;
			}
		}

		public ZipFile(Stream stream)
		{
			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}
			if (!stream.CanSeek)
			{
				throw new ArgumentException("Stream is not seekable", "stream");
			}
			this.baseStream_ = stream;
			if (this.baseStream_.Length > 0L)
			{
				try
				{
					this.ReadEntries();
					return;
				}
				catch
				{
					this.DisposeInternal(true);
					throw;
				}
			}
			this.entries_ = new ZipEntry[0];
			this.isNewArchive_ = true;
		}

		internal ZipFile()
		{
			this.entries_ = new ZipEntry[0];
			this.isNewArchive_ = true;
		}

		~ZipFile()
		{
			this.Dispose(false);
		}

		public void Close()
		{
			this.DisposeInternal(true);
			GC.SuppressFinalize(this);
		}

		public static ZipFile Create(string fileName)
		{
			if (fileName == null)
			{
				throw new ArgumentNullException("fileName");
			}
			return new ZipFile
			{
				name_ = fileName,
				baseStream_ = File.Create(fileName)
			};
		}

		public static ZipFile Create(Stream outStream)
		{
			if (outStream == null)
			{
				throw new ArgumentNullException("outStream");
			}
			if (!outStream.CanWrite)
			{
				throw new ArgumentException("Stream is not writeable", "outStream");
			}
			if (!outStream.CanSeek)
			{
				throw new ArgumentException("Stream is not seekable", "outStream");
			}
			return new ZipFile
			{
				baseStream_ = outStream
			};
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

		public bool IsEmbeddedArchive
		{
			get
			{
				return this.offsetOfFirstEntry > 0L;
			}
		}

		public bool IsNewArchive
		{
			get
			{
				return this.isNewArchive_;
			}
		}

		public string ZipFileComment
		{
			get
			{
				return this.comment_;
			}
		}

		public string Name
		{
			get
			{
				return this.name_;
			}
		}

		[Obsolete("Use the Count property instead")]
		public int Size
		{
			get
			{
				if (this.entries_ != null)
				{
					return this.entries_.Length;
				}
				throw new InvalidOperationException("ZipFile is closed");
			}
		}

		public long Count
		{
			get
			{
				if (this.entries_ != null)
				{
					return (long)this.entries_.Length;
				}
				throw new InvalidOperationException("ZipFile is closed");
			}
		}

		[IndexerName("EntryByIndex")]
		public ZipEntry this[int index]
		{
			get
			{
				return (ZipEntry)this.entries_[index].Clone();
			}
		}

		public IEnumerator GetEnumerator()
		{
			if (this.entries_ == null)
			{
				throw new InvalidOperationException("ZipFile has closed");
			}
			return new ZipFile.ZipEntryEnumerator(this.entries_);
		}

		public int FindEntry(string name, bool ignoreCase)
		{
			if (this.entries_ == null)
			{
				throw new InvalidOperationException("ZipFile has been closed");
			}
			for (int i = 0; i < this.entries_.Length; i++)
			{
				if (string.Compare(name, this.entries_[i].Name, ignoreCase, CultureInfo.InvariantCulture) == 0)
				{
					return i;
				}
			}
			return -1;
		}

		public ZipEntry GetEntry(string name)
		{
			if (this.entries_ == null)
			{
				throw new InvalidOperationException("ZipFile has been closed");
			}
			int num = this.FindEntry(name, true);
			if (num < 0)
			{
				return null;
			}
			return (ZipEntry)this.entries_[num].Clone();
		}

		public Stream GetInputStream(ZipEntry entry)
		{
			if (entry == null)
			{
				throw new ArgumentNullException("entry");
			}
			if (this.entries_ == null)
			{
				throw new InvalidOperationException("ZipFile has closed");
			}
			long num = entry.ZipFileIndex;
			if (num < 0L || num >= (long)this.entries_.Length || this.entries_[(int)(checked((IntPtr)num))].Name != entry.Name)
			{
				num = (long)this.FindEntry(entry.Name, true);
				if (num < 0L)
				{
					throw new ZipException("Entry cannot be found");
				}
			}
			return this.GetInputStream(num);
		}

		public Stream GetInputStream(long entryIndex)
		{
			if (this.entries_ == null)
			{
				throw new InvalidOperationException("ZipFile is not open");
			}
			checked
			{
				long start = this.LocateEntry(this.entries_[(int)((IntPtr)entryIndex)]);
				CompressionMethod compressionMethod = this.entries_[(int)((IntPtr)entryIndex)].CompressionMethod;
				Stream stream = new ZipFile.PartialInputStream(this.baseStream_, start, this.entries_[(int)((IntPtr)entryIndex)].CompressedSize);
				if (this.entries_[(int)((IntPtr)entryIndex)].IsCrypted)
				{
					stream = this.CreateAndInitDecryptionStream(stream, this.entries_[(int)((IntPtr)entryIndex)]);
					if (stream == null)
					{
						throw new ZipException("Unable to decrypt this entry");
					}
				}
				CompressionMethod compressionMethod2 = compressionMethod;
				if (compressionMethod2 != CompressionMethod.Stored)
				{
					if (compressionMethod2 != CompressionMethod.Deflated)
					{
						throw new ZipException("Unsupported compression method " + compressionMethod);
					}
					stream = new InflaterInputStream(stream, new Inflater(true));
				}
				return stream;
			}
		}

		public bool TestArchive(bool testData)
		{
			return this.TestArchive(testData, TestStrategy.FindFirstError, null);
		}

		public bool TestArchive(bool testData, TestStrategy strategy, ZipTestResultHandler resultHandler)
		{
			TestStatus testStatus = new TestStatus(this);
			if (resultHandler != null)
			{
				resultHandler(testStatus, null);
			}
			ZipFile.HeaderTest tests = testData ? (ZipFile.HeaderTest.Extract | ZipFile.HeaderTest.Header) : ZipFile.HeaderTest.Header;
			bool flag = true;
			try
			{
				int num = 0;
				while (flag && (long)num < this.Count)
				{
					if (resultHandler != null)
					{
						testStatus.SetEntry(this[num]);
						testStatus.SetOperation(TestOperation.EntryHeader);
						resultHandler(testStatus, null);
					}
					try
					{
						this.TestLocalHeader(this[num], tests);
					}
					catch (ZipException ex)
					{
						testStatus.AddError();
						if (resultHandler != null)
						{
							resultHandler(testStatus, string.Format("Exception during test - '{0}'", ex.Message));
						}
						if (strategy == TestStrategy.FindFirstError)
						{
							flag = false;
						}
					}
					if (flag && testData && this[num].IsFile)
					{
						if (resultHandler != null)
						{
							testStatus.SetOperation(TestOperation.EntryData);
							resultHandler(testStatus, null);
						}
						Stream inputStream = this.GetInputStream(this[num]);
						Crc32 crc = new Crc32();
						byte[] array = new byte[4096];
						long num2 = 0L;
						int num3;
						while ((num3 = inputStream.Read(array, 0, array.Length)) > 0)
						{
							crc.Update(array, 0, num3);
							if (resultHandler != null)
							{
								num2 += (long)num3;
								testStatus.SetBytesTested(num2);
								resultHandler(testStatus, null);
							}
						}
						if (this[num].Crc != crc.Value)
						{
							testStatus.AddError();
							if (resultHandler != null)
							{
								resultHandler(testStatus, "CRC mismatch");
							}
							if (strategy == TestStrategy.FindFirstError)
							{
								flag = false;
							}
						}
					}
					if (resultHandler != null)
					{
						testStatus.SetOperation(TestOperation.EntryComplete);
						resultHandler(testStatus, null);
					}
					num++;
				}
				if (resultHandler != null)
				{
					testStatus.SetOperation(TestOperation.MiscellaneousTests);
					resultHandler(testStatus, null);
				}
			}
			catch (Exception ex2)
			{
				testStatus.AddError();
				if (resultHandler != null)
				{
					resultHandler(testStatus, string.Format("Exception during test - '{0}'", ex2.Message));
				}
			}
			if (resultHandler != null)
			{
				testStatus.SetOperation(TestOperation.Complete);
				testStatus.SetEntry(null);
				resultHandler(testStatus, null);
			}
			return testStatus.ErrorCount == 0;
		}

		private long TestLocalHeader(ZipEntry entry, ZipFile.HeaderTest tests)
		{
			long result;
			lock (this.baseStream_)
			{
				bool flag = (tests & ZipFile.HeaderTest.Header) != (ZipFile.HeaderTest)0;
				bool flag2 = (tests & ZipFile.HeaderTest.Extract) != (ZipFile.HeaderTest)0;
				this.baseStream_.Seek(this.offsetOfFirstEntry + entry.Offset, SeekOrigin.Begin);
				if (this.ReadLEUint() != 67324752U)
				{
					throw new ZipException(string.Format("Wrong local header signature @{0:X}", this.offsetOfFirstEntry + entry.Offset));
				}
				short num = (short)this.ReadLEUshort();
				short num2 = (short)this.ReadLEUshort();
				short num3 = (short)this.ReadLEUshort();
				short num4 = (short)this.ReadLEUshort();
				short num5 = (short)this.ReadLEUshort();
				uint num6 = this.ReadLEUint();
				long num7 = (long)((ulong)this.ReadLEUint());
				long num8 = (long)((ulong)this.ReadLEUint());
				int num9 = (int)this.ReadLEUshort();
				int num10 = (int)this.ReadLEUshort();
				if (flag2 && entry.IsFile)
				{
					if (!entry.IsCompressionMethodSupported())
					{
						throw new ZipException("Compression method not supported");
					}
					if (num > 45 || (num > 20 && num < 45))
					{
						throw new ZipException(string.Format("Version required to extract this entry not supported ({0})", num));
					}
					if ((num2 & 12384) != 0)
					{
						throw new ZipException("The library does not support the zip version required to extract this entry");
					}
				}
				if (flag)
				{
					if (num <= 63 && num != 10 && num != 11 && num != 20 && num != 21 && num != 25 && num != 27 && num != 45 && num != 46 && num != 50 && num != 51 && num != 52 && num != 61 && num != 62 && num != 63)
					{
						throw new ZipException(string.Format("Version required to extract this entry is invalid ({0})", num));
					}
					if (((int)num2 & 49168) != 0)
					{
						throw new ZipException("Reserved bit flags cannot be set.");
					}
					if ((num2 & 1) != 0 && num < 20)
					{
						throw new ZipException(string.Format("Version required to extract this entry is too low for encryption ({0})", num));
					}
					if ((num2 & 64) != 0)
					{
						if ((num2 & 1) == 0)
						{
							throw new ZipException("Strong encryption flag set but encryption flag is not set");
						}
						if (num < 50)
						{
							throw new ZipException(string.Format("Version required to extract this entry is too low for encryption ({0})", num));
						}
					}
					if ((num2 & 32) != 0 && num < 27)
					{
						throw new ZipException(string.Format("Patched data requires higher version than ({0})", num));
					}
					if ((int)num2 != entry.Flags)
					{
						throw new ZipException("Central header/local header flags mismatch");
					}
					if (entry.CompressionMethod != (CompressionMethod)num3)
					{
						throw new ZipException("Central header/local header compression method mismatch");
					}
					if ((num2 & 64) != 0 && num < 62)
					{
						throw new ZipException("Strong encryption flag set but version not high enough");
					}
					if ((num2 & 8192) != 0 && (num4 != 0 || num5 != 0))
					{
						throw new ZipException("Header masked set but date/time values non-zero");
					}
					if ((num2 & 8) == 0 && num6 != (uint)entry.Crc)
					{
						throw new ZipException("Central header/local header crc mismatch");
					}
					if (num7 == 0L && num8 == 0L && num6 != 0U)
					{
						throw new ZipException("Invalid CRC for empty entry");
					}
					if (entry.Name.Length > num9)
					{
						throw new ZipException("File name length mismatch");
					}
					byte[] array = new byte[num9];
					StreamUtils.ReadFully(this.baseStream_, array);
					string text = ZipConstants.ConvertToStringExt((int)num2, array);
					if (text != entry.Name)
					{
						throw new ZipException("Central header and local header file name mismatch");
					}
					if (entry.IsDirectory && (num8 != 0L || num7 != 0L))
					{
						throw new ZipException("Directory cannot have size");
					}
					if (!ZipNameTransform.IsValidName(text, true))
					{
						throw new ZipException("Name is invalid");
					}
					byte[] array2 = new byte[num10];
					StreamUtils.ReadFully(this.baseStream_, array2);
					ZipExtraData zipExtraData = new ZipExtraData(array2);
					if (zipExtraData.Find(1))
					{
						if (num < 45)
						{
							throw new ZipException(string.Format("Extra data contains Zip64 information but version {0}.{1} is not high enough", (int)(num / 10), (int)(num % 10)));
						}
						if ((uint)num7 != 4294967295U && (uint)num8 != 4294967295U)
						{
							throw new ZipException("Entry sizes not correct for Zip64");
						}
						num7 = zipExtraData.ReadLong();
						num8 = zipExtraData.ReadLong();
					}
					else if (num >= 45 && ((uint)num7 == 4294967295U || (uint)num8 == 4294967295U))
					{
						throw new ZipException("Required Zip64 extended information missing");
					}
				}
				int num11 = num9 + num10;
				result = this.offsetOfFirstEntry + entry.Offset + 30L + (long)num11;
			}
			return result;
		}

		public INameTransform NameTransform
		{
			get
			{
				return this.updateNameTransform_;
			}
			set
			{
				this.updateNameTransform_ = value;
			}
		}

		public int BufferSize
		{
			get
			{
				return this.bufferSize_;
			}
			set
			{
				if (value < 1024)
				{
					throw new ArgumentOutOfRangeException("value", "cannot be below 1024");
				}
				if (this.bufferSize_ != value)
				{
					this.bufferSize_ = value;
					this.copyBuffer_ = null;
				}
			}
		}

		public bool IsUpdating
		{
			get
			{
				return this.updates_ != null;
			}
		}

		public UseZip64 UseZip64
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

		public void BeginUpdate(IArchiveStorage archiveStorage, IDynamicDataSource dataSource)
		{
			if (this.IsEmbeddedArchive)
			{
				throw new ZipException("Cannot update embedded/SFX archives");
			}
			if (archiveStorage == null)
			{
				throw new ArgumentNullException("archiveStorage");
			}
			if (dataSource == null)
			{
				throw new ArgumentNullException("dataSource");
			}
			this.archiveStorage_ = archiveStorage;
			this.updateDataSource_ = dataSource;
			if (this.entries_ != null)
			{
				this.updates_ = new ArrayList(this.entries_.Length);
				foreach (ZipEntry entry in this.entries_)
				{
					this.updates_.Add(new ZipFile.ZipUpdate(entry));
				}
			}
			else
			{
				this.updates_ = new ArrayList();
			}
			this.contentsEdited_ = false;
			this.commentEdited_ = false;
			this.newComment_ = null;
		}

		public void BeginUpdate(IArchiveStorage archiveStorage)
		{
			this.BeginUpdate(archiveStorage, new DynamicDiskDataSource());
		}

		public void BeginUpdate()
		{
			if (this.Name == null)
			{
				this.BeginUpdate(new MemoryArchiveStorage(), new DynamicDiskDataSource());
				return;
			}
			this.BeginUpdate(new DiskArchiveStorage(this), new DynamicDiskDataSource());
		}

		public void CommitUpdate()
		{
			this.CheckUpdating();
			if (this.contentsEdited_)
			{
				this.RunUpdates();
			}
			else if (this.commentEdited_)
			{
				this.UpdateCommentOnly();
			}
			else if (this.entries_ != null && this.entries_.Length == 0)
			{
				byte[] comment = (this.newComment_ != null) ? this.newComment_.RawComment : ZipConstants.ConvertToArray(this.comment_);
				using (ZipHelperStream zipHelperStream = new ZipHelperStream(this.baseStream_))
				{
					zipHelperStream.WriteEndOfCentralDirectory(0L, 0L, 0L, comment);
				}
			}
			this.PostUpdateCleanup();
		}

		public void AbortUpdate()
		{
			this.updates_ = null;
			this.PostUpdateCleanup();
		}

		public void SetComment(string comment)
		{
			this.CheckUpdating();
			this.newComment_ = new ZipFile.ZipString(comment);
			if (this.newComment_.RawLength > 65535)
			{
				this.newComment_ = null;
				throw new ZipException("Comment length exceeds maximum - 65535");
			}
			this.commentEdited_ = true;
		}

		public void Add(string fileName, CompressionMethod compressionMethod, bool useUnicodeText)
		{
			if (fileName == null)
			{
				throw new ArgumentNullException("fileName");
			}
			if (!ZipEntry.IsCompressionMethodSupported(compressionMethod))
			{
				throw new ZipException("Compression method not supported");
			}
			this.CheckUpdating();
			this.contentsEdited_ = true;
			string transformedFileName = this.GetTransformedFileName(fileName);
			int num = this.FindExistingUpdate(transformedFileName);
			if (num >= 0)
			{
				this.updates_.RemoveAt(num);
			}
			ZipFile.ZipUpdate zipUpdate = new ZipFile.ZipUpdate(fileName, transformedFileName, compressionMethod);
			zipUpdate.Entry.IsUnicodeText = useUnicodeText;
			this.updates_.Add(zipUpdate);
		}

		public void Add(string fileName, CompressionMethod compressionMethod)
		{
			if (fileName == null)
			{
				throw new ArgumentNullException("fileName");
			}
			if (!ZipEntry.IsCompressionMethodSupported(compressionMethod))
			{
				throw new ZipException("Compression method not supported");
			}
			this.CheckUpdating();
			this.contentsEdited_ = true;
			string transformedFileName = this.GetTransformedFileName(fileName);
			int num = this.FindExistingUpdate(transformedFileName);
			if (num >= 0)
			{
				this.updates_.RemoveAt(num);
			}
			this.updates_.Add(new ZipFile.ZipUpdate(fileName, transformedFileName, compressionMethod));
		}

		public void Add(string fileName)
		{
			if (fileName == null)
			{
				throw new ArgumentNullException("fileName");
			}
			this.CheckUpdating();
			this.Add(fileName, CompressionMethod.Deflated);
		}

		public void Add(IStaticDataSource dataSource, string entryName)
		{
			if (dataSource == null)
			{
				throw new ArgumentNullException("dataSource");
			}
			this.CheckUpdating();
			this.contentsEdited_ = true;
			this.updates_.Add(new ZipFile.ZipUpdate(dataSource, this.GetTransformedFileName(entryName), CompressionMethod.Deflated));
		}

		public void Add(IStaticDataSource dataSource, string entryName, CompressionMethod compressionMethod)
		{
			if (dataSource == null)
			{
				throw new ArgumentNullException("dataSource");
			}
			this.CheckUpdating();
			this.contentsEdited_ = true;
			this.updates_.Add(new ZipFile.ZipUpdate(dataSource, this.GetTransformedFileName(entryName), compressionMethod));
		}

		public void Add(IStaticDataSource dataSource, string entryName, CompressionMethod compressionMethod, bool useUnicodeText)
		{
			if (dataSource == null)
			{
				throw new ArgumentNullException("dataSource");
			}
			this.CheckUpdating();
			this.contentsEdited_ = true;
			ZipFile.ZipUpdate zipUpdate = new ZipFile.ZipUpdate(dataSource, this.GetTransformedFileName(entryName), compressionMethod);
			zipUpdate.Entry.IsUnicodeText = useUnicodeText;
			this.updates_.Add(zipUpdate);
		}

		public void Add(ZipEntry entry)
		{
			if (entry == null)
			{
				throw new ArgumentNullException("entry");
			}
			this.CheckUpdating();
			if (entry.Size != 0L || entry.CompressedSize != 0L)
			{
				throw new ZipException("Entry cannot have any data");
			}
			this.contentsEdited_ = true;
			this.updates_.Add(new ZipFile.ZipUpdate(ZipFile.UpdateCommand.Add, entry));
		}

		public void AddDirectory(string directoryName)
		{
			if (directoryName == null)
			{
				throw new ArgumentNullException("directoryName");
			}
			this.CheckUpdating();
			ZipEntry zipEntry = new ZipEntry(this.GetTransformedDirectoryName(directoryName));
			zipEntry.ExternalFileAttributes = 16;
			this.updates_.Add(new ZipFile.ZipUpdate(ZipFile.UpdateCommand.Add, zipEntry));
		}

		public bool Delete(string fileName)
		{
			this.CheckUpdating();
			int num = this.FindExistingUpdate(fileName);
			if (num >= 0)
			{
				bool result = true;
				this.contentsEdited_ = true;
				this.updates_.RemoveAt(num);
				return result;
			}
			throw new ZipException("Cannot find entry to delete");
		}

		public void Delete(ZipEntry entry)
		{
			this.CheckUpdating();
			int num = this.FindExistingUpdate(entry);
			if (num >= 0)
			{
				this.contentsEdited_ = true;
				this.updates_.RemoveAt(num);
				return;
			}
			throw new ZipException("Cannot find entry to delete");
		}

		private void WriteLEShort(int value)
		{
			this.baseStream_.WriteByte((byte)(value & 255));
			this.baseStream_.WriteByte((byte)(value >> 8 & 255));
		}

		private void WriteLEUshort(ushort value)
		{
			this.baseStream_.WriteByte((byte)(value & 255));
			this.baseStream_.WriteByte((byte)(value >> 8));
		}

		private void WriteLEInt(int value)
		{
			this.WriteLEShort(value);
			this.WriteLEShort(value >> 16);
		}

		private void WriteLEUint(uint value)
		{
			this.WriteLEUshort((ushort)(value & 65535U));
			this.WriteLEUshort((ushort)(value >> 16));
		}

		private void WriteLeLong(long value)
		{
			this.WriteLEInt((int)(value & unchecked((long)((ulong)-1))));
			this.WriteLEInt((int)(value >> 32));
		}

		private void WriteLEUlong(ulong value)
		{
			this.WriteLEUint((uint)(value & unchecked((ulong)-1)));
			this.WriteLEUint((uint)(value >> 32));
		}

		private void WriteLocalEntryHeader(ZipFile.ZipUpdate update)
		{
			ZipEntry outEntry = update.OutEntry;
			outEntry.Offset = this.baseStream_.Position;
			if (update.Command != ZipFile.UpdateCommand.Copy)
			{
				if (outEntry.CompressionMethod == CompressionMethod.Deflated)
				{
					if (outEntry.Size == 0L)
					{
						outEntry.CompressedSize = outEntry.Size;
						outEntry.Crc = 0L;
						outEntry.CompressionMethod = CompressionMethod.Stored;
					}
				}
				else if (outEntry.CompressionMethod == CompressionMethod.Stored)
				{
					outEntry.Flags &= -9;
				}
				if (this.HaveKeys)
				{
					outEntry.IsCrypted = true;
					if (outEntry.Crc < 0L)
					{
						outEntry.Flags |= 8;
					}
				}
				else
				{
					outEntry.IsCrypted = false;
				}
				switch (this.useZip64_)
				{
				case UseZip64.On:
					outEntry.ForceZip64();
					break;
				case UseZip64.Dynamic:
					if (outEntry.Size < 0L)
					{
						outEntry.ForceZip64();
					}
					break;
				}
			}
			this.WriteLEInt(67324752);
			this.WriteLEShort(outEntry.Version);
			this.WriteLEShort(outEntry.Flags);
			this.WriteLEShort((int)((byte)outEntry.CompressionMethod));
			this.WriteLEInt((int)outEntry.DosTime);
			if (!outEntry.HasCrc)
			{
				update.CrcPatchOffset = this.baseStream_.Position;
				this.WriteLEInt(0);
			}
			else
			{
				this.WriteLEInt((int)outEntry.Crc);
			}
			if (outEntry.LocalHeaderRequiresZip64)
			{
				this.WriteLEInt(-1);
				this.WriteLEInt(-1);
			}
			else
			{
				if (outEntry.CompressedSize < 0L || outEntry.Size < 0L)
				{
					update.SizePatchOffset = this.baseStream_.Position;
				}
				this.WriteLEInt((int)outEntry.CompressedSize);
				this.WriteLEInt((int)outEntry.Size);
			}
			byte[] array = ZipConstants.ConvertToArray(outEntry.Flags, outEntry.Name);
			if (array.Length > 65535)
			{
				throw new ZipException("Entry name too long.");
			}
			ZipExtraData zipExtraData = new ZipExtraData(outEntry.ExtraData);
			if (outEntry.LocalHeaderRequiresZip64)
			{
				zipExtraData.StartNewEntry();
				zipExtraData.AddLeLong(outEntry.Size);
				zipExtraData.AddLeLong(outEntry.CompressedSize);
				zipExtraData.AddNewEntry(1);
			}
			else
			{
				zipExtraData.Delete(1);
			}
			outEntry.ExtraData = zipExtraData.GetEntryData();
			this.WriteLEShort(array.Length);
			this.WriteLEShort(outEntry.ExtraData.Length);
			if (array.Length > 0)
			{
				this.baseStream_.Write(array, 0, array.Length);
			}
			if (outEntry.LocalHeaderRequiresZip64)
			{
				if (!zipExtraData.Find(1))
				{
					throw new ZipException("Internal error cannot find extra data");
				}
				update.SizePatchOffset = this.baseStream_.Position + (long)zipExtraData.CurrentReadIndex;
			}
			if (outEntry.ExtraData.Length > 0)
			{
				this.baseStream_.Write(outEntry.ExtraData, 0, outEntry.ExtraData.Length);
			}
		}

		private int WriteCentralDirectoryHeader(ZipEntry entry)
		{
			this.WriteLEInt(33639248);
			this.WriteLEShort(45);
			this.WriteLEShort(entry.Version);
			this.WriteLEShort(entry.Flags);
			this.WriteLEShort((int)((byte)entry.CompressionMethod));
			this.WriteLEInt((int)entry.DosTime);
			this.WriteLEInt((int)entry.Crc);
			if (entry.CompressedSize >= unchecked((long)((ulong)-1)))
			{
				this.WriteLEInt(-1);
			}
			else
			{
				this.WriteLEInt((int)(entry.CompressedSize & unchecked((long)((ulong)-1))));
			}
			if (entry.Size >= unchecked((long)((ulong)-1)))
			{
				this.WriteLEInt(-1);
			}
			else
			{
				this.WriteLEInt((int)entry.Size);
			}
			byte[] array = ZipConstants.ConvertToArray(entry.Flags, entry.Name);
			if (array.Length > 65535)
			{
				throw new ZipException("Entry name is too long.");
			}
			this.WriteLEShort(array.Length);
			ZipExtraData zipExtraData = new ZipExtraData(entry.ExtraData);
			if (entry.CentralHeaderRequiresZip64)
			{
				zipExtraData.StartNewEntry();
				if (entry.Size >= unchecked((long)((ulong)-1)) || this.useZip64_ == UseZip64.On)
				{
					zipExtraData.AddLeLong(entry.Size);
				}
				if (entry.CompressedSize >= unchecked((long)((ulong)-1)) || this.useZip64_ == UseZip64.On)
				{
					zipExtraData.AddLeLong(entry.CompressedSize);
				}
				if (entry.Offset >= unchecked((long)((ulong)-1)))
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
			this.WriteLEShort(entryData.Length);
			this.WriteLEShort((entry.Comment != null) ? entry.Comment.Length : 0);
			this.WriteLEShort(0);
			this.WriteLEShort(0);
			if (entry.ExternalFileAttributes != -1)
			{
				this.WriteLEInt(entry.ExternalFileAttributes);
			}
			else if (entry.IsDirectory)
			{
				this.WriteLEUint(16U);
			}
			else
			{
				this.WriteLEUint(0U);
			}
			if (entry.Offset >= unchecked((long)((ulong)-1)))
			{
				this.WriteLEUint(uint.MaxValue);
			}
			else
			{
				this.WriteLEUint((uint)((int)entry.Offset));
			}
			if (array.Length > 0)
			{
				this.baseStream_.Write(array, 0, array.Length);
			}
			if (entryData.Length > 0)
			{
				this.baseStream_.Write(entryData, 0, entryData.Length);
			}
			byte[] array2 = (entry.Comment != null) ? Encoding.ASCII.GetBytes(entry.Comment) : new byte[0];
			if (array2.Length > 0)
			{
				this.baseStream_.Write(array2, 0, array2.Length);
			}
			return 46 + array.Length + entryData.Length + array2.Length;
		}

		private void PostUpdateCleanup()
		{
			if (this.archiveStorage_ != null)
			{
				this.archiveStorage_.Dispose();
				this.archiveStorage_ = null;
			}
			this.updateDataSource_ = null;
		}

		private string GetTransformedFileName(string name)
		{
			if (this.updateNameTransform_ == null)
			{
				return name;
			}
			return this.updateNameTransform_.TransformFile(name);
		}

		private string GetTransformedDirectoryName(string name)
		{
			if (this.updateNameTransform_ == null)
			{
				return name;
			}
			return this.updateNameTransform_.TransformDirectory(name);
		}

		private byte[] GetBuffer()
		{
			if (this.copyBuffer_ == null)
			{
				this.copyBuffer_ = new byte[this.bufferSize_];
			}
			return this.copyBuffer_;
		}

		private void CopyDescriptorBytes(ZipFile.ZipUpdate update, Stream dest, Stream source)
		{
			int i = this.GetDescriptorSize(update);
			if (i > 0)
			{
				byte[] buffer = this.GetBuffer();
				while (i > 0)
				{
					int count = Math.Min(buffer.Length, i);
					int num = source.Read(buffer, 0, count);
					if (num <= 0)
					{
						throw new ZipException("Unxpected end of stream");
					}
					dest.Write(buffer, 0, num);
					i -= num;
				}
			}
		}

		private void CopyBytes(ZipFile.ZipUpdate update, Stream destination, Stream source, long bytesToCopy, bool updateCrc)
		{
			if (destination == source)
			{
				throw new InvalidOperationException("Destination and source are the same");
			}
			Crc32 crc = new Crc32();
			byte[] buffer = this.GetBuffer();
			long num = bytesToCopy;
			long num2 = 0L;
			int num4;
			do
			{
				int num3 = buffer.Length;
				if (bytesToCopy < (long)num3)
				{
					num3 = (int)bytesToCopy;
				}
				num4 = source.Read(buffer, 0, num3);
				if (num4 > 0)
				{
					if (updateCrc)
					{
						crc.Update(buffer, 0, num4);
					}
					destination.Write(buffer, 0, num4);
					bytesToCopy -= (long)num4;
					num2 += (long)num4;
				}
			}
			while (num4 > 0 && bytesToCopy > 0L);
			if (num2 != num)
			{
				throw new ZipException(string.Format("Failed to copy bytes expected {0} read {1}", num, num2));
			}
			if (updateCrc)
			{
				update.OutEntry.Crc = crc.Value;
			}
		}

		private int GetDescriptorSize(ZipFile.ZipUpdate update)
		{
			int result = 0;
			if ((update.Entry.Flags & 8) != 0)
			{
				result = 12;
				if (update.Entry.LocalHeaderRequiresZip64)
				{
					result = 20;
				}
			}
			return result;
		}

		private void CopyDescriptorBytesDirect(ZipFile.ZipUpdate update, Stream stream, ref long destinationPosition, long sourcePosition)
		{
			int i = this.GetDescriptorSize(update);
			while (i > 0)
			{
				int count = i;
				byte[] buffer = this.GetBuffer();
				stream.Position = sourcePosition;
				int num = stream.Read(buffer, 0, count);
				if (num <= 0)
				{
					throw new ZipException("Unxpected end of stream");
				}
				stream.Position = destinationPosition;
				stream.Write(buffer, 0, num);
				i -= num;
				destinationPosition += (long)num;
				sourcePosition += (long)num;
			}
		}

		private void CopyEntryDataDirect(ZipFile.ZipUpdate update, Stream stream, bool updateCrc, ref long destinationPosition, ref long sourcePosition)
		{
			long num = update.Entry.CompressedSize;
			Crc32 crc = new Crc32();
			byte[] buffer = this.GetBuffer();
			long num2 = num;
			long num3 = 0L;
			int num5;
			do
			{
				int num4 = buffer.Length;
				if (num < (long)num4)
				{
					num4 = (int)num;
				}
				stream.Position = sourcePosition;
				num5 = stream.Read(buffer, 0, num4);
				if (num5 > 0)
				{
					if (updateCrc)
					{
						crc.Update(buffer, 0, num5);
					}
					stream.Position = destinationPosition;
					stream.Write(buffer, 0, num5);
					destinationPosition += (long)num5;
					sourcePosition += (long)num5;
					num -= (long)num5;
					num3 += (long)num5;
				}
			}
			while (num5 > 0 && num > 0L);
			if (num3 != num2)
			{
				throw new ZipException(string.Format("Failed to copy bytes expected {0} read {1}", num2, num3));
			}
			if (updateCrc)
			{
				update.OutEntry.Crc = crc.Value;
			}
		}

		private int FindExistingUpdate(ZipEntry entry)
		{
			int result = -1;
			string transformedFileName = this.GetTransformedFileName(entry.Name);
			for (int i = 0; i < this.updates_.Count; i++)
			{
				ZipFile.ZipUpdate zipUpdate = (ZipFile.ZipUpdate)this.updates_[i];
				if (zipUpdate.Entry.ZipFileIndex == entry.ZipFileIndex && string.Compare(transformedFileName, zipUpdate.Entry.Name, true, CultureInfo.InvariantCulture) == 0)
				{
					result = i;
					break;
				}
			}
			return result;
		}

		private int FindExistingUpdate(string fileName)
		{
			int result = -1;
			string transformedFileName = this.GetTransformedFileName(fileName);
			for (int i = 0; i < this.updates_.Count; i++)
			{
				if (string.Compare(transformedFileName, ((ZipFile.ZipUpdate)this.updates_[i]).Entry.Name, true, CultureInfo.InvariantCulture) == 0)
				{
					result = i;
					break;
				}
			}
			return result;
		}

		private Stream GetOutputStream(ZipEntry entry)
		{
			Stream stream = this.baseStream_;
			if (entry.IsCrypted)
			{
				stream = this.CreateAndInitEncryptionStream(stream, entry);
			}
			CompressionMethod compressionMethod = entry.CompressionMethod;
			if (compressionMethod != CompressionMethod.Stored)
			{
				if (compressionMethod != CompressionMethod.Deflated)
				{
					throw new ZipException("Unknown compression method " + entry.CompressionMethod);
				}
				stream = new DeflaterOutputStream(stream, new Deflater(9, true))
				{
					IsStreamOwner = false
				};
			}
			else
			{
				stream = new ZipFile.UncompressedStream(stream);
			}
			return stream;
		}

		private void AddEntry(ZipFile workFile, ZipFile.ZipUpdate update)
		{
			long num = 0L;
			Stream stream = null;
			if (update.Entry.IsFile)
			{
				stream = update.GetSource();
				if (stream == null)
				{
					stream = this.updateDataSource_.GetSource(update.Entry, update.Filename);
				}
			}
			if (stream != null)
			{
				using (stream)
				{
					long length = stream.Length;
					if (update.OutEntry.Size < 0L)
					{
						update.OutEntry.Size = length;
					}
					else if (update.OutEntry.Size != length)
					{
						throw new ZipException("Entry size/stream size mismatch");
					}
					workFile.WriteLocalEntryHeader(update);
					num = workFile.baseStream_.Position;
					using (Stream outputStream = workFile.GetOutputStream(update.OutEntry))
					{
						this.CopyBytes(update, outputStream, stream, length, true);
					}
					goto IL_D2;
				}
			}
			workFile.WriteLocalEntryHeader(update);
			num = workFile.baseStream_.Position;
			IL_D2:
			long position = workFile.baseStream_.Position;
			update.OutEntry.CompressedSize = position - num;
		}

		private void ModifyEntry(ZipFile workFile, ZipFile.ZipUpdate update)
		{
			workFile.WriteLocalEntryHeader(update);
			long position = workFile.baseStream_.Position;
			if (update.Entry.IsFile && update.Filename != null)
			{
				using (Stream outputStream = workFile.GetOutputStream(update.OutEntry))
				{
					using (Stream inputStream = this.GetInputStream(update.Entry))
					{
						this.CopyBytes(update, outputStream, inputStream, inputStream.Length, true);
					}
				}
			}
			long position2 = workFile.baseStream_.Position;
			update.Entry.CompressedSize = position2 - position;
		}

		private void CopyEntryDirect(ZipFile workFile, ZipFile.ZipUpdate update, ref long destinationPosition)
		{
			bool flag = false;
			if (update.Entry.Offset == destinationPosition)
			{
				flag = true;
			}
			if (!flag)
			{
				this.baseStream_.Position = destinationPosition;
				workFile.WriteLocalEntryHeader(update);
				destinationPosition = this.baseStream_.Position;
			}
			long num = 0L;
			long num2 = update.Entry.Offset + 26L;
			this.baseStream_.Seek(num2, SeekOrigin.Begin);
			uint num3 = (uint)this.ReadLEUshort();
			uint num4 = (uint)this.ReadLEUshort();
			num = this.baseStream_.Position + (long)((ulong)num3) + (long)((ulong)num4);
			if (flag)
			{
				destinationPosition += num - num2 + 26L + update.Entry.CompressedSize + (long)this.GetDescriptorSize(update);
				return;
			}
			if (update.Entry.CompressedSize > 0L)
			{
				this.CopyEntryDataDirect(update, this.baseStream_, false, ref destinationPosition, ref num);
			}
			this.CopyDescriptorBytesDirect(update, this.baseStream_, ref destinationPosition, num);
		}

		private void CopyEntry(ZipFile workFile, ZipFile.ZipUpdate update)
		{
			workFile.WriteLocalEntryHeader(update);
			if (update.Entry.CompressedSize > 0L)
			{
				long offset = update.Entry.Offset + 26L;
				this.baseStream_.Seek(offset, SeekOrigin.Begin);
				uint num = (uint)this.ReadLEUshort();
				uint num2 = (uint)this.ReadLEUshort();
				this.baseStream_.Seek((long)((ulong)(num + num2)), SeekOrigin.Current);
				this.CopyBytes(update, workFile.baseStream_, this.baseStream_, update.Entry.CompressedSize, false);
			}
			this.CopyDescriptorBytes(update, workFile.baseStream_, this.baseStream_);
		}

		private void Reopen(Stream source)
		{
			if (source == null)
			{
				throw new ZipException("Failed to reopen archive - no source");
			}
			this.isNewArchive_ = false;
			this.baseStream_ = source;
			this.ReadEntries();
		}

		private void Reopen()
		{
			if (this.Name == null)
			{
				throw new InvalidOperationException("Name is not known cannot Reopen");
			}
			this.Reopen(File.OpenRead(this.Name));
		}

		private void UpdateCommentOnly()
		{
			long length = this.baseStream_.Length;
			ZipHelperStream zipHelperStream;
			if (this.archiveStorage_.UpdateMode == FileUpdateMode.Safe)
			{
				Stream stream = this.archiveStorage_.MakeTemporaryCopy(this.baseStream_);
				zipHelperStream = new ZipHelperStream(stream);
				zipHelperStream.IsStreamOwner = true;
				this.baseStream_.Close();
				this.baseStream_ = null;
			}
			else if (this.archiveStorage_.UpdateMode == FileUpdateMode.Direct)
			{
				this.baseStream_ = this.archiveStorage_.OpenForDirectUpdate(this.baseStream_);
				zipHelperStream = new ZipHelperStream(this.baseStream_);
			}
			else
			{
				this.baseStream_.Close();
				this.baseStream_ = null;
				zipHelperStream = new ZipHelperStream(this.Name);
			}
			using (zipHelperStream)
			{
				long num = zipHelperStream.LocateBlockWithSignature(101010256, length, 22, 65535);
				if (num < 0L)
				{
					throw new ZipException("Cannot find central directory");
				}
				zipHelperStream.Position += 16L;
				byte[] rawComment = this.newComment_.RawComment;
				zipHelperStream.WriteLEShort(rawComment.Length);
				zipHelperStream.Write(rawComment, 0, rawComment.Length);
				zipHelperStream.SetLength(zipHelperStream.Position);
			}
			if (this.archiveStorage_.UpdateMode == FileUpdateMode.Safe)
			{
				this.Reopen(this.archiveStorage_.ConvertTemporaryToFinal());
				return;
			}
			this.ReadEntries();
		}

		private void RunUpdates()
		{
			long num = 0L;
			long length = 0L;
			bool flag = true;
			bool flag2 = false;
			long position = 0L;
			ZipFile zipFile;
			if (this.IsNewArchive)
			{
				zipFile = this;
				zipFile.baseStream_.Position = 0L;
				flag2 = true;
			}
			else if (this.archiveStorage_.UpdateMode == FileUpdateMode.Direct)
			{
				zipFile = this;
				zipFile.baseStream_.Position = 0L;
				flag2 = true;
				this.updates_.Sort(new ZipFile.UpdateComparer());
			}
			else
			{
				zipFile = ZipFile.Create(this.archiveStorage_.GetTemporaryOutput());
				if (this.key != null)
				{
					zipFile.key = (byte[])this.key.Clone();
				}
			}
			try
			{
				foreach (object obj in this.updates_)
				{
					ZipFile.ZipUpdate zipUpdate = (ZipFile.ZipUpdate)obj;
					switch (zipUpdate.Command)
					{
					case ZipFile.UpdateCommand.Copy:
						if (flag2)
						{
							this.CopyEntryDirect(zipFile, zipUpdate, ref position);
						}
						else
						{
							this.CopyEntry(zipFile, zipUpdate);
						}
						break;
					case ZipFile.UpdateCommand.Modify:
						this.ModifyEntry(zipFile, zipUpdate);
						break;
					case ZipFile.UpdateCommand.Add:
						if (!this.IsNewArchive && flag2)
						{
							zipFile.baseStream_.Position = position;
						}
						this.AddEntry(zipFile, zipUpdate);
						if (flag2)
						{
							position = zipFile.baseStream_.Position;
						}
						break;
					}
				}
				if (!this.IsNewArchive && flag2)
				{
					zipFile.baseStream_.Position = position;
				}
				long position2 = zipFile.baseStream_.Position;
				foreach (object obj2 in this.updates_)
				{
					ZipFile.ZipUpdate zipUpdate2 = (ZipFile.ZipUpdate)obj2;
					num += (long)zipFile.WriteCentralDirectoryHeader(zipUpdate2.OutEntry);
				}
				byte[] comment = (this.newComment_ != null) ? this.newComment_.RawComment : ZipConstants.ConvertToArray(this.comment_);
				using (ZipHelperStream zipHelperStream = new ZipHelperStream(zipFile.baseStream_))
				{
					zipHelperStream.WriteEndOfCentralDirectory((long)this.updates_.Count, num, position2, comment);
				}
				length = zipFile.baseStream_.Position;
				foreach (object obj3 in this.updates_)
				{
					ZipFile.ZipUpdate zipUpdate3 = (ZipFile.ZipUpdate)obj3;
					if (zipUpdate3.CrcPatchOffset > 0L && zipUpdate3.OutEntry.CompressedSize > 0L)
					{
						zipFile.baseStream_.Position = zipUpdate3.CrcPatchOffset;
						zipFile.WriteLEInt((int)zipUpdate3.OutEntry.Crc);
					}
					if (zipUpdate3.SizePatchOffset > 0L)
					{
						zipFile.baseStream_.Position = zipUpdate3.SizePatchOffset;
						if (zipUpdate3.Entry.LocalHeaderRequiresZip64)
						{
							zipFile.WriteLeLong(zipUpdate3.OutEntry.Size);
							zipFile.WriteLeLong(zipUpdate3.OutEntry.CompressedSize);
						}
						else
						{
							zipFile.WriteLEInt((int)zipUpdate3.OutEntry.CompressedSize);
							zipFile.WriteLEInt((int)zipUpdate3.OutEntry.Size);
						}
					}
				}
			}
			catch (Exception)
			{
				flag = false;
			}
			finally
			{
				if (flag2)
				{
					if (flag)
					{
						zipFile.baseStream_.Flush();
						zipFile.baseStream_.SetLength(length);
					}
				}
				else
				{
					zipFile.Close();
				}
			}
			if (!flag)
			{
				zipFile.Close();
				if (!flag2 && zipFile.Name != null)
				{
					File.Delete(zipFile.Name);
				}
				return;
			}
			if (flag2)
			{
				this.isNewArchive_ = false;
				zipFile.baseStream_.Flush();
				this.ReadEntries();
				return;
			}
			this.baseStream_.Close();
			this.Reopen(this.archiveStorage_.ConvertTemporaryToFinal());
		}

		private void CheckUpdating()
		{
			if (this.updates_ == null)
			{
				throw new ZipException("Cannot update until BeginUpdate has been called");
			}
		}

		void IDisposable.Dispose()
		{
			this.Close();
		}

		private void DisposeInternal(bool disposing)
		{
			if (!this.isDisposed_)
			{
				this.isDisposed_ = true;
				this.entries_ = null;
				if (this.IsStreamOwner)
				{
					lock (this.baseStream_)
					{
						this.baseStream_.Close();
					}
				}
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			this.DisposeInternal(disposing);
		}

		private ushort ReadLEUshort()
		{
			int num = this.baseStream_.ReadByte();
			if (num < 0)
			{
				throw new IOException("End of stream");
			}
			int num2 = this.baseStream_.ReadByte();
			if (num2 < 0)
			{
				throw new IOException("End of stream");
			}
			return Convert.ToUInt16(((ushort)num) | (ushort)(num2 << 8));
		}

		private uint ReadLEUint()
		{
			return (uint)((int)this.ReadLEUshort() | (int)this.ReadLEUshort() << 16);
		}

		private ulong ReadLEUlong()
		{
			return (ulong)(this.ReadLEUint() | this.ReadLEUint());
		}

		private long LocateBlockWithSignature(int signature, long endLocation, int minimumBlockSize, int maximumVariableData)
		{
			long result;
			using (ZipHelperStream zipHelperStream = new ZipHelperStream(this.baseStream_))
			{
				result = zipHelperStream.LocateBlockWithSignature(signature, endLocation, minimumBlockSize, maximumVariableData);
			}
			return result;
		}

		private void ReadEntries()
		{
			if (!this.baseStream_.CanSeek)
			{
				throw new ZipException("ZipFile stream must be seekable");
			}
			long num = this.LocateBlockWithSignature(101010256, this.baseStream_.Length, 22, 65535);
			if (num < 0L)
			{
				throw new ZipException("Cannot find central directory");
			}
			ushort num2 = this.ReadLEUshort();
			ushort num3 = this.ReadLEUshort();
			ulong num4 = (ulong)this.ReadLEUshort();
			ulong num5 = (ulong)this.ReadLEUshort();
			ulong num6 = (ulong)this.ReadLEUint();
			long num7 = (long)((ulong)this.ReadLEUint());
			uint num8 = (uint)this.ReadLEUshort();
			if (num8 > 0U)
			{
				byte[] array = new byte[num8];
				StreamUtils.ReadFully(this.baseStream_, array);
				this.comment_ = ZipConstants.ConvertToString(array);
			}
			else
			{
				this.comment_ = string.Empty;
			}
			bool flag = false;
			if (num2 == 65535 || num3 == 65535 || num4 == 65535UL || num5 == 65535UL || num6 == unchecked((ulong)-1) || num7 == unchecked((long)((ulong)-1)))
			{
				flag = true;
				long num9 = this.LocateBlockWithSignature(117853008, num, 0, 4096);
				if (num9 < 0L)
				{
					throw new ZipException("Cannot find Zip64 locator");
				}
				this.ReadLEUint();
				ulong num10 = this.ReadLEUlong();
				this.ReadLEUint();
				this.baseStream_.Position = (long)num10;
				long num11 = (long)((ulong)this.ReadLEUint());
				if (num11 != 101075792L)
				{
					throw new ZipException(string.Format("Invalid Zip64 Central directory signature at {0:X}", num10));
				}
				this.ReadLEUlong();
				this.ReadLEUshort();
				this.ReadLEUshort();
				this.ReadLEUint();
				this.ReadLEUint();
				num4 = this.ReadLEUlong();
				num5 = this.ReadLEUlong();
				num6 = this.ReadLEUlong();
				num7 = (long)this.ReadLEUlong();
			}
			this.entries_ = new ZipEntry[num4];
			if (!flag && num7 < num - (long)(4UL + num6))
			{
				this.offsetOfFirstEntry = num - (long)(4UL + num6 + (ulong)num7);
				if (this.offsetOfFirstEntry <= 0L)
				{
					throw new ZipException("Invalid embedded zip archive");
				}
			}
			this.baseStream_.Seek(this.offsetOfFirstEntry + num7, SeekOrigin.Begin);
			for (ulong num12 = 0UL; num12 < num4; num12 += 1UL)
			{
				if (this.ReadLEUint() != 33639248U)
				{
					throw new ZipException("Wrong Central Directory signature");
				}
				int madeByInfo = (int)this.ReadLEUshort();
				int versionRequiredToExtract = (int)this.ReadLEUshort();
				int num13 = (int)this.ReadLEUshort();
				int method = (int)this.ReadLEUshort();
				uint num14 = this.ReadLEUint();
				uint num15 = this.ReadLEUint();
				long num16 = (long)((ulong)this.ReadLEUint());
				long num17 = (long)((ulong)this.ReadLEUint());
				int num18 = (int)this.ReadLEUshort();
				int num19 = (int)this.ReadLEUshort();
				int num20 = (int)this.ReadLEUshort();
				this.ReadLEUshort();
				this.ReadLEUshort();
				uint externalFileAttributes = this.ReadLEUint();
				long offset = (long)((ulong)this.ReadLEUint());
				byte[] array2 = new byte[Math.Max(num18, num20)];
				StreamUtils.ReadFully(this.baseStream_, array2, 0, num18);
				string name = ZipConstants.ConvertToStringExt(num13, array2, num18);
				ZipEntry zipEntry = new ZipEntry(name, versionRequiredToExtract, madeByInfo, (CompressionMethod)method);
				zipEntry.Crc = (long)((ulong)num15 & unchecked((ulong)-1));
				zipEntry.Size = (num17 & unchecked((long)((ulong)-1)));
				zipEntry.CompressedSize = (num16 & unchecked((long)((ulong)-1)));
				zipEntry.Flags = num13;
				zipEntry.DosTime = (long)((ulong)num14);
				if ((num13 & 8) == 0)
				{
					zipEntry.CryptoCheckValue = (byte)(num15 >> 24);
				}
				else
				{
					zipEntry.CryptoCheckValue = (byte)(num14 >> 8 & 255U);
				}
				if (num19 > 0)
				{
					byte[] array3 = new byte[num19];
					StreamUtils.ReadFully(this.baseStream_, array3);
					zipEntry.ExtraData = array3;
				}
				zipEntry.ProcessExtraData(false);
				if (num20 > 0)
				{
					StreamUtils.ReadFully(this.baseStream_, array2, 0, num20);
					zipEntry.Comment = ZipConstants.ConvertToStringExt(num13, array2, num20);
				}
				zipEntry.ZipFileIndex = (long)num12;
				zipEntry.Offset = offset;
				zipEntry.ExternalFileAttributes = (int)externalFileAttributes;
				this.entries_[(int)(checked((IntPtr)num12))] = zipEntry;
			}
		}

		private long LocateEntry(ZipEntry entry)
		{
			return this.TestLocalHeader(entry, ZipFile.HeaderTest.Extract);
		}

		private Stream CreateAndInitDecryptionStream(Stream baseStream, ZipEntry entry)
		{
			if (entry.Version >= 50 && (entry.Flags & 64) != 0)
			{
				throw new ZipException("Decryption method not supported");
			}
			PkzipClassicManaged pkzipClassicManaged = new PkzipClassicManaged();
			this.OnKeysRequired(entry.Name);
			if (!this.HaveKeys)
			{
				throw new ZipException("No password available for encrypted stream");
			}
			CryptoStream cryptoStream = new CryptoStream(baseStream, pkzipClassicManaged.CreateDecryptor(this.key, null), CryptoStreamMode.Read);
			ZipFile.CheckClassicPassword(cryptoStream, entry);
			return cryptoStream;
		}

		private Stream CreateAndInitEncryptionStream(Stream baseStream, ZipEntry entry)
		{
			CryptoStream cryptoStream = null;
			if (entry.Version < 50 || (entry.Flags & 64) == 0)
			{
				PkzipClassicManaged pkzipClassicManaged = new PkzipClassicManaged();
				this.OnKeysRequired(entry.Name);
				if (!this.HaveKeys)
				{
					throw new ZipException("No password available for encrypted stream");
				}
				cryptoStream = new CryptoStream(new ZipFile.UncompressedStream(baseStream), pkzipClassicManaged.CreateEncryptor(this.key, null), CryptoStreamMode.Write);
				if (entry.Crc < 0L || (entry.Flags & 8) != 0)
				{
					ZipFile.WriteEncryptionHeader(cryptoStream, entry.DosTime << 16);
				}
				else
				{
					ZipFile.WriteEncryptionHeader(cryptoStream, entry.Crc);
				}
			}
			return cryptoStream;
		}

		private static void CheckClassicPassword(CryptoStream classicCryptoStream, ZipEntry entry)
		{
			byte[] array = new byte[12];
			StreamUtils.ReadFully(classicCryptoStream, array);
			if (array[11] != entry.CryptoCheckValue)
			{
				throw new ZipException("Invalid password");
			}
		}

		private static void WriteEncryptionHeader(Stream stream, long crcValue)
		{
			byte[] array = new byte[12];
			Random random = new Random();
			random.NextBytes(array);
			array[11] = (byte)(crcValue >> 24);
			stream.Write(array, 0, array.Length);
		}

		private const int DefaultBufferSize = 4096;

		public ZipFile.KeysRequiredEventHandler KeysRequired;

		private bool isDisposed_;

		private string name_;

		private string comment_;

		private Stream baseStream_;

		private bool isStreamOwner = true;

		private long offsetOfFirstEntry;

		private ZipEntry[] entries_;

		private byte[] key;

		private bool isNewArchive_;

		private UseZip64 useZip64_;

		private ArrayList updates_;

		private IArchiveStorage archiveStorage_;

		private IDynamicDataSource updateDataSource_;

		private bool contentsEdited_;

		private int bufferSize_ = 4096;

		private byte[] copyBuffer_;

		private ZipFile.ZipString newComment_;

		private bool commentEdited_;

		private INameTransform updateNameTransform_ = new ZipNameTransform();

		private string tempDirectory_ = string.Empty;

		public delegate void KeysRequiredEventHandler(object sender, KeysRequiredEventArgs e);

		[Flags]
		private enum HeaderTest
		{
			Extract = 1,
			Header = 2
		}

		private enum UpdateCommand
		{
			Copy,
			Modify,
			Add
		}

		private class UpdateComparer : IComparer
		{
			public int Compare(object x, object y)
			{
				ZipFile.ZipUpdate zipUpdate = x as ZipFile.ZipUpdate;
				ZipFile.ZipUpdate zipUpdate2 = y as ZipFile.ZipUpdate;
				int num = (zipUpdate.Command == ZipFile.UpdateCommand.Copy || zipUpdate.Command == ZipFile.UpdateCommand.Modify) ? 0 : 1;
				int num2 = (zipUpdate2.Command == ZipFile.UpdateCommand.Copy || zipUpdate2.Command == ZipFile.UpdateCommand.Modify) ? 0 : 1;
				int num3 = num - num2;
				if (num3 == 0)
				{
					long num4 = zipUpdate.Entry.Offset - zipUpdate2.Entry.Offset;
					if (num4 < 0L)
					{
						num3 = -1;
					}
					else if (num4 == 0L)
					{
						num3 = 0;
					}
					else
					{
						num3 = 1;
					}
				}
				return num3;
			}
		}

		private class ZipUpdate
		{
			public ZipUpdate(string fileName, string entryName, CompressionMethod compressionMethod)
			{
				this.command_ = ZipFile.UpdateCommand.Add;
				this.entry_ = new ZipEntry(entryName);
				this.entry_.CompressionMethod = compressionMethod;
				this.filename_ = fileName;
			}

			public ZipUpdate(string fileName, string entryName)
			{
				this.command_ = ZipFile.UpdateCommand.Add;
				this.entry_ = new ZipEntry(entryName);
				this.filename_ = fileName;
			}

			public ZipUpdate(IStaticDataSource dataSource, string entryName, CompressionMethod compressionMethod)
			{
				this.command_ = ZipFile.UpdateCommand.Add;
				this.entry_ = new ZipEntry(entryName);
				this.entry_.CompressionMethod = compressionMethod;
				this.dataSource_ = dataSource;
			}

			public ZipUpdate(ZipEntry original, ZipEntry updated)
			{
				throw new ZipException("Modify not currently supported");
			}

			public ZipUpdate(ZipFile.UpdateCommand command, ZipEntry entry)
			{
				this.command_ = command;
				this.entry_ = (ZipEntry)entry.Clone();
			}

			public ZipUpdate(ZipEntry entry) : this(ZipFile.UpdateCommand.Copy, entry)
			{
			}

			public ZipEntry Entry
			{
				get
				{
					return this.entry_;
				}
			}

			public ZipEntry OutEntry
			{
				get
				{
					if (this.outEntry_ == null)
					{
						this.outEntry_ = (ZipEntry)this.entry_.Clone();
					}
					return this.outEntry_;
				}
			}

			public ZipFile.UpdateCommand Command
			{
				get
				{
					return this.command_;
				}
			}

			public string Filename
			{
				get
				{
					return this.filename_;
				}
			}

			public long SizePatchOffset
			{
				get
				{
					return this.sizePatchOffset_;
				}
				set
				{
					this.sizePatchOffset_ = value;
				}
			}

			public long CrcPatchOffset
			{
				get
				{
					return this.crcPatchOffset_;
				}
				set
				{
					this.crcPatchOffset_ = value;
				}
			}

			public Stream GetSource()
			{
				Stream result = null;
				if (this.dataSource_ != null)
				{
					result = this.dataSource_.GetSource();
				}
				return result;
			}

			private ZipEntry entry_;

			private ZipEntry outEntry_;

			private ZipFile.UpdateCommand command_;

			private IStaticDataSource dataSource_;

			private string filename_;

			private long sizePatchOffset_ = -1L;

			private long crcPatchOffset_ = -1L;
		}

		private class ZipString
		{
			public ZipString(string comment)
			{
				this.comment_ = comment;
				this.sourceIsString_ = true;
			}

			public ZipString(byte[] rawString)
			{
				this.rawComment_ = rawString;
			}

			public int RawLength
			{
				get
				{
					this.MakeBytesAvailable();
					return this.rawComment_.Length;
				}
			}

			public byte[] RawComment
			{
				get
				{
					this.MakeBytesAvailable();
					return (byte[])this.rawComment_.Clone();
				}
			}

			public void Reset()
			{
				if (this.sourceIsString_)
				{
					this.rawComment_ = null;
					return;
				}
				this.comment_ = null;
			}

			private void MakeTextAvailable()
			{
				if (this.comment_ == null)
				{
					this.comment_ = ZipConstants.ConvertToString(this.rawComment_);
				}
			}

			private void MakeBytesAvailable()
			{
				if (this.rawComment_ == null)
				{
					this.rawComment_ = ZipConstants.ConvertToArray(this.comment_);
				}
			}

			public static implicit operator string(ZipFile.ZipString comment)
			{
				comment.MakeTextAvailable();
				return comment.comment_;
			}

			private string comment_;

			private byte[] rawComment_;

			private bool sourceIsString_;
		}

		private class ZipEntryEnumerator : IEnumerator
		{
			public ZipEntryEnumerator(ZipEntry[] entries)
			{
				this.array = entries;
			}

			public object Current
			{
				get
				{
					return this.array[this.index];
				}
			}

			public void Reset()
			{
				this.index = -1;
			}

			public bool MoveNext()
			{
				return ++this.index < this.array.Length;
			}

			private ZipEntry[] array;

			private int index = -1;
		}

		private class UncompressedStream : Stream
		{
			public UncompressedStream(Stream baseStream)
			{
				this.baseStream_ = baseStream;
			}

			public override void Close()
			{
			}

			public override bool CanRead
			{
				get
				{
					return false;
				}
			}

			public override void Flush()
			{
				this.baseStream_.Flush();
			}

			public override bool CanWrite
			{
				get
				{
					return this.baseStream_.CanWrite;
				}
			}

			public override bool CanSeek
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
					return 0L;
				}
			}

			public override long Position
			{
				get
				{
					return this.baseStream_.Position;
				}
				set
				{
				}
			}

			public override int Read(byte[] buffer, int offset, int count)
			{
				return 0;
			}

			public override long Seek(long offset, SeekOrigin origin)
			{
				return 0L;
			}

			public override void SetLength(long value)
			{
			}

			public override void Write(byte[] buffer, int offset, int count)
			{
				this.baseStream_.Write(buffer, offset, count);
			}

			private Stream baseStream_;
		}

		private class PartialInputStream : InflaterInputStream
		{
			public PartialInputStream(Stream baseStream, long start, long length) : base(baseStream)
			{
				this.baseStream_ = baseStream;
				this.filepos_ = start;
				this.end_ = start + length;
			}

			public long SkipBytes(long count)
			{
				if (count < 0L)
				{
					throw new ArgumentOutOfRangeException("count", "is less than zero");
				}
				if (count > this.end_ - this.filepos_)
				{
					count = this.end_ - this.filepos_;
				}
				this.filepos_ += count;
				return count;
			}

			public override int Available
			{
				get
				{
					long num = this.end_ - this.filepos_;
					if (num > 2147483647L)
					{
						return int.MaxValue;
					}
					return (int)num;
				}
			}

			public override int ReadByte()
			{
				if (this.filepos_ == this.end_)
				{
					return -1;
				}
				int result;
				lock (this.baseStream_)
				{
					Stream stream = this.baseStream_;
					long offset;
					this.filepos_ = (offset = this.filepos_) + 1L;
					stream.Seek(offset, SeekOrigin.Begin);
					result = this.baseStream_.ReadByte();
				}
				return result;
			}

			public override void Close()
			{
			}

			public override int Read(byte[] buffer, int offset, int count)
			{
				if ((long)count > this.end_ - this.filepos_)
				{
					count = (int)(this.end_ - this.filepos_);
					if (count == 0)
					{
						return 0;
					}
				}
				int result;
				lock (this.baseStream_)
				{
					this.baseStream_.Seek(this.filepos_, SeekOrigin.Begin);
					int num = this.baseStream_.Read(buffer, offset, count);
					if (num > 0)
					{
						this.filepos_ += (long)num;
					}
					result = num;
				}
				return result;
			}

			private Stream baseStream_;

			private long filepos_;

			private long end_;
		}
	}
}
