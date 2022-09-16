using System.IO;
using ZXMAK.Platform;

namespace ZXMAK.Engine.Loaders.SnapshotSerializers;

public class SitSerializer : FormatSerializer
{
	protected Spectrum _spec;

	public override string FormatGroup => "Snapshots";

	public override string FormatName => "SIT snapshot";

	public override string FormatExtension => "SIT";

	public override bool CanDeserialize => true;

	public SitSerializer(Spectrum spec)
	{
		_spec = spec;
	}

	public override void Deserialize(Stream stream)
	{
		loadFromStream(stream);
	}

	private void loadFromStream(Stream stream)
	{
		byte[] array = new byte[28];
		byte[] array2 = new byte[16384];
		byte[] array3 = new byte[49152];
		if (stream.Length != 65564)
		{
			PlatformFactory.Platform.ShowWarning("Invalid data, file corrupt!", "SIT loader");
			return;
		}
		stream.Read(array, 0, array.Length);
		stream.Read(array2, 0, 16384);
		stream.Read(array3, 0, 49152);
		_spec.CPU.Tact += 1000uL;
		_spec.DoReset();
		if (_spec is IBetaDiskDevice)
		{
			((IBetaDiskDevice)_spec).SEL_TRDOS = false;
		}
		if (_spec is ISpectrum128K)
		{
			((ISpectrum128K)_spec).Port7FFD = 48;
		}
		_spec.SetRomImage(RomName.ROM_48, array2, 0, 16384);
		_spec.SetRamImage(5, array3, 0, 16384);
		_spec.SetRamImage(2, array3, 16384, 16384);
		_spec.SetRamImage(0, array3, 32768, 16384);
		_spec.CPU.regs.BC = FormatSerializer.getUInt16(array, 0);
		_spec.CPU.regs.DE = FormatSerializer.getUInt16(array, 2);
		_spec.CPU.regs.HL = FormatSerializer.getUInt16(array, 4);
		_spec.CPU.regs.AF = FormatSerializer.getUInt16(array, 6);
		_spec.CPU.regs.IX = FormatSerializer.getUInt16(array, 8);
		_spec.CPU.regs.IY = FormatSerializer.getUInt16(array, 10);
		_spec.CPU.regs.SP = FormatSerializer.getUInt16(array, 12);
		_spec.CPU.regs.PC = FormatSerializer.getUInt16(array, 14);
		_spec.CPU.regs.IR = FormatSerializer.getUInt16(array, 16);
		_spec.CPU.regs._BC = FormatSerializer.getUInt16(array, 18);
		_spec.CPU.regs._DE = FormatSerializer.getUInt16(array, 20);
		_spec.CPU.regs._HL = FormatSerializer.getUInt16(array, 22);
		_spec.CPU.regs._AF = FormatSerializer.getUInt16(array, 24);
		_spec.CPU.IM = (byte)(array[26] & 2u);
		if (_spec.CPU.IM == 0)
		{
			_spec.CPU.IM = 1;
		}
		_spec.CPU.IFF2 = (array[26] & 1) != 0;
		_spec.CPU.IFF2 = _spec.CPU.IFF1;
		if (_spec is ISpectrum)
		{
			((ISpectrum)_spec).PortFE = array[27];
		}
		_spec.CPU.HALTED = false;
	}
}
