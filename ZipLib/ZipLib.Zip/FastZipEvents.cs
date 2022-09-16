using System;
using ZipLib.Core;

namespace ZipLib.Zip;

public class FastZipEvents
{
	public ProcessDirectoryDelegate ProcessDirectory;

	public ProcessFileDelegate ProcessFile;

	public DirectoryFailureDelegate DirectoryFailure;

	public FileFailureDelegate FileFailure;

	public bool OnDirectoryFailure(string directory, Exception e)
	{
		bool result = false;
		if (DirectoryFailure != null)
		{
			ScanFailureEventArgs scanFailureEventArgs = new ScanFailureEventArgs(directory, e);
			DirectoryFailure(this, scanFailureEventArgs);
			result = scanFailureEventArgs.ContinueRunning;
		}
		return result;
	}

	public bool OnFileFailure(string file, Exception e)
	{
		bool result = false;
		if (FileFailure != null)
		{
			ScanFailureEventArgs scanFailureEventArgs = new ScanFailureEventArgs(file, e);
			FileFailure(this, scanFailureEventArgs);
			result = scanFailureEventArgs.ContinueRunning;
		}
		return result;
	}

	public bool OnProcessFile(string file)
	{
		bool result = true;
		if (ProcessFile != null)
		{
			ScanEventArgs scanEventArgs = new ScanEventArgs(file);
			ProcessFile(this, scanEventArgs);
			result = scanEventArgs.ContinueRunning;
		}
		return result;
	}

	public void OnProcessDirectory(string directory, bool hasMatchingFiles)
	{
		if (ProcessDirectory != null)
		{
			DirectoryEventArgs e = new DirectoryEventArgs(directory, hasMatchingFiles);
			ProcessDirectory(this, e);
		}
	}
}
