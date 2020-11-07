using System;
using System.IO;

namespace ZipLib.Zip
{
	internal class DynamicDiskDataSource : IDynamicDataSource
	{
		public Stream GetSource(ZipEntry entry, string name)
		{
			Stream result = null;
			if (name != null)
			{
				result = File.OpenRead(name);
			}
			return result;
		}
	}
}
