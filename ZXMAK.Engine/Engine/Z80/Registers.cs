using System;
using System.Runtime.InteropServices;

namespace ZXMAK.Engine.Z80
{
	[StructLayout(LayoutKind.Explicit)]
	public class Registers
	{
		internal void EXX()
		{
			ushort num = this.BC;
			this.BC = this._BC;
			this._BC = num;
			num = this.DE;
			this.DE = this._DE;
			this._DE = num;
			num = this.HL;
			this.HL = this._HL;
			this._HL = num;
		}

		internal void EXAF()
		{
			ushort af = this.AF;
			this.AF = this._AF;
			this._AF = af;
		}

		internal byte this[int index]
		{
			get
			{
				switch (index)
				{
				case 0:
					return this.B;
				case 1:
					return this.C;
				case 2:
					return this.D;
				case 3:
					return this.E;
				case 4:
					return this.H;
				case 5:
					return this.L;
				case 6:
					return this.F;
				case 7:
					return this.A;
				default:
					throw new Exception("RegistersZ80 indexer wrong!");
				}
			}
			set
			{
				switch (index)
				{
				case 0:
					this.B = value;
					return;
				case 1:
					this.C = value;
					return;
				case 2:
					this.D = value;
					return;
				case 3:
					this.E = value;
					return;
				case 4:
					this.H = value;
					return;
				case 5:
					this.L = value;
					return;
				case 6:
					this.F = value;
					return;
				case 7:
					this.A = value;
					return;
				default:
					throw new Exception("RegistersZ80 indexer wrong!");
				}
			}
		}

		internal void SetPair(int RR, ushort value)
		{
			switch (RR)
			{
			case 0:
				this.BC = value;
				return;
			case 1:
				this.DE = value;
				return;
			case 2:
				this.HL = value;
				return;
			case 3:
				this.SP = value;
				return;
			default:
				throw new Exception("RegistersZ80 SetPair index wrong!");
			}
		}

		internal ushort GetPair(int RR)
		{
			switch (RR)
			{
			case 0:
				return this.BC;
			case 1:
				return this.DE;
			case 2:
				return this.HL;
			case 3:
				return this.SP;
			default:
				throw new Exception("RegistersZ80 GetPair index wrong!");
			}
		}

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
	}
}
