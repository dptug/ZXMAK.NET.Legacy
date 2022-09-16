using ZipLib.Core;

namespace ZipLib.Zip;

public interface IEntryFactory
{
	INameTransform NameTransform { get; set; }

	ZipEntry MakeFileEntry(string fileName);

	ZipEntry MakeDirectoryEntry(string directoryName);
}
