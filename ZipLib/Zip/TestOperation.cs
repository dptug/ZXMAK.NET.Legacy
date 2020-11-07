using System;

namespace ZipLib.Zip
{
	public enum TestOperation
	{
		Initialising,
		EntryHeader,
		EntryData,
		EntryComplete,
		MiscellaneousTests,
		Complete
	}
}
