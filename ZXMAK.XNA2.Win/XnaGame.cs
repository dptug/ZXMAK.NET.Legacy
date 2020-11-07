using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ZXMAK.Engine;

namespace ZXMAK.Platform.XNA2.Win
{
	public class XnaGame : Game, IVideoDevice
	{
		public XnaGame(GenericPlatform platform)
		{
			this._platform = platform;
			this._graphicsDeviceManager = new GraphicsDeviceManager(this);
		}

		protected override void Initialize()
		{
			base.Initialize();
			this._spriteBatch = new SpriteBatch(this._graphicsDeviceManager.GraphicsDevice);
			this._platform.VideoManager.SetVideoDevice(this);
		}

		protected override void BeginRun()
		{
			base.BeginRun();
			this._platform.OnStartup();
		}

		protected override void EndRun()
		{
			this._platform.OnShutdown();
			base.EndRun();
		}

		protected unsafe override void Update(GameTime gameTime)
		{
			this._platform.Spectrum.KeyboardState = this.parseKeyboard(Keyboard.GetState());
			MouseState state = Mouse.GetState();
			int num = 0;
			if (state.LeftButton == 1)
			{
				num |= 1;
			}
			if (state.RightButton == 1)
			{
				num |= 2;
			}
			if (state.MiddleButton == 1)
			{
				num |= 4;
			}
			this._platform.Spectrum.MouseX = state.X;
			this._platform.Spectrum.MouseY = state.Y;
			this._platform.Spectrum.MouseButtons = num;
			fixed (uint* videoBuffer = this._videoBuffer)
			{
				this._platform.Spectrum.ExecuteFrame((IntPtr)((void*)videoBuffer), IntPtr.Zero);
			}
			if (this._targetTexture != null)
			{
				Texture2D texture2D = new Texture2D(this._graphicsDeviceManager.GraphicsDevice, this._targetTexture.Width, this._targetTexture.Height, 1, 0, 2);
				texture2D.SetData<uint>(this._videoBuffer, 0, texture2D.Width * texture2D.Height, 0);
				this._targetTexture.Dispose();
				this._targetTexture = texture2D;
			}
			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			this._graphicsDeviceManager.GraphicsDevice.Clear(Color.Black);
			this._spriteBatch.Begin(1);
			if (this._targetTexture != null)
			{
				this._spriteBatch.Draw(this._targetTexture, new Rectangle(0, 0, this._graphicsDeviceManager.PreferredBackBufferWidth, this._graphicsDeviceManager.PreferredBackBufferHeight), Color.White);
			}
			this._spriteBatch.End();
			base.Draw(gameTime);
		}

		private long parseKeyboard(KeyboardState state)
		{
			long num = 0L;
			num = (num << 5 | (long)((ulong)XnaGame.parse_7FFE(state)));
			num = (num << 5 | (long)((ulong)XnaGame.parse_BFFE(state)));
			num = (num << 5 | (long)((ulong)XnaGame.parse_DFFE(state)));
			num = (num << 5 | (long)((ulong)XnaGame.parse_EFFE(state)));
			num = (num << 5 | (long)((ulong)XnaGame.parse_F7FE(state)));
			num = (num << 5 | (long)((ulong)XnaGame.parse_FBFE(state)));
			num = (num << 5 | (long)((ulong)XnaGame.parse_FDFE(state)));
			return num << 5 | (long)((ulong)XnaGame.parse_FEFE(state));
		}

		private static byte parse_7FFE(KeyboardState state)
		{
			byte b = 0;
			if (state[32] == 1)
			{
				b |= 1;
			}
			if (state[161] == 1)
			{
				b |= 2;
			}
			if (state[77] == 1)
			{
				b |= 4;
			}
			if (state[78] == 1)
			{
				b |= 8;
			}
			if (state[66] == 1)
			{
				b |= 16;
			}
			return b;
		}

