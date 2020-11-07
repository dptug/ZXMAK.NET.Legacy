using System;
using System.IO;
using ZXMAK.Engine.Z80;
using ZXMAK.Platform;

namespace ZXMAK.Engine.Loaders.SnapshotSerializers
{
	public class SnaSerializer : FormatSerializer
	{
		public SnaSerializer(Spectrum spec)
		{
			this._spec = spec;
		}

		public override string FormatGroup
		{
			get
			{
				return "Snapshots";
			}
		}

		public override string FormatName
		{
			get
			{
				return "SNA snapshot";
			}
		}

		public override string FormatExtension
		{
			get
			{
				return "SNA";
			}
		}

		public override bool CanDeserialize
		{
			get
			{
				return true;
			}
		}

		public override bool CanSerialize
		{
			get
			{
				return true;
			}
		}

		public override void Deserialize(Stream stream)
		{
			this.loadFromStream(stream);
		}

		public override void Serialize(Stream stream)
		{
			this.saveToStream(stream);
		}

		private void loadFromStream(Stream stream)
		{
			if (stream.Length != 49179L && stream.Length != 131103L)
			{
				PlatformFactory.Platform.ShowWarning("Invalid SNA file size!", "SNA loader");
				return;
			}
			this._spec.CPU.Tact += 1000UL;
			this._spec.DoReset();
			if (this._spec is ISpectrum128K)
			{
				((ISpectrum128K)this._spec).Port7FFD = 48;
			}
			byte[] array = new byte[27];
			stream.Read(array, 0, 27);
			byte[] array2 = new byte[49152];
			stream.Read(array2, 0, 49152);
			this._spec.SetRamImage(5, array2, 0, 16384);
			this._spec.SetRamImage(2, array2, 16384, 16384);
			this._spec.SetRamImage(0, array2, 32768, 16384);
			Registers registers = new Registers();
			registers.I = array[0];
			registers._HL = (ushort)((int)array[1] + 256 * (int)array[2]);
			registers._DE = (ushort)((int)array[3] + 256 * (int)array[4]);
			registers._BC = (ushort)((int)array[5] + 256 * (int)array[6]);
			registers._AF = (ushort)((int)array[7] + 256 * (int)array[8]);
			registers.HL = (ushort)((int)array[9] + 256 * (int)array[10]);
			registers.DE = (ushort)((int)array[11] + 256 * (int)array[12]);
			registers.BC = (ushort)((int)array[13] + 256 * (int)array[14]);
			registers.IY = (ushort)((int)array[15] + 256 * (int)array[16]);
			registers.IX = (ushort)((int)array[17] + 256 * (int)array[18]);
			registers.R = array[20];
			registers.AF = (ushort)((int)array[21] + 256 * (int)array[22]);
			registers.SP = (ushort)((int)array[23] + 256 * (int)array[24]);
			this._spec.CPU.regs = registers;
			this._spec.CPU.IM = (array[25] & 3);
			if (this._spec.CPU.IM > 2)
			{
				this._spec.CPU.IM = 2;
			}
			this._spec.CPU.IFF2 = ((array[19] & 4) == 4);
			this._spec.CPU.IFF1 = this._spec.CPU.IFF2;
			this._spec.CPU.HALTED = false;
			if (this._spec is ISpectrum)
			{
				((ISpectrum)this._spec).PortFE = array[26];
			}
			ushort num = (ushort)this._spec.ReadMemory(this._spec.CPU.regs.SP);
			num |= (ushort)(this._spec.ReadMemory(this._spec.CPU.regs.SP + 1) << 8);
			if (stream.Length > 49179L)
			{
				num = (ushort)stream.ReadByte();
				num |= (ushort)(stream.ReadByte() << 8);
				this._spec.CPU.regs.PC = num;
				byte b = (byte)stream.ReadByte();
				if (this._spec is ISpectrum128K)
				{
					((ISpectrum128K)this._spec).Port7FFD = b;
				}
				stream.ReadByte();
				if ((b & 7) != 0)
				{
					this._spec.SetRamImage((int)(b & 7), this._spec.GetRamImage(0), 0, 16384);
				}
				for (int i = 0; i < 8; i++)
				{
					if (i != 5 && i != 2 && i != (int)(b & 7))
					{
						stream.Read(array2, 0, 16384);
						this._spec.SetRamImage(i, array2, 0, 16384);
					}
				}
				return;
			}
			Registers regs = this._spec.CPU.regs;
			regs.SP += 1;
			Registers regs2 = this._spec.CPU.regs;
			regs2.SP += 1;
			this._spec.CPU.regs.PC = num;
		}

