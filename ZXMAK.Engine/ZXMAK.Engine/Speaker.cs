namespace ZXMAK.Engine;

public class Speaker
{
	private bool _tapeInEnable = true;

	private bool _tapeOutEnable = true;

	private bool _beepOutEnable = true;

	private int _sampleRate;

	private int _frameTactCount;

	private uint[] _frameData;

	private int _pos;

	private int _lastValue;

	public bool TapeInEnable
	{
		get
		{
			return _tapeInEnable;
		}
		set
		{
			_tapeInEnable = value;
		}
	}

	public bool TapeOutEnable
	{
		get
		{
			return _tapeOutEnable;
		}
		set
		{
			_tapeOutEnable = value;
		}
	}

	public bool BeepOutEnable
	{
		get
		{
			return _beepOutEnable;
		}
		set
		{
			_beepOutEnable = value;
		}
	}

	public Speaker(int sampleRate, int frameTactCount)
	{
		_frameTactCount = frameTactCount;
		_sampleRate = sampleRate;
		_frameData = new uint[_sampleRate / 50];
		_pos = 0;
		_lastValue = 0;
	}

	public void SetPort(int frameTact, int value)
	{
		if (_lastValue != value)
		{
			process(frameTact);
		}
		_lastValue = value;
	}

	public uint[] FlushFrame()
	{
		process(_frameTactCount);
		_pos = 0;
		return _frameData;
	}

	private void process(int frameTact)
	{
		int num = _frameData.Length * frameTact / _frameTactCount;
		if (num > _frameData.Length)
		{
			num = _frameData.Length;
		}
		if (num > _pos)
		{
			uint num2 = 0u;
			if (_beepOutEnable && ((uint)_lastValue & 0x10u) != 0)
			{
				num2 += 8191;
			}
			if (_tapeOutEnable && ((uint)_lastValue & 8u) != 0)
			{
				num2 += 8191;
			}
			if (_tapeInEnable && ((uint)_lastValue & 0x40u) != 0)
			{
				num2 += 8191;
			}
			num2 /= 3u;
			num2 |= num2 << 16;
			while (_pos < num)
			{
				_frameData[_pos] = num2;
				_pos++;
			}
		}
	}
}
