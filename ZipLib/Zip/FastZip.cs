using System;
using System.IO;
using ZipLib.Core;

namespace ZipLib.Zip
{
	public class FastZip
	{
		public FastZip()
		{
		}

		public FastZip(FastZipEvents events)
		{
			this.events_ = events;
		}

		public bool CreateEmptyDirectories
		{
			get
			{
				return this.createEmptyDirectories_;
			}
			set
			{
				this.createEmptyDirectories_ = value;
			}
		}

		public string Password
		{
			get
			{
				return this.password_;
			}
			set
			{
				this.password_ = value;
			}
		}

		public INameTransform NameTransform
		{
			get
			{
				return this.entryFactory_.NameTransform;
			}
			set
			{
				this.entryFactory_.NameTransform = value;
			}
		}

		public IEntryFactory EntryFactory
		{
			get
			{
				return this.entryFactory_;
			}
			set
			{
				if (value == null)
				{
					this.entryFactory_ = new ZipEntryFactory();
					return;
				}
				this.entryFactory_ = value;
			}
		}

		public bool RestoreDateTimeOnExtract
		{
			get
			{
				return this.restoreDateTimeOnExtract_;
			}
			set
			{
				this.restoreDateTimeOnExtract_ = value;
			}
		}

		public void CreateZip(string zipFileName, string sourceDirectory, bool recurse, string fileFilter, string directoryFilter)
		{
			this.CreateZip(File.Create(zipFileName), sourceDirectory, recurse, fileFilter, directoryFilter);
		}

		public void CreateZip(string zipFileName, string sourceDirectory, bool recurse, string fileFilter)
		{
			this.CreateZip(File.Create(zipFileName), sourceDirectory, recurse, fileFilter, null);
		}

		public void CreateZip(Stream outputStream, string sourceDirectory, bool recurse, string fileFilter, string directoryFilter)
		{
			this.NameTransform = new ZipNameTransform(sourceDirectory);
			this.sourceDirectory_ = sourceDirectory;
			using (this.outputStream_ = new ZipOutputStream(outputStream))
			{
				FileSystemScanner fileSystemScanner = new FileSystemScanner(fileFilter, directoryFilter);
				FileSystemScanner fileSystemScanner2 = fileSystemScanner;
				fileSystemScanner2.ProcessFile = (ProcessFileDelegate)Delegate.Combine(fileSystemScanner2.ProcessFile, new ProcessFileDelegate(this.ProcessFile));
				if (this.CreateEmptyDirectories)
				{
					FileSystemScanner fileSystemScanner3 = fileSystemScanner;
					fileSystemScanner3.ProcessDirectory = (ProcessDirectoryDelegate)Delegate.Combine(fileSystemScanner3.ProcessDirectory, new ProcessDirectoryDelegate(this.ProcessDirectory));
				}
				if (this.events_ != null)
				{
					if (this.events_.FileFailure != null)
					{
						FileSystemScanner fileSystemScanner4 = fileSystemScanner;
						fileSystemScanner4.FileFailure = (FileFailureDelegate)Delegate.Combine(fileSystemScanner4.FileFailure, this.events_.FileFailure);
					}
					if (this.events_.DirectoryFailure != null)
					{
						FileSystemScanner fileSystemScanner5 = fileSystemScanner;
						fileSystemScanner5.DirectoryFailure = (DirectoryFailureDelegate)Delegate.Combine(fileSystemScanner5.DirectoryFailure, this.events_.DirectoryFailure);
					}
				}
				fileSystemScanner.Scan(sourceDirectory, recurse);
			}
		}

		public void ExtractZip(string zipFileName, string targetDirectory, string fileFilter)
		{
			this.ExtractZip(zipFileName, targetDirectory, FastZip.Overwrite.Always, null, fileFilter, null, this.restoreDateTimeOnExtract_);
		}

		public void ExtractZip(string zipFileName, string targetDirectory, FastZip.Overwrite overwrite, FastZip.ConfirmOverwriteDelegate confirmDelegate, string fileFilter, string directoryFilter, bool restoreDateTime)
		{
			if (overwrite == FastZip.Overwrite.Prompt && confirmDelegate == null)
			{
				throw new ArgumentNullException("confirmDelegate");
			}
			this.continueRunning_ = true;
			this.overwrite_ = overwrite;
			this.confirmDelegate_ = confirmDelegate;
			this.targetDirectory_ = targetDirectory;
			this.fileFilter_ = new NameFilter(fileFilter);
			this.directoryFilter_ = new NameFilter(directoryFilter);
			this.restoreDateTimeOnExtract_ = restoreDateTime;
			using (this.inputStream_ = new ZipInputStream(File.OpenRead(zipFileName)))
			{
				if (this.password_ != null)
				{
					this.inputStream_.Password = this.password_;
				}
				ZipEntry nextEntry;
				while (this.continueRunning_ && (nextEntry = this.inputStream_.GetNextEntry()) != null)
				{
					if (this.directoryFilter_.IsMatch(Path.GetDirectoryName(nextEntry.Name)) && this.fileFilter_.IsMatch(nextEntry.Name))
					{
						this.ExtractEntry(nextEntry);
					}
				}
			}
		}

