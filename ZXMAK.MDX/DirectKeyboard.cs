using System;
using System.Windows.Forms;
using SharpDX.DirectInput;

namespace ZXMAK.Platform.MDX
{
	public class DirectKeyboard : IDisposable
	{
		public DirectKeyboard(Form mainForm)
		{
			this._form = mainForm;
			if (this.diKeyboard == null)
			{
				this.diKeyboard = new Device(SystemGuid.Keyboard);
				this.diKeyboard.SetCooperativeLevel(mainForm, CooperativeLevelFlags.Foreground | CooperativeLevelFlags.NonExclusive);
				mainForm.Activated += this.WndActivated;
				mainForm.Deactivate += this.WndDeactivate;
				this.WndActivated(null, null);
			}
		}

		public void Dispose()
		{
			if (this.diKeyboard != null)
			{
				this.kbdActive = false;
				this.diKeyboard.Unacquire();
				this.diKeyboard.Dispose();
				this.diKeyboard = null;
			}
		}

		public void Scan()
		{
			this._state = 0L;
			if (this.diKeyboard == null)
			{
				return;
			}
			if (!this.kbdActive)
			{
				this.WndActivated(null, null);
				return;
			}
			KeyboardState currentKeyboardState;
			try
			{
				currentKeyboardState = this.diKeyboard.GetCurrentKeyboardState();
			}
			catch
			{
				this.WndActivated(null, null);
				return;
			}
			this._state = this.parseKeyboardState(currentKeyboardState);
		}

		public long State
		{
			get
			{
				return this._state;
			}
		}

		private void WndActivated(object sender, EventArgs e)
		{
			if (this.diKeyboard != null)
			{
				try
				{
					this.diKeyboard.Acquire();
					this.kbdActive = true;
				}
				catch
				{
					this.kbdActive = false;
				}
			}
		}

