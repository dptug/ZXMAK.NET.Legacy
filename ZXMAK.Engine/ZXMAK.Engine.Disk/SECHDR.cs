namespace ZXMAK.Engine.Disk;

public class SECHDR
{
	public byte c;

	public byte s;

	public byte n;

	public byte l;

	public ushort crc1;

	public ushort crc2;

	public bool c1;

	public bool c2;

	public int idOffset;

	public int dataOffset;

	public int datlen;

	public ulong idTime;

	public ulong dataTime;
}
