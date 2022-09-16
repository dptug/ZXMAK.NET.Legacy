using System.IO;

namespace ZipLib.Zip;

public interface IStaticDataSource
{
	Stream GetSource();
}
