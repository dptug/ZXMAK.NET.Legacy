using System;
using ZipLib.Core;

namespace ZipLib.Zip
{
	public interface IEntryFactory
	{
		ZipEntry MakeFileEntry(string fileName);

		ZipEntry MakeDirectoryEntry(string directoryName);

		INameTransform NameTransform { get; set; }
	}
}
