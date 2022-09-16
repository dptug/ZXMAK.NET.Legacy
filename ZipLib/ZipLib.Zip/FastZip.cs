using System;
using System.IO;
using ZipLib.Core;

namespace ZipLib.Zip;

public class FastZip
{
	public enum Overwrite
	{
		Prompt,
		Never,
		Always
	}

	public delegate bool ConfirmOverwriteDelegate(string fileName);

	private bool continueRunning_;

	private byte[] buffer_;

	private ZipOutputStream outputStream_;

	private ZipInputStream inputStream_;

	private string password_;

	private string targetDirectory_;

	private string sourceDirectory_;

	private NameFilter fileFilter_;

	private NameFilter directoryFilter_;

	private Overwrite overwrite_;

	private ConfirmOverwriteDelegate confirmDelegate_;

	private bool restoreDateTimeOnExtract_;

	private bool createEmptyDirectories_;

	private FastZipEvents events_;

	private IEntryFactory entryFactory_ = new ZipEntryFactory();

	public bool CreateEmptyDirectories
	{
		get
		{
			return createEmptyDirectories_;
		}
		set
		{
			createEmptyDirectories_ = value;
		}
	}

	public string Password
	{
		get
		{
			return password_;
		}
		set
		{
			password_ = value;
		}
	}

	public INameTransform NameTransform
	{
		get
		{
			return entryFactory_.NameTransform;
		}
		set
		{
			entryFactory_.NameTransform = value;
		}
	}

	public IEntryFactory EntryFactory
	{
		get
		{
			return entryFactory_;
		}
		set
		{
			if (value == null)
			{
				entryFactory_ = new ZipEntryFactory();
			}
			else
			{
				entryFactory_ = value;
			}
		}
	}

	public bool RestoreDateTimeOnExtract
	{
		get
		{
			return restoreDateTimeOnExtract_;
		}
		set
		{
			restoreDateTimeOnExtract_ = value;
		}
	}

	public FastZip()
	{
	}

	public FastZip(FastZipEvents events)
	{
		events_ = events;
	}

	public void CreateZip(string zipFileName, string sourceDirectory, bool recurse, string fileFilter, string directoryFilter)
	{
		CreateZip(File.Create(zipFileName), sourceDirectory, recurse, fileFilter, directoryFilter);
	}

	public void CreateZip(string zipFileName, string sourceDirectory, bool recurse, string fileFilter)
	{
		CreateZip(File.Create(zipFileName), sourceDirectory, recurse, fileFilter, null);
	}

	public void CreateZip(Stream outputStream, string sourceDirectory, bool recurse, string fileFilter, string directoryFilter)
	{
		NameTransform = new ZipNameTransform(sourceDirectory);
		sourceDirectory_ = sourceDirectory;
		using (outputStream_ = new ZipOutputStream(outputStream))
		{
			FileSystemScanner fileSystemScanner = new FileSystemScanner(fileFilter, directoryFilter);
			fileSystemScanner.ProcessFile = (ProcessFileDelegate)Delegate.Combine(fileSystemScanner.ProcessFile, new ProcessFileDelegate(ProcessFile));
			if (CreateEmptyDirectories)
			{
				fileSystemScanner.ProcessDirectory = (ProcessDirectoryDelegate)Delegate.Combine(fileSystemScanner.ProcessDirectory, new ProcessDirectoryDelegate(ProcessDirectory));
			}
			if (events_ != null)
			{
				if (events_.FileFailure != null)
				{
					fileSystemScanner.FileFailure = (FileFailureDelegate)Delegate.Combine(fileSystemScanner.FileFailure, events_.FileFailure);
				}
				if (events_.DirectoryFailure != null)
				{
					fileSystemScanner.DirectoryFailure = (DirectoryFailureDelegate)Delegate.Combine(fileSystemScanner.DirectoryFailure, events_.DirectoryFailure);
				}
			}
			fileSystemScanner.Scan(sourceDirectory, recurse);
		}
	}

	public void ExtractZip(string zipFileName, string targetDirectory, string fileFilter)
	{
		ExtractZip(zipFileName, targetDirectory, Overwrite.Always, null, fileFilter, null, restoreDateTimeOnExtract_);
	}

