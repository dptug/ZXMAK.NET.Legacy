using System;
using SdlDotNet.Input;

namespace ZXMAK.Platform.SDL
{
	public class Keyboard
	{
		public Keyboard()
		{
			this._keyboardState = new KeyboardState();
		}

		public void Scan()
		{
			this._state = 0L;
			this._keyboardState.Update();
			this._state = this.parseKeyboardState(this._keyboardState);
		}

		public long State
		{
			get
			{
				return this._state;
			}
		}

		private long parseKeyboardState(KeyboardState state)
		{
			long num = 0L;
			if (state[308] || state[307])
			{
				return 0L;
			}
			num = (num << 5 | (long)((ulong)Keyboard.parse_7FFE(state)));
			num = (num << 5 | (long)((ulong)Keyboard.parse_BFFE(state)));
			num = (num << 5 | (long)((ulong)Keyboard.parse_DFFE(state)));
			num = (num << 5 | (long)((ulong)Keyboard.parse_EFFE(state)));
			num = (num << 5 | (long)((ulong)Keyboard.parse_F7FE(state)));
			num = (num << 5 | (long)((ulong)Keyboard.parse_FBFE(state)));
			num = (num << 5 | (long)((ulong)Keyboard.parse_FDFE(state)));
			return num << 5 | (long)((ulong)Keyboard.parse_FEFE(state));
		}

		private static byte parse_7FFE(KeyboardState state)
		{
			byte b = 0;
			if (state[32])
			{
				b |= 1;
			}
			if (state[303])
			{
				b |= 2;
			}
			if (state[109])
			{
				b |= 4;
			}
			if (state[110])
			{
				b |= 8;
			}
			if (state[98])
			{
				b |= 16;
			}
			if (state[301] || state[270] || state[269] || state[268] || state[267])
			{
				b |= 2;
			}
			if (state[46] || state[44] || state[59] || state[39] || state[47] || state[45] || state[61] || state[91] || state[93])
			{
				b |= 2;
			}
			if (state[268])
			{
				b |= 16;
			}
			if (!state[303])
			{
				if (state[46])
				{
					b |= 4;
				}
				if (state[44])
				{
					b |= 8;
				}
			}
			return b;
		}

		private static byte parse_BFFE(KeyboardState state)
		{
			byte b = 0;
			if (state[13])
			{
				b |= 1;
			}
			if (state[108])
			{
				b |= 2;
			}
			if (state[107])
			{
				b |= 4;
			}
			if (state[106])
			{
				b |= 8;
			}
			if (state[104])
			{
				b |= 16;
			}
			if (state[271])
			{
				b |= 1;
			}
			if (state[269])
			{
				b |= 8;
			}
			if (state[270])
			{
				b |= 4;
			}
			if (state[303])
			{
				if (state[61])
				{
					b |= 4;
				}
			}
			else
			{
				if (state[45])
				{
					b |= 8;
				}
				if (state[61])
				{
					b |= 2;
				}
			}
			return b;
		}

		private static byte parse_DFFE(KeyboardState state)
		{
			byte b = 0;
			if (state[112])
			{
				b |= 1;
			}
			if (state[111])
			{
				b |= 2;
			}
			if (state[105])
			{
				b |= 4;
			}
			if (state[117])
			{
				b |= 8;
			}
			if (state[121])
			{
				b |= 16;
			}
			if (state[303])
			{
				if (state[39])
				{
					b |= 1;
				}
			}
			else if (state[59])
			{
				b |= 2;
			}
			return b;
		}

		private static byte parse_EFFE(KeyboardState state)
		{
			byte b = 0;
			if (state[48])
			{
				b |= 1;
			}
			if (state[57])
			{
				b |= 2;
			}
			if (state[56])
			{
				b |= 4;
			}
			if (state[55])
			{
				b |= 8;
			}
			if (state[54])
			{
				b |= 16;
			}
			if (state[275])
			{
				b |= 4;
			}
			if (state[273])
			{
				b |= 8;
			}
			if (state[274])
			{
				b |= 16;
			}
			if (state[8])
			{
				b |= 1;
			}
			if (state[303])
			{
				if (state[45])
				{
					b |= 1;
				}
			}
			else if (state[39])
			{
				b |= 8;
			}
			return b;
		}

		private static byte parse_F7FE(KeyboardState state)
		{
			byte b = 0;
			if (state[49])
			{
				b |= 1;
			}
			if (state[50])
			{
				b |= 2;
			}
			if (state[51])
			{
				b |= 4;
			}
			if (state[52])
			{
				b |= 8;
			}
			if (state[53])
			{
				b |= 16;
			}
			if (state[276])
			{
				b |= 16;
			}
			return b;
		}

		private static byte parse_FBFE(KeyboardState state)
		{
			byte b = 0;
			if (state[113])
			{
				b |= 1;
			}
			if (state[119])
			{
				b |= 2;
			}
			if (state[101])
			{
				b |= 4;
			}
			if (state[114])
			{
				b |= 8;
			}
			if (state[116])
			{
				b |= 16;
			}
			if (state[303])
			{
				if (state[46])
				{
					b |= 16;
				}
				if (state[44])
				{
					b |= 8;
				}
			}
			return b;
		}

		private static byte parse_FDFE(KeyboardState state)
		{
			byte b = 0;
			if (state[97])
			{
				b |= 1;
			}
			if (state[115])
			{
				b |= 2;
			}
			if (state[100])
			{
				b |= 4;
			}
			if (state[102])
			{
				b |= 8;
			}
			if (state[103])
			{
				b |= 16;
			}
			return b;
		}

		private static byte parse_FEFE(KeyboardState state)
		{
			byte b = 0;
			if (state[304])
			{
				b |= 1;
			}
			if (state[122])
			{
				b |= 2;
			}
			if (state[120])
			{
				b |= 4;
			}
			if (state[99])
			{
				b |= 8;
			}
			if (state[118])
			{
				b |= 16;
			}
			if (state[276])
			{
				b |= 1;
			}
			if (state[275])
			{
				b |= 1;
			}
			if (state[273])
			{
				b |= 1;
			}
			if (state[274])
			{
				b |= 1;
			}
			if (state[8])
			{
				b |= 1;
			}
			if (state[301])
			{
				b |= 1;
			}
			if (state[267])
			{
				b |= 16;
			}
			if (state[303])
			{
				if (state[59])
				{
					b |= 2;
				}
				if (state[47])
				{
					b |= 8;
				}
			}
			else if (state[47])
			{
				b |= 16;
			}
			return b;
		}

		private long _state;

		private KeyboardState _keyboardState;
	}
}
