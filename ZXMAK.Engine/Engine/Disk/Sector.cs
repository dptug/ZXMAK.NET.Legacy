using System;

namespace ZXMAK.Engine.Disk
{
	public abstract class Sector
	{
		public abstract bool AdPresent { get; }

		public abstract bool DataPresent { get; }

		public virtual bool DataDeleteMark
		{
			get
			{
				return false;
			}
		}

		public abstract byte[] Data { get; }

		public abstract byte C { get; }

		public abstract byte H { get; }

		public abstract byte R { get; }

		public abstract byte N { get; }

		public virtual int AdSyncCount
		{
			get
			{
				return 3;
			}
		}

		public virtual int DataSyncCount
		{
			get
			{
				return 3;
			}
		}

		public void SetAdCrc(bool valid)
		{
			byte[] array = this.CreateAdBlock()[0];
			ushort num = ushort.MaxValue;
			if (array.Length > 0)
			{
				num = Sector.BuildCrc(num, array, 0, array.Length - 2);
			}
			if (!valid)
			{
				num ^= ushort.MaxValue;
			}
			this._adCrc = num;
		}

		public void SetDataCrc(bool valid)
		{
			byte[] array = this.CreateDataBlock()[0];
			ushort num = ushort.MaxValue;
			if (array.Length > 0)
			{
				num = Sector.BuildCrc(num, array, 0, array.Length - 2);
			}
			if (!valid)
			{
				num ^= ushort.MaxValue;
			}
			this._dataCrc = num;
		}

		public byte[][] CreateAdBlock()
		{
			if (!this.AdPresent)
			{
				return new byte[][]
				{
					new byte[0],
					new byte[0]
				};
			}
			int num = 0;
			byte[][] array = new byte[2][];
			array[0] = new byte[this.GetAdBlockSize()];
			array[1] = new byte[array[0].Length / 8 + (((array[0].Length & 7) != 0) ? 1 : 0)];
			for (int i = 0; i < this.AdSyncCount; i++)
			{
				array[0][num] = 161;
				byte[] array2 = array[1];
				int num2 = num / 8;
				array2[num2] |= (byte)(1 << (num & 7));
				num++;
			}
			array[0][num] = 254;
			byte[] array3 = array[1];
			int num3 = num / 8;
			array3[num3] &= (byte)(~(byte)(1 << (num & 7)));
			num++;
			byte[] array4 = new byte[]
			{
				this.C,
				this.H,
				this.R,
				this.N
			};
			for (int j = 0; j < 4; j++)
			{
				array[0][num] = array4[j];
				byte[] array5 = array[1];
				int num4 = num / 8;
				array5[num4] &= (byte)(~(byte)(1 << (num & 7)));
				num++;
			}
			array[0][num] = (byte)this._adCrc;
			byte[] array6 = array[1];
			int num5 = num / 8;
			array6[num5] &= (byte)(~(byte)(1 << (num & 7)));
			num++;
			array[0][num] = (byte)(this._adCrc >> 8);
			byte[] array7 = array[1];
			int num6 = num / 8;
			array7[num6] &= (byte)(~(byte)(1 << (num & 7)));
			num++;
			return array;
		}

		public byte[][] CreateDataBlock()
		{
			if (!this.DataPresent)
			{
				return new byte[][]
				{
					new byte[0],
					new byte[0]
				};
			}
			int num = 0;
			byte[][] array = new byte[2][];
			array[0] = new byte[this.GetDataBlockSize()];
			array[1] = new byte[array[0].Length / 8 + (((array[0].Length & 7) != 0) ? 1 : 0)];
			for (int i = 0; i < this.DataSyncCount; i++)
			{
				array[0][num] = 161;
				byte[] array2 = array[1];
				int num2 = num / 8;
				array2[num2] |= (byte)(1 << (num & 7));
				num++;
			}
			if (this.DataDeleteMark)
			{
				array[0][num] = 248;
			}
			else
			{
				array[0][num] = 251;
			}
			byte[] array3 = array[1];
			int num3 = num / 8;
			array3[num3] &= (byte)(~(byte)(1 << (num & 7)));
			num++;
			for (int j = 0; j < this.Data.Length; j++)
			{
				array[0][num] = this.Data[j];
				byte[] array4 = array[1];
				int num4 = num / 8;
				array4[num4] &= (byte)(~(byte)(1 << (num & 7)));
				num++;
			}
			array[0][num] = (byte)this._dataCrc;
			byte[] array5 = array[1];
			int num5 = num / 8;
			array5[num5] &= (byte)(~(byte)(1 << (num & 7)));
			num++;
			array[0][num] = (byte)(this._dataCrc >> 8);
			byte[] array6 = array[1];
			int num6 = num / 8;
			array6[num6] &= (byte)(~(byte)(1 << (num & 7)));
			num++;
			return array;
		}

		public int GetAdBlockSize()
		{
			if (this.AdPresent)
			{
				return this.AdSyncCount + 4 + 3;
			}
			return 0;
		}

		public int GetDataBlockSize()
		{
			if (this.DataPresent)
			{
				return this.DataSyncCount + this.Data.Length + 3;
			}
			return 0;
		}

		public static ushort BuildCrc(ushort value, byte[] buffer, int startIndex, int length)
		{
			int num = (int)value;
			while (length-- > 0)
			{
				num ^= (int)buffer[startIndex++] << 8;
				for (int num2 = 8; num2 != 0; num2--)
				{
					if (((num *= 2) & 65536) != 0)
					{
						num ^= 4129;
					}
				}
			}
			num = ((num & 65280) >> 8 | (num & 255) << 8);
			return (ushort)num;
		}

		private ushort _adCrc = ushort.MaxValue;

		private ushort _dataCrc = ushort.MaxValue;
	}
}
