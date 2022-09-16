using System;
using System.IO;
using ZipLib.Core;

namespace ZipLib.Zip;

internal class ZipEntryFactory : IEntryFactory
{
	public enum TimeSetting
	{
		LastWriteTime,
		LastWriteTimeUtc,
		CreateTime,
		CreateTimeUtc,
		LastAccessTime,
		LastAccessTimeUtc,
		Fixed
	}

	private INameTransform nameTransform_;

	private DateTime fixedDateTime_ = DateTime.Now;

	private TimeSetting timeSetting_;

	private int getAttributes_ = -1;

	private int setAttributes_;

	public INameTransform NameTransform
	{
		get
		{
			return nameTransform_;
		}
		set
		{
			if (value == null)
			{
				nameTransform_ = new ZipNameTransform();
			}
			else
			{
				nameTransform_ = value;
			}
		}
	}

	public TimeSetting Setting
	{
		get
		{
			return timeSetting_;
		}
		set
		{
			timeSetting_ = value;
		}
	}

	public DateTime FixedDateTime
	{
		get
		{
			return fixedDateTime_;
		}
		set
		{
			if (value.Year < 1970)
			{
				throw new ArgumentException("Value is too old to be valid", "value");
			}
			fixedDateTime_ = value;
		}
	}

	public int GetAttributes
	{
		get
		{
			return getAttributes_;
		}
		set
		{
			getAttributes_ = value;
		}
	}

	public int SetAttributes
	{
		get
		{
			return setAttributes_;
		}
		set
		{
			setAttributes_ = value;
		}
	}

	public ZipEntryFactory()
	{
		nameTransform_ = new ZipNameTransform();
	}

	public ZipEntryFactory(TimeSetting timeSetting)
	{
		timeSetting_ = timeSetting;
	}

	public ZipEntryFactory(DateTime time)
	{
		timeSetting_ = TimeSetting.Fixed;
		FixedDateTime = time;
	}

	public ZipEntry MakeFileEntry(string fileName)
	{
		FileInfo fileInfo = new FileInfo(fileName);
		ZipEntry zipEntry = new ZipEntry(nameTransform_.TransformFile(fileName));
		zipEntry.Size = fileInfo.Length;
		int num = (int)fileInfo.Attributes & getAttributes_;
		num = (zipEntry.ExternalFileAttributes = num | setAttributes_);
		switch (timeSetting_)
		{
		case TimeSetting.CreateTime:
			zipEntry.DateTime = fileInfo.CreationTime;
			break;
		case TimeSetting.CreateTimeUtc:
			zipEntry.DateTime = fileInfo.CreationTimeUtc;
			break;
		case TimeSetting.LastAccessTime:
			zipEntry.DateTime = fileInfo.LastAccessTime;
			break;
		case TimeSetting.LastAccessTimeUtc:
			zipEntry.DateTime = fileInfo.LastAccessTimeUtc;
			break;
		case TimeSetting.LastWriteTime:
			zipEntry.DateTime = fileInfo.LastWriteTime;
			break;
		case TimeSetting.LastWriteTimeUtc:
			zipEntry.DateTime = fileInfo.LastWriteTimeUtc;
			break;
		case TimeSetting.Fixed:
			zipEntry.DateTime = fixedDateTime_;
			break;
		}
		zipEntry.DateTime = fileInfo.LastWriteTime;
		return zipEntry;
	}

	public ZipEntry MakeDirectoryEntry(string directoryName)
	{
		DirectoryInfo directoryInfo = new DirectoryInfo(directoryName);
		ZipEntry zipEntry = new ZipEntry(nameTransform_.TransformDirectory(directoryName));
		int num = (int)directoryInfo.Attributes & getAttributes_;
		num = (zipEntry.ExternalFileAttributes = num | setAttributes_);
		switch (timeSetting_)
		{
		case TimeSetting.CreateTime:
			zipEntry.DateTime = directoryInfo.CreationTime;
			break;
		case TimeSetting.CreateTimeUtc:
			zipEntry.DateTime = directoryInfo.CreationTimeUtc;
			break;
		case TimeSetting.LastAccessTime:
			zipEntry.DateTime = directoryInfo.LastAccessTime;
			break;
		case TimeSetting.LastAccessTimeUtc:
			zipEntry.DateTime = directoryInfo.LastAccessTimeUtc;
			break;
		case TimeSetting.LastWriteTime:
			zipEntry.DateTime = directoryInfo.LastWriteTime;
			break;
		case TimeSetting.LastWriteTimeUtc:
			zipEntry.DateTime = directoryInfo.LastWriteTimeUtc;
			break;
		case TimeSetting.Fixed:
			zipEntry.DateTime = fixedDateTime_;
			break;
		}
		return zipEntry;
	}
}
