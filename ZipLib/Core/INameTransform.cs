using System;

namespace ZipLib.Core
{
	public interface INameTransform
	{
		string TransformFile(string name);

		string TransformDirectory(string name);
	}
}
