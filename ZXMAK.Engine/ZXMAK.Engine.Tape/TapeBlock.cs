using System.Collections.Generic;

namespace ZXMAK.Engine.Tape;

public class TapeBlock
{
	public string Description;

	public List<int> Periods = new List<int>();

	public TapeCommand Command;
}
