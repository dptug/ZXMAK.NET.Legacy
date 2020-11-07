using System;
using System.IO;

namespace ZipLib.Zip
{
	internal class StaticDiskDataSource : IStaticDataSource
	{
		public StaticDiskDataSource(string fileName)
		{
			this.fileName_ = fileName;
		}

		public Stream GetSource()
		{
			return File.OpenRead(this.fileName_);
		}

		private string fileName_;
	}
}
