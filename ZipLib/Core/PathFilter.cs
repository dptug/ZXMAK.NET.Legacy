using System;
using System.IO;

namespace ZipLib.Core
{
	public class PathFilter : IScanFilter
	{
		public PathFilter(string filter)
		{
			this.nameFilter_ = new NameFilter(filter);
		}

		public virtual bool IsMatch(string name)
		{
			return this.nameFilter_.IsMatch(Path.GetFullPath(name));
		}

		private NameFilter nameFilter_;
	}
}
