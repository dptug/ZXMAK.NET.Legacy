using System;
using System.Collections;

namespace ZXMAK.Engine.Disk
{
	public class Track
	{
		public Track(ulong trackTime)
		{
			this._trackTime = trackTime;
			this._byteTime = trackTime / 6400UL;
			if (this._byteTime < 1UL)
			{
				this._byteTime = 1UL;
			}
		}

		public void RefreshHeaders()
		{
			this._headerList.Clear();
			if (this._trackImage == null)
			{
				return;
			}
			for (int i = 0; i < this._trackImage.Length - 8; i++)
			{
				if (this._trackImage[i] == 161 && this._trackImage[i + 1] == 254 && this.RawTestClock(i))
				{
					SECHDR sechdr = new SECHDR();
					this._headerList.Add(sechdr);
					sechdr.idOffset = i + 2;
					sechdr.idTime = (ulong)((long)sechdr.idOffset * (long)this._byteTime);
					sechdr.c = this._trackImage[sechdr.idOffset];
					sechdr.s = this._trackImage[sechdr.idOffset + 1];
					sechdr.n = this._trackImage[sechdr.idOffset + 2];
					sechdr.l = this._trackImage[sechdr.idOffset + 3];
					sechdr.crc1 = (ushort)((int)this._trackImage[i + 6] | (int)this._trackImage[i + 7] << 8);
					sechdr.c1 = (this.WD1793_CRC(i + 1, 5) == sechdr.crc1);
					sechdr.dataOffset = -1;
					sechdr.datlen = 0;
					if (sechdr.l <= 5)
					{
						int num = this._trackImage.Length - 8;
						int j = i + 8;
						while (j < num)
						{
							if (this._trackImage[j] == 161 && this.RawTestClock(j) && !this.RawTestClock(j + 1))
							{
								if (this._trackImage[j + 1] != 248 && this._trackImage[j + 1] != 251)
								{
									break;
								}
								sechdr.datlen = 128 << (int)sechdr.l;
								sechdr.dataOffset = j + 2;
								sechdr.dataTime = (ulong)((long)sechdr.dataOffset * (long)this._byteTime);
								if (sechdr.dataOffset + sechdr.datlen + 2 > this._trackImage.Length)
								{
									sechdr.datlen = this._trackImage.Length - sechdr.dataOffset;
									sechdr.crc2 = (this.WD1793_CRC(sechdr.dataOffset - 1, sechdr.datlen + 1) ^ ushort.MaxValue);
									sechdr.c2 = false;
									break;
								}
								sechdr.crc2 = (ushort)((int)this._trackImage[sechdr.dataOffset + sechdr.datlen] | (int)this._trackImage[sechdr.dataOffset + sechdr.datlen + 1] << 8);
								sechdr.c2 = (this.WD1793_CRC(sechdr.dataOffset - 1, sechdr.datlen + 1) == sechdr.crc2);
								break;
							}
							else
							{
								j++;
							}
						}
					}
				}
			}
		}

		public void AssignImage(byte[] trackImage, byte[] trackClock)
		{
			if (trackImage.Length <= 0 || trackImage.Length / 8 + (((trackImage.Length & 7) != 0) ? 1 : 0) != trackClock.Length)
			{
				throw new InvalidOperationException("Invalid track image length!");
			}
			this._trackImage = trackImage;
			this._trackClock = trackClock;
			this._byteTime = this._trackTime / (ulong)((long)trackImage.Length);
			if (this._byteTime < 1UL)
			{
				this._byteTime = 1UL;
			}
			this.RefreshHeaders();
		}

