using System;
using System.IO;

namespace ZipLib.Core;

public class FileSystemScanner
{
	public ProcessDirectoryDelegate ProcessDirectory;

	public ProcessFileDelegate ProcessFile;

	public DirectoryFailureDelegate DirectoryFailure;

	public FileFailureDelegate FileFailure;

	private IScanFilter fileFilter_;

	private IScanFilter directoryFilter_;

	private bool alive_;

	public FileSystemScanner(string filter)
	{
		fileFilter_ = new PathFilter(filter);
	}

	public FileSystemScanner(string fileFilter, string directoryFilter)
	{
		fileFilter_ = new PathFilter(fileFilter);
		directoryFilter_ = new PathFilter(directoryFilter);
	}

	public FileSystemScanner(IScanFilter fileFilter)
	{
		fileFilter_ = fileFilter;
	}

	public FileSystemScanner(IScanFilter fileFilter, IScanFilter directoryFilter)
	{
		fileFilter_ = fileFilter;
		directoryFilter_ = directoryFilter;
	}

	public void OnDirectoryFailure(string directory, Exception e)
	{
		if (DirectoryFailure == null)
		{
			alive_ = false;
			return;
		}
		ScanFailureEventArgs scanFailureEventArgs = new ScanFailureEventArgs(directory, e);
		DirectoryFailure(this, scanFailureEventArgs);
		alive_ = scanFailureEventArgs.ContinueRunning;
	}

	public void OnFileFailure(string file, Exception e)
	{
		if (FileFailure == null)
		{
			alive_ = false;
			return;
		}
		ScanFailureEventArgs scanFailureEventArgs = new ScanFailureEventArgs(file, e);
		FileFailure(this, scanFailureEventArgs);
		alive_ = scanFailureEventArgs.ContinueRunning;
	}

	public void OnProcessFile(string file)
	{
		if (ProcessFile != null)
		{
			ScanEventArgs scanEventArgs = new ScanEventArgs(file);
			ProcessFile(this, scanEventArgs);
			alive_ = scanEventArgs.ContinueRunning;
		}
	}

	public void OnProcessDirectory(string directory, bool hasMatchingFiles)
	{
		if (ProcessDirectory != null)
		{
			DirectoryEventArgs directoryEventArgs = new DirectoryEventArgs(directory, hasMatchingFiles);
			ProcessDirectory(this, directoryEventArgs);
			alive_ = directoryEventArgs.ContinueRunning;
		}
	}

	public void Scan(string directory, bool recurse)
	{
		alive_ = true;
		ScanDir(directory, recurse);
	}

	private void ScanDir(string directory, bool recurse)
	{
		try
		{
			string[] files = Directory.GetFiles(directory);
			bool flag = false;
			for (int i = 0; i < files.Length; i++)
			{
				if (!fileFilter_.IsMatch(files[i]))
				{
					files[i] = null;
				}
				else
				{
					flag = true;
				}
			}
			OnProcessDirectory(directory, flag);
			if (alive_ && flag)
			{
				string[] array = files;
				foreach (string text in array)
				{
					try
					{
						if (text != null)
						{
							OnProcessFile(text);
							if (!alive_)
							{
								break;
							}
						}
					}
					catch (Exception e)
					{
						OnFileFailure(text, e);
					}
				}
			}
		}
		catch (Exception e2)
		{
			OnDirectoryFailure(directory, e2);
		}
		if (!alive_ || !recurse)
		{
			return;
		}
		try
		{
			string[] directories = Directory.GetDirectories(directory);
			string[] array2 = directories;
			foreach (string text2 in array2)
			{
				if (directoryFilter_ == null || directoryFilter_.IsMatch(text2))
				{
					ScanDir(text2, recurse: true);
					if (!alive_)
					{
						break;
					}
				}
			}
		}
		catch (Exception e3)
		{
			OnDirectoryFailure(directory, e3);
		}
	}
}
