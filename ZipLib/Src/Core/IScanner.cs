using System;

namespace ZipLib.Src.Core
{
	public interface IScanner
	{
		void Scan();

		event MatchHandler Match;
	}
}
