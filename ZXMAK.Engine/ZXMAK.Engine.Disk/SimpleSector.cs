namespace ZXMAK.Engine.Disk;

public class SimpleSector : Sector
{
	private bool _adPresent;

	private bool _dataPresent;

	private byte[] _adMark = new byte[4];

	private byte[] _data = new byte[0];

	public override byte[] Data => _data;

	public override bool DataPresent => _dataPresent;

	public override bool AdPresent => _adPresent;

	public override byte C => _adMark[0];

	public override byte H => _adMark[1];

	public override byte R => _adMark[2];

	public override byte N => _adMark[3];

	public SimpleSector(int cc, int hh, int rr, int nn, byte[] data)
	{
		_adPresent = true;
		_adMark[0] = (byte)cc;
		_adMark[1] = (byte)hh;
		_adMark[2] = (byte)rr;
		_adMark[3] = (byte)nn;
		if (data != null)
		{
			_dataPresent = true;
			_data = data;
		}
		else
		{
			_dataPresent = false;
		}
	}

	public SimpleSector(int cc, int hh, int rr, int nn)
		: this(cc, hh, rr, nn, new byte[128 << (nn & 7)])
	{
	}

	public SimpleSector(byte[] data)
		: this(0, 0, 0, 0, data)
	{
		_adPresent = false;
	}
}
