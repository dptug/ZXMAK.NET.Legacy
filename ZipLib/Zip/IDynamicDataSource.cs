using System;
using System.IO;

namespace ZipLib.Zip
{
	public interface IDynamicDataSource
	{
		Stream GetSource(ZipEntry entry, string name);
	}
}
