using System.IO;

namespace ZipLib.Core;

public class PathFilter : IScanFilter
{
	private NameFilter nameFilter_;

	public PathFilter(string filter)
	{
		nameFilter_ = new NameFilter(filter);
	}

	public virtual bool IsMatch(string name)
	{
		return nameFilter_.IsMatch(Path.GetFullPath(name));
	}
}
