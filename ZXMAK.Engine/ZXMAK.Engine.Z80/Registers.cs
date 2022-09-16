using System;
using System.Runtime.InteropServices;

namespace ZXMAK.Engine.Z80;

[StructLayout(LayoutKind.Explicit)]
public class Registers
{
	public const int ZR_BC = 0;

	public const int ZR_DE = 1;

	public const int ZR_HL = 2;

	public const int ZR_SP = 3;

	public const int ZR_B = 0;

	public const int ZR_C = 1;

	public const int ZR_D = 2;

	public const int ZR_E = 3;

	public const int ZR_H = 4;

	public const int ZR_L = 5;

	public const int ZR_F = 6;

	public const int ZR_A = 7;

	[FieldOffset(0)]
	public ushort AF;

	[FieldOffset(2)]
	public ushort BC;

	[FieldOffset(4)]
	public ushort DE;

	[FieldOffset(6)]
	public ushort HL;

	[FieldOffset(8)]
	public ushort _AF;

	[FieldOffset(10)]
	public ushort _BC;

	[FieldOffset(12)]
	public ushort _DE;

	[FieldOffset(14)]
	public ushort _HL;

	[FieldOffset(16)]
	public ushort IX;

	[FieldOffset(18)]
	public ushort IY;

	[FieldOffset(20)]
	public ushort IR;

	[FieldOffset(22)]
	public ushort PC;

	[FieldOffset(24)]
	public ushort SP;

	[FieldOffset(26)]
	public ushort MW;

	[FieldOffset(1)]
	public byte A;

	[FieldOffset(0)]
	public byte F;

	[FieldOffset(3)]
	public byte B;

	[FieldOffset(2)]
	public byte C;

	[FieldOffset(5)]
	public byte D;

	[FieldOffset(4)]
	public byte E;

	[FieldOffset(7)]
	public byte H;

	[FieldOffset(6)]
	public byte L;

	[FieldOffset(17)]
	public byte XH;

	[FieldOffset(16)]
	public byte XL;

	[FieldOffset(19)]
	public byte YH;

	[FieldOffset(18)]
	public byte YL;

	[FieldOffset(21)]
	public byte I;

	[FieldOffset(20)]
	public byte R;

	[FieldOffset(27)]
	public byte MH;

	[FieldOffset(26)]
	public byte ML;

	internal byte this[int index]
	{
		get
		{
			return index switch
			{
				0 => B, 
				1 => C, 
				2 => D, 
				3 => E, 
				4 => H, 
				5 => L, 
				7 => A, 
				6 => F, 
				_ => throw new Exception("RegistersZ80 indexer wrong!"), 
			};
		}
		set
		{
			switch (index)
			{
			case 0:
				B = value;
				break;
			case 1:
				C = value;
				break;
			case 2:
				D = value;
				break;
			case 3:
				E = value;
				break;
			case 4:
				H = value;
				break;
			case 5:
				L = value;
				break;
			case 7:
				A = value;
				break;
			case 6:
				F = value;
				break;
			default:
				throw new Exception("RegistersZ80 indexer wrong!");
			}
		}
	}

	internal void EXX()
	{
		ushort bC = BC;
		BC = _BC;
		_BC = bC;
		bC = DE;
		DE = _DE;
		_DE = bC;
		bC = HL;
		HL = _HL;
		_HL = bC;
	}

	internal void EXAF()
	{
		ushort aF = AF;
		AF = _AF;
		_AF = aF;
	}

	internal void SetPair(int RR, ushort value)
	{
		switch (RR)
		{
		case 0:
			BC = value;
			break;
		case 1:
			DE = value;
			break;
		case 2:
			HL = value;
			break;
		case 3:
			SP = value;
			break;
		default:
			throw new Exception("RegistersZ80 SetPair index wrong!");
		}
	}

	internal ushort GetPair(int RR)
	{
		return RR switch
		{
			0 => BC, 
			1 => DE, 
			2 => HL, 
			3 => SP, 
			_ => throw new Exception("RegistersZ80 GetPair index wrong!"), 
		};
	}
}
