namespace ZipLib.Core;

public interface IScanFilter
{
	bool IsMatch(string name);
}
