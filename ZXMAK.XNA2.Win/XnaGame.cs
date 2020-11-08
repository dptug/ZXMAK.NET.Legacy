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
			if (state.LeftButton == (Microsoft.Xna.Framework.Input.ButtonState)(object)(1))
			{
				num |= 1;
			}
			if (state.RightButton == (Microsoft.Xna.Framework.Input.ButtonState)(object)(1))
			{
				num |= 2;
			}
			if (state.MiddleButton == (Microsoft.Xna.Framework.Input.ButtonState)(object)(1))
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
				Texture2D texture2D = new Texture2D(this._graphicsDeviceManager.GraphicsDevice, this._targetTexture.Width, this._targetTexture.Height, Convert.ToBoolean(1), 0, 2);
				texture2D.SetData<uint>(this._videoBuffer, 0, texture2D.Width * texture2D.Height);
				this._targetTexture.Dispose();
				this._targetTexture = texture2D;
			}
			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			this._graphicsDeviceManager.GraphicsDevice.Clear(Color.Black);
			this._spriteBatch.Begin((Microsoft.Xna.Framework.Graphics.SpriteSortMode)(object)(1));
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
			if (state[(Microsoft.Xna.Framework.Input.Keys)(object)(32)] == (Microsoft.Xna.Framework.Input.KeyState)(object)(1))
			{
				b |= 1;
			}
			if (state[(Microsoft.Xna.Framework.Input.Keys)(object)(161)] == (Microsoft.Xna.Framework.Input.KeyState)(object)(1))
			{
				b |= 2;
			}
			if (state[(Microsoft.Xna.Framework.Input.Keys)(object)(77)] == (Microsoft.Xna.Framework.Input.KeyState)(object)(1))
			{
				b |= 4;
			}
			if (state[(Microsoft.Xna.Framework.Input.Keys)(object)(78)] == (Microsoft.Xna.Framework.Input.KeyState)(object)(1))
			{
				b |= 8;
			}
			if (state[(Microsoft.Xna.Framework.Input.Keys)(object)(66)] == (Microsoft.Xna.Framework.Input.KeyState)(object)(1))
			{
				b |= 16;
			}
			return b;
		}

		private static byte parse_BFFE(KeyboardState state)
		{
			byte b = 0;
			if (state[(Microsoft.Xna.Framework.Input.Keys)(object)(13)] == (Microsoft.Xna.Framework.Input.KeyState)(object)(1))
			{
				b |= 1;
			}
			if (state[(Microsoft.Xna.Framework.Input.Keys)(object)(76)] == (Microsoft.Xna.Framework.Input.KeyState)(object)(1))
			{
				b |= 2;
			}
			if (state[(Microsoft.Xna.Framework.Input.Keys)(object)(75)] == (Microsoft.Xna.Framework.Input.KeyState)(object)(1))
			{
				b |= 4;
			}
			if (state[(Microsoft.Xna.Framework.Input.Keys)(object)(74)] == (Microsoft.Xna.Framework.Input.KeyState)(object)(1))
			{
				b |= 8;
			}
			if (state[(Microsoft.Xna.Framework.Input.Keys)(object)(72)] == (Microsoft.Xna.Framework.Input.KeyState)(object)(1))
			{
				b |= 16;
			}
			return b;
		}

		private static byte parse_DFFE(KeyboardState state)
		{
			byte b = 0;
			if (state[(Microsoft.Xna.Framework.Input.Keys)(object)(80)] == (Microsoft.Xna.Framework.Input.KeyState)(object)(1))
			{
				b |= 1;
			}
			if (state[(Microsoft.Xna.Framework.Input.Keys)(object)(79)] == (Microsoft.Xna.Framework.Input.KeyState)(object)(1))
			{
				b |= 2;
			}
			if (state[(Microsoft.Xna.Framework.Input.Keys)(object)(73)] == (Microsoft.Xna.Framework.Input.KeyState)(object)(1))
			{
				b |= 4;
			}
			if (state[(Microsoft.Xna.Framework.Input.Keys)(object)(85)] == (Microsoft.Xna.Framework.Input.KeyState)(object)(1))
			{
				b |= 8;
			}
			if (state[(Microsoft.Xna.Framework.Input.Keys)(object)(89)] == (Microsoft.Xna.Framework.Input.KeyState)(object)(1))
			{
				b |= 16;
			}
			return b;
		}

		private static byte parse_EFFE(KeyboardState state)
		{
			byte b = 0;
			if (state[(Microsoft.Xna.Framework.Input.Keys)(object)(48)] == (Microsoft.Xna.Framework.Input.KeyState)(object)(1))
			{
				b |= 1;
			}
			if (state[(Microsoft.Xna.Framework.Input.Keys)(object)(57)] == (Microsoft.Xna.Framework.Input.KeyState)(object)(1))
			{
				b |= 2;
			}
			if (state[(Microsoft.Xna.Framework.Input.Keys)(object)(56)] == (Microsoft.Xna.Framework.Input.KeyState)(object)(1))
			{
				b |= 4;
			}
			if (state[(Microsoft.Xna.Framework.Input.Keys)(object)(55)] == (Microsoft.Xna.Framework.Input.KeyState)(object)(1))
			{
				b |= 8;
			}
			if (state[(Microsoft.Xna.Framework.Input.Keys)(object)(54)] == (Microsoft.Xna.Framework.Input.KeyState)(object)(1))
			{
				b |= 16;
			}
			if (state[(Microsoft.Xna.Framework.Input.Keys)(object)(39)] == (Microsoft.Xna.Framework.Input.KeyState)(object)(1))
			{
				b |= 4;
			}
			if (state[(Microsoft.Xna.Framework.Input.Keys)(object)(38)] == (Microsoft.Xna.Framework.Input.KeyState)(object)(1))
			{
				b |= 8;
			}
			if (state[(Microsoft.Xna.Framework.Input.Keys)(object)(40)] == (Microsoft.Xna.Framework.Input.KeyState)(object)(1))
			{
				b |= 16;
			}
			if (state[(Microsoft.Xna.Framework.Input.Keys)(object)(8)] == (Microsoft.Xna.Framework.Input.KeyState)(object)(1))
			{
				b |= 1;
			}
			return b;
		}

		private static byte parse_F7FE(KeyboardState state)
		{
			byte b = 0;
			if (state[(Microsoft.Xna.Framework.Input.Keys)(object)(49)] == (Microsoft.Xna.Framework.Input.KeyState)(object)(1))
			{
				b |= 1;
			}
			if (state[(Microsoft.Xna.Framework.Input.Keys)(object)(50)] == (Microsoft.Xna.Framework.Input.KeyState)(object)(1))
			{
				b |= 2;
			}
			if (state[(Microsoft.Xna.Framework.Input.Keys)(object)(51)] == (Microsoft.Xna.Framework.Input.KeyState)(object)(1))
			{
				b |= 4;
			}
			if (state[(Microsoft.Xna.Framework.Input.Keys)(object)(52)] == (Microsoft.Xna.Framework.Input.KeyState)(object)(1))
			{
				b |= 8;
			}
			if (state[(Microsoft.Xna.Framework.Input.Keys)(object)(53)] == (Microsoft.Xna.Framework.Input.KeyState)(object)(1))
			{
				b |= 16;
			}
			if (state[(Microsoft.Xna.Framework.Input.Keys)(object)(37)] == (Microsoft.Xna.Framework.Input.KeyState)(object)(1))
			{
				b |= 16;
			}
			return b;
		}

		private static byte parse_FBFE(KeyboardState state)
		{
			byte b = 0;
			if (state[(Microsoft.Xna.Framework.Input.Keys)(object)(81)] == (Microsoft.Xna.Framework.Input.KeyState)(object)(1))
			{
				b |= 1;
			}
			if (state[(Microsoft.Xna.Framework.Input.Keys)(object)(87)] == (Microsoft.Xna.Framework.Input.KeyState)(object)(1))
			{
				b |= 2;
			}
			if (state[(Microsoft.Xna.Framework.Input.Keys)(object)(69)] == (Microsoft.Xna.Framework.Input.KeyState)(object)(1))
			{
				b |= 4;
			}
			if (state[(Microsoft.Xna.Framework.Input.Keys)(object)(82)] == (Microsoft.Xna.Framework.Input.KeyState)(object)(1))
			{
				b |= 8;
			}
			if (state[(Microsoft.Xna.Framework.Input.Keys)(object)(84)] == (Microsoft.Xna.Framework.Input.KeyState)(object)(1))
			{
				b |= 16;
			}
			return b;
		}

		private static byte parse_FDFE(KeyboardState state)
		{
			byte b = 0;
			if (state[(Microsoft.Xna.Framework.Input.Keys)(object)(65)] == (Microsoft.Xna.Framework.Input.KeyState)(object)(1))
			{
				b |= 1;
			}
			if (state[(Microsoft.Xna.Framework.Input.Keys)(object)(83)] == (Microsoft.Xna.Framework.Input.KeyState)(object)(1))
			{
				b |= 2;
			}
			if (state[(Microsoft.Xna.Framework.Input.Keys)(object)(68)] == (Microsoft.Xna.Framework.Input.KeyState)(object)(1))
			{
				b |= 4;
			}
			if (state[(Microsoft.Xna.Framework.Input.Keys)(object)(70)] == (Microsoft.Xna.Framework.Input.KeyState)(object)(1))
			{
				b |= 8;
			}
			if (state[(Microsoft.Xna.Framework.Input.Keys)(object)(71)] == (Microsoft.Xna.Framework.Input.KeyState)(object)(1))
			{
				b |= 16;
			}
			return b;
		}

		private static byte parse_FEFE(KeyboardState state)
		{
			byte b = 0;
			if (state[(Microsoft.Xna.Framework.Input.Keys)(object)(160)] == (Microsoft.Xna.Framework.Input.KeyState)(object)(1))
			{
				b |= 1;
			}
			if (state[(Microsoft.Xna.Framework.Input.Keys)(object)(90)] == (Microsoft.Xna.Framework.Input.KeyState)(object)(1))
			{
				b |= 2;
			}
			if (state[(Microsoft.Xna.Framework.Input.Keys)(object)(88)] == (Microsoft.Xna.Framework.Input.KeyState)(object)(1))
			{
				b |= 4;
			}
			if (state[(Microsoft.Xna.Framework.Input.Keys)(object)(67)] == (Microsoft.Xna.Framework.Input.KeyState)(object)(1))
			{
				b |= 8;
			}
			if (state[(Microsoft.Xna.Framework.Input.Keys)(object)(86)] == (Microsoft.Xna.Framework.Input.KeyState)(object)(1))
			{
				b |= 16;
			}
			if (state[(Microsoft.Xna.Framework.Input.Keys)(object)(37)] == (Microsoft.Xna.Framework.Input.KeyState)(object)(1))
			{
				b |= 1;
			}
			if (state[(Microsoft.Xna.Framework.Input.Keys)(object)(39)] == (Microsoft.Xna.Framework.Input.KeyState)(object)(1))
			{
				b |= 1;
			}
			if (state[(Microsoft.Xna.Framework.Input.Keys)(object)(38)] == (Microsoft.Xna.Framework.Input.KeyState)(object)(1))
			{
				b |= 1;
			}
			if (state[(Microsoft.Xna.Framework.Input.Keys)(object)(40)] == (Microsoft.Xna.Framework.Input.KeyState)(object)(1))
			{
				b |= 1;
			}
			if (state[(Microsoft.Xna.Framework.Input.Keys)(object)(8)] == (Microsoft.Xna.Framework.Input.KeyState)(object)(1))
			{
				b |= 1;
			}
			return b;
		}

		void IVideoDevice.SetResolution(VideoManager sender, int width, int height)
		{
			this._targetTexture = new Texture2D(this._graphicsDeviceManager.GraphicsDevice, width, height, Convert.ToBoolean(1), 0, 2);
			this._platform.VideoManager.SetVideoParams(new VideoParams(width, height, width, 32));
		}

		private GenericPlatform _platform;

		private GraphicsDeviceManager _graphicsDeviceManager;

		private Texture2D _targetTexture;

		private SpriteBatch _spriteBatch;

		private uint[] _videoBuffer = new uint[76800];
	}
}
