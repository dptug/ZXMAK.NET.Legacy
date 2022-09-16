namespace ZXMAK.Engine.AY;

public class AyPortState
{
	private byte _outState;

	private byte _oldOutState;

	private byte _inState;

	public byte OutState
	{
		get
		{
			return _outState;
		}
		set
		{
			_oldOutState = _outState;
			_outState = value;
		}
	}

	public byte OldOutState => _oldOutState;

	public byte InState
	{
		get
		{
			return _inState;
		}
		set
		{
			_inState = value;
		}
	}

	public AyPortState(byte value)
	{
		_outState = value;
		_oldOutState = value;
		_inState = value;
	}
}
