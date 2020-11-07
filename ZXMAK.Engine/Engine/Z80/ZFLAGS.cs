using System;

namespace ZXMAK.Engine.Z80
{
	[Flags]
	public enum ZFLAGS : byte
	{
		S = 128,
		Z = 64,
		F5 = 32,
		H = 16,
		F3 = 8,
		PV = 4,
		N = 2,
		C = 1
	}
}
