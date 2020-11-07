using System;
using System.IO;

namespace ZipLib.Core
{
	public class FileSystemScanner
	{
		public FileSystemScanner(string filter)
		{
			this.fileFilter_ = new PathFilter(filter);
		}

		public FileSystemScanner(string fileFilter, string directoryFilter)
		{
			this.fileFilter_ = new PathFilter(fileFilter);
			this.directoryFilter_ = new PathFilter(directoryFilter);
		}

		public FileSystemScanner(IScanFilter fileFilter)
		{
			this.fileFilter_ = fileFilter;
		}

		public FileSystemScanner(IScanFilter fileFilter, IScanFilter directoryFilter)
		{
			this.fileFilter_ = fileFilter;
			this.directoryFilter_ = directoryFilter;
		}

		public void OnDirectoryFailure(string directory, Exception e)
		{
			if (this.DirectoryFailure == null)
			{
				this.alive_ = false;
				return;
			}
			ScanFailureEventArgs scanFailureEventArgs = new ScanFailureEventArgs(directory, e);
			this.DirectoryFailure(this, scanFailureEventArgs);
			this.alive_ = scanFailureEventArgs.ContinueRunning;
		}

		public void OnFileFailure(string file, Exception e)
		{
			if (this.FileFailure == null)
			{
				this.alive_ = false;
				return;
			}
			ScanFailureEventArgs scanFailureEventArgs = new ScanFailureEventArgs(file, e);
			this.FileFailure(this, scanFailureEventArgs);
			this.alive_ = scanFailureEventArgs.ContinueRunning;
		}

		public void OnProcessFile(string file)
		{
			if (this.ProcessFile != null)
			{
				ScanEventArgs scanEventArgs = new ScanEventArgs(file);
				this.ProcessFile(this, scanEventArgs);
				this.alive_ = scanEventArgs.ContinueRunning;
			}
		}

		public void OnProcessDirectory(string directory, bool hasMatchingFiles)
		{
			if (this.ProcessDirectory != null)
			{
				DirectoryEventArgs directoryEventArgs = new DirectoryEventArgs(directory, hasMatchingFiles);
				this.ProcessDirectory(this, directoryEventArgs);
				this.alive_ = directoryEventArgs.ContinueRunning;
			}
		}

		public void Scan(string directory, bool recurse)
		{
			this.alive_ = true;
			this.ScanDir(directory, recurse);
		}

		private void ScanDir(string directory, bool recurse)
		{
			try
			{
				string[] files = Directory.GetFiles(directory);
				bool flag = false;
				for (int i = 0; i < files.Length; i++)
				{
					if (!this.fileFilter_.IsMatch(files[i]))
					{
						files[i] = null;
					}
					else
					{
						flag = true;
					}
				}
				this.OnProcessDirectory(directory, flag);
				if (this.alive_ && flag)
				{
					foreach (string text in files)
					{
						try
						{
							if (text != null)
							{
								this.OnProcessFile(text);
								if (!this.alive_)
								{
									break;
								}
							}
						}
						catch (Exception e)
						{
							this.OnFileFailure(text, e);
						}
					}
				}
			}
			catch (Exception e2)
			{
				this.OnDirectoryFailure(directory, e2);
			}
			if (this.alive_ && recurse)
			{
				try
				{
					string[] directories = Directory.GetDirectories(directory);
					foreach (string text2 in directories)
					{
						if (this.directoryFilter_ == null || this.directoryFilter_.IsMatch(text2))
						{
							this.ScanDir(text2, true);
							if (!this.alive_)
							{
								break;
							}
						}
					}
				}
				catch (Exception e3)
				{
					this.OnDirectoryFailure(directory, e3);
				}
			}
		}

		public ProcessDirectoryDelegate ProcessDirectory;

		public ProcessFileDelegate ProcessFile;

		public DirectoryFailureDelegate DirectoryFailure;

		public FileFailureDelegate FileFailure;

		private IScanFilter fileFilter_;

		private IScanFilter directoryFilter_;

		private bool alive_;
	}
}
