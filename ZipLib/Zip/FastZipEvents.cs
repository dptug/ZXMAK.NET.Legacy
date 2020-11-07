using System;
using ZipLib.Core;

namespace ZipLib.Zip
{
	public class FastZipEvents
	{
		public bool OnDirectoryFailure(string directory, Exception e)
		{
			bool result = false;
			if (this.DirectoryFailure != null)
			{
				ScanFailureEventArgs scanFailureEventArgs = new ScanFailureEventArgs(directory, e);
				this.DirectoryFailure(this, scanFailureEventArgs);
				result = scanFailureEventArgs.ContinueRunning;
			}
			return result;
		}

		public bool OnFileFailure(string file, Exception e)
		{
			bool result = false;
			if (this.FileFailure != null)
			{
				ScanFailureEventArgs scanFailureEventArgs = new ScanFailureEventArgs(file, e);
				this.FileFailure(this, scanFailureEventArgs);
				result = scanFailureEventArgs.ContinueRunning;
			}
			return result;
		}

		public bool OnProcessFile(string file)
		{
			bool result = true;
			if (this.ProcessFile != null)
			{
				ScanEventArgs scanEventArgs = new ScanEventArgs(file);
				this.ProcessFile(this, scanEventArgs);
				result = scanEventArgs.ContinueRunning;
			}
			return result;
		}

		public void OnProcessDirectory(string directory, bool hasMatchingFiles)
		{
			if (this.ProcessDirectory != null)
			{
				DirectoryEventArgs e = new DirectoryEventArgs(directory, hasMatchingFiles);
				this.ProcessDirectory(this, e);
			}
		}

		public ProcessDirectoryDelegate ProcessDirectory;

		public ProcessFileDelegate ProcessFile;

		public DirectoryFailureDelegate DirectoryFailure;

		public FileFailureDelegate FileFailure;
	}
}
