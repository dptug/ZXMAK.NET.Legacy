namespace ZipLib.Src.Core;

public interface IScanner
{
	event MatchHandler Match;

	void Scan();
}
