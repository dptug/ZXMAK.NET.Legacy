using SdlDotNet.Input;

namespace ZXMAK.Platform.SDL;

public class Keyboard
{
	private long _state;

	private KeyboardState _keyboardState;

	public long State => _state;

	public Keyboard()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		_keyboardState = new KeyboardState();
	}

	public void Scan()
	{
		_state = 0L;
		_keyboardState.Update();
		_state = parseKeyboardState(_keyboardState);
	}

	private long parseKeyboardState(KeyboardState state)
	{
		long num = 0L;
		if (state.get_Item((Key)308) || state.get_Item((Key)307))
		{
			return 0L;
		}
		num = (num << 5) | parse_7FFE(state);
		num = (num << 5) | parse_BFFE(state);
		num = (num << 5) | parse_DFFE(state);
		num = (num << 5) | parse_EFFE(state);
		num = (num << 5) | parse_F7FE(state);
		num = (num << 5) | parse_FBFE(state);
		num = (num << 5) | parse_FDFE(state);
		return (num << 5) | parse_FEFE(state);
	}

	private static byte parse_7FFE(KeyboardState state)
	{
		byte b = 0;
		if (state.get_Item((Key)32))
		{
			b = (byte)(b | 1u);
		}
		if (state.get_Item((Key)303))
		{
			b = (byte)(b | 2u);
		}
		if (state.get_Item((Key)109))
		{
			b = (byte)(b | 4u);
		}
		if (state.get_Item((Key)110))
		{
			b = (byte)(b | 8u);
		}
		if (state.get_Item((Key)98))
		{
			b = (byte)(b | 0x10u);
		}
		if (state.get_Item((Key)301) || state.get_Item((Key)270) || state.get_Item((Key)269) || state.get_Item((Key)268) || state.get_Item((Key)267))
		{
			b = (byte)(b | 2u);
		}
		if (state.get_Item((Key)46) || state.get_Item((Key)44) || state.get_Item((Key)59) || state.get_Item((Key)39) || state.get_Item((Key)47) || state.get_Item((Key)45) || state.get_Item((Key)61) || state.get_Item((Key)91) || state.get_Item((Key)93))
		{
			b = (byte)(b | 2u);
		}
		if (state.get_Item((Key)268))
		{
			b = (byte)(b | 0x10u);
		}
		if (!state.get_Item((Key)303))
		{
			if (state.get_Item((Key)46))
			{
				b = (byte)(b | 4u);
			}
			if (state.get_Item((Key)44))
			{
				b = (byte)(b | 8u);
			}
		}
		return b;
	}

	private static byte parse_BFFE(KeyboardState state)
	{
		byte b = 0;
		if (state.get_Item((Key)13))
		{
			b = (byte)(b | 1u);
		}
		if (state.get_Item((Key)108))
		{
			b = (byte)(b | 2u);
		}
		if (state.get_Item((Key)107))
		{
			b = (byte)(b | 4u);
		}
		if (state.get_Item((Key)106))
		{
			b = (byte)(b | 8u);
		}
		if (state.get_Item((Key)104))
		{
			b = (byte)(b | 0x10u);
		}
		if (state.get_Item((Key)271))
		{
			b = (byte)(b | 1u);
		}
		if (state.get_Item((Key)269))
		{
			b = (byte)(b | 8u);
		}
		if (state.get_Item((Key)270))
		{
			b = (byte)(b | 4u);
		}
		if (state.get_Item((Key)303))
		{
			if (state.get_Item((Key)61))
			{
				b = (byte)(b | 4u);
			}
		}
		else
		{
			if (state.get_Item((Key)45))
			{
				b = (byte)(b | 8u);
			}
			if (state.get_Item((Key)61))
			{
				b = (byte)(b | 2u);
			}
		}
		return b;
	}

	private static byte parse_DFFE(KeyboardState state)
	{
		byte b = 0;
		if (state.get_Item((Key)112))
		{
			b = (byte)(b | 1u);
		}
		if (state.get_Item((Key)111))
		{
			b = (byte)(b | 2u);
		}
		if (state.get_Item((Key)105))
		{
			b = (byte)(b | 4u);
		}
		if (state.get_Item((Key)117))
		{
			b = (byte)(b | 8u);
		}
		if (state.get_Item((Key)121))
		{
			b = (byte)(b | 0x10u);
		}
		if (state.get_Item((Key)303))
		{
			if (state.get_Item((Key)39))
			{
				b = (byte)(b | 1u);
			}
		}
		else if (state.get_Item((Key)59))
		{
			b = (byte)(b | 2u);
		}
		return b;
	}

	private static byte parse_EFFE(KeyboardState state)
	{
		byte b = 0;
		if (state.get_Item((Key)48))
		{
			b = (byte)(b | 1u);
		}
		if (state.get_Item((Key)57))
		{
			b = (byte)(b | 2u);
		}
		if (state.get_Item((Key)56))
		{
			b = (byte)(b | 4u);
		}
		if (state.get_Item((Key)55))
		{
			b = (byte)(b | 8u);
		}
		if (state.get_Item((Key)54))
		{
			b = (byte)(b | 0x10u);
		}
		if (state.get_Item((Key)275))
		{
			b = (byte)(b | 4u);
		}
		if (state.get_Item((Key)273))
		{
			b = (byte)(b | 8u);
		}
		if (state.get_Item((Key)274))
		{
			b = (byte)(b | 0x10u);
		}
		if (state.get_Item((Key)8))
		{
			b = (byte)(b | 1u);
		}
		if (state.get_Item((Key)303))
		{
			if (state.get_Item((Key)45))
			{
				b = (byte)(b | 1u);
			}
		}
		else if (state.get_Item((Key)39))
		{
			b = (byte)(b | 8u);
		}
		return b;
	}

	private static byte parse_F7FE(KeyboardState state)
	{
		byte b = 0;
		if (state.get_Item((Key)49))
		{
			b = (byte)(b | 1u);
		}
		if (state.get_Item((Key)50))
		{
			b = (byte)(b | 2u);
		}
		if (state.get_Item((Key)51))
		{
			b = (byte)(b | 4u);
		}
		if (state.get_Item((Key)52))
		{
			b = (byte)(b | 8u);
		}
		if (state.get_Item((Key)53))
		{
			b = (byte)(b | 0x10u);
		}
		if (state.get_Item((Key)276))
		{
			b = (byte)(b | 0x10u);
		}
		return b;
	}

	private static byte parse_FBFE(KeyboardState state)
	{
		byte b = 0;
		if (state.get_Item((Key)113))
		{
			b = (byte)(b | 1u);
		}
		if (state.get_Item((Key)119))
		{
			b = (byte)(b | 2u);
		}
		if (state.get_Item((Key)101))
		{
			b = (byte)(b | 4u);
		}
		if (state.get_Item((Key)114))
		{
			b = (byte)(b | 8u);
		}
		if (state.get_Item((Key)116))
		{
			b = (byte)(b | 0x10u);
		}
		if (state.get_Item((Key)303))
		{
			if (state.get_Item((Key)46))
			{
				b = (byte)(b | 0x10u);
			}
			if (state.get_Item((Key)44))
			{
				b = (byte)(b | 8u);
			}
		}
		return b;
	}

	private static byte parse_FDFE(KeyboardState state)
	{
		byte b = 0;
		if (state.get_Item((Key)97))
		{
			b = (byte)(b | 1u);
		}
		if (state.get_Item((Key)115))
		{
			b = (byte)(b | 2u);
		}
		if (state.get_Item((Key)100))
		{
			b = (byte)(b | 4u);
		}
		if (state.get_Item((Key)102))
		{
			b = (byte)(b | 8u);
		}
		if (state.get_Item((Key)103))
		{
			b = (byte)(b | 0x10u);
		}
		return b;
	}

	private static byte parse_FEFE(KeyboardState state)
	{
		byte b = 0;
		if (state.get_Item((Key)304))
		{
			b = (byte)(b | 1u);
		}
		if (state.get_Item((Key)122))
		{
			b = (byte)(b | 2u);
		}
		if (state.get_Item((Key)120))
		{
			b = (byte)(b | 4u);
		}
		if (state.get_Item((Key)99))
		{
			b = (byte)(b | 8u);
		}
		if (state.get_Item((Key)118))
		{
			b = (byte)(b | 0x10u);
		}
		if (state.get_Item((Key)276))
		{
			b = (byte)(b | 1u);
		}
		if (state.get_Item((Key)275))
		{
			b = (byte)(b | 1u);
		}
		if (state.get_Item((Key)273))
		{
			b = (byte)(b | 1u);
		}
		if (state.get_Item((Key)274))
		{
			b = (byte)(b | 1u);
		}
		if (state.get_Item((Key)8))
		{
			b = (byte)(b | 1u);
		}
		if (state.get_Item((Key)301))
		{
			b = (byte)(b | 1u);
		}
		if (state.get_Item((Key)267))
		{
			b = (byte)(b | 0x10u);
		}
		if (state.get_Item((Key)303))
		{
			if (state.get_Item((Key)59))
			{
				b = (byte)(b | 2u);
			}
			if (state.get_Item((Key)47))
			{
				b = (byte)(b | 8u);
			}
		}
		else if (state.get_Item((Key)47))
		{
			b = (byte)(b | 0x10u);
		}
		return b;
	}
}
