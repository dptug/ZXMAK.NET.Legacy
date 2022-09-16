using System.IO;
using ZXMAK.Platform;

namespace ZXMAK.Engine.Loaders.SnapshotSerializers;

public class Z80Serializer : FormatSerializer
{
	private const int Z80HDR_A = 0;

	private const int Z80HDR_F = 1;

	private const int Z80HDR_BC = 2;

	private const int Z80HDR_HL = 4;

	private const int Z80HDR_PC = 6;

	private const int Z80HDR_SP = 8;

	private const int Z80HDR_I = 10;

	private const int Z80HDR_R7F = 11;

	private const int Z80HDR_FLAGS = 12;

	private const int Z80HDR_DE = 13;

	private const int Z80HDR_BC_ = 15;

	private const int Z80HDR_DE_ = 17;

	private const int Z80HDR_HL_ = 19;

	private const int Z80HDR_A_ = 21;

	private const int Z80HDR_F_ = 22;

	private const int Z80HDR_IY = 23;

	private const int Z80HDR_IX = 25;

	private const int Z80HDR_IFF1 = 27;

	private const int Z80HDR_IFF2 = 28;

	private const int Z80HDR_CONFIG = 29;

	private const int Z80HDR1_EXTSIZE = 0;

	private const int Z80HDR1_PC = 2;

	private const int Z80HDR1_HWMODE = 4;

	private const int Z80HDR1_SR7FFD = 5;

	private const int Z80HDR1_IF1PAGED = 6;

	private const int Z80HDR1_STUFF = 7;

	private const int Z80HDR1_7FFD = 8;

	private const int Z80HDR1_AYSTATE = 9;

	protected Spectrum _spec;

	public override string FormatGroup => "Snapshots";

	public override string FormatName => "Z80 snapshot";

	public override string FormatExtension => "Z80";

	public override bool CanDeserialize => true;

	public override bool CanSerialize => true;