	public void ExtractZip(string zipFileName, string targetDirectory, Overwrite overwrite, ConfirmOverwriteDelegate confirmDelegate, string fileFilter, string directoryFilter, bool restoreDateTime)
	{
		if (overwrite == Overwrite.Prompt && confirmDelegate == null)
		{
			throw new ArgumentNullException("confirmDelegate");
		}
		continueRunning_ = true;
		overwrite_ = overwrite;
		confirmDelegate_ = confirmDelegate;
		targetDirectory_ = targetDirectory;
		fileFilter_ = new NameFilter(fileFilter);
		directoryFilter_ = new NameFilter(directoryFilter);
		restoreDateTimeOnExtract_ = restoreDateTime;
		using (inputStream_ = new ZipInputStream(File.OpenRead(zipFileName)))
		{
			if (password_ != null)
			{
				inputStream_.Password = password_;
			}
			ZipEntry nextEntry;
			while (continueRunning_ && (nextEntry = inputStream_.GetNextEntry()) != null)
			{
				if (directoryFilter_.IsMatch(Path.GetDirectoryName(nextEntry.Name)) && fileFilter_.IsMatch(nextEntry.Name))
				{
					ExtractEntry(nextEntry);
				}
			}
		}
	}

	private void ProcessDirectory(object sender, DirectoryEventArgs e)
	{
		if (!e.HasMatchingFiles && CreateEmptyDirectories)
		{
			if (events_ != null)
			{
				events_.OnProcessDirectory(e.Name, e.HasMatchingFiles);
			}
			if (e.Name != sourceDirectory_)
			{
				ZipEntry entry = entryFactory_.MakeDirectoryEntry(e.Name);
				outputStream_.PutNextEntry(entry);
			}
		}
	}

	private void ProcessFile(object sender, ScanEventArgs e)
	{
		if (events_ != null && events_.ProcessFile != null)
		{
			events_.ProcessFile(sender, e);
		}
		if (e.ContinueRunning)
		{
			ZipEntry entry = entryFactory_.MakeFileEntry(e.Name);
			outputStream_.PutNextEntry(entry);
			AddFileContents(e.Name);
		}
	}

	private void AddFileContents(string name)
	{
		if (buffer_ == null)
		{
			buffer_ = new byte[4096];
		}
		using FileStream source = File.OpenRead(name);
		StreamUtils.Copy(source, outputStream_, buffer_);
	}

	private void ExtractFileEntry(ZipEntry entry, string targetName)
	{
		bool flag = true;
		if (overwrite_ != Overwrite.Always && File.Exists(targetName))
		{
			flag = overwrite_ == Overwrite.Prompt && confirmDelegate_ != null && confirmDelegate_(targetName);
		}
		if (!flag)
		{
			return;
		}
		if (events_ != null)
		{
			continueRunning_ = events_.OnProcessFile(entry.Name);
		}
		if (!continueRunning_)
		{
			return;
		}
		try
		{
			using (FileStream destination = File.Create(targetName))
			{
				if (buffer_ == null)
				{
					buffer_ = new byte[4096];
				}
				StreamUtils.Copy(inputStream_, destination, buffer_);
			}
			if (restoreDateTimeOnExtract_)
			{
				File.SetLastWriteTime(targetName, entry.DateTime);
			}
		}
		catch (Exception e)
		{
			if (events_ != null)
			{
				continueRunning_ = events_.OnFileFailure(targetName, e);
			}
			else
			{
				continueRunning_ = false;
			}
		}
	}

	private void ExtractEntry(ZipEntry entry)
	{
		bool flag = NameIsValid(entry.Name) && entry.IsCompressionMethodSupported();
		string path = null;
		string text = null;
		if (flag)
		{
			string text2;
			if (Path.IsPathRooted(entry.Name))
			{
				string pathRoot = Path.GetPathRoot(entry.Name);
				pathRoot = entry.Name.Substring(pathRoot.Length);
				text2 = Path.Combine(Path.GetDirectoryName(pathRoot), Path.GetFileName(entry.Name));
			}
			else
			{
				text2 = entry.Name;
			}
			text = Path.Combine(targetDirectory_, text2);
			path = Path.GetDirectoryName(Path.GetFullPath(text));
			flag = text2.Length > 0;
		}
		if (flag && !Directory.Exists(path) && (!entry.IsDirectory || CreateEmptyDirectories))
		{
			try
			{
				Directory.CreateDirectory(path);
			}
			catch
			{
				flag = false;
			}
		}
		if (flag && entry.IsFile)
		{
			ExtractFileEntry(entry, text);
		}
	}

	private static int MakeExternalAttributes(FileInfo info)
	{
		return (int)info.Attributes;
	}

	private static bool NameIsValid(string name)
	{
		if (name != null && name.Length > 0)
		{
			return name.IndexOfAny(Path.GetInvalidPathChars()) < 0;
		}
		return false;
	}
}