		private void saveToStream(Stream stream)
		{
			byte[] array = new byte[27];
			ushort num = this._spec.CPU.regs.SP - 2;
			array[0] = this._spec.CPU.regs.I;
			array[1] = (byte)(this._spec.CPU.regs._HL & 255);
			array[2] = (byte)(this._spec.CPU.regs._HL >> 8);
			array[3] = (byte)(this._spec.CPU.regs._DE & 255);
			array[4] = (byte)(this._spec.CPU.regs._DE >> 8);
			array[5] = (byte)(this._spec.CPU.regs._BC & 255);
			array[6] = (byte)(this._spec.CPU.regs._BC >> 8);
			array[7] = (byte)(this._spec.CPU.regs._AF & 255);
			array[8] = (byte)(this._spec.CPU.regs._AF >> 8);
			array[9] = (byte)(this._spec.CPU.regs.HL & 255);
			array[10] = (byte)(this._spec.CPU.regs.HL >> 8);
			array[11] = (byte)(this._spec.CPU.regs.DE & 255);
			array[12] = (byte)(this._spec.CPU.regs.DE >> 8);
			array[13] = (byte)(this._spec.CPU.regs.BC & 255);
			array[14] = (byte)(this._spec.CPU.regs.BC >> 8);
			array[15] = (byte)(this._spec.CPU.regs.IY & 255);
			array[16] = (byte)(this._spec.CPU.regs.IY >> 8);
			array[17] = (byte)(this._spec.CPU.regs.IX & 255);
			array[18] = (byte)(this._spec.CPU.regs.IX >> 8);
			array[20] = this._spec.CPU.regs.R;
			array[21] = (byte)(this._spec.CPU.regs.AF & 255);
			array[22] = (byte)(this._spec.CPU.regs.AF >> 8);
			array[23] = (byte)(num & 255);
			array[24] = (byte)(num >> 8);
			array[25] = (this._spec.CPU.IM & 3);
			array[19] = (this._spec.CPU.IFF2 ? 4 : 0);
			if (this._spec is ISpectrum)
			{
				array[26] = ((ISpectrum)this._spec).PortFE;
			}
			else
			{
				array[26] = 0;
			}
			stream.Write(array, 0, 27);
			byte b = 48;
			if (this._spec is ISpectrum128K)
			{
				b = ((ISpectrum128K)this._spec).Port7FFD;
			}
			if ((b & 32) != 0)
			{
				byte value = this._spec.ReadMemory(num);
				Spectrum spec = this._spec;
				ushort num2 = num;
				num = num2 + 1;
				spec.WriteMemory(num2, (byte)(this._spec.CPU.regs.PC & 255));
				byte value2 = this._spec.ReadMemory(num);
				Spectrum spec2 = this._spec;
				ushort num3 = num;
				num = num3 + 1;
				spec2.WriteMemory(num3, (byte)(this._spec.CPU.regs.PC >> 8));
				num -= 2;
				stream.Write(this._spec.GetRamImage(5), 0, 16384);
				stream.Write(this._spec.GetRamImage(2), 0, 16384);
				stream.Write(this._spec.GetRamImage((int)(b & 7)), 0, 16384);
				Spectrum spec3 = this._spec;
				ushort num4 = num;
				num = num4 + 1;
				spec3.WriteMemory(num4, value);
				Spectrum spec4 = this._spec;
				ushort num5 = num;
				num = num5 + 1;
				spec4.WriteMemory(num5, value2);
				return;
			}
			stream.Write(this._spec.GetRamImage(5), 0, 16384);
			stream.Write(this._spec.GetRamImage(2), 0, 16384);
			stream.Write(this._spec.GetRamImage((int)(b & 7)), 0, 16384);
			stream.WriteByte((byte)(this._spec.CPU.regs.PC & 255));
			stream.WriteByte((byte)(this._spec.CPU.regs.PC >> 8));
			stream.WriteByte(b);
			byte value3 = 0;
			stream.WriteByte(value3);
			for (int i = 0; i < 8; i++)
			{
				if (i != 5 && i != 2 && i != (int)(b & 7))
				{
					stream.Write(this._spec.GetRamImage(i), 0, 16384);
				}
			}
		}

		protected Spectrum _spec;
	}
}
