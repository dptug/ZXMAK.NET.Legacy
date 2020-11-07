using System;
using System.IO;
using ZXMAK.Platform;

namespace ZXMAK.Engine.Loaders.SnapshotSerializers
{
	public class Z80Serializer : FormatSerializer
	{
		public Z80Serializer(Spectrum spec)
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
				return "Z80 snapshot";
			}
		}

		public override string FormatExtension
		{
			get
			{
				return "Z80";
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
			byte[] array = new byte[30];
			byte[] array2 = new byte[25];
			stream.Read(array, 0, 30);
			if (array[12] == 255)
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
					PlatformFactory.Platform.ShowWarning("Z80 format version not recognized!\n(ExtensionSize = " + array2[0].ToString() + ",\nsupported only ExtensionSize={0(old format), 23, 54})", "Z80 loader");
					return;
				}
			}
			this._spec.CPU.Tact += 1000UL;
			this._spec.DoReset();
			if (this._spec is IBetaDiskDevice)
			{
				((IBetaDiskDevice)this._spec).SEL_TRDOS = false;
			}
			this._spec.CPU.regs.A = array[0];
			this._spec.CPU.regs.F = array[1];
			this._spec.CPU.regs.HL = FormatSerializer.getUInt16(array, 4);
			this._spec.CPU.regs.DE = FormatSerializer.getUInt16(array, 13);
			this._spec.CPU.regs.BC = FormatSerializer.getUInt16(array, 2);
			this._spec.CPU.regs._AF = (ushort)((int)array[21] * 256 + (int)array[22]);
			this._spec.CPU.regs._HL = FormatSerializer.getUInt16(array, 19);
			this._spec.CPU.regs._DE = FormatSerializer.getUInt16(array, 17);
			this._spec.CPU.regs._BC = FormatSerializer.getUInt16(array, 15);
			this._spec.CPU.regs.IX = FormatSerializer.getUInt16(array, 25);
			this._spec.CPU.regs.IY = FormatSerializer.getUInt16(array, 23);
			this._spec.CPU.regs.SP = FormatSerializer.getUInt16(array, 8);
			this._spec.CPU.regs.I = array[10];
			this._spec.CPU.regs.R = (array[11] | (((array[12] & 1) != 0) ? 128 : 0));
			if (num == 1)
			{
				this._spec.CPU.regs.PC = FormatSerializer.getUInt16(array, 6);
			}
			else
			{
				this._spec.CPU.regs.PC = FormatSerializer.getUInt16(array2, 2);
			}
			this._spec.CPU.IFF1 = (array[27] != 0);
			this._spec.CPU.IFF2 = (array[28] != 0);
			this._spec.CPU.IM = (array[29] & 3);
			this._spec.CPU.HALTED = false;
			if (this._spec is ISpectrum)
			{
				((ISpectrum)this._spec).PortFE = (byte)(array[12] >> 1 & 7);
			}
			if (num > 1)
			{
				if (array2[6] == 255)
				{
					PlatformFactory.Platform.ShowWarning("Interface I not implemented, but Interface I ROM required!", "Z80 loader");
				}
				IAyDevice ayDevice = this._spec as IAyDevice;
				if (ayDevice != null)
				{
					for (int i = 0; i < 16; i++)
					{
						ayDevice.Sound.ADDR_REG = (byte)i;
						ayDevice.Sound.DATA_REG = array2[9 + i];
					}
				}
			}
			bool flag = (array[12] & 32) != 0;
			bool flag2 = false;
			if (num == 2)
			{
				switch (array2[4])
				{
				case 0:
				case 1:
					goto IL_42F;
				case 2:
					PlatformFactory.Platform.ShowWarning("SamRam not implemented!", "Z80 loader");
					goto IL_42F;
				case 3:
				case 4:
				case 9:
				case 10:
					flag2 = true;
					goto IL_42F;
				}
				PlatformFactory.Platform.ShowWarning("Unrecognized ZX Spectrum config (" + array2[4].ToString() + ")!", "Z80 loader");
			}
			IL_42F:
			if (num == 3)
			{
				switch (array2[4])
				{
				case 0:
				case 1:
				case 2:
					PlatformFactory.Platform.ShowWarning("SamRam not implemented!", "Z80 loader");
					goto IL_4B2;
				case 3:
					goto IL_4B2;
				case 4:
				case 5:
				case 6:
				case 9:
				case 10:
					flag2 = true;
					goto IL_4B2;
				}
				PlatformFactory.Platform.ShowWarning("Unrecognized ZX Spectrum config (" + array2[4].ToString() + ")!", "Z80 loader");
			}
			IL_4B2:
			byte b;
			if (num == 1)
			{
				b = 48;
			}
			else if (flag2)
			{
				b = array2[5];
			}
			else
			{
				b = 48;
			}
			if (this._spec is ISpectrum128K)
			{
				((ISpectrum128K)this._spec).Port7FFD = b;
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
					this.DecompressZ80(array4, array3, 49152);
				}
				else
				{
					for (int j = 0; j < 49152; j++)
					{
						array4[j] = array3[j];
					}
				}
				int page = (int)(b & 7);
				this._spec.SetRamImage(5, array4, 0, 16384);
				this._spec.SetRamImage(2, array4, 16384, 16384);
				this._spec.SetRamImage(page, array4, 32768, 16384);
				return;
			}
			byte[] array5 = new byte[4];
			byte[] array6 = new byte[129000];
			byte[] array7 = new byte[16384];
			while (stream.Position < stream.Length)
			{
				stream.Read(array5, 0, 2);
				int @uint = (int)FormatSerializer.getUInt16(array5, 0);
				stream.Read(array5, 0, 1);
				int num3 = (int)array5[0];
				stream.Read(array6, 0, @uint);
				this.DecompressZ80(array7, array6, 16384);
				if (num3 >= 3 && num3 <= 10 && flag2)
				{
					this._spec.SetRamImage(num3 - 3 & 7, array7, 0, 16384);
				}
				else if ((num3 == 4 || num3 == 5 || num3 == 8) && !flag2)
				{
					int page2 = (int)(b & 7);
					if (num3 == 8)
					{
						page2 = 5;
					}
					if (num3 == 4)
					{
						page2 = 2;
					}
					this._spec.SetRamImage(page2, array7, 0, 16384);
				}
				else if (num3 == 0)
				{
					this._spec.SetRomImage(RomName.ROM_48, array7, 0, 16384);
					PlatformFactory.Platform.ShowWarning("ROM 48K loaded from snapshot!", "Z80 loader");
				}
				else if (num3 == 2)
				{
					this._spec.SetRomImage(RomName.ROM_128, array7, 0, 16384);
					PlatformFactory.Platform.ShowWarning("ROM 128K loaded from snapshot!", "Z80 loader");
				}
			}
		}

		private void saveToStream(Stream stream)
		{
			byte[] array = new byte[30];
			byte[] array2 = new byte[25];
			FormatSerializer.setUint16(array, 6, 0);
			FormatSerializer.setUint16(array2, 0, 23);
			array[0] = this._spec.CPU.regs.A;
			array[1] = this._spec.CPU.regs.F;
			FormatSerializer.setUint16(array, 4, this._spec.CPU.regs.HL);
			FormatSerializer.setUint16(array, 13, this._spec.CPU.regs.DE);
			FormatSerializer.setUint16(array, 2, this._spec.CPU.regs.BC);
			array[21] = (byte)(this._spec.CPU.regs._AF >> 8);
			array[22] = (byte)this._spec.CPU.regs._AF;
			FormatSerializer.setUint16(array, 19, this._spec.CPU.regs._HL);
			FormatSerializer.setUint16(array, 17, this._spec.CPU.regs._DE);
			FormatSerializer.setUint16(array, 15, this._spec.CPU.regs._BC);
			FormatSerializer.setUint16(array, 25, this._spec.CPU.regs.IX);
			FormatSerializer.setUint16(array, 23, this._spec.CPU.regs.IY);
			FormatSerializer.setUint16(array, 8, this._spec.CPU.regs.SP);
			array[10] = this._spec.CPU.regs.I;
			array[11] = (this._spec.CPU.regs.R & 127);
			if ((this._spec.CPU.regs.R & 128) != 0)
			{
				byte[] array3 = array;
				int num = 12;
				array3[num] |= 1;
			}
			byte b = byte.MaxValue;
			if (this._spec is ISpectrum)
			{
				b = ((ISpectrum)this._spec).PortFE;
			}
			array[12] = (byte)((b & 7) << 1);
			byte[] array4 = array;
			int num2 = 12;
			array4[num2] |= 32;
			FormatSerializer.setUint16(array2, 2, this._spec.CPU.regs.PC);
			if (this._spec.CPU.IFF1)
			{
				array[27] = byte.MaxValue;
			}
			else
			{
				array[27] = 0;
			}
			if (this._spec.CPU.IFF2)
			{
				array[28] = byte.MaxValue;
			}
			else
			{
				array[28] = 0;
			}
			array[29] = this._spec.CPU.IM;
			if (this._spec.CPU.IM > 2)
			{
				array[29] = 0;
			}
			byte b2 = 48;
			if (this._spec is ISpectrum128K)
			{
				b2 = ((ISpectrum128K)this._spec).Port7FFD;
			}
			bool flag = (b2 & 48) != 48;
			if (!flag)
			{
				FormatSerializer.setUint16(array, 6, this._spec.CPU.regs.PC);
			}
			array2[4] = 3;
			array2[5] = b2;
			array2[6] = 0;
			array2[7] = 3;
			array2[8] = 14;
			IAyDevice ayDevice = this._spec as IAyDevice;
			if (ayDevice != null)
			{
				byte addr_REG = ayDevice.Sound.ADDR_REG;
				for (int i = 0; i < 16; i++)
				{
					ayDevice.Sound.ADDR_REG = (byte)i;
					array2[9 + i] = ayDevice.Sound.DATA_REG;
				}
				ayDevice.Sound.ADDR_REG = addr_REG;
			}
			byte[] array5 = new byte[200000];
			if (!flag)
			{
				byte[] array6 = new byte[65535];
				this._spec.GetRamImage(5).CopyTo(array6, 0);
				this._spec.GetRamImage(2).CopyTo(array6, 16384);
				this._spec.GetRamImage((int)(b2 & 7)).CopyTo(array6, 32768);
				int num3 = this.CompressZ80(array5, array6, 49152);
				if (num3 + 4 >= 49152)
				{
					byte[] array7 = array;
					int num4 = 12;
					array7[num4] &= 223;
					num3 = 49152;
					for (int j = 0; j < num3; j++)
					{
						array5[j] = array6[j];
					}
				}
				stream.Write(array, 0, 30);
				stream.Write(array5, 0, num3);
				if ((array[12] & 32) != 0)
				{
					byte[] array8 = new byte[4];
					array8[1] = 237;
					array8[2] = 237;
					byte[] buffer = array8;
					stream.Write(buffer, 0, 4);
					return;
				}
			}
			else
			{
				stream.Write(array, 0, 30);
				stream.Write(array2, 0, 25);
				for (int k = 0; k < 8; k++)
				{
					int num3 = this.CompressZ80(array5, this._spec.GetRamImage(k), 16384);
					int value = (k & 7) + 3;
					stream.Write(FormatSerializer.getBytes(num3), 0, 2);
					stream.Write(FormatSerializer.getBytes(value), 0, 1);
					stream.Write(array5, 0, num3);
				}
			}
		}

		private int CompressZ80(byte[] dest, byte[] src, int SrcSize)
		{
			int num = dest.Length;
			uint num2 = 0U;
			uint num3 = 1U;
			uint num4 = 0U;
			uint num5 = 0U;
			uint num6 = num2;
			while ((ulong)num4 < (ulong)((long)SrcSize))
			{
				byte b = src[(int)((UIntPtr)num4)];
				num4 += 1U;
				byte b2;
				if ((ulong)num4 < (ulong)((long)SrcSize))
				{
					b2 = src[(int)((UIntPtr)num4)];
				}
				else
				{
					b2 = b;
					b2 += 1;
				}
				if (b != b2)
				{
					dest[(int)((UIntPtr)num5)] = b;
					num5 += 1U;
					if (b == 237)
					{
						num6 = num3;
					}
					else
					{
						num6 = num2;
					}
				}
				else if (b == 237)
				{
					dest[(int)((UIntPtr)num5)] = 237;
					num5 += 1U;
					dest[(int)((UIntPtr)num5)] = 237;
					num5 += 1U;
					dest[(int)((UIntPtr)num5)] = 2;
					num5 += 1U;
					dest[(int)((UIntPtr)num5)] = 237;
					num5 += 1U;
					num4 += 1U;
					num6 = num2;
				}
				else if (num6 == num3)
				{
					dest[(int)((UIntPtr)num5)] = b;
					num5 += 1U;
					num6 = num2;
				}
				else
				{
					uint num7 = 1U;
					while ((ulong)num4 < (ulong)((long)SrcSize) && b == src[(int)((UIntPtr)num4)])
					{
						num7 += 1U;
						num4 += 1U;
						if (num7 == 255U)
						{
							break;
						}
					}
					if (num7 <= 4U)
					{
						while (num7 != 0U)
						{
							dest[(int)((UIntPtr)num5)] = b;
							num5 += 1U;
							num7 -= 1U;
						}
					}
					else
					{
						dest[(int)((UIntPtr)num5)] = 237;
						num5 += 1U;
						dest[(int)((UIntPtr)num5)] = 237;
						num5 += 1U;
						dest[(int)((UIntPtr)num5)] = (byte)num7;
						num5 += 1U;
						dest[(int)((UIntPtr)num5)] = b;
						num5 += 1U;
					}
				}
				if ((ulong)num5 >= (ulong)((long)num))
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
			uint num = 0U;
			uint num2 = 0U;
			while ((ulong)num2 < (ulong)((long)size))
			{
				uint num3 = (uint)src[(int)((UIntPtr)(num++))];
				byte b = (byte)num3;
				if (b != 237)
				{
					dest[(int)((UIntPtr)(num2++))] = b;
				}
				else
				{
					num3 = (uint)src[(int)((UIntPtr)(num++))];
					b = (byte)num3;
					if (b != 237)
					{
						dest[(int)((UIntPtr)(num2++))] = 237;
						num -= 1U;
					}
					else
					{
						uint num4 = (uint)src[(int)((UIntPtr)(num++))];
						num3 = (uint)src[(int)((UIntPtr)(num++))];
						byte b2 = (byte)num3;
						while (num4 != 0U)
						{
							dest[(int)((UIntPtr)(num2++))] = b2;
							num4 -= 1U;
						}
					}
				}
			}
			if ((ulong)num2 != (ulong)((long)size))
			{
				PlatformFactory.Platform.ShowWarning("Decompression error: file corrupt?", "Z80 loader");
				return 1;
			}
			return size;
		}

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
	}
}
