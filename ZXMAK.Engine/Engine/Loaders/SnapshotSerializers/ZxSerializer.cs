using System;
using System.IO;
using ZXMAK.Platform;

namespace ZXMAK.Engine.Loaders.SnapshotSerializers
{
	public class ZxSerializer : FormatSerializer
	{
		public ZxSerializer(Spectrum spec)
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
				return "ZX snapshot";
			}
		}

		public override string FormatExtension
		{
			get
			{
				return "ZX";
			}
		}

		public override bool CanDeserialize
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

		private void loadFromStream(Stream stream)
		{
			byte[] array = new byte[49284];
			byte[] array2 = new byte[202];
			if (stream.Length != 49486L)
			{
				PlatformFactory.Platform.ShowWarning("Invalid file size, file corrupt!", "ZX loader");
				return;
			}
			stream.Read(array, 0, array.Length);
			stream.Read(array2, 0, array2.Length);
			this._spec.CPU.Tact += 1000UL;
			this._spec.DoReset();
			if (this._spec is IBetaDiskDevice)
			{
				((IBetaDiskDevice)this._spec).SEL_TRDOS = false;
			}
			if (this._spec is ISpectrum128K)
			{
				((ISpectrum128K)this._spec).Port7FFD = 48;
			}
			this._spec.SetRamImage(5, array, 132, 16384);
			this._spec.SetRamImage(2, array, 16516, 16384);
			this._spec.SetRamImage(0, array, 32900, 16384);
			this._spec.CPU.regs._HL = (ushort)((int)array2[160] << 8 | (int)array2[161]);
			this._spec.CPU.regs._DE = (ushort)((int)array2[156] << 8 | (int)array2[157]);
			this._spec.CPU.regs._BC = (ushort)((int)array2[152] << 8 | (int)array2[153]);
			this._spec.CPU.regs._AF = (ushort)((int)array2[171] << 8 | (int)array2[175]);
			this._spec.CPU.regs.HL = (ushort)((int)array2[158] << 8 | (int)array2[159]);
			this._spec.CPU.regs.DE = (ushort)((int)array2[154] << 8 | (int)array2[155]);
			this._spec.CPU.regs.BC = (ushort)((int)array2[150] << 8 | (int)array2[151]);
			this._spec.CPU.regs.AF = (ushort)((int)array2[173] << 8 | (int)array2[177]);
			this._spec.CPU.regs.IR = (ushort)((int)array2[166] << 8 | (int)array2[167]);
			this._spec.CPU.regs.IX = (ushort)((int)array2[162] << 8 | (int)array2[163]);
			this._spec.CPU.regs.IY = (ushort)((int)array2[164] << 8 | (int)array2[165]);
			this._spec.CPU.regs.SP = (ushort)((int)array2[184] << 8 | (int)array2[185]);
			this._spec.CPU.regs.PC = (ushort)((int)array2[180] << 8 | (int)array2[181]);
			switch (FormatSerializer.getUInt16(array2, 190))
			{
			case 0:
				this._spec.CPU.IM = 1;
				break;
			case 1:
				this._spec.CPU.IM = 2;
				break;
			default:
				this._spec.CPU.IM = 0;
				break;
			}
			this._spec.CPU.IFF1 = ((array2[142] & 1) != 0);
			this._spec.CPU.IFF2 = ((array2[142] & 1) != 0);
			this._spec.CPU.HALTED = ((array2[189] & 1) != 0);
		}

		protected Spectrum _spec;
	}
}
