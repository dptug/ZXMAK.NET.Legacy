using System;
using System.IO;
using ZXMAK.Platform;

namespace ZXMAK.Engine.Loaders.SnapshotSerializers
{
	public class SitSerializer : FormatSerializer
	{
		public SitSerializer(Spectrum spec)
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
				return "SIT snapshot";
			}
		}

		public override string FormatExtension
		{
			get
			{
				return "SIT";
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
			byte[] array = new byte[28];
			byte[] array2 = new byte[16384];
			byte[] array3 = new byte[49152];
			if (stream.Length != 65564L)
			{
				PlatformFactory.Platform.ShowWarning("Invalid data, file corrupt!", "SIT loader");
				return;
			}
			stream.Read(array, 0, array.Length);
			stream.Read(array2, 0, 16384);
			stream.Read(array3, 0, 49152);
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
			this._spec.SetRomImage(RomName.ROM_48, array2, 0, 16384);
			this._spec.SetRamImage(5, array3, 0, 16384);
			this._spec.SetRamImage(2, array3, 16384, 16384);
			this._spec.SetRamImage(0, array3, 32768, 16384);
			this._spec.CPU.regs.BC = FormatSerializer.getUInt16(array, 0);
			this._spec.CPU.regs.DE = FormatSerializer.getUInt16(array, 2);
			this._spec.CPU.regs.HL = FormatSerializer.getUInt16(array, 4);
			this._spec.CPU.regs.AF = FormatSerializer.getUInt16(array, 6);
			this._spec.CPU.regs.IX = FormatSerializer.getUInt16(array, 8);
			this._spec.CPU.regs.IY = FormatSerializer.getUInt16(array, 10);
			this._spec.CPU.regs.SP = FormatSerializer.getUInt16(array, 12);
			this._spec.CPU.regs.PC = FormatSerializer.getUInt16(array, 14);
			this._spec.CPU.regs.IR = FormatSerializer.getUInt16(array, 16);
			this._spec.CPU.regs._BC = FormatSerializer.getUInt16(array, 18);
			this._spec.CPU.regs._DE = FormatSerializer.getUInt16(array, 20);
			this._spec.CPU.regs._HL = FormatSerializer.getUInt16(array, 22);
			this._spec.CPU.regs._AF = FormatSerializer.getUInt16(array, 24);
			this._spec.CPU.IM = (array[26] & 2);
			if (this._spec.CPU.IM == 0)
			{
				this._spec.CPU.IM = 1;
			}
			this._spec.CPU.IFF2 = ((array[26] & 1) != 0);
			this._spec.CPU.IFF2 = this._spec.CPU.IFF1;
			if (this._spec is ISpectrum)
			{
				((ISpectrum)this._spec).PortFE = array[27];
			}
			this._spec.CPU.HALTED = false;
		}

		protected Spectrum _spec;
	}
}
