using System;

namespace ZXMAK.Engine.Tape
{
	public enum TapeCommand
	{
		NONE,
		STOP_THE_TAPE,
		STOP_THE_TAPE_48K,
		BEGIN_GROUP,
		END_GROUP,
		SHOW_MESSAGE
	}
}