		private static byte parse_BFFE(KeyboardState state)
		{
			byte b = 0;
			if (state[13] == 1)
			{
				b |= 1;
			}
			if (state[76] == 1)
			{
				b |= 2;
			}
			if (state[75] == 1)
			{
				b |= 4;
			}
			if (state[74] == 1)
			{
				b |= 8;
			}
			if (state[72] == 1)
			{
				b |= 16;
			}
			return b;
		}

		private static byte parse_DFFE(KeyboardState state)
		{
			byte b = 0;
			if (state[80] == 1)
			{
				b |= 1;
			}
			if (state[79] == 1)
			{
				b |= 2;
			}
			if (state[73] == 1)
			{
				b |= 4;
			}
			if (state[85] == 1)
			{
				b |= 8;
			}
			if (state[89] == 1)
			{
				b |= 16;
			}
			return b;
		}

		private static byte parse_EFFE(KeyboardState state)
		{
			byte b = 0;
			if (state[48] == 1)
			{
				b |= 1;
			}
			if (state[57] == 1)
			{
				b |= 2;
			}
			if (state[56] == 1)
			{
				b |= 4;
			}
			if (state[55] == 1)
			{
				b |= 8;
			}
			if (state[54] == 1)
			{
				b |= 16;
			}
			if (state[39] == 1)
			{
				b |= 4;
			}
			if (state[38] == 1)
			{
				b |= 8;
			}
			if (state[40] == 1)
			{
				b |= 16;
			}
			if (state[8] == 1)
			{
				b |= 1;
			}
			return b;
		}

		private static byte parse_F7FE(KeyboardState state)
		{
			byte b = 0;
			if (state[49] == 1)
			{
				b |= 1;
			}
			if (state[50] == 1)
			{
				b |= 2;
			}
			if (state[51] == 1)
			{
				b |= 4;
			}
			if (state[52] == 1)
			{
				b |= 8;
			}
			if (state[53] == 1)
			{
				b |= 16;
			}
			if (state[37] == 1)
			{
				b |= 16;
			}
			return b;
		}

		private static byte parse_FBFE(KeyboardState state)
		{
			byte b = 0;
			if (state[81] == 1)
			{
				b |= 1;
			}
			if (state[87] == 1)
			{
				b |= 2;
			}
			if (state[69] == 1)
			{
				b |= 4;
			}
			if (state[82] == 1)
			{
				b |= 8;
			}
			if (state[84] == 1)
			{
				b |= 16;
			}
			return b;
		}

		private static byte parse_FDFE(KeyboardState state)
		{
			byte b = 0;
			if (state[65] == 1)
			{
				b |= 1;
			}
			if (state[83] == 1)
			{
				b |= 2;
			}
			if (state[68] == 1)
			{
				b |= 4;
			}
			if (state[70] == 1)
			{
				b |= 8;
			}
			if (state[71] == 1)
			{
				b |= 16;
			}
			return b;
		}

		private static byte parse_FEFE(KeyboardState state)
		{
			byte b = 0;
			if (state[160] == 1)
			{
				b |= 1;
			}
			if (state[90] == 1)
			{
				b |= 2;
			}
			if (state[88] == 1)
			{
				b |= 4;
			}
			if (state[67] == 1)
			{
				b |= 8;
			}
			if (state[86] == 1)
			{
				b |= 16;
			}
			if (state[37] == 1)
			{
				b |= 1;
			}
			if (state[39] == 1)
			{
				b |= 1;
			}
			if (state[38] == 1)
			{
				b |= 1;
			}
			if (state[40] == 1)
			{
				b |= 1;
			}
			if (state[8] == 1)
			{
				b |= 1;
			}
			return b;
		}

		void IVideoDevice.SetResolution(VideoManager sender, int width, int height)
		{
			this._targetTexture = new Texture2D(this._graphicsDeviceManager.GraphicsDevice, width, height, 1, 0, 2);
			this._platform.VideoManager.SetVideoParams(new VideoParams(width, height, width, 32));
		}

		private GenericPlatform _platform;

		private GraphicsDeviceManager _graphicsDeviceManager;

		private Texture2D _targetTexture;

		private SpriteBatch _spriteBatch;

		private uint[] _videoBuffer = new uint[76800];
	}
}
