using System;
using System.IO;
using ZipLib.Core;

namespace ZipLib.Zip
{
	internal class ZipEntryFactory : IEntryFactory
	{
		public ZipEntryFactory()
		{
			this.nameTransform_ = new ZipNameTransform();
		}

		public ZipEntryFactory(ZipEntryFactory.TimeSetting timeSetting)
		{
			this.timeSetting_ = timeSetting;
		}

		public ZipEntryFactory(DateTime time)
		{
			this.timeSetting_ = ZipEntryFactory.TimeSetting.Fixed;
			this.FixedDateTime = time;
		}

		public INameTransform NameTransform
		{
			get
			{
				return this.nameTransform_;
			}
			set
			{
				if (value == null)
				{
					this.nameTransform_ = new ZipNameTransform();
					return;
				}
				this.nameTransform_ = value;
			}
		}

		public ZipEntryFactory.TimeSetting Setting
		{
			get
			{
				return this.timeSetting_;
			}
			set
			{
				this.timeSetting_ = value;
			}
		}

		public DateTime FixedDateTime
		{
			get
			{
				return this.fixedDateTime_;
			}
			set
			{
				if (value.Year < 1970)
				{
					throw new ArgumentException("Value is too old to be valid", "value");
				}
				this.fixedDateTime_ = value;
			}
		}

		public int GetAttributes
		{
			get
			{
				return this.getAttributes_;
			}
			set
			{
				this.getAttributes_ = value;
			}
		}

		public int SetAttributes
		{
			get
			{
				return this.setAttributes_;
			}
			set
			{
				this.setAttributes_ = value;
			}
		}

		public ZipEntry MakeFileEntry(string fileName)
		{
			FileInfo fileInfo = new FileInfo(fileName);
			ZipEntry zipEntry = new ZipEntry(this.nameTransform_.TransformFile(fileName));
			zipEntry.Size = fileInfo.Length;
			int num = (int)(fileInfo.Attributes & (FileAttributes)this.getAttributes_);
			num |= this.setAttributes_;
			zipEntry.ExternalFileAttributes = num;
			switch (this.timeSetting_)
			{
			case ZipEntryFactory.TimeSetting.LastWriteTime:
				zipEntry.DateTime = fileInfo.LastWriteTime;
				break;
			case ZipEntryFactory.TimeSetting.LastWriteTimeUtc:
				zipEntry.DateTime = fileInfo.LastWriteTimeUtc;
				break;
			case ZipEntryFactory.TimeSetting.CreateTime:
				zipEntry.DateTime = fileInfo.CreationTime;
				break;
			case ZipEntryFactory.TimeSetting.CreateTimeUtc:
				zipEntry.DateTime = fileInfo.CreationTimeUtc;
				break;
			case ZipEntryFactory.TimeSetting.LastAccessTime:
				zipEntry.DateTime = fileInfo.LastAccessTime;
				break;
			case ZipEntryFactory.TimeSetting.LastAccessTimeUtc:
				zipEntry.DateTime = fileInfo.LastAccessTimeUtc;
				break;
			case ZipEntryFactory.TimeSetting.Fixed:
				zipEntry.DateTime = this.fixedDateTime_;
				break;
			}
			zipEntry.DateTime = fileInfo.LastWriteTime;
			return zipEntry;
		}

		public ZipEntry MakeDirectoryEntry(string directoryName)
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(directoryName);
			ZipEntry zipEntry = new ZipEntry(this.nameTransform_.TransformDirectory(directoryName));
			int num = (int)(directoryInfo.Attributes & (FileAttributes)this.getAttributes_);
			num |= this.setAttributes_;
			zipEntry.ExternalFileAttributes = num;
			switch (this.timeSetting_)
			{
			case ZipEntryFactory.TimeSetting.LastWriteTime:
				zipEntry.DateTime = directoryInfo.LastWriteTime;
				break;
			case ZipEntryFactory.TimeSetting.LastWriteTimeUtc:
				zipEntry.DateTime = directoryInfo.LastWriteTimeUtc;
				break;
			case ZipEntryFactory.TimeSetting.CreateTime:
				zipEntry.DateTime = directoryInfo.CreationTime;
				break;
			case ZipEntryFactory.TimeSetting.CreateTimeUtc:
				zipEntry.DateTime = directoryInfo.CreationTimeUtc;
				break;
			case ZipEntryFactory.TimeSetting.LastAccessTime:
				zipEntry.DateTime = directoryInfo.LastAccessTime;
				break;
			case ZipEntryFactory.TimeSetting.LastAccessTimeUtc:
				zipEntry.DateTime = directoryInfo.LastAccessTimeUtc;
				break;
			case ZipEntryFactory.TimeSetting.Fixed:
				zipEntry.DateTime = this.fixedDateTime_;
				break;
			}
			return zipEntry;
		}

		private INameTransform nameTransform_;

		private DateTime fixedDateTime_ = DateTime.Now;

		private ZipEntryFactory.TimeSetting timeSetting_;

		private int getAttributes_ = -1;

		private int setAttributes_;

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
	}
}