		private void ProcessDirectory(object sender, DirectoryEventArgs e)
		{
			if (!e.HasMatchingFiles && this.CreateEmptyDirectories)
			{
				if (this.events_ != null)
				{
					this.events_.OnProcessDirectory(e.Name, e.HasMatchingFiles);
				}
				if (e.Name != this.sourceDirectory_)
				{
					ZipEntry entry = this.entryFactory_.MakeDirectoryEntry(e.Name);
					this.outputStream_.PutNextEntry(entry);
				}
			}
		}

		private void ProcessFile(object sender, ScanEventArgs e)
		{
			if (this.events_ != null && this.events_.ProcessFile != null)
			{
				this.events_.ProcessFile(sender, e);
			}
			if (e.ContinueRunning)
			{
				ZipEntry entry = this.entryFactory_.MakeFileEntry(e.Name);
				this.outputStream_.PutNextEntry(entry);
				this.AddFileContents(e.Name);
			}
		}

		private void AddFileContents(string name)
		{
			if (this.buffer_ == null)
			{
				this.buffer_ = new byte[4096];
			}
			using (FileStream fileStream = File.OpenRead(name))
			{
				StreamUtils.Copy(fileStream, this.outputStream_, this.buffer_);
			}
		}

		private void ExtractFileEntry(ZipEntry entry, string targetName)
		{
			bool flag = true;
			if (this.overwrite_ != FastZip.Overwrite.Always && File.Exists(targetName))
			{
				flag = (this.overwrite_ == FastZip.Overwrite.Prompt && this.confirmDelegate_ != null && this.confirmDelegate_(targetName));
			}
			if (flag)
			{
				if (this.events_ != null)
				{
					this.continueRunning_ = this.events_.OnProcessFile(entry.Name);
				}
				if (this.continueRunning_)
				{
					try
					{
						using (FileStream fileStream = File.Create(targetName))
						{
							if (this.buffer_ == null)
							{
								this.buffer_ = new byte[4096];
							}
							StreamUtils.Copy(this.inputStream_, fileStream, this.buffer_);
						}
						if (this.restoreDateTimeOnExtract_)
						{
							File.SetLastWriteTime(targetName, entry.DateTime);
						}
					}
					catch (Exception e)
					{
						if (this.events_ != null)
						{
							this.continueRunning_ = this.events_.OnFileFailure(targetName, e);
						}
						else
						{
							this.continueRunning_ = false;
						}
					}
				}
			}
		}

		private void ExtractEntry(ZipEntry entry)
		{
			bool flag = FastZip.NameIsValid(entry.Name) && entry.IsCompressionMethodSupported();
			string path = null;
			string text = null;
			if (flag)
			{
				string text3;
				if (Path.IsPathRooted(entry.Name))
				{
					string text2 = Path.GetPathRoot(entry.Name);
					text2 = entry.Name.Substring(text2.Length);
					text3 = Path.Combine(Path.GetDirectoryName(text2), Path.GetFileName(entry.Name));
				}
				else
				{
					text3 = entry.Name;
				}
				text = Path.Combine(this.targetDirectory_, text3);
				path = Path.GetDirectoryName(Path.GetFullPath(text));
				flag = (text3.Length > 0);
			}
			if (flag && !Directory.Exists(path))
			{
				if (entry.IsDirectory)
				{
					if (!this.CreateEmptyDirectories)
					{
						goto IL_B9;
					}
				}
				try
				{
					Directory.CreateDirectory(path);
				}
				catch
				{
					flag = false;
				}
			}
			IL_B9:
			if (flag && entry.IsFile)
			{
				this.ExtractFileEntry(entry, text);
			}
		}

		private static int MakeExternalAttributes(FileInfo info)
		{
			return (int)info.Attributes;
		}

		private static bool NameIsValid(string name)
		{
			return name != null && name.Length > 0 && name.IndexOfAny(Path.GetInvalidPathChars()) < 0;
		}

		private bool continueRunning_;

		private byte[] buffer_;

		private ZipOutputStream outputStream_;

		private ZipInputStream inputStream_;

		private string password_;

		private string targetDirectory_;

		private string sourceDirectory_;

		private NameFilter fileFilter_;

		private NameFilter directoryFilter_;

		private FastZip.Overwrite overwrite_;

		private FastZip.ConfirmOverwriteDelegate confirmDelegate_;

		private bool restoreDateTimeOnExtract_;

		private bool createEmptyDirectories_;

		private FastZipEvents events_;

		private IEntryFactory entryFactory_ = new ZipEntryFactory();

		public enum Overwrite
		{
			Prompt,
			Never,
			Always
		}

		public delegate bool ConfirmOverwriteDelegate(string fileName);
	}
}