		private void WndDeactivate(object sender, EventArgs e)
		{
			if (this.diKeyboard != null)
			{
				this.kbdActive = false;
				try
				{
					this.diKeyboard.Unacquire();
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
			num = (num << 5 | (long)((ulong)DirectKeyboard.parse_7FFE(state)));
			num = (num << 5 | (long)((ulong)DirectKeyboard.parse_BFFE(state)));
			num = (num << 5 | (long)((ulong)DirectKeyboard.parse_DFFE(state)));
			num = (num << 5 | (long)((ulong)DirectKeyboard.parse_EFFE(state)));
			num = (num << 5 | (long)((ulong)DirectKeyboard.parse_F7FE(state)));
			num = (num << 5 | (long)((ulong)DirectKeyboard.parse_FBFE(state)));
			num = (num << 5 | (long)((ulong)DirectKeyboard.parse_FDFE(state)));
			return num << 5 | (long)((ulong)DirectKeyboard.parse_FEFE(state));
		}

		private static byte parse_7FFE(KeyboardState state)
		{
			byte b = 0;
			if (state[Key.Space])
			{
				b |= 1;
			}
			if (state[Key.RightShift])
			{
				b |= 2;
			}
			if (state[Key.M])
			{
				b |= 4;
			}
			if (state[Key.N])
			{
				b |= 8;
			}
			if (state[Key.B])
			{
				b |= 16;
			}
			if (state[Key.CapsLock] || state[Key.NumPadPlus] || state[Key.NumPadMinus] || state[Key.NumPadStar] || state[Key.NumPadSlash])
			{
				b |= 2;
			}
			if (state[Key.Period] || state[Key.Comma] || state[Key.SemiColon] || state[Key.Apostrophe] || state[Key.Slash] || state[Key.Minus] || state[Key.Equals] || state[Key.LeftBracket] || state[Key.RightBracket])
			{
				b |= 2;
			}
			if (state[Key.NumPadStar])
			{
				b |= 16;
			}
			if (!state[Key.RightShift])
			{
				if (state[Key.Period])
				{
					b |= 4;
				}
				if (state[Key.Comma])
				{
					b |= 8;
				}
			}
			return b;
		}

		private static byte parse_BFFE(KeyboardState state)
		{
			byte b = 0;
			if (state[Key.Return])
			{
				b |= 1;
			}
			if (state[Key.L])
			{
				b |= 2;
			}
			if (state[Key.K])
			{
				b |= 4;
			}
			if (state[Key.J])
			{
				b |= 8;
			}
			if (state[Key.H])
			{
				b |= 16;
			}
			if (state[Key.NumPadEnter])
			{
				b |= 1;
			}
			if (state[Key.NumPadMinus])
			{
				b |= 8;
			}
			if (state[Key.NumPadPlus])
			{
				b |= 4;
			}
			if (state[Key.RightShift])
			{
				if (state[Key.Equals])
				{
					b |= 4;
				}
			}
			else
			{
				if (state[Key.Minus])
				{
					b |= 8;
				}
				if (state[Key.Equals])
				{
					b |= 2;
				}
			}
			return b;
		}

		private static byte parse_DFFE(KeyboardState state)
		{
			byte b = 0;
			if (state[Key.P])
			{
				b |= 1;
			}
			if (state[Key.O])
			{
				b |= 2;
			}
			if (state[Key.I])
			{
				b |= 4;
			}
			if (state[Key.U])
			{
				b |= 8;
			}
			if (state[Key.Y])
			{
				b |= 16;
			}
			if (state[Key.RightShift])
			{
				if (state[Key.Apostrophe])
				{
					b |= 1;
				}
			}
			else if (state[Key.SemiColon])
			{
				b |= 2;
			}
			return b;
		}

		private static byte parse_EFFE(KeyboardState state)
		{
			byte b = 0;
			if (state[Key.D0])
			{
				b |= 1;
			}
			if (state[Key.D9])
			{
				b |= 2;
			}
			if (state[Key.D8])
			{
				b |= 4;
			}
			if (state[Key.D7])
			{
				b |= 8;
			}
			if (state[Key.D6])
			{
				b |= 16;
			}
			if (state[Key.Right])
			{
				b |= 4;
			}
			if (state[Key.UpArrow])
			{
				b |= 8;
			}
			if (state[Key.DownArrow])
			{
				b |= 16;
			}
			if (state[Key.BackSpace])
			{
				b |= 1;
			}
			if (state[Key.RightShift])
			{
				if (state[Key.Minus])
				{
					b |= 1;
				}
			}
			else if (state[Key.Apostrophe])
			{
				b |= 8;
			}
			return b;
		}

		private static byte parse_F7FE(KeyboardState state)
		{
			byte b = 0;
			if (state[Key.D1])
			{
				b |= 1;
			}
			if (state[Key.D2])
			{
				b |= 2;
			}
			if (state[Key.D3])
			{
				b |= 4;
			}
			if (state[Key.D4])
			{
				b |= 8;
			}
			if (state[Key.D5])
			{
				b |= 16;
			}
			if (state[Key.Left])
			{
				b |= 16;
			}
			return b;
		}

		private static byte parse_FBFE(KeyboardState state)
		{
			byte b = 0;
			if (state[Key.Q])
			{
				b |= 1;
			}
			if (state[Key.W])
			{
				b |= 2;
			}
			if (state[Key.E])
			{
				b |= 4;
			}
			if (state[Key.R])
			{
				b |= 8;
			}
			if (state[Key.T])
			{
				b |= 16;
			}
			if (state[Key.RightShift])
			{
				if (state[Key.Period])
				{
					b |= 16;
				}
				if (state[Key.Comma])
				{
					b |= 8;
				}
			}
			return b;
		}

		private static byte parse_FDFE(KeyboardState state)
		{
			byte b = 0;
			if (state[Key.A])
			{
				b |= 1;
			}
			if (state[Key.S])
			{
				b |= 2;
			}
			if (state[Key.D])
			{
				b |= 4;
			}
			if (state[Key.F])
			{
				b |= 8;
			}
			if (state[Key.G])
			{
				b |= 16;
			}
			return b;
		}

		private static byte parse_FEFE(KeyboardState state)
		{
			byte b = 0;
			if (state[Key.LeftShift])
			{
				b |= 1;
			}
			if (state[Key.Z])
			{
				b |= 2;
			}
			if (state[Key.X])
			{
				b |= 4;
			}
			if (state[Key.C])
			{
				b |= 8;
			}
			if (state[Key.V])
			{
				b |= 16;
			}
			if (state[Key.Left])
			{
				b |= 1;
			}
			if (state[Key.Right])
			{
				b |= 1;
			}
			if (state[Key.UpArrow])
			{
				b |= 1;
			}
			if (state[Key.DownArrow])
			{
				b |= 1;
			}
			if (state[Key.BackSpace])
			{
				b |= 1;
			}
			if (state[Key.CapsLock])
			{
				b |= 1;
			}
			if (state[Key.NumPadSlash])
			{
				b |= 16;
			}
			if (state[Key.RightShift])
			{
				if (state[Key.SemiColon])
				{
					b |= 2;
				}
				if (state[Key.Slash])
				{
					b |= 8;
				}
			}
			else if (state[Key.Slash])
			{
				b |= 16;
			}
			return b;
		}

		private Form _form;

		private bool kbdActive;

		private Device diKeyboard;

		private long _state;
	}
}
