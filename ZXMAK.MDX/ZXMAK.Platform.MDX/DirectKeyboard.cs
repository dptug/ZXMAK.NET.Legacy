using System;
using System.Windows.Forms;
using Microsoft.DirectX.DirectInput;

namespace ZXMAK.Platform.MDX;

public class DirectKeyboard : IDisposable
{
	private Form _form;

	private bool kbdActive;

	private Device diKeyboard;

	private long _state;

	public long State => _state;

	public DirectKeyboard(Form mainForm)
	{
		_form = mainForm;
		if (diKeyboard == null)
		{
			diKeyboard = new Device(SystemGuid.Keyboard);
			diKeyboard.SetCooperativeLevel(mainForm, CooperativeLevelFlags.Foreground | CooperativeLevelFlags.NonExclusive);
			mainForm.Activated += WndActivated;
			mainForm.Deactivate += WndDeactivate;
			WndActivated(null, null);
		}
	}

	public void Dispose()
	{
		if (diKeyboard != null)
		{
			kbdActive = false;
			diKeyboard.Unacquire();
			diKeyboard.Dispose();
			diKeyboard = null;
		}
	}

	public void Scan()
	{
		_state = 0L;
		if (diKeyboard == null)
		{
			return;
		}
		if (!kbdActive)
		{
			WndActivated(null, null);
			return;
		}
		KeyboardState currentKeyboardState;
		try
		{
			currentKeyboardState = diKeyboard.GetCurrentKeyboardState();
		}
		catch
		{
			WndActivated(null, null);
			return;
		}
		_state = parseKeyboardState(currentKeyboardState);
	}

	private void WndActivated(object sender, EventArgs e)
	{
		if (diKeyboard != null)
		{
			try
			{
				diKeyboard.Acquire();
				kbdActive = true;
			}
			catch
			{
				kbdActive = false;
			}
		}
	}

	private void WndDeactivate(object sender, EventArgs e)
	{
		if (diKeyboard != null)
		{
			kbdActive = false;
			try
			{
				diKeyboard.Unacquire();
			}
			catch
			{
			}
		}
	}

	private long parseKeyboardState(KeyboardState state)
	{
		long num = 0L;
		if (state[Key.LeftAlt] || state[Key.RightAlt])
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
		if (state[Key.Space])
		{
			b = (byte)(b | 1u);
		}
		if (state[Key.RightShift])
		{
			b = (byte)(b | 2u);
		}
		if (state[Key.M])
		{
			b = (byte)(b | 4u);
		}
		if (state[Key.N])
		{
			b = (byte)(b | 8u);
		}
		if (state[Key.B])
		{
			b = (byte)(b | 0x10u);
		}
		if (state[Key.CapsLock] || state[Key.NumPadPlus] || state[Key.NumPadMinus] || state[Key.NumPadStar] || state[Key.NumPadSlash])
		{
			b = (byte)(b | 2u);
		}
		if (state[Key.Period] || state[Key.Comma] || state[Key.SemiColon] || state[Key.Apostrophe] || state[Key.Slash] || state[Key.Minus] || state[Key.Equals] || state[Key.LeftBracket] || state[Key.RightBracket])
		{
			b = (byte)(b | 2u);
		}
		if (state[Key.NumPadStar])
		{
			b = (byte)(b | 0x10u);
		}
		if (!state[Key.RightShift])
		{
			if (state[Key.Period])
			{
				b = (byte)(b | 4u);
			}
			if (state[Key.Comma])
			{
				b = (byte)(b | 8u);
			}
		}
		return b;
	}

	private static byte parse_BFFE(KeyboardState state)
	{
		byte b = 0;
		if (state[Key.Return])
		{
			b = (byte)(b | 1u);
		}
		if (state[Key.L])
		{
			b = (byte)(b | 2u);
		}
		if (state[Key.K])
		{
			b = (byte)(b | 4u);
		}
		if (state[Key.J])
		{
			b = (byte)(b | 8u);
		}
		if (state[Key.H])
		{
			b = (byte)(b | 0x10u);
		}
		if (state[Key.NumPadEnter])
		{
			b = (byte)(b | 1u);
		}
		if (state[Key.NumPadMinus])
		{
			b = (byte)(b | 8u);
		}
		if (state[Key.NumPadPlus])
		{
			b = (byte)(b | 4u);
		}
		if (state[Key.RightShift])
		{
			if (state[Key.Equals])
			{
				b = (byte)(b | 4u);
			}
		}
		else
		{
			if (state[Key.Minus])
			{
				b = (byte)(b | 8u);
			}
			if (state[Key.Equals])
			{
				b = (byte)(b | 2u);
			}
		}
		return b;
	}

