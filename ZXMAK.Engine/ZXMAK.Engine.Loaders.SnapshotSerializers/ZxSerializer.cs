using System.IO;
using ZXMAK.Platform;

namespace ZXMAK.Engine.Loaders.SnapshotSerializers;

public class ZxSerializer : FormatSerializer
{
	protected Spectrum _spec;

	public override string FormatGroup => "Snapshots";

	public override string FormatName => "ZX snapshot";

	public override string FormatExtension => "ZX";

	public override bool CanDeserialize => true;

	public ZxSerializer(Spectrum spec)
	{
		_spec = spec;
	}

	public override void Deserialize(Stream stream)
	{
		loadFromStream(stream);
	}

	private void loadFromStream(Stream stream)
	{
		byte[] array = new byte[49284];
		byte[] array2 = new byte[202];
		if (stream.Length != 49486)
		{
			PlatformFactory.Platform.ShowWarning("Invalid file size, file corrupt!", "ZX loader");
			return;
		}
		stream.Read(array, 0, array.Length);
		stream.Read(array2, 0, array2.Length);
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
		_spec.SetRamImage(5, array, 132, 16384);
		_spec.SetRamImage(2, array, 16516, 16384);
		_spec.SetRamImage(0, array, 32900, 16384);
		_spec.CPU.regs._HL = (ushort)((array2[160] << 8) | array2[161]);
		_spec.CPU.regs._DE = (ushort)((array2[156] << 8) | array2[157]);
		_spec.CPU.regs._BC = (ushort)((array2[152] << 8) | array2[153]);
		_spec.CPU.regs._AF = (ushort)((array2[171] << 8) | array2[175]);
		_spec.CPU.regs.HL = (ushort)((array2[158] << 8) | array2[159]);
		_spec.CPU.regs.DE = (ushort)((array2[154] << 8) | array2[155]);
		_spec.CPU.regs.BC = (ushort)((array2[150] << 8) | array2[151]);
		_spec.CPU.regs.AF = (ushort)((array2[173] << 8) | array2[177]);
		_spec.CPU.regs.IR = (ushort)((array2[166] << 8) | array2[167]);
		_spec.CPU.regs.IX = (ushort)((array2[162] << 8) | array2[163]);
		_spec.CPU.regs.IY = (ushort)((array2[164] << 8) | array2[165]);
		_spec.CPU.regs.SP = (ushort)((array2[184] << 8) | array2[185]);
		_spec.CPU.regs.PC = (ushort)((array2[180] << 8) | array2[181]);
		switch (FormatSerializer.getUInt16(array2, 190))
		{
		case 0:
			_spec.CPU.IM = 1;
			break;
		case 1:
			_spec.CPU.IM = 2;
			break;
		default:
			_spec.CPU.IM = 0;
			break;
		}
		_spec.CPU.IFF1 = (array2[142] & 1) != 0;
		_spec.CPU.IFF2 = (array2[142] & 1) != 0;
		_spec.CPU.HALTED = (array2[189] & 1) != 0;
	}
}
