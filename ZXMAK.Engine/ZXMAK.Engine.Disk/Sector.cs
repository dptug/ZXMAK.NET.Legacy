namespace ZXMAK.Engine.Disk;

public abstract class Sector
{
	private ushort _adCrc = ushort.MaxValue;

	private ushort _dataCrc = ushort.MaxValue;

	public abstract bool AdPresent { get; }

	public abstract bool DataPresent { get; }

	public virtual bool DataDeleteMark => false;

	public abstract byte[] Data { get; }

	public abstract byte C { get; }

	public abstract byte H { get; }

	public abstract byte R { get; }

	public abstract byte N { get; }

	public virtual int AdSyncCount => 3;

	public virtual int DataSyncCount => 3;

	public void SetAdCrc(bool valid)
	{
		byte[] array = CreateAdBlock()[0];
		ushort num = ushort.MaxValue;
		if (array.Length > 0)
		{
			num = BuildCrc(num, array, 0, array.Length - 2);
		}
		if (!valid)
		{
			num = (ushort)(num ^ 0xFFFFu);
		}
		_adCrc = num;
	}

	public void SetDataCrc(bool valid)
	{
		byte[] array = CreateDataBlock()[0];
		ushort num = ushort.MaxValue;
		if (array.Length > 0)
		{
			num = BuildCrc(num, array, 0, array.Length - 2);
		}
		if (!valid)
		{
			num = (ushort)(num ^ 0xFFFFu);
		}
		_dataCrc = num;
	}

	public byte[][] CreateAdBlock()
	{
		if (!AdPresent)
		{
			return new byte[2][]
			{
				new byte[0],
				new byte[0]
			};
		}
		int num = 0;
		byte[][] array = new byte[2][];
		array[0] = new byte[GetAdBlockSize()];
		array[1] = new byte[array[0].Length / 8 + ((((uint)array[0].Length & 7u) != 0) ? 1 : 0)];
		for (int i = 0; i < AdSyncCount; i++)
		{
			array[0][num] = 161;
			array[1][num / 8] |= (byte)(1 << (num & 7));
			num++;
		}
		array[0][num] = 254;
		array[1][num / 8] &= (byte)(~(1 << (num & 7)));
		num++;
		byte[] array2 = new byte[4] { C, H, R, N };
		for (int j = 0; j < 4; j++)
		{
			array[0][num] = array2[j];
			array[1][num / 8] &= (byte)(~(1 << (num & 7)));
			num++;
		}
		array[0][num] = (byte)_adCrc;
		array[1][num / 8] &= (byte)(~(1 << (num & 7)));
		num++;
		array[0][num] = (byte)(_adCrc >> 8);
		array[1][num / 8] &= (byte)(~(1 << (num & 7)));
		num++;
		return array;
	}

	public byte[][] CreateDataBlock()
	{
		if (!DataPresent)
		{
			return new byte[2][]
			{
				new byte[0],
				new byte[0]
			};
		}
		int num = 0;
		byte[][] array = new byte[2][];
		array[0] = new byte[GetDataBlockSize()];
		array[1] = new byte[array[0].Length / 8 + ((((uint)array[0].Length & 7u) != 0) ? 1 : 0)];
		for (int i = 0; i < DataSyncCount; i++)
		{
			array[0][num] = 161;
			array[1][num / 8] |= (byte)(1 << (num & 7));
			num++;
		}
		if (DataDeleteMark)
		{
			array[0][num] = 248;
		}
		else
		{
			array[0][num] = 251;
		}
		array[1][num / 8] &= (byte)(~(1 << (num & 7)));
		num++;
		for (int j = 0; j < Data.Length; j++)
		{
			array[0][num] = Data[j];
			array[1][num / 8] &= (byte)(~(1 << (num & 7)));
			num++;
		}
		array[0][num] = (byte)_dataCrc;
		array[1][num / 8] &= (byte)(~(1 << (num & 7)));
		num++;
		array[0][num] = (byte)(_dataCrc >> 8);
		array[1][num / 8] &= (byte)(~(1 << (num & 7)));
		num++;
		return array;
	}

	public int GetAdBlockSize()
	{
		if (AdPresent)
		{
			return AdSyncCount + 4 + 3;
		}
		return 0;
	}

	public int GetDataBlockSize()
	{
		if (DataPresent)
		{
			return DataSyncCount + Data.Length + 3;
		}
		return 0;
	}

	public static ushort BuildCrc(ushort value, byte[] buffer, int startIndex, int length)
	{
		int num = value;
		while (length-- > 0)
		{
			num ^= buffer[startIndex++] << 8;
			for (int num2 = 8; num2 != 0; num2--)
			{
				if (((uint)(num *= 2) & 0x10000u) != 0)
				{
					num ^= 0x1021;
				}
			}
		}
		num = ((num & 0xFF00) >> 8) | ((num & 0xFF) << 8);
		return (ushort)num;
	}
}
