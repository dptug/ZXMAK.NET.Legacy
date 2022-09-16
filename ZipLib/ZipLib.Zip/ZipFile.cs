using System;
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

namespace ZipLib.Zip;

public class ZipFile : IEnumerable, IDisposable
{
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
			ZipUpdate zipUpdate = x as ZipUpdate;
			ZipUpdate zipUpdate2 = y as ZipUpdate;
			int num = ((zipUpdate.Command != 0 && zipUpdate.Command != UpdateCommand.Modify) ? 1 : 0);
			int num2 = ((zipUpdate2.Command != 0 && zipUpdate2.Command != UpdateCommand.Modify) ? 1 : 0);
			int num3 = num - num2;
			if (num3 == 0)
			{
				long num4 = zipUpdate.Entry.Offset - zipUpdate2.Entry.Offset;
				num3 = ((num4 < 0) ? (-1) : ((num4 != 0) ? 1 : 0));
			}
			return num3;
		}
	}

	private class ZipUpdate
	{
		private ZipEntry entry_;

		private ZipEntry outEntry_;

		private UpdateCommand command_;

		private IStaticDataSource dataSource_;

		private string filename_;

		private long sizePatchOffset_ = -1L;

		private long crcPatchOffset_ = -1L;

		public ZipEntry Entry => entry_;

		public ZipEntry OutEntry
		{
			get
			{
				if (outEntry_ == null)
				{
					outEntry_ = (ZipEntry)entry_.Clone();
				}
				return outEntry_;
			}
		}

		public UpdateCommand Command => command_;

		public string Filename => filename_;

		public long SizePatchOffset
		{
			get
			{
				return sizePatchOffset_;
			}
			set
			{
				sizePatchOffset_ = value;
			}
		}

		public long CrcPatchOffset
		{
			get
			{
				return crcPatchOffset_;
			}
			set
			{
				crcPatchOffset_ = value;
			}
		}

		public ZipUpdate(string fileName, string entryName, CompressionMethod compressionMethod)
		{
			command_ = UpdateCommand.Add;
			entry_ = new ZipEntry(entryName);
			entry_.CompressionMethod = compressionMethod;
			filename_ = fileName;
		}

		public ZipUpdate(string fileName, string entryName)
		{
			command_ = UpdateCommand.Add;
			entry_ = new ZipEntry(entryName);
			filename_ = fileName;
		}

		public ZipUpdate(IStaticDataSource dataSource, string entryName, CompressionMethod compressionMethod)
		{
			command_ = UpdateCommand.Add;
			entry_ = new ZipEntry(entryName);
			entry_.CompressionMethod = compressionMethod;
			dataSource_ = dataSource;
		}

		public ZipUpdate(ZipEntry original, ZipEntry updated)
		{
			throw new ZipException("Modify not currently supported");
		}

		public ZipUpdate(UpdateCommand command, ZipEntry entry)
		{
			command_ = command;
			entry_ = (ZipEntry)entry.Clone();
		}

		public ZipUpdate(ZipEntry entry)
			: this(UpdateCommand.Copy, entry)
		{
		}

		public Stream GetSource()
		{
			Stream result = null;
			if (dataSource_ != null)
			{
				result = dataSource_.GetSource();
			}
			return result;
		}
	}

	private class ZipString
	{
		private string comment_;

		private byte[] rawComment_;

		private bool sourceIsString_;

		public int RawLength
		{
			get
			{
				MakeBytesAvailable();
				return rawComment_.Length;
			}
		}

		public byte[] RawComment
		{
			get
			{
				MakeBytesAvailable();
				return (byte[])rawComment_.Clone();
			}
		}

		public ZipString(string comment)
		{
			comment_ = comment;
			sourceIsString_ = true;
		}

		public ZipString(byte[] rawString)
		{
			rawComment_ = rawString;
		}

		public void Reset()
		{
			if (sourceIsString_)
			{
				rawComment_ = null;
			}
			else
			{
				comment_ = null;
			}
		}

		private void MakeTextAvailable()
		{
			if (comment_ == null)
			{
				comment_ = ZipConstants.ConvertToString(rawComment_);
			}
		}

		private void MakeBytesAvailable()
		{
			if (rawComment_ == null)
			{
				rawComment_ = ZipConstants.ConvertToArray(comment_);
			}
		}

		public static implicit operator string(ZipString comment)
		{
			comment.MakeTextAvailable();
			return comment.comment_;
		}
	}

	private class ZipEntryEnumerator : IEnumerator
	{
		private ZipEntry[] array;

		private int index = -1;

		public object Current => array[index];

		public ZipEntryEnumerator(ZipEntry[] entries)
		{
			array = entries;
		}

		public void Reset()
		{
			index = -1;
		}

		public bool MoveNext()
		{
			return ++index < array.Length;
		}
	}

	private class UncompressedStream : Stream
	{
		private Stream baseStream_;

		public override bool CanRead => false;

		public override bool CanWrite => baseStream_.CanWrite;

		public override bool CanSeek => false;

		public override long Length => 0L;

		public override long Position
		{
			get
			{
				return baseStream_.Position;
			}
			set
			{
			}
		}

		public UncompressedStream(Stream baseStream)
		{
			baseStream_ = baseStream;
		}

		public override void Close()
		{
		}

		public override void Flush()
		{
			baseStream_.Flush();
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
			baseStream_.Write(buffer, offset, count);
		}
	}

	private class PartialInputStream : InflaterInputStream
	{
		private Stream baseStream_;

		private long filepos_;

		private long end_;

		public override int Available
		{
			get
			{
				long num = end_ - filepos_;
				if (num > int.MaxValue)
				{
					return int.MaxValue;
				}
				return (int)num;
			}
		}

		public PartialInputStream(Stream baseStream, long start, long length)
			: base(baseStream)
		{
			baseStream_ = baseStream;
			filepos_ = start;
			end_ = start + length;
		}

		public long SkipBytes(long count)
		{
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException("count", "is less than zero");
			}
			if (count > end_ - filepos_)
			{
				count = end_ - filepos_;
			}
			filepos_ += count;
			return count;
		}

		public override int ReadByte()
		{
			if (filepos_ == end_)
			{
				return -1;
			}
			lock (baseStream_)
			{
				baseStream_.Seek(filepos_++, SeekOrigin.Begin);
				return baseStream_.ReadByte();
			}
		}

		public override void Close()
		{
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			if (count > end_ - filepos_)
			{
				count = (int)(end_ - filepos_);
				if (count == 0)
				{
					return 0;
				}
			}
			lock (baseStream_)
			{
				baseStream_.Seek(filepos_, SeekOrigin.Begin);
				int num = baseStream_.Read(buffer, offset, count);
				if (num > 0)
				{
					filepos_ += num;
				}
				return num;
			}
		}
	}

	private const int DefaultBufferSize = 4096;

	public KeysRequiredEventHandler KeysRequired;

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

	private ZipString newComment_;

	private bool commentEdited_;

	private INameTransform updateNameTransform_ = new ZipNameTransform();

	private string tempDirectory_ = string.Empty;

	private byte[] Key
	{
		get
		{
			return key;
		}
		set
		{
			key = value;
		}
	}

	public string Password
	{
		set
		{
			if (value == null || value.Length == 0)
			{
				key = null;
			}
			else
			{
				key = PkzipClassic.GenerateKeys(Encoding.ASCII.GetBytes(value));
			}
		}
	}

	private bool HaveKeys => key != null;

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

	public bool IsEmbeddedArchive => offsetOfFirstEntry > 0;

	public bool IsNewArchive => isNewArchive_;

	public string ZipFileComment => comment_;

	public string Name => name_;

	[Obsolete("Use the Count property instead")]
	public int Size
	{
		get
		{
			if (entries_ != null)
			{
				return entries_.Length;
			}
			throw new InvalidOperationException("ZipFile is closed");
		}
	}

	public long Count
	{
		get
		{
			if (entries_ != null)
			{
				return entries_.Length;
			}
			throw new InvalidOperationException("ZipFile is closed");
		}
	}

	[IndexerName("EntryByIndex")]
	public ZipEntry this[int index] => (ZipEntry)entries_[index].Clone();

	public INameTransform NameTransform
	{
		get
		{
			return updateNameTransform_;
		}
		set
		{
			updateNameTransform_ = value;
		}
	}

	public int BufferSize
	{
		get
		{
			return bufferSize_;
		}
		set
		{
			if (value < 1024)
			{
				throw new ArgumentOutOfRangeException("value", "cannot be below 1024");
			}
			if (bufferSize_ != value)
			{
				bufferSize_ = value;
				copyBuffer_ = null;
			}
		}
	}

	public bool IsUpdating => updates_ != null;

	public UseZip64 UseZip64
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

	private void OnKeysRequired(string fileName)
	{
		if (KeysRequired != null)
		{
			KeysRequiredEventArgs keysRequiredEventArgs = new KeysRequiredEventArgs(fileName, key);
			KeysRequired(this, keysRequiredEventArgs);
			key = keysRequiredEventArgs.Key;
		}
	}

	public ZipFile(string name)
	{
		name_ = name;
		isStreamOwner = false;
		baseStream_ = File.OpenRead(name);
		isStreamOwner = true;
		try
		{
			ReadEntries();
		}
		catch
		{
			DisposeInternal(disposing: true);
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
		baseStream_ = file;
		name_ = file.Name;
		try
		{
			ReadEntries();
		}
		catch
		{
			DisposeInternal(disposing: true);
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
		baseStream_ = stream;
		if (baseStream_.Length > 0)
		{
			try
			{
				ReadEntries();
				return;
			}
			catch
			{
				DisposeInternal(disposing: true);
				throw;
			}
		}
		entries_ = new ZipEntry[0];
		isNewArchive_ = true;
	}

	internal ZipFile()
	{
		entries_ = new ZipEntry[0];
		isNewArchive_ = true;
	}

	~ZipFile()
	{
		Dispose(disposing: false);
	}

	public void Close()
	{
		DisposeInternal(disposing: true);
		GC.SuppressFinalize(this);
	}

	public static ZipFile Create(string fileName)
	{
		if (fileName == null)
		{
			throw new ArgumentNullException("fileName");
		}
		ZipFile zipFile = new ZipFile();
		zipFile.name_ = fileName;
		zipFile.baseStream_ = File.Create(fileName);
		return zipFile;
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
		ZipFile zipFile = new ZipFile();
		zipFile.baseStream_ = outStream;
		return zipFile;
	}

	public IEnumerator GetEnumerator()
	{
		if (entries_ == null)
		{
			throw new InvalidOperationException("ZipFile has closed");
		}
		return new ZipEntryEnumerator(entries_);
	}

	public int FindEntry(string name, bool ignoreCase)
	{
		if (entries_ == null)
		{
			throw new InvalidOperationException("ZipFile has been closed");
		}
		for (int i = 0; i < entries_.Length; i++)
		{
			if (string.Compare(name, entries_[i].Name, ignoreCase, CultureInfo.InvariantCulture) == 0)
			{
				return i;
			}
		}
		return -1;
	}

	public ZipEntry GetEntry(string name)
	{
		if (entries_ == null)
		{
			throw new InvalidOperationException("ZipFile has been closed");
		}
		int num = FindEntry(name, ignoreCase: true);
		if (num < 0)
		{
			return null;
		}
		return (ZipEntry)entries_[num].Clone();
	}

	public Stream GetInputStream(ZipEntry entry)
	{
		if (entry == null)
		{
			throw new ArgumentNullException("entry");
		}
		if (entries_ == null)
		{
			throw new InvalidOperationException("ZipFile has closed");
		}
		long num = entry.ZipFileIndex;
		if (num < 0 || num >= entries_.Length || entries_[num].Name != entry.Name)
		{
			num = FindEntry(entry.Name, ignoreCase: true);
			if (num < 0)
			{
				throw new ZipException("Entry cannot be found");
			}
		}
		return GetInputStream(num);
	}

	public Stream GetInputStream(long entryIndex)
	{
		if (entries_ == null)
		{
			throw new InvalidOperationException("ZipFile is not open");
		}
		long start = LocateEntry(entries_[entryIndex]);
		CompressionMethod compressionMethod = entries_[entryIndex].CompressionMethod;
		Stream stream = new PartialInputStream(baseStream_, start, entries_[entryIndex].CompressedSize);
		if (entries_[entryIndex].IsCrypted)
		{
			stream = CreateAndInitDecryptionStream(stream, entries_[entryIndex]);
			if (stream == null)
			{
				throw new ZipException("Unable to decrypt this entry");
			}
		}
		switch (compressionMethod)
		{
		case CompressionMethod.Deflated:
			stream = new InflaterInputStream(stream, new Inflater(noHeader: true));
			break;
		default:
			throw new ZipException("Unsupported compression method " + compressionMethod);
		case CompressionMethod.Stored:
			break;
		}
		return stream;
	}

	public bool TestArchive(bool testData)
	{
		return TestArchive(testData, TestStrategy.FindFirstError, null);
	}

	public bool TestArchive(bool testData, TestStrategy strategy, ZipTestResultHandler resultHandler)
	{
		TestStatus testStatus = new TestStatus(this);
		resultHandler?.Invoke(testStatus, null);
		HeaderTest tests = (testData ? (HeaderTest.Extract | HeaderTest.Header) : HeaderTest.Header);
		bool flag = true;
		try
		{
			int num = 0;
			while (flag && num < Count)
			{
				if (resultHandler != null)
				{
					testStatus.SetEntry(this[num]);
					testStatus.SetOperation(TestOperation.EntryHeader);
					resultHandler(testStatus, null);
				}
				try
				{
					TestLocalHeader(this[num], tests);
				}
				catch (ZipException ex)
				{
					testStatus.AddError();
					resultHandler?.Invoke(testStatus, $"Exception during test - '{ex.Message}'");
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
					Stream inputStream = GetInputStream(this[num]);
					Crc32 crc = new Crc32();
					byte[] array = new byte[4096];
					long num2 = 0L;
					int num3;
					while ((num3 = inputStream.Read(array, 0, array.Length)) > 0)
					{
						crc.Update(array, 0, num3);
						if (resultHandler != null)
						{
							num2 += num3;
							testStatus.SetBytesTested(num2);
							resultHandler(testStatus, null);
						}
					}
					if (this[num].Crc != crc.Value)
					{
						testStatus.AddError();
						resultHandler?.Invoke(testStatus, "CRC mismatch");
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
			resultHandler?.Invoke(testStatus, $"Exception during test - '{ex2.Message}'");
		}
		if (resultHandler != null)
		{
			testStatus.SetOperation(TestOperation.Complete);
			testStatus.SetEntry(null);
			resultHandler(testStatus, null);
		}
		return testStatus.ErrorCount == 0;
	}

	private long TestLocalHeader(ZipEntry entry, HeaderTest tests)
	{
		lock (baseStream_)
		{
			bool flag = (tests & HeaderTest.Header) != 0;
			bool flag2 = (tests & HeaderTest.Extract) != 0;
			baseStream_.Seek(offsetOfFirstEntry + entry.Offset, SeekOrigin.Begin);
			if (ReadLEUint() != 67324752)
			{
				throw new ZipException($"Wrong local header signature @{offsetOfFirstEntry + entry.Offset:X}");
			}
			short num = (short)ReadLEUshort();
			short num2 = (short)ReadLEUshort();
			short num3 = (short)ReadLEUshort();
			short num4 = (short)ReadLEUshort();
			short num5 = (short)ReadLEUshort();
			uint num6 = ReadLEUint();
			long num7 = ReadLEUint();
			long num8 = ReadLEUint();
			int num9 = ReadLEUshort();
			int num10 = ReadLEUshort();
			if (flag2 && entry.IsFile)
			{
				if (!entry.IsCompressionMethodSupported())
				{
					throw new ZipException("Compression method not supported");
				}
				if (num > 45 || (num > 20 && num < 45))
				{
					throw new ZipException($"Version required to extract this entry not supported ({num})");
				}
				if (((uint)num2 & 0x3060u) != 0)
				{
					throw new ZipException("The library does not support the zip version required to extract this entry");
				}
			}
			if (flag)
			{
				if (num <= 63 && num != 10 && num != 11 && num != 20 && num != 21 && num != 25 && num != 27 && num != 45 && num != 46 && num != 50 && num != 51 && num != 52 && num != 61 && num != 62 && num != 63)
				{
					throw new ZipException($"Version required to extract this entry is invalid ({num})");
				}
				if (((uint)num2 & 0xC010u) != 0)
				{
					throw new ZipException("Reserved bit flags cannot be set.");
				}
				if (((uint)num2 & (true ? 1u : 0u)) != 0 && num < 20)
				{
					throw new ZipException($"Version required to extract this entry is too low for encryption ({num})");
				}
				if (((uint)num2 & 0x40u) != 0)
				{
					if ((num2 & 1) == 0)
					{
						throw new ZipException("Strong encryption flag set but encryption flag is not set");
					}
					if (num < 50)
					{
						throw new ZipException($"Version required to extract this entry is too low for encryption ({num})");
					}
				}
				if (((uint)num2 & 0x20u) != 0 && num < 27)
				{
					throw new ZipException($"Patched data requires higher version than ({num})");
				}
				if (num2 != entry.Flags)
				{
					throw new ZipException("Central header/local header flags mismatch");
				}
				if (entry.CompressionMethod != (CompressionMethod)num3)
				{
					throw new ZipException("Central header/local header compression method mismatch");
				}
				if (((uint)num2 & 0x40u) != 0 && num < 62)
				{
					throw new ZipException("Strong encryption flag set but version not high enough");
				}
				if (((uint)num2 & 0x2000u) != 0 && (num4 != 0 || num5 != 0))
				{
					throw new ZipException("Header masked set but date/time values non-zero");
				}
				if ((num2 & 8) == 0 && num6 != (uint)entry.Crc)
				{
					throw new ZipException("Central header/local header crc mismatch");
				}
				if (num7 == 0 && num8 == 0 && num6 != 0)
				{
					throw new ZipException("Invalid CRC for empty entry");
				}
				if (entry.Name.Length > num9)
				{
					throw new ZipException("File name length mismatch");
				}
				byte[] array = new byte[num9];
				StreamUtils.ReadFully(baseStream_, array);
				string text = ZipConstants.ConvertToStringExt(num2, array);
				if (text != entry.Name)
				{
					throw new ZipException("Central header and local header file name mismatch");
				}
				if (entry.IsDirectory && (num8 != 0 || num7 != 0))
				{
					throw new ZipException("Directory cannot have size");
				}
				if (!ZipNameTransform.IsValidName(text, relaxed: true))
				{
					throw new ZipException("Name is invalid");
				}
				byte[] array2 = new byte[num10];
				StreamUtils.ReadFully(baseStream_, array2);
				ZipExtraData zipExtraData = new ZipExtraData(array2);
				if (zipExtraData.Find(1))
				{
					if (num < 45)
					{
						throw new ZipException($"Extra data contains Zip64 information but version {num / 10}.{num % 10} is not high enough");
					}
					if ((int)num7 != -1 && (int)num8 != -1)
					{
						throw new ZipException("Entry sizes not correct for Zip64");
					}
					num7 = zipExtraData.ReadLong();
					num8 = zipExtraData.ReadLong();
				}
				else if (num >= 45 && ((int)num7 == -1 || (int)num8 == -1))
				{
					throw new ZipException("Required Zip64 extended information missing");
				}
			}
			int num11 = num9 + num10;
			return offsetOfFirstEntry + entry.Offset + 30 + num11;
		}
	}

	public void BeginUpdate(IArchiveStorage archiveStorage, IDynamicDataSource dataSource)
	{
		if (IsEmbeddedArchive)
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
		archiveStorage_ = archiveStorage;
		updateDataSource_ = dataSource;
		if (entries_ != null)
		{
			updates_ = new ArrayList(entries_.Length);
			ZipEntry[] array = entries_;
			foreach (ZipEntry entry in array)
			{
				updates_.Add(new ZipUpdate(entry));
			}
		}
		else
		{
			updates_ = new ArrayList();
		}
		contentsEdited_ = false;
		commentEdited_ = false;
		newComment_ = null;
	}

	public void BeginUpdate(IArchiveStorage archiveStorage)
	{
		BeginUpdate(archiveStorage, new DynamicDiskDataSource());
	}

	public void BeginUpdate()
	{
		if (Name == null)
		{
			BeginUpdate(new MemoryArchiveStorage(), new DynamicDiskDataSource());
		}
		else
		{
			BeginUpdate(new DiskArchiveStorage(this), new DynamicDiskDataSource());
		}
	}

	public void CommitUpdate()
	{
		CheckUpdating();
		if (contentsEdited_)
		{
			RunUpdates();
		}
		else if (commentEdited_)
		{
			UpdateCommentOnly();
		}
		else if (entries_ != null && entries_.Length == 0)
		{
			byte[] comment = ((newComment_ != null) ? newComment_.RawComment : ZipConstants.ConvertToArray(comment_));
			using ZipHelperStream zipHelperStream = new ZipHelperStream(baseStream_);
			zipHelperStream.WriteEndOfCentralDirectory(0L, 0L, 0L, comment);
		}
		PostUpdateCleanup();
	}

	public void AbortUpdate()
	{
		updates_ = null;
		PostUpdateCleanup();
	}

	public void SetComment(string comment)
	{
		CheckUpdating();
		newComment_ = new ZipString(comment);
		if (newComment_.RawLength > 65535)
		{
			newComment_ = null;
			throw new ZipException("Comment length exceeds maximum - 65535");
		}
		commentEdited_ = true;
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
		CheckUpdating();
		contentsEdited_ = true;
		string transformedFileName = GetTransformedFileName(fileName);
		int num = FindExistingUpdate(transformedFileName);
		if (num >= 0)
		{
			updates_.RemoveAt(num);
		}
		ZipUpdate zipUpdate = new ZipUpdate(fileName, transformedFileName, compressionMethod);
		zipUpdate.Entry.IsUnicodeText = useUnicodeText;
		updates_.Add(zipUpdate);
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
		CheckUpdating();
		contentsEdited_ = true;
		string transformedFileName = GetTransformedFileName(fileName);
		int num = FindExistingUpdate(transformedFileName);
		if (num >= 0)
		{
			updates_.RemoveAt(num);
		}
		updates_.Add(new ZipUpdate(fileName, transformedFileName, compressionMethod));
	}

	public void Add(string fileName)
	{
		if (fileName == null)
		{
			throw new ArgumentNullException("fileName");
		}
		CheckUpdating();
		Add(fileName, CompressionMethod.Deflated);
	}

	public void Add(IStaticDataSource dataSource, string entryName)
	{
		if (dataSource == null)
		{
			throw new ArgumentNullException("dataSource");
		}
		CheckUpdating();
		contentsEdited_ = true;
		updates_.Add(new ZipUpdate(dataSource, GetTransformedFileName(entryName), CompressionMethod.Deflated));
	}

	public void Add(IStaticDataSource dataSource, string entryName, CompressionMethod compressionMethod)
	{
		if (dataSource == null)
		{
			throw new ArgumentNullException("dataSource");
		}
		CheckUpdating();
		contentsEdited_ = true;
		updates_.Add(new ZipUpdate(dataSource, GetTransformedFileName(entryName), compressionMethod));
	}

	public void Add(IStaticDataSource dataSource, string entryName, CompressionMethod compressionMethod, bool useUnicodeText)
	{
		if (dataSource == null)
		{
			throw new ArgumentNullException("dataSource");
		}
		CheckUpdating();
		contentsEdited_ = true;
		ZipUpdate zipUpdate = new ZipUpdate(dataSource, GetTransformedFileName(entryName), compressionMethod);
		zipUpdate.Entry.IsUnicodeText = useUnicodeText;
		updates_.Add(zipUpdate);
	}

	public void Add(ZipEntry entry)
	{
		if (entry == null)
		{
			throw new ArgumentNullException("entry");
		}
		CheckUpdating();
		if (entry.Size != 0 || entry.CompressedSize != 0)
		{
			throw new ZipException("Entry cannot have any data");
		}
		contentsEdited_ = true;
		updates_.Add(new ZipUpdate(UpdateCommand.Add, entry));
	}

	public void AddDirectory(string directoryName)
	{
		if (directoryName == null)
		{
			throw new ArgumentNullException("directoryName");
		}
		CheckUpdating();
		ZipEntry zipEntry = new ZipEntry(GetTransformedDirectoryName(directoryName));
		zipEntry.ExternalFileAttributes = 16;
		updates_.Add(new ZipUpdate(UpdateCommand.Add, zipEntry));
	}

	public bool Delete(string fileName)
	{
		CheckUpdating();
		bool flag = false;
		int num = FindExistingUpdate(fileName);
		if (num >= 0)
		{
			flag = true;
			contentsEdited_ = true;
			updates_.RemoveAt(num);
			return flag;
		}
		throw new ZipException("Cannot find entry to delete");
	}

	public void Delete(ZipEntry entry)
	{
		CheckUpdating();
		int num = FindExistingUpdate(entry);
		if (num >= 0)
		{
			contentsEdited_ = true;
			updates_.RemoveAt(num);
			return;
		}
		throw new ZipException("Cannot find entry to delete");
	}

	private void WriteLEShort(int value)
	{
		baseStream_.WriteByte((byte)((uint)value & 0xFFu));
		baseStream_.WriteByte((byte)((uint)(value >> 8) & 0xFFu));
	}

	private void WriteLEUshort(ushort value)
	{
		baseStream_.WriteByte((byte)(value & 0xFFu));
		baseStream_.WriteByte((byte)(value >> 8));
	}

	private void WriteLEInt(int value)
	{
		WriteLEShort(value);
		WriteLEShort(value >> 16);
	}

	private void WriteLEUint(uint value)
	{
		WriteLEUshort((ushort)(value & 0xFFFFu));
		WriteLEUshort((ushort)(value >> 16));
	}

	private void WriteLeLong(long value)
	{
		WriteLEInt((int)(value & 0xFFFFFFFFu));
		WriteLEInt((int)(value >> 32));
	}

	private void WriteLEUlong(ulong value)
	{
		WriteLEUint((uint)(value & 0xFFFFFFFFu));
		WriteLEUint((uint)(value >> 32));
	}

	private void WriteLocalEntryHeader(ZipUpdate update)
	{
		ZipEntry outEntry = update.OutEntry;
		outEntry.Offset = baseStream_.Position;
		if (update.Command != 0)
		{
			if (outEntry.CompressionMethod == CompressionMethod.Deflated)
			{
				if (outEntry.Size == 0)
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
			if (HaveKeys)
			{
				outEntry.IsCrypted = true;
				if (outEntry.Crc < 0)
				{
					outEntry.Flags |= 8;
				}
			}
			else
			{
				outEntry.IsCrypted = false;
			}
			switch (useZip64_)
			{
			case UseZip64.Dynamic:
				if (outEntry.Size < 0)
				{
					outEntry.ForceZip64();
				}
				break;
			case UseZip64.On:
				outEntry.ForceZip64();
				break;
			}
		}
		WriteLEInt(67324752);
		WriteLEShort(outEntry.Version);
		WriteLEShort(outEntry.Flags);
		WriteLEShort((byte)outEntry.CompressionMethod);
		WriteLEInt((int)outEntry.DosTime);
		if (!outEntry.HasCrc)
		{
			update.CrcPatchOffset = baseStream_.Position;
			WriteLEInt(0);
		}
		else
		{
			WriteLEInt((int)outEntry.Crc);
		}
		if (outEntry.LocalHeaderRequiresZip64)
		{
			WriteLEInt(-1);
			WriteLEInt(-1);
		}
		else
		{
			if (outEntry.CompressedSize < 0 || outEntry.Size < 0)
			{
				update.SizePatchOffset = baseStream_.Position;
			}
			WriteLEInt((int)outEntry.CompressedSize);
			WriteLEInt((int)outEntry.Size);
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
		WriteLEShort(array.Length);
		WriteLEShort(outEntry.ExtraData.Length);
		if (array.Length > 0)
		{
			baseStream_.Write(array, 0, array.Length);
		}
		if (outEntry.LocalHeaderRequiresZip64)
		{
			if (!zipExtraData.Find(1))
			{
				throw new ZipException("Internal error cannot find extra data");
			}
			update.SizePatchOffset = baseStream_.Position + zipExtraData.CurrentReadIndex;
		}
		if (outEntry.ExtraData.Length > 0)
		{
			baseStream_.Write(outEntry.ExtraData, 0, outEntry.ExtraData.Length);
		}
	}

	private int WriteCentralDirectoryHeader(ZipEntry entry)
	{
		WriteLEInt(33639248);
		WriteLEShort(45);
		WriteLEShort(entry.Version);
		WriteLEShort(entry.Flags);
		WriteLEShort((byte)entry.CompressionMethod);
		WriteLEInt((int)entry.DosTime);
		WriteLEInt((int)entry.Crc);
		if (entry.CompressedSize >= uint.MaxValue)
		{
			WriteLEInt(-1);
		}
		else
		{
			WriteLEInt((int)(entry.CompressedSize & 0xFFFFFFFFu));
		}
		if (entry.Size >= uint.MaxValue)
		{
			WriteLEInt(-1);
		}
		else
		{
			WriteLEInt((int)entry.Size);
		}
		byte[] array = ZipConstants.ConvertToArray(entry.Flags, entry.Name);
		if (array.Length > 65535)
		{
			throw new ZipException("Entry name is too long.");
		}
		WriteLEShort(array.Length);
		ZipExtraData zipExtraData = new ZipExtraData(entry.ExtraData);
		if (entry.CentralHeaderRequiresZip64)
		{
			zipExtraData.StartNewEntry();
			if (entry.Size >= uint.MaxValue || useZip64_ == UseZip64.On)
			{
				zipExtraData.AddLeLong(entry.Size);
			}
			if (entry.CompressedSize >= uint.MaxValue || useZip64_ == UseZip64.On)
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
		WriteLEShort(entryData.Length);
		WriteLEShort((entry.Comment != null) ? entry.Comment.Length : 0);
		WriteLEShort(0);
		WriteLEShort(0);
		if (entry.ExternalFileAttributes != -1)
		{
			WriteLEInt(entry.ExternalFileAttributes);
		}
		else if (entry.IsDirectory)
		{
			WriteLEUint(16u);
		}
		else
		{
			WriteLEUint(0u);
		}
		if (entry.Offset >= uint.MaxValue)
		{
			WriteLEUint(uint.MaxValue);
		}
		else
		{
			WriteLEUint((uint)entry.Offset);
		}
		if (array.Length > 0)
		{
			baseStream_.Write(array, 0, array.Length);
		}
		if (entryData.Length > 0)
		{
			baseStream_.Write(entryData, 0, entryData.Length);
		}
		byte[] array2 = ((entry.Comment != null) ? Encoding.ASCII.GetBytes(entry.Comment) : new byte[0]);
		if (array2.Length > 0)
		{
			baseStream_.Write(array2, 0, array2.Length);
		}
		return 46 + array.Length + entryData.Length + array2.Length;
	}

	private void PostUpdateCleanup()
	{
		if (archiveStorage_ != null)
		{
			archiveStorage_.Dispose();
			archiveStorage_ = null;
		}
		updateDataSource_ = null;
	}

	private string GetTransformedFileName(string name)
	{
		if (updateNameTransform_ == null)
		{
			return name;
		}
		return updateNameTransform_.TransformFile(name);
	}

	private string GetTransformedDirectoryName(string name)
	{
		if (updateNameTransform_ == null)
		{
			return name;
		}
		return updateNameTransform_.TransformDirectory(name);
	}

	private byte[] GetBuffer()
	{
		if (copyBuffer_ == null)
		{
			copyBuffer_ = new byte[bufferSize_];
		}
		return copyBuffer_;
	}

	private void CopyDescriptorBytes(ZipUpdate update, Stream dest, Stream source)
	{
		int num = GetDescriptorSize(update);
		if (num <= 0)
		{
			return;
		}
		byte[] buffer = GetBuffer();
		while (num > 0)
		{
			int count = Math.Min(buffer.Length, num);
			int num2 = source.Read(buffer, 0, count);
			if (num2 > 0)
			{
				dest.Write(buffer, 0, num2);
				num -= num2;
				continue;
			}
			throw new ZipException("Unxpected end of stream");
		}
	}

	private void CopyBytes(ZipUpdate update, Stream destination, Stream source, long bytesToCopy, bool updateCrc)
	{
		if (destination == source)
		{
			throw new InvalidOperationException("Destination and source are the same");
		}
		Crc32 crc = new Crc32();
		byte[] buffer = GetBuffer();
		long num = bytesToCopy;
		long num2 = 0L;
		int num4;
		do
		{
			int num3 = buffer.Length;
			if (bytesToCopy < num3)
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
				bytesToCopy -= num4;
				num2 += num4;
			}
		}
		while (num4 > 0 && bytesToCopy > 0);
		if (num2 != num)
		{
			throw new ZipException($"Failed to copy bytes expected {num} read {num2}");
		}
		if (updateCrc)
		{
			update.OutEntry.Crc = crc.Value;
		}
	}

	private int GetDescriptorSize(ZipUpdate update)
	{
		int result = 0;
		if (((uint)update.Entry.Flags & 8u) != 0)
		{
			result = 12;
			if (update.Entry.LocalHeaderRequiresZip64)
			{
				result = 20;
			}
		}
		return result;
	}

	private void CopyDescriptorBytesDirect(ZipUpdate update, Stream stream, ref long destinationPosition, long sourcePosition)
	{
		int num = GetDescriptorSize(update);
		while (num > 0)
		{
			int count = num;
			byte[] buffer = GetBuffer();
			stream.Position = sourcePosition;
			int num2 = stream.Read(buffer, 0, count);
			if (num2 > 0)
			{
				stream.Position = destinationPosition;
				stream.Write(buffer, 0, num2);
				num -= num2;
				destinationPosition += num2;
				sourcePosition += num2;
				continue;
			}
			throw new ZipException("Unxpected end of stream");
		}
	}

	private void CopyEntryDataDirect(ZipUpdate update, Stream stream, bool updateCrc, ref long destinationPosition, ref long sourcePosition)
	{
		long num = update.Entry.CompressedSize;
		Crc32 crc = new Crc32();
		byte[] buffer = GetBuffer();
		long num2 = num;
		long num3 = 0L;
		int num5;
		do
		{
			int num4 = buffer.Length;
			if (num < num4)
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
				destinationPosition += num5;
				sourcePosition += num5;
				num -= num5;
				num3 += num5;
			}
		}
		while (num5 > 0 && num > 0);
		if (num3 != num2)
		{
			throw new ZipException($"Failed to copy bytes expected {num2} read {num3}");
		}
		if (updateCrc)
		{
			update.OutEntry.Crc = crc.Value;
		}
	}

	private int FindExistingUpdate(ZipEntry entry)
	{
		int result = -1;
		string transformedFileName = GetTransformedFileName(entry.Name);
		for (int i = 0; i < updates_.Count; i++)
		{
			ZipUpdate zipUpdate = (ZipUpdate)updates_[i];
			if (zipUpdate.Entry.ZipFileIndex == entry.ZipFileIndex && string.Compare(transformedFileName, zipUpdate.Entry.Name, ignoreCase: true, CultureInfo.InvariantCulture) == 0)
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
		string transformedFileName = GetTransformedFileName(fileName);
		for (int i = 0; i < updates_.Count; i++)
		{
			if (string.Compare(transformedFileName, ((ZipUpdate)updates_[i]).Entry.Name, ignoreCase: true, CultureInfo.InvariantCulture) == 0)
			{
				result = i;
				break;
			}
		}
		return result;
	}

	private Stream GetOutputStream(ZipEntry entry)
	{
		Stream stream = baseStream_;
		if (entry.IsCrypted)
		{
			stream = CreateAndInitEncryptionStream(stream, entry);
		}
		switch (entry.CompressionMethod)
		{
		case CompressionMethod.Stored:
			return new UncompressedStream(stream);
		case CompressionMethod.Deflated:
		{
			DeflaterOutputStream deflaterOutputStream = new DeflaterOutputStream(stream, new Deflater(9, noZlibHeaderOrFooter: true));
			deflaterOutputStream.IsStreamOwner = false;
			return deflaterOutputStream;
		}
		default:
			throw new ZipException("Unknown compression method " + entry.CompressionMethod);
		}
	}

	private void AddEntry(ZipFile workFile, ZipUpdate update)
	{
		long num = 0L;
		Stream stream = null;
		if (update.Entry.IsFile)
		{
			stream = update.GetSource();
			if (stream == null)
			{
				stream = updateDataSource_.GetSource(update.Entry, update.Filename);
			}
		}
		if (stream != null)
		{
			using (stream)
			{
				long length = stream.Length;
				if (update.OutEntry.Size < 0)
				{
					update.OutEntry.Size = length;
				}
				else if (update.OutEntry.Size != length)
				{
					throw new ZipException("Entry size/stream size mismatch");
				}
				workFile.WriteLocalEntryHeader(update);
				num = workFile.baseStream_.Position;
				using Stream destination = workFile.GetOutputStream(update.OutEntry);
				CopyBytes(update, destination, stream, length, updateCrc: true);
			}
		}
		else
		{
			workFile.WriteLocalEntryHeader(update);
			num = workFile.baseStream_.Position;
		}
		long position = workFile.baseStream_.Position;
		update.OutEntry.CompressedSize = position - num;
	}

	private void ModifyEntry(ZipFile workFile, ZipUpdate update)
	{
		workFile.WriteLocalEntryHeader(update);
		long position = workFile.baseStream_.Position;
		if (update.Entry.IsFile && update.Filename != null)
		{
			using Stream destination = workFile.GetOutputStream(update.OutEntry);
			using Stream stream = GetInputStream(update.Entry);
			CopyBytes(update, destination, stream, stream.Length, updateCrc: true);
		}
		long position2 = workFile.baseStream_.Position;
		update.Entry.CompressedSize = position2 - position;
	}

	private void CopyEntryDirect(ZipFile workFile, ZipUpdate update, ref long destinationPosition)
	{
		bool flag = false;
		if (update.Entry.Offset == destinationPosition)
		{
			flag = true;
		}
		if (!flag)
		{
			baseStream_.Position = destinationPosition;
			workFile.WriteLocalEntryHeader(update);
			destinationPosition = baseStream_.Position;
		}
		long num = 0L;
		long num2 = update.Entry.Offset + 26;
		baseStream_.Seek(num2, SeekOrigin.Begin);
		uint num3 = ReadLEUshort();
		uint num4 = ReadLEUshort();
		num = baseStream_.Position + num3 + num4;
		if (flag)
		{
			destinationPosition += num - num2 + 26 + update.Entry.CompressedSize + GetDescriptorSize(update);
			return;
		}
		if (update.Entry.CompressedSize > 0)
		{
			CopyEntryDataDirect(update, baseStream_, updateCrc: false, ref destinationPosition, ref num);
		}
		CopyDescriptorBytesDirect(update, baseStream_, ref destinationPosition, num);
	}

	private void CopyEntry(ZipFile workFile, ZipUpdate update)
	{
		workFile.WriteLocalEntryHeader(update);
		if (update.Entry.CompressedSize > 0)
		{
			long offset = update.Entry.Offset + 26;
			baseStream_.Seek(offset, SeekOrigin.Begin);
			uint num = ReadLEUshort();
			uint num2 = ReadLEUshort();
			baseStream_.Seek(num + num2, SeekOrigin.Current);
			CopyBytes(update, workFile.baseStream_, baseStream_, update.Entry.CompressedSize, updateCrc: false);
		}
		CopyDescriptorBytes(update, workFile.baseStream_, baseStream_);
	}

	private void Reopen(Stream source)
	{
		if (source == null)
		{
			throw new ZipException("Failed to reopen archive - no source");
		}
		isNewArchive_ = false;
		baseStream_ = source;
		ReadEntries();
	}

	private void Reopen()
	{
		if (Name == null)
		{
			throw new InvalidOperationException("Name is not known cannot Reopen");
		}
		Reopen(File.OpenRead(Name));
	}

	private void UpdateCommentOnly()
	{
		long length = baseStream_.Length;
		ZipHelperStream zipHelperStream = null;
		if (archiveStorage_.UpdateMode == FileUpdateMode.Safe)
		{
			Stream stream = archiveStorage_.MakeTemporaryCopy(baseStream_);
			zipHelperStream = new ZipHelperStream(stream);
			zipHelperStream.IsStreamOwner = true;
			baseStream_.Close();
			baseStream_ = null;
		}
		else if (archiveStorage_.UpdateMode == FileUpdateMode.Direct)
		{
			baseStream_ = archiveStorage_.OpenForDirectUpdate(baseStream_);
			zipHelperStream = new ZipHelperStream(baseStream_);
		}
		else
		{
			baseStream_.Close();
			baseStream_ = null;
			zipHelperStream = new ZipHelperStream(Name);
		}
		using (zipHelperStream)
		{
			long num = zipHelperStream.LocateBlockWithSignature(101010256, length, 22, 65535);
			if (num < 0)
			{
				throw new ZipException("Cannot find central directory");
			}
			zipHelperStream.Position += 16L;
			byte[] rawComment = newComment_.RawComment;
			zipHelperStream.WriteLEShort(rawComment.Length);
			zipHelperStream.Write(rawComment, 0, rawComment.Length);
			zipHelperStream.SetLength(zipHelperStream.Position);
		}
		if (archiveStorage_.UpdateMode == FileUpdateMode.Safe)
		{
			Reopen(archiveStorage_.ConvertTemporaryToFinal());
		}
		else
		{
			ReadEntries();
		}
	}

	private void RunUpdates()
	{
		long num = 0L;
		long length = 0L;
		bool flag = true;
		bool flag2 = false;
		long destinationPosition = 0L;
		ZipFile zipFile;
		if (IsNewArchive)
		{
			zipFile = this;
			zipFile.baseStream_.Position = 0L;
			flag2 = true;
		}
		else if (archiveStorage_.UpdateMode == FileUpdateMode.Direct)
		{
			zipFile = this;
			zipFile.baseStream_.Position = 0L;
			flag2 = true;
			updates_.Sort(new UpdateComparer());
		}
		else
		{
			zipFile = Create(archiveStorage_.GetTemporaryOutput());
			if (key != null)
			{
				zipFile.key = (byte[])key.Clone();
			}
		}
		try
		{
			foreach (ZipUpdate item in updates_)
			{
				switch (item.Command)
				{
				case UpdateCommand.Copy:
					if (flag2)
					{
						CopyEntryDirect(zipFile, item, ref destinationPosition);
					}
					else
					{
						CopyEntry(zipFile, item);
					}
					break;
				case UpdateCommand.Modify:
					ModifyEntry(zipFile, item);
					break;
				case UpdateCommand.Add:
					if (!IsNewArchive && flag2)
					{
						zipFile.baseStream_.Position = destinationPosition;
					}
					AddEntry(zipFile, item);
					if (flag2)
					{
						destinationPosition = zipFile.baseStream_.Position;
					}
					break;
				}
			}
			if (!IsNewArchive && flag2)
			{
				zipFile.baseStream_.Position = destinationPosition;
			}
			long position = zipFile.baseStream_.Position;
			foreach (ZipUpdate item2 in updates_)
			{
				num += zipFile.WriteCentralDirectoryHeader(item2.OutEntry);
			}
			byte[] comment = ((newComment_ != null) ? newComment_.RawComment : ZipConstants.ConvertToArray(comment_));
			using (ZipHelperStream zipHelperStream = new ZipHelperStream(zipFile.baseStream_))
			{
				zipHelperStream.WriteEndOfCentralDirectory(updates_.Count, num, position, comment);
			}
			length = zipFile.baseStream_.Position;
			foreach (ZipUpdate item3 in updates_)
			{
				if (item3.CrcPatchOffset > 0 && item3.OutEntry.CompressedSize > 0)
				{
					zipFile.baseStream_.Position = item3.CrcPatchOffset;
					zipFile.WriteLEInt((int)item3.OutEntry.Crc);
				}
				if (item3.SizePatchOffset > 0)
				{
					zipFile.baseStream_.Position = item3.SizePatchOffset;
					if (item3.Entry.LocalHeaderRequiresZip64)
					{
						zipFile.WriteLeLong(item3.OutEntry.Size);
						zipFile.WriteLeLong(item3.OutEntry.CompressedSize);
					}
					else
					{
						zipFile.WriteLEInt((int)item3.OutEntry.CompressedSize);
						zipFile.WriteLEInt((int)item3.OutEntry.Size);
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
		if (flag)
		{
			if (flag2)
			{
				isNewArchive_ = false;
				zipFile.baseStream_.Flush();
				ReadEntries();
			}
			else
			{
				baseStream_.Close();
				Reopen(archiveStorage_.ConvertTemporaryToFinal());
			}
		}
		else
		{
			zipFile.Close();
			if (!flag2 && zipFile.Name != null)
			{
				File.Delete(zipFile.Name);
			}
		}
	}

	private void CheckUpdating()
	{
		if (updates_ == null)
		{
			throw new ZipException("Cannot update until BeginUpdate has been called");
		}
	}

	void IDisposable.Dispose()
	{
		Close();
	}

	private void DisposeInternal(bool disposing)
	{
		if (isDisposed_)
		{
			return;
		}
		isDisposed_ = true;
		entries_ = null;
		if (IsStreamOwner)
		{
			lock (baseStream_)
			{
				baseStream_.Close();
			}
		}
	}

	protected virtual void Dispose(bool disposing)
	{
		DisposeInternal(disposing);
	}

	private ushort ReadLEUshort()
	{
		int num = baseStream_.ReadByte();
		if (num < 0)
		{
			throw new IOException("End of stream");
		}
		int num2 = baseStream_.ReadByte();
		if (num2 < 0)
		{
			throw new IOException("End of stream");
		}
		return (ushort)((ushort)num | (ushort)(num2 << 8));
	}

	private uint ReadLEUint()
	{
		return (uint)(ReadLEUshort() | (ReadLEUshort() << 16));
	}

	private ulong ReadLEUlong()
	{
		return ReadLEUint() | ReadLEUint();
	}

	private long LocateBlockWithSignature(int signature, long endLocation, int minimumBlockSize, int maximumVariableData)
	{
		using ZipHelperStream zipHelperStream = new ZipHelperStream(baseStream_);
		return zipHelperStream.LocateBlockWithSignature(signature, endLocation, minimumBlockSize, maximumVariableData);
	}

	private void ReadEntries()
	{
		if (!baseStream_.CanSeek)
		{
			throw new ZipException("ZipFile stream must be seekable");
		}
		long num = LocateBlockWithSignature(101010256, baseStream_.Length, 22, 65535);
		if (num < 0)
		{
			throw new ZipException("Cannot find central directory");
		}
		ushort num2 = ReadLEUshort();
		ushort num3 = ReadLEUshort();
		ulong num4 = ReadLEUshort();
		ulong num5 = ReadLEUshort();
		ulong num6 = ReadLEUint();
		long num7 = ReadLEUint();
		uint num8 = ReadLEUshort();
		if (num8 != 0)
		{
			byte[] array = new byte[num8];
			StreamUtils.ReadFully(baseStream_, array);
			comment_ = ZipConstants.ConvertToString(array);
		}
		else
		{
			comment_ = string.Empty;
		}
		bool flag = false;
		if (num2 == ushort.MaxValue || num3 == ushort.MaxValue || num4 == 65535 || num5 == 65535 || num6 == uint.MaxValue || num7 == uint.MaxValue)
		{
			flag = true;
			long num9 = LocateBlockWithSignature(117853008, num, 0, 4096);
			if (num9 < 0)
			{
				throw new ZipException("Cannot find Zip64 locator");
			}
			ReadLEUint();
			ulong num10 = ReadLEUlong();
			ReadLEUint();
			baseStream_.Position = (long)num10;
			long num11 = ReadLEUint();
			if (num11 != 101075792)
			{
				throw new ZipException($"Invalid Zip64 Central directory signature at {num10:X}");
			}
			ReadLEUlong();
			ReadLEUshort();
			ReadLEUshort();
			ReadLEUint();
			ReadLEUint();
			num4 = ReadLEUlong();
			num5 = ReadLEUlong();
			num6 = ReadLEUlong();
			num7 = (long)ReadLEUlong();
		}
		entries_ = new ZipEntry[num4];
		if (!flag && num7 < num - (long)(4 + num6))
		{
			offsetOfFirstEntry = num - ((long)(4 + num6) + num7);
			if (offsetOfFirstEntry <= 0)
			{
				throw new ZipException("Invalid embedded zip archive");
			}
		}
		baseStream_.Seek(offsetOfFirstEntry + num7, SeekOrigin.Begin);
		for (ulong num12 = 0uL; num12 < num4; num12++)
		{
			if (ReadLEUint() != 33639248)
			{
				throw new ZipException("Wrong Central Directory signature");
			}
			int madeByInfo = ReadLEUshort();
			int versionRequiredToExtract = ReadLEUshort();
			int num13 = ReadLEUshort();
			int method = ReadLEUshort();
			uint num14 = ReadLEUint();
			uint num15 = ReadLEUint();
			long num16 = ReadLEUint();
			long num17 = ReadLEUint();
			int num18 = ReadLEUshort();
			int num19 = ReadLEUshort();
			int num20 = ReadLEUshort();
			ReadLEUshort();
			ReadLEUshort();
			uint externalFileAttributes = ReadLEUint();
			long offset = ReadLEUint();
			byte[] array2 = new byte[Math.Max(num18, num20)];
			StreamUtils.ReadFully(baseStream_, array2, 0, num18);
			string name = ZipConstants.ConvertToStringExt(num13, array2, num18);
			ZipEntry zipEntry = new ZipEntry(name, versionRequiredToExtract, madeByInfo, (CompressionMethod)method);
			zipEntry.Crc = (long)num15 & 0xFFFFFFFFL;
			zipEntry.Size = num17 & 0xFFFFFFFFu;
			zipEntry.CompressedSize = num16 & 0xFFFFFFFFu;
			zipEntry.Flags = num13;
			zipEntry.DosTime = num14;
			if ((num13 & 8) == 0)
			{
				zipEntry.CryptoCheckValue = (byte)(num15 >> 24);
			}
			else
			{
				zipEntry.CryptoCheckValue = (byte)((num14 >> 8) & 0xFFu);
			}
			if (num19 > 0)
			{
				byte[] array3 = new byte[num19];
				StreamUtils.ReadFully(baseStream_, array3);
				zipEntry.ExtraData = array3;
			}
			zipEntry.ProcessExtraData(localHeader: false);
			if (num20 > 0)
			{
				StreamUtils.ReadFully(baseStream_, array2, 0, num20);
				zipEntry.Comment = ZipConstants.ConvertToStringExt(num13, array2, num20);
			}
			zipEntry.ZipFileIndex = (long)num12;
			zipEntry.Offset = offset;
			zipEntry.ExternalFileAttributes = (int)externalFileAttributes;
			entries_[num12] = zipEntry;
		}
	}

	private long LocateEntry(ZipEntry entry)
	{
		return TestLocalHeader(entry, HeaderTest.Extract);
	}

	private Stream CreateAndInitDecryptionStream(Stream baseStream, ZipEntry entry)
	{
		CryptoStream cryptoStream = null;
		if (entry.Version < 50 || (entry.Flags & 0x40) == 0)
		{
			PkzipClassicManaged pkzipClassicManaged = new PkzipClassicManaged();
			OnKeysRequired(entry.Name);
			if (!HaveKeys)
			{
				throw new ZipException("No password available for encrypted stream");
			}
			cryptoStream = new CryptoStream(baseStream, pkzipClassicManaged.CreateDecryptor(key, null), CryptoStreamMode.Read);
			CheckClassicPassword(cryptoStream, entry);
			return cryptoStream;
		}
		throw new ZipException("Decryption method not supported");
	}

	private Stream CreateAndInitEncryptionStream(Stream baseStream, ZipEntry entry)
	{
		CryptoStream cryptoStream = null;
		if (entry.Version < 50 || (entry.Flags & 0x40) == 0)
		{
			PkzipClassicManaged pkzipClassicManaged = new PkzipClassicManaged();
			OnKeysRequired(entry.Name);
			if (!HaveKeys)
			{
				throw new ZipException("No password available for encrypted stream");
			}
			cryptoStream = new CryptoStream(new UncompressedStream(baseStream), pkzipClassicManaged.CreateEncryptor(key, null), CryptoStreamMode.Write);
			if (entry.Crc < 0 || ((uint)entry.Flags & 8u) != 0)
			{
				WriteEncryptionHeader(cryptoStream, entry.DosTime << 16);
			}
			else
			{
				WriteEncryptionHeader(cryptoStream, entry.Crc);
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
}