	public Z80Serializer(Spectrum spec)
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
		byte[] array = new byte[30];
		byte[] array2 = new byte[25];
		stream.Read(array, 0, 30);
		if (array[12] == byte.MaxValue)
		{
			array[12] = 1;
		}
		int num = 1;
		if (FormatSerializer.getUInt16(array, 6) == 0)
		{
			num = 2;
			stream.Read(array2, 0, 25);
			if (array2[0] == 54)
			{
				num = 3;
				byte[] buffer = new byte[31];
				stream.Read(buffer, 0, 31);
			}
			else if (array2[0] != 23)
			{
				PlatformFactory.Platform.ShowWarning("Z80 format version not recognized!\n(ExtensionSize = " + array2[0] + ",\nsupported only ExtensionSize={0(old format), 23, 54})", "Z80 loader");
				return;
			}
		}
		_spec.CPU.Tact += 1000uL;
		_spec.DoReset();
		if (_spec is IBetaDiskDevice)
		{
			((IBetaDiskDevice)_spec).SEL_TRDOS = false;
		}
		_spec.CPU.regs.A = array[0];
		_spec.CPU.regs.F = array[1];
		_spec.CPU.regs.HL = FormatSerializer.getUInt16(array, 4);
		_spec.CPU.regs.DE = FormatSerializer.getUInt16(array, 13);
		_spec.CPU.regs.BC = FormatSerializer.getUInt16(array, 2);
		_spec.CPU.regs._AF = (ushort)(array[21] * 256 + array[22]);
		_spec.CPU.regs._HL = FormatSerializer.getUInt16(array, 19);
		_spec.CPU.regs._DE = FormatSerializer.getUInt16(array, 17);
		_spec.CPU.regs._BC = FormatSerializer.getUInt16(array, 15);
		_spec.CPU.regs.IX = FormatSerializer.getUInt16(array, 25);
		_spec.CPU.regs.IY = FormatSerializer.getUInt16(array, 23);
		_spec.CPU.regs.SP = FormatSerializer.getUInt16(array, 8);
		_spec.CPU.regs.I = array[10];
		_spec.CPU.regs.R = (byte)(array[11] | ((((uint)array[12] & (true ? 1u : 0u)) != 0) ? 128u : 0u));
		if (num == 1)
		{
			_spec.CPU.regs.PC = FormatSerializer.getUInt16(array, 6);
		}
		else
		{
			_spec.CPU.regs.PC = FormatSerializer.getUInt16(array2, 2);
		}
		_spec.CPU.IFF1 = array[27] != 0;
		_spec.CPU.IFF2 = array[28] != 0;
		_spec.CPU.IM = (byte)(array[29] & 3u);
		_spec.CPU.HALTED = false;
		if (_spec is ISpectrum)
		{
			((ISpectrum)_spec).PortFE = (byte)((uint)(array[12] >> 1) & 7u);
		}
		if (num > 1)
		{
			if (array2[6] == byte.MaxValue)
			{
				PlatformFactory.Platform.ShowWarning("Interface I not implemented, but Interface I ROM required!", "Z80 loader");
			}
			if (_spec is IAyDevice ayDevice)
			{
				for (int i = 0; i < 16; i++)
				{
					ayDevice.Sound.ADDR_REG = (byte)i;
					ayDevice.Sound.DATA_REG = array2[9 + i];
				}
			}
		}
		bool flag = (array[12] & 0x20) != 0;
		bool flag2 = false;
		if (num == 2)
		{
			switch (array2[4])
			{
			case 2:
				PlatformFactory.Platform.ShowWarning("SamRam not implemented!", "Z80 loader");
				break;
			case 3:
			case 4:
			case 9:
			case 10:
				flag2 = true;
				break;
			default:
				PlatformFactory.Platform.ShowWarning("Unrecognized ZX Spectrum config (" + array2[4] + ")!", "Z80 loader");
				break;
			case 0:
			case 1:
				break;
			}
		}
		if (num == 3)
		{
			switch (array2[4])
			{
			case 0:
			case 1:
			case 2:
				PlatformFactory.Platform.ShowWarning("SamRam not implemented!", "Z80 loader");
				break;
			case 4:
			case 5:
			case 6:
			case 9:
			case 10:
				flag2 = true;
				break;
			default:
				PlatformFactory.Platform.ShowWarning("Unrecognized ZX Spectrum config (" + array2[4] + ")!", "Z80 loader");
				break;
			case 3:
				break;
			}
		}
		byte b = 48;
		b = (byte)((num == 1) ? 48 : ((!flag2) ? 48 : array2[5]));
		if (_spec is ISpectrum128K)
		{
			((ISpectrum128K)_spec).Port7FFD = b;
		}
		if (num == 1)
		{
			int num2 = (int)(stream.Length - stream.Position);
			if (num2 < 0)
			{
				num2 = 0;
			}
			byte[] array3 = new byte[num2 + 1024];
			stream.Read(array3, 0, num2);
			byte[] array4 = new byte[131071];
			if (flag)
			{
				DecompressZ80(array4, array3, 49152);
			}
			else
			{
				for (int j = 0; j < 49152; j++)
				{
					array4[j] = array3[j];
				}
			}
			int page = b & 7;
			_spec.SetRamImage(5, array4, 0, 16384);
			_spec.SetRamImage(2, array4, 16384, 16384);
			_spec.SetRamImage(page, array4, 32768, 16384);
			return;
		}
		byte[] array5 = new byte[4];
		int num3 = 0;
		int num4 = 0;
		byte[] array6 = new byte[129000];
		byte[] array7 = new byte[16384];
		while (stream.Position < stream.Length)
		{
			stream.Read(array5, 0, 2);
			num3 = FormatSerializer.getUInt16(array5, 0);
			stream.Read(array5, 0, 1);
			num4 = array5[0];
			stream.Read(array6, 0, num3);
			DecompressZ80(array7, array6, 16384);
			if (num4 >= 3 && num4 <= 10 && flag2)
			{
				_spec.SetRamImage((num4 - 3) & 7, array7, 0, 16384);
				continue;
			}
			if ((num4 == 4 || num4 == 5 || num4 == 8) && !flag2)
			{
				int page2 = b & 7;
				if (num4 == 8)
				{
					page2 = 5;
				}
				if (num4 == 4)
				{
					page2 = 2;
				}
				_spec.SetRamImage(page2, array7, 0, 16384);
				continue;
			}
			switch (num4)
			{
			case 0:
				_spec.SetRomImage(RomName.ROM_48, array7, 0, 16384);
				PlatformFactory.Platform.ShowWarning("ROM 48K loaded from snapshot!", "Z80 loader");
				break;
			case 2:
				_spec.SetRomImage(RomName.ROM_128, array7, 0, 16384);
				PlatformFactory.Platform.ShowWarning("ROM 128K loaded from snapshot!", "Z80 loader");
				break;
			}
		}
	}

	private void saveToStream(Stream stream)
	{
		byte[] array = new byte[30];
		byte[] array2 = new byte[25];
		FormatSerializer.setUint16(array, 6, 0);
		FormatSerializer.setUint16(array2, 0, 23);
		array[0] = _spec.CPU.regs.A;
		array[1] = _spec.CPU.regs.F;
		FormatSerializer.setUint16(array, 4, _spec.CPU.regs.HL);
		FormatSerializer.setUint16(array, 13, _spec.CPU.regs.DE);
		FormatSerializer.setUint16(array, 2, _spec.CPU.regs.BC);
		array[21] = (byte)(_spec.CPU.regs._AF >> 8);
		array[22] = (byte)_spec.CPU.regs._AF;
		FormatSerializer.setUint16(array, 19, _spec.CPU.regs._HL);
		FormatSerializer.setUint16(array, 17, _spec.CPU.regs._DE);
		FormatSerializer.setUint16(array, 15, _spec.CPU.regs._BC);
		FormatSerializer.setUint16(array, 25, _spec.CPU.regs.IX);
		FormatSerializer.setUint16(array, 23, _spec.CPU.regs.IY);
		FormatSerializer.setUint16(array, 8, _spec.CPU.regs.SP);
		array[10] = _spec.CPU.regs.I;
		array[11] = (byte)(_spec.CPU.regs.R & 0x7Fu);
		if ((_spec.CPU.regs.R & 0x80u) != 0)
		{
			array[12] |= 1;
		}
		byte b = byte.MaxValue;
		if (_spec is ISpectrum)
		{
			b = ((ISpectrum)_spec).PortFE;
		}
		array[12] = (byte)((b & 7) << 1);
		array[12] |= 32;
		FormatSerializer.setUint16(array2, 2, _spec.CPU.regs.PC);
		if (_spec.CPU.IFF1)
		{
			array[27] = byte.MaxValue;
		}
		else
		{
			array[27] = 0;
		}
		if (_spec.CPU.IFF2)
		{
			array[28] = byte.MaxValue;
		}
		else
		{
			array[28] = 0;
		}
		array[29] = _spec.CPU.IM;
		if (_spec.CPU.IM > 2)
		{
			array[29] = 0;
		}
		byte b2 = 48;
		if (_spec is ISpectrum128K)
		{
			b2 = ((ISpectrum128K)_spec).Port7FFD;
		}
		bool flag = (b2 & 0x30) != 48;
		if (!flag)
		{
			FormatSerializer.setUint16(array, 6, _spec.CPU.regs.PC);
		}
		array2[4] = 3;
		array2[5] = b2;
		array2[6] = 0;
		array2[7] = 3;
		array2[8] = 14;
		if (_spec is IAyDevice ayDevice)
		{
			byte aDDR_REG = ayDevice.Sound.ADDR_REG;
			for (int i = 0; i < 16; i++)
			{
				ayDevice.Sound.ADDR_REG = (byte)i;
				array2[9 + i] = ayDevice.Sound.DATA_REG;
			}
			ayDevice.Sound.ADDR_REG = aDDR_REG;
		}
		int num = 0;
		int num2 = 0;
		byte[] array3 = new byte[200000];
		if (!flag)
		{
			byte[] array4 = new byte[65535];
			_spec.GetRamImage(5).CopyTo(array4, 0);
			_spec.GetRamImage(2).CopyTo(array4, 16384);
			_spec.GetRamImage(b2 & 7).CopyTo(array4, 32768);
			num = CompressZ80(array3, array4, 49152);
			if (num + 4 >= 49152)
			{
				array[12] &= 223;
				num = 49152;
				for (int j = 0; j < num; j++)
				{
					array3[j] = array4[j];
				}
			}
			stream.Write(array, 0, 30);
			stream.Write(array3, 0, num);
			if ((array[12] & 0x20u) != 0)
			{
				byte[] buffer = new byte[4] { 0, 237, 237, 0 };
				stream.Write(buffer, 0, 4);
			}
		}
		else
		{
			stream.Write(array, 0, 30);
			stream.Write(array2, 0, 25);
			for (int k = 0; k < 8; k++)
			{
				num = CompressZ80(array3, _spec.GetRamImage(k), 16384);
				num2 = (k & 7) + 3;
				stream.Write(FormatSerializer.getBytes(num), 0, 2);
				stream.Write(FormatSerializer.getBytes(num2), 0, 1);
				stream.Write(array3, 0, num);
			}
		}
	}

	private int CompressZ80(byte[] dest, byte[] src, int SrcSize)
	{
		int num = dest.Length;
		uint num2 = 0u;
		uint num3 = 1u;
		uint num4 = 0u;
		uint num5 = 0u;
		uint num6 = num2;
		while (num4 < SrcSize)
		{
			byte b = src[num4];
			num4++;
			byte b2;
			if (num4 < SrcSize)
			{
				b2 = src[num4];
			}
			else
			{
				b2 = b;
				b2 = (byte)(b2 + 1);
			}
			if (b != b2)
			{
				dest[num5] = b;
				num5++;
				num6 = ((b != 237) ? num2 : num3);
			}
			else if (b == 237)
			{
				dest[num5] = 237;
				num5++;
				dest[num5] = 237;
				num5++;
				dest[num5] = 2;
				num5++;
				dest[num5] = 237;
				num5++;
				num4++;
				num6 = num2;
			}
			else if (num6 == num3)
			{
				dest[num5] = b;
				num5++;
				num6 = num2;
			}
			else
			{
				uint num7 = 1u;
				while (num4 < SrcSize && b == src[num4])
				{
					num7++;
					num4++;
					if (num7 == 255)
					{
						break;
					}
				}
				if (num7 <= 4)
				{
					while (num7 != 0)
					{
						dest[num5] = b;
						num5++;
						num7--;
					}
				}
				else
				{
					dest[num5] = 237;
					num5++;
					dest[num5] = 237;
					num5++;
					dest[num5] = (byte)num7;
					num5++;
					dest[num5] = b;
					num5++;
				}
			}
			if (num5 >= num)
			{
				PlatformFactory.Platform.ShowWarning("Compression error: buffer overflow,\nfile can contain invalid data!", "Z80 loader");
				for (int i = 0; i < SrcSize; i++)
				{
					dest[i] = src[i];
				}
				return SrcSize;
			}
		}
		return (int)num5;
	}

	private int DecompressZ80(byte[] dest, byte[] src, int size)
	{
		uint num = 0u;
		uint num2 = 0u;
		while (num2 < size)
		{
			uint num3 = src[num++];
			byte b = (byte)num3;
			if (b != 237)
			{
				dest[num2++] = b;
				continue;
			}
			num3 = src[num++];
			b = (byte)num3;
			if (b != 237)
			{
				dest[num2++] = 237;
				num--;
				continue;
			}
			uint num4 = src[num++];
			num3 = src[num++];
			byte b2 = (byte)num3;
			while (num4 != 0)
			{
				dest[num2++] = b2;
				num4--;
			}
		}
		if (num2 != size)
		{
			PlatformFactory.Platform.ShowWarning("Decompression error: file corrupt?", "Z80 loader");
			return 1;
		}
		return size;
	}
}
