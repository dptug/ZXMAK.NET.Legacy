using System;

namespace ZXMAK.Engine.Disk
{
	public class SimpleSector : Sector
	{
		public override byte[] Data
		{
			get
			{
				return this._data;
			}
		}

		public override bool DataPresent
		{
			get
			{
				return this._dataPresent;
			}
		}

		public override bool AdPresent
		{
			get
			{
				return this._adPresent;
			}
		}

		public override byte C
		{
			get
			{
				return this._adMark[0];
			}
		}

		public override byte H
		{
			get
			{
				return this._adMark[1];
			}
		}

		public override byte R
		{
			get
			{
				return this._adMark[2];
			}
		}

		public override byte N
		{
			get
			{
				return this._adMark[3];
			}
		}

		public SimpleSector(int cc, int hh, int rr, int nn, byte[] data)
		{
			this._adPresent = true;
			this._adMark[0] = (byte)cc;
			this._adMark[1] = (byte)hh;
			this._adMark[2] = (byte)rr;
			this._adMark[3] = (byte)nn;
			if (data != null)
			{
				this._dataPresent = true;
				this._data = data;
				return;
			}
			this._dataPresent = false;
		}

		public SimpleSector(int cc, int hh, int rr, int nn) : this(cc, hh, rr, nn, new byte[128 << (nn & 7)])
		{
		}

		public SimpleSector(byte[] data) : this(0, 0, 0, 0, data)
		{
			this._adPresent = false;
		}

		private bool _adPresent;

		private bool _dataPresent;

		private byte[] _adMark = new byte[4];

		private byte[] _data = new byte[0];
	}
}
