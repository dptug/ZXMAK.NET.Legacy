using System;

namespace ZXMAK.Engine.Z80;

[Flags]
public enum ZFLAGS : byte
{
	S = 0x80,
	Z = 0x40,
	F5 = 0x20,
	H = 0x10,
	F3 = 8,
	PV = 4,
	N = 2,
	C = 1
}
