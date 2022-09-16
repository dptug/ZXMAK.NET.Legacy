using System.IO;

namespace ZipLib.Zip;

public interface IArchiveStorage
{
	FileUpdateMode UpdateMode { get; }

	Stream GetTemporaryOutput();

	Stream ConvertTemporaryToFinal();

	Stream MakeTemporaryCopy(Stream stream);

	Stream OpenForDirectUpdate(Stream stream);

	void Dispose();
}