		public void AssignSectors(ArrayList sectorList)
		{
			byte[][] array = new byte[2][];
			int num = 6250;
			int count = sectorList.Count;
			int num2 = 0;
			foreach (object obj in sectorList)
			{
				Sector sector = (Sector)obj;
				num2 += sector.GetAdBlockSize();
				num2 += sector.GetDataBlockSize();
			}
			int i = num - (num2 + count * 5);
			int num3 = 1;
			int num4 = 1;
			int num5 = 1;
			int num6 = 1;
			i -= num3 + num4 + num5 + num6;
			if (i < 0)
			{
				num += -i;
				i = 0;
			}
			while (i > 0)
			{
				if (i >= count * 2 && num6 < 12)
				{
					num6++;
					i -= count * 2;
				}
				if (i < count)
				{
					break;
				}
				if (num3 < 10)
				{
					num3++;
					i -= count;
				}
				if (i < count)
				{
					break;
				}
				if (num4 < 22)
				{
					num4++;
					i -= count;
				}
				if (i < count)
				{
					break;
				}
				if (num5 < 60)
				{
					num5++;
					i -= count;
				}
				if (i < count || (num6 >= 12 && num3 >= 10 && num4 >= 22 && num5 >= 60))
				{
					break;
				}
			}
			if (i < 0)
			{
				num += -i;
			}
			array[0] = new byte[num];
			array[1] = new byte[array[0].Length / 8 + (((array[0].Length & 7) != 0) ? 1 : 0)];
			int num7 = 0;
			foreach (object obj2 in sectorList)
			{
				Sector sector2 = (Sector)obj2;
				for (int j = 0; j < num3; j++)
				{
					array[0][num7] = 78;
					byte[] array2 = array[1];
					int num8 = num7 / 8;
					array2[num8] &= (byte)(~(byte)(1 << (num7 & 7)));
					num7++;
				}
				for (int j = 0; j < num6; j++)
				{
					array[0][num7] = 0;
					byte[] array3 = array[1];
					int num9 = num7 / 8;
					array3[num9] &= (byte)(~(byte)(1 << (num7 & 7)));
					num7++;
				}
				if (sector2.AdPresent)
				{
					byte[][] array4 = sector2.CreateAdBlock();
					for (int j = 0; j < sector2.GetAdBlockSize(); j++)
					{
						array[0][num7] = array4[0][j];
						if (((int)array4[1][j / 8] & 1 << (j & 7)) != 0)
						{
							byte[] array5 = array[1];
							int num10 = num7 / 8;
							array5[num10] |= (byte)(1 << (num7 & 7));
						}
						else
						{
							byte[] array6 = array[1];
							int num11 = num7 / 8;
							array6[num11] &= (byte)(~(byte)(1 << (num7 & 7)));
						}
						num7++;
					}
				}
				for (int j = 0; j < num4; j++)
				{
					array[0][num7] = 78;
					byte[] array7 = array[1];
					int num12 = num7 / 8;
					array7[num12] &= (byte)(~(byte)(1 << (num7 & 7)));
					num7++;
				}
				for (int j = 0; j < num6; j++)
				{
					array[0][num7] = 0;
					byte[] array8 = array[1];
					int num13 = num7 / 8;
					array8[num13] &= (byte)(~(byte)(1 << (num7 & 7)));
					num7++;
				}
				if (sector2.DataPresent)
				{
					byte[][] array9 = sector2.CreateDataBlock();
					for (int j = 0; j < sector2.GetDataBlockSize(); j++)
					{
						array[0][num7] = array9[0][j];
						if (((int)array9[1][j / 8] & 1 << (j & 7)) != 0)
						{
							byte[] array10 = array[1];
							int num14 = num7 / 8;
							array10[num14] |= (byte)(1 << (num7 & 7));
						}
						else
						{
							byte[] array11 = array[1];
							int num15 = num7 / 8;
							array11[num15] &= (byte)(~(byte)(1 << (num7 & 7)));
						}
						num7++;
					}
				}
				for (int j = 0; j < num5; j++)
				{
					array[0][num7] = 78;
					byte[] array12 = array[1];
					int num16 = num7 / 8;
					array12[num16] &= (byte)(~(byte)(1 << (num7 & 7)));
					num7++;
				}
			}
			for (int k = num7; k < array[0].Length; k++)
			{
				array[0][num7] = 78;
				byte[] array13 = array[1];
				int num17 = num7 / 8;
				array13[num17] &= (byte)(~(byte)(1 << (num7 & 7)));
				num7++;
			}
			this.AssignImage(array[0], array[1]);
		}

		public bool RawTestClock(int pos)
		{
			return this._trackClock != null && ((int)this._trackClock[pos / 8] & 1 << (pos & 7)) != 0;
		}

		public void RawWrite(int pos, byte value, bool clock)
		{
			if (this._trackImage == null)
			{
				return;
			}
			this._trackImage[pos] = value;
			if (clock)
			{
				byte[] trackClock = this._trackClock;
				int num = pos / 8;
				trackClock[num] |= (byte)(1 << (pos & 7));
				return;
			}
			byte[] trackClock2 = this._trackClock;
			int num2 = pos / 8;
			trackClock2[num2] &= (byte)(~(byte)(1 << (pos & 7)));
		}

		public byte RawRead(int pos)
		{
			if (this._trackImage == null)
			{
				return 0;
			}
			return this._trackImage[pos];
		}

		public int RawLength
		{
			get
			{
				if (this._trackImage == null)
				{
					return 6400;
				}
				return this._trackImage.Length;
			}
		}

		public byte[][] RawImage
		{
			get
			{
				return new byte[][]
				{
					this._trackImage,
					this._trackClock
				};
			}
		}

		public ulong ByteTime
		{
			get
			{
				return this._byteTime;
			}
		}

		public ArrayList HeaderList
		{
			get
			{
				return this._headerList;
			}
		}

		public ushort WD1793_CRC(int startIndex, int size)
		{
			if (this._trackImage == null)
			{
				return 0;
			}
			uint num = 52660U;
			while (size-- > 0)
			{
				num ^= (uint)((uint)this._trackImage[startIndex++] << 8);
				for (int num2 = 8; num2 != 0; num2--)
				{
					if (((num *= 2U) & 65536U) != 0U)
					{
						num ^= 4129U;
					}
				}
			}
			return (ushort)((num & 65280U) >> 8 | (num & 255U) << 8);
		}

		private ulong _trackTime;

		private ulong _byteTime;

		private byte[] _trackImage;

		private byte[] _trackClock;

		private ArrayList _headerList = new ArrayList();

		public bool sf;
	}
}