	private static byte parse_DFFE(KeyboardState state)
	{
		byte b = 0;
		if (state[Key.P])
		{
			b = (byte)(b | 1u);
		}
		if (state[Key.O])
		{
			b = (byte)(b | 2u);
		}
		if (state[Key.I])
		{
			b = (byte)(b | 4u);
		}
		if (state[Key.U])
		{
			b = (byte)(b | 8u);
		}
		if (state[Key.Y])
		{
			b = (byte)(b | 0x10u);
		}
		if (state[Key.RightShift])
		{
			if (state[Key.Apostrophe])
			{
				b = (byte)(b | 1u);
			}
		}
		else if (state[Key.SemiColon])
		{
			b = (byte)(b | 2u);
		}
		return b;
	}

	private static byte parse_EFFE(KeyboardState state)
	{
		byte b = 0;
		if (state[Key.D0])
		{
			b = (byte)(b | 1u);
		}
		if (state[Key.D9])
		{
			b = (byte)(b | 2u);
		}
		if (state[Key.D8])
		{
			b = (byte)(b | 4u);
		}
		if (state[Key.D7])
		{
			b = (byte)(b | 8u);
		}
		if (state[Key.D6])
		{
			b = (byte)(b | 0x10u);
		}
		if (state[Key.Right])
		{
			b = (byte)(b | 4u);
		}
		if (state[Key.UpArrow])
		{
			b = (byte)(b | 8u);
		}
		if (state[Key.DownArrow])
		{
			b = (byte)(b | 0x10u);
		}
		if (state[Key.BackSpace])
		{
			b = (byte)(b | 1u);
		}
		if (state[Key.RightShift])
		{
			if (state[Key.Minus])
			{
				b = (byte)(b | 1u);
			}
		}
		else if (state[Key.Apostrophe])
		{
			b = (byte)(b | 8u);
		}
		return b;
	}

	private static byte parse_F7FE(KeyboardState state)
	{
		byte b = 0;
		if (state[Key.D1])
		{
			b = (byte)(b | 1u);
		}
		if (state[Key.D2])
		{
			b = (byte)(b | 2u);
		}
		if (state[Key.D3])
		{
			b = (byte)(b | 4u);
		}
		if (state[Key.D4])
		{
			b = (byte)(b | 8u);
		}
		if (state[Key.D5])
		{
			b = (byte)(b | 0x10u);
		}
		if (state[Key.Left])
		{
			b = (byte)(b | 0x10u);
		}
		return b;
	}

	private static byte parse_FBFE(KeyboardState state)
	{
		byte b = 0;
		if (state[Key.Q])
		{
			b = (byte)(b | 1u);
		}
		if (state[Key.W])
		{
			b = (byte)(b | 2u);
		}
		if (state[Key.E])
		{
			b = (byte)(b | 4u);
		}
		if (state[Key.R])
		{
			b = (byte)(b | 8u);
		}
		if (state[Key.T])
		{
			b = (byte)(b | 0x10u);
		}
		if (state[Key.RightShift])
		{
			if (state[Key.Period])
			{
				b = (byte)(b | 0x10u);
			}
			if (state[Key.Comma])
			{
				b = (byte)(b | 8u);
			}
		}
		return b;
	}

	private static byte parse_FDFE(KeyboardState state)
	{
		byte b = 0;
		if (state[Key.A])
		{
			b = (byte)(b | 1u);
		}
		if (state[Key.S])
		{
			b = (byte)(b | 2u);
		}
		if (state[Key.D])
		{
			b = (byte)(b | 4u);
		}
		if (state[Key.F])
		{
			b = (byte)(b | 8u);
		}
		if (state[Key.G])
		{
			b = (byte)(b | 0x10u);
		}
		return b;
	}

	private static byte parse_FEFE(KeyboardState state)
	{
		byte b = 0;
		if (state[Key.LeftShift])
		{
			b = (byte)(b | 1u);
		}
		if (state[Key.Z])
		{
			b = (byte)(b | 2u);
		}
		if (state[Key.X])
		{
			b = (byte)(b | 4u);
		}
		if (state[Key.C])
		{
			b = (byte)(b | 8u);
		}
		if (state[Key.V])
		{
			b = (byte)(b | 0x10u);
		}
		if (state[Key.Left])
		{
			b = (byte)(b | 1u);
		}
		if (state[Key.Right])
		{
			b = (byte)(b | 1u);
		}
		if (state[Key.UpArrow])
		{
			b = (byte)(b | 1u);
		}
		if (state[Key.DownArrow])
		{
			b = (byte)(b | 1u);
		}
		if (state[Key.BackSpace])
		{
			b = (byte)(b | 1u);
		}
		if (state[Key.CapsLock])
		{
			b = (byte)(b | 1u);
		}
		if (state[Key.NumPadSlash])
		{
			b = (byte)(b | 0x10u);
		}
		if (state[Key.RightShift])
		{
			if (state[Key.SemiColon])
			{
				b = (byte)(b | 2u);
			}
			if (state[Key.Slash])
			{
				b = (byte)(b | 8u);
			}
		}
		else if (state[Key.Slash])
		{
			b = (byte)(b | 0x10u);
		}
		return b;
	}
}
