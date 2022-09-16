using System.IO;
using ZXMAK.Engine.Z80;
using ZXMAK.Platform;

namespace ZXMAK.Engine.Loaders.SnapshotSerializers;

public class SnaSerializer : FormatSerializer
{
	protected Spectrum _spec;

	public override string FormatGroup => "Snapshots";

	public override string FormatName => "SNA snapshot";

	public override string FormatExtension => "SNA";

	public override bool CanDeserialize => true;

	public override bool CanSerialize => true;

	public SnaSerializer(Spectrum spec)
	{
		_spec = spec;
	}

	public override void Deserialize(Stream stream)
	{
		loadFromStream(stream);
	}

	public override void Serialize(Stream stream)
	{
		saveToStream(stream);
	}

	private void loadFromStream(Stream stream)
	{
		if (stream.Length != 49179 && stream.Length != 131103)
		{
			PlatformFactory.Platform.ShowWarning("Invalid SNA file size!", "SNA loader");
			return;
		}
		_spec.CPU.Tact += 1000uL;
		_spec.DoReset();
		if (_spec is ISpectrum128K)
		{
			((ISpectrum128K)_spec).Port7FFD = 48;
		}
		byte[] array = new byte[27];
		stream.Read(array, 0, 27);
		byte[] array2 = new byte[49152];
		stream.Read(array2, 0, 49152);
		_spec.SetRamImage(5, array2, 0, 16384);
		_spec.SetRamImage(2, array2, 16384, 16384);
		_spec.SetRamImage(0, array2, 32768, 16384);
		Registers registers = new Registers();
		registers.I = array[0];
		registers._HL = (ushort)(array[1] + 256 * array[2]);
		registers._DE = (ushort)(array[3] + 256 * array[4]);
		registers._BC = (ushort)(array[5] + 256 * array[6]);
		registers._AF = (ushort)(array[7] + 256 * array[8]);
		registers.HL = (ushort)(array[9] + 256 * array[10]);
		registers.DE = (ushort)(array[11] + 256 * array[12]);
		registers.BC = (ushort)(array[13] + 256 * array[14]);
		registers.IY = (ushort)(array[15] + 256 * array[16]);
		registers.IX = (ushort)(array[17] + 256 * array[18]);
		registers.R = array[20];
		registers.AF = (ushort)(array[21] + 256 * array[22]);
		registers.SP = (ushort)(array[23] + 256 * array[24]);
		_spec.CPU.regs = registers;
		_spec.CPU.IM = (byte)(array[25] & 3u);
		if (_spec.CPU.IM > 2)
		{
			_spec.CPU.IM = 2;
		}
		_spec.CPU.IFF2 = (array[19] & 4) == 4;
		_spec.CPU.IFF1 = _spec.CPU.IFF2;
		_spec.CPU.HALTED = false;
		if (_spec is ISpectrum)
		{
			((ISpectrum)_spec).PortFE = array[26];
		}
		ushort num = _spec.ReadMemory(_spec.CPU.regs.SP);
		num = (ushort)(num | (ushort)(_spec.ReadMemory((ushort)(_spec.CPU.regs.SP + 1)) << 8));
		if (stream.Length > 49179)
		{
			num = (ushort)stream.ReadByte();
			num = (ushort)(num | (ushort)(stream.ReadByte() << 8));
			_spec.CPU.regs.PC = num;
			byte b = (byte)stream.ReadByte();
			if (_spec is ISpectrum128K)
			{
				((ISpectrum128K)_spec).Port7FFD = b;
			}
			stream.ReadByte();
			if ((b & 7u) != 0)
			{
				_spec.SetRamImage(b & 7, _spec.GetRamImage(0), 0, 16384);
			}
			for (int i = 0; i < 8; i++)
			{
				if (i != 5 && i != 2 && i != (b & 7))
				{
					stream.Read(array2, 0, 16384);
					_spec.SetRamImage(i, array2, 0, 16384);
				}
			}
		}
		else
		{
			_spec.CPU.regs.SP++;
			_spec.CPU.regs.SP++;
			_spec.CPU.regs.PC = num;
		}
	}

	private void saveToStream(Stream stream)
	{
		byte[] array = new byte[27];
		ushort num = (ushort)(_spec.CPU.regs.SP - 2);
		array[0] = _spec.CPU.regs.I;
		array[1] = (byte)(_spec.CPU.regs._HL & 0xFFu);
		array[2] = (byte)(_spec.CPU.regs._HL >> 8);
		array[3] = (byte)(_spec.CPU.regs._DE & 0xFFu);
		array[4] = (byte)(_spec.CPU.regs._DE >> 8);
		array[5] = (byte)(_spec.CPU.regs._BC & 0xFFu);
		array[6] = (byte)(_spec.CPU.regs._BC >> 8);
		array[7] = (byte)(_spec.CPU.regs._AF & 0xFFu);
		array[8] = (byte)(_spec.CPU.regs._AF >> 8);
		array[9] = (byte)(_spec.CPU.regs.HL & 0xFFu);
		array[10] = (byte)(_spec.CPU.regs.HL >> 8);
		array[11] = (byte)(_spec.CPU.regs.DE & 0xFFu);
		array[12] = (byte)(_spec.CPU.regs.DE >> 8);
		array[13] = (byte)(_spec.CPU.regs.BC & 0xFFu);
		array[14] = (byte)(_spec.CPU.regs.BC >> 8);
		array[15] = (byte)(_spec.CPU.regs.IY & 0xFFu);
		array[16] = (byte)(_spec.CPU.regs.IY >> 8);
		array[17] = (byte)(_spec.CPU.regs.IX & 0xFFu);
		array[18] = (byte)(_spec.CPU.regs.IX >> 8);
		array[20] = _spec.CPU.regs.R;
		array[21] = (byte)(_spec.CPU.regs.AF & 0xFFu);
		array[22] = (byte)(_spec.CPU.regs.AF >> 8);
		array[23] = (byte)(num & 0xFFu);
		array[24] = (byte)(num >> 8);
		array[25] = (byte)(_spec.CPU.IM & 3u);
		array[19] = (byte)(_spec.CPU.IFF2 ? 4u : 0u);
		if (_spec is ISpectrum)
		{
			array[26] = ((ISpectrum)_spec).PortFE;
		}
		else
		{
			array[26] = 0;
		}
		stream.Write(array, 0, 27);
		byte b = 48;
		if (_spec is ISpectrum128K)
		{
			b = ((ISpectrum128K)_spec).Port7FFD;
		}
		byte b2 = 0;
		byte b3 = 0;
		if ((b & 0x20u) != 0)
		{
			b2 = _spec.ReadMemory(num);
			_spec.WriteMemory(num++, (byte)(_spec.CPU.regs.PC & 0xFFu));
			b3 = _spec.ReadMemory(num);
			_spec.WriteMemory(num++, (byte)(_spec.CPU.regs.PC >> 8));
			num = (ushort)(num - 2);
			stream.Write(_spec.GetRamImage(5), 0, 16384);
			stream.Write(_spec.GetRamImage(2), 0, 16384);
			stream.Write(_spec.GetRamImage(b & 7), 0, 16384);
			_spec.WriteMemory(num++, b2);
			_spec.WriteMemory(num++, b3);
			return;
		}
		stream.Write(_spec.GetRamImage(5), 0, 16384);
		stream.Write(_spec.GetRamImage(2), 0, 16384);
		stream.Write(_spec.GetRamImage(b & 7), 0, 16384);
		stream.WriteByte((byte)(_spec.CPU.regs.PC & 0xFFu));
		stream.WriteByte((byte)(_spec.CPU.regs.PC >> 8));
		stream.WriteByte(b);
		byte value = 0;
		stream.WriteByte(value);
		for (int i = 0; i < 8; i++)
		{
			if (i != 5 && i != 2 && i != (b & 7))
			{
				stream.Write(_spec.GetRamImage(i), 0, 16384);
			}
		}
	}
}
