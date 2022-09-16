using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ZXMAK.Engine;

namespace ZXMAK.Platform.XNA2.Win;

public class XnaGame : Game, IVideoDevice
{
	private GenericPlatform _platform;

	private GraphicsDeviceManager _graphicsDeviceManager;

	private Texture2D _targetTexture;

	private SpriteBatch _spriteBatch;

	private uint[] _videoBuffer = new uint[76800];

	public XnaGame(GenericPlatform platform)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		_platform = platform;
		_graphicsDeviceManager = new GraphicsDeviceManager((Game)(object)this);
	}

	protected override void Initialize()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		((Game)this).Initialize();
		_spriteBatch = new SpriteBatch(_graphicsDeviceManager.get_GraphicsDevice());
		_platform.VideoManager.SetVideoDevice(this);
	}

	protected override void BeginRun()
	{
		((Game)this).BeginRun();
		_platform.OnStartup();
	}

	protected override void EndRun()
	{
		_platform.OnShutdown();
		((Game)this).EndRun();
	}

	protected unsafe override void Update(GameTime gameTime)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Invalid comparison between Unknown and I4
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Invalid comparison between Unknown and I4
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Invalid comparison between Unknown and I4
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Expected O, but got Unknown
		_platform.Spectrum.KeyboardState = parseKeyboard(Keyboard.GetState());
		MouseState state = Mouse.GetState();
		int num = 0;
		if ((int)((MouseState)(ref state)).get_LeftButton() == 1)
		{
			num |= 1;
		}
		if ((int)((MouseState)(ref state)).get_RightButton() == 1)
		{
			num |= 2;
		}
		if ((int)((MouseState)(ref state)).get_MiddleButton() == 1)
		{
			num |= 4;
		}
		_platform.Spectrum.MouseX = ((MouseState)(ref state)).get_X();
		_platform.Spectrum.MouseY = ((MouseState)(ref state)).get_Y();
		_platform.Spectrum.MouseButtons = num;
		fixed (uint* ptr = _videoBuffer)
		{
			_platform.Spectrum.ExecuteFrame((IntPtr)ptr, IntPtr.Zero);
		}
		if (_targetTexture != null)
		{
			Texture2D val = new Texture2D(_graphicsDeviceManager.get_GraphicsDevice(), _targetTexture.get_Width(), _targetTexture.get_Height(), 1, (TextureUsage)0, (SurfaceFormat)2);
			val.SetData<uint>(_videoBuffer, 0, val.get_Width() * val.get_Height(), (SetDataOptions)0);
			((GraphicsResource)_targetTexture).Dispose();
			_targetTexture = val;
		}
		((Game)this).Update(gameTime);
	}

	protected override void Draw(GameTime gameTime)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		_graphicsDeviceManager.get_GraphicsDevice().Clear(Color.get_Black());
		_spriteBatch.Begin((SpriteBlendMode)1);
		if (_targetTexture != null)
		{
			_spriteBatch.Draw(_targetTexture, new Rectangle(0, 0, _graphicsDeviceManager.get_PreferredBackBufferWidth(), _graphicsDeviceManager.get_PreferredBackBufferHeight()), Color.get_White());
		}
		_spriteBatch.End();
		((Game)this).Draw(gameTime);
	}

	private long parseKeyboard(KeyboardState state)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		long num = 0L;
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
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Invalid comparison between Unknown and I4
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Invalid comparison between Unknown and I4
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Invalid comparison between Unknown and I4
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Invalid comparison between Unknown and I4
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Invalid comparison between Unknown and I4
		byte b = 0;
		if ((int)((KeyboardState)(ref state)).get_Item((Keys)32) == 1)
		{
			b = (byte)(b | 1u);
		}
		if ((int)((KeyboardState)(ref state)).get_Item((Keys)161) == 1)
		{
			b = (byte)(b | 2u);
		}
		if ((int)((KeyboardState)(ref state)).get_Item((Keys)77) == 1)
		{
			b = (byte)(b | 4u);
		}
		if ((int)((KeyboardState)(ref state)).get_Item((Keys)78) == 1)
		{
			b = (byte)(b | 8u);
		}
		if ((int)((KeyboardState)(ref state)).get_Item((Keys)66) == 1)
		{
			b = (byte)(b | 0x10u);
		}
		return b;
	}

	private static byte parse_BFFE(KeyboardState state)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Invalid comparison between Unknown and I4
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Invalid comparison between Unknown and I4
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Invalid comparison between Unknown and I4
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Invalid comparison between Unknown and I4
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Invalid comparison between Unknown and I4
		byte b = 0;
		if ((int)((KeyboardState)(ref state)).get_Item((Keys)13) == 1)
		{
			b = (byte)(b | 1u);
		}
		if ((int)((KeyboardState)(ref state)).get_Item((Keys)76) == 1)
		{
			b = (byte)(b | 2u);
		}
		if ((int)((KeyboardState)(ref state)).get_Item((Keys)75) == 1)
		{
			b = (byte)(b | 4u);
		}
		if ((int)((KeyboardState)(ref state)).get_Item((Keys)74) == 1)
		{
			b = (byte)(b | 8u);
		}
		if ((int)((KeyboardState)(ref state)).get_Item((Keys)72) == 1)
		{
			b = (byte)(b | 0x10u);
		}
		return b;
	}

	private static byte parse_DFFE(KeyboardState state)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Invalid comparison between Unknown and I4
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Invalid comparison between Unknown and I4
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Invalid comparison between Unknown and I4
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Invalid comparison between Unknown and I4
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Invalid comparison between Unknown and I4
		byte b = 0;
		if ((int)((KeyboardState)(ref state)).get_Item((Keys)80) == 1)
		{
			b = (byte)(b | 1u);
		}
		if ((int)((KeyboardState)(ref state)).get_Item((Keys)79) == 1)
		{
			b = (byte)(b | 2u);
		}
		if ((int)((KeyboardState)(ref state)).get_Item((Keys)73) == 1)
		{
			b = (byte)(b | 4u);
		}
		if ((int)((KeyboardState)(ref state)).get_Item((Keys)85) == 1)
		{
			b = (byte)(b | 8u);
		}
		if ((int)((KeyboardState)(ref state)).get_Item((Keys)89) == 1)
		{
			b = (byte)(b | 0x10u);
		}
		return b;
	}

	private static byte parse_EFFE(KeyboardState state)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Invalid comparison between Unknown and I4
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Invalid comparison between Unknown and I4
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Invalid comparison between Unknown and I4
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Invalid comparison between Unknown and I4
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Invalid comparison between Unknown and I4
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Invalid comparison between Unknown and I4
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Invalid comparison between Unknown and I4
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Invalid comparison between Unknown and I4
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Invalid comparison between Unknown and I4
		byte b = 0;
		if ((int)((KeyboardState)(ref state)).get_Item((Keys)48) == 1)
		{
			b = (byte)(b | 1u);
		}
		if ((int)((KeyboardState)(ref state)).get_Item((Keys)57) == 1)
		{
			b = (byte)(b | 2u);
		}
		if ((int)((KeyboardState)(ref state)).get_Item((Keys)56) == 1)
		{
			b = (byte)(b | 4u);
		}
		if ((int)((KeyboardState)(ref state)).get_Item((Keys)55) == 1)
		{
			b = (byte)(b | 8u);
		}
		if ((int)((KeyboardState)(ref state)).get_Item((Keys)54) == 1)
		{
			b = (byte)(b | 0x10u);
		}
		if ((int)((KeyboardState)(ref state)).get_Item((Keys)39) == 1)
		{
			b = (byte)(b | 4u);
		}
		if ((int)((KeyboardState)(ref state)).get_Item((Keys)38) == 1)
		{
			b = (byte)(b | 8u);
		}
		if ((int)((KeyboardState)(ref state)).get_Item((Keys)40) == 1)
		{
			b = (byte)(b | 0x10u);
		}
		if ((int)((KeyboardState)(ref state)).get_Item((Keys)8) == 1)
		{
			b = (byte)(b | 1u);
		}
		return b;
	}

	private static byte parse_F7FE(KeyboardState state)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Invalid comparison between Unknown and I4
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Invalid comparison between Unknown and I4
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Invalid comparison between Unknown and I4
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Invalid comparison between Unknown and I4
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Invalid comparison between Unknown and I4
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Invalid comparison between Unknown and I4
		byte b = 0;
		if ((int)((KeyboardState)(ref state)).get_Item((Keys)49) == 1)
		{
			b = (byte)(b | 1u);
		}
		if ((int)((KeyboardState)(ref state)).get_Item((Keys)50) == 1)
		{
			b = (byte)(b | 2u);
		}
		if ((int)((KeyboardState)(ref state)).get_Item((Keys)51) == 1)
		{
			b = (byte)(b | 4u);
		}
		if ((int)((KeyboardState)(ref state)).get_Item((Keys)52) == 1)
		{
			b = (byte)(b | 8u);
		}
		if ((int)((KeyboardState)(ref state)).get_Item((Keys)53) == 1)
		{
			b = (byte)(b | 0x10u);
		}
		if ((int)((KeyboardState)(ref state)).get_Item((Keys)37) == 1)
		{
			b = (byte)(b | 0x10u);
		}
		return b;
	}

	private static byte parse_FBFE(KeyboardState state)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Invalid comparison between Unknown and I4
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Invalid comparison between Unknown and I4
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Invalid comparison between Unknown and I4
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Invalid comparison between Unknown and I4
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Invalid comparison between Unknown and I4
		byte b = 0;
		if ((int)((KeyboardState)(ref state)).get_Item((Keys)81) == 1)
		{
			b = (byte)(b | 1u);
		}
		if ((int)((KeyboardState)(ref state)).get_Item((Keys)87) == 1)
		{
			b = (byte)(b | 2u);
		}
		if ((int)((KeyboardState)(ref state)).get_Item((Keys)69) == 1)
		{
			b = (byte)(b | 4u);
		}
		if ((int)((KeyboardState)(ref state)).get_Item((Keys)82) == 1)
		{
			b = (byte)(b | 8u);
		}
		if ((int)((KeyboardState)(ref state)).get_Item((Keys)84) == 1)
		{
			b = (byte)(b | 0x10u);
		}
		return b;
	}

	private static byte parse_FDFE(KeyboardState state)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Invalid comparison between Unknown and I4
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Invalid comparison between Unknown and I4
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Invalid comparison between Unknown and I4
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Invalid comparison between Unknown and I4
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Invalid comparison between Unknown and I4
		byte b = 0;
		if ((int)((KeyboardState)(ref state)).get_Item((Keys)65) == 1)
		{
			b = (byte)(b | 1u);
		}
		if ((int)((KeyboardState)(ref state)).get_Item((Keys)83) == 1)
		{
			b = (byte)(b | 2u);
		}
		if ((int)((KeyboardState)(ref state)).get_Item((Keys)68) == 1)
		{
			b = (byte)(b | 4u);
		}
		if ((int)((KeyboardState)(ref state)).get_Item((Keys)70) == 1)
		{
			b = (byte)(b | 8u);
		}
		if ((int)((KeyboardState)(ref state)).get_Item((Keys)71) == 1)
		{
			b = (byte)(b | 0x10u);
		}
		return b;
	}

	private static byte parse_FEFE(KeyboardState state)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Invalid comparison between Unknown and I4
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Invalid comparison between Unknown and I4
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Invalid comparison between Unknown and I4
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Invalid comparison between Unknown and I4
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Invalid comparison between Unknown and I4
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Invalid comparison between Unknown and I4
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Invalid comparison between Unknown and I4
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Invalid comparison between Unknown and I4
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Invalid comparison between Unknown and I4
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Invalid comparison between Unknown and I4
		byte b = 0;
		if ((int)((KeyboardState)(ref state)).get_Item((Keys)160) == 1)
		{
			b = (byte)(b | 1u);
		}
		if ((int)((KeyboardState)(ref state)).get_Item((Keys)90) == 1)
		{
			b = (byte)(b | 2u);
		}
		if ((int)((KeyboardState)(ref state)).get_Item((Keys)88) == 1)
		{
			b = (byte)(b | 4u);
		}
		if ((int)((KeyboardState)(ref state)).get_Item((Keys)67) == 1)
		{
			b = (byte)(b | 8u);
		}
		if ((int)((KeyboardState)(ref state)).get_Item((Keys)86) == 1)
		{
			b = (byte)(b | 0x10u);
		}
		if ((int)((KeyboardState)(ref state)).get_Item((Keys)37) == 1)
		{
			b = (byte)(b | 1u);
		}
		if ((int)((KeyboardState)(ref state)).get_Item((Keys)39) == 1)
		{
			b = (byte)(b | 1u);
		}
		if ((int)((KeyboardState)(ref state)).get_Item((Keys)38) == 1)
		{
			b = (byte)(b | 1u);
		}
		if ((int)((KeyboardState)(ref state)).get_Item((Keys)40) == 1)
		{
			b = (byte)(b | 1u);
		}
		if ((int)((KeyboardState)(ref state)).get_Item((Keys)8) == 1)
		{
			b = (byte)(b | 1u);
		}
		return b;
	}

	void IVideoDevice.SetResolution(VideoManager sender, int width, int height)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		_targetTexture = new Texture2D(_graphicsDeviceManager.get_GraphicsDevice(), width, height, 1, (TextureUsage)0, (SurfaceFormat)2);
		_platform.VideoManager.SetVideoParams(new VideoParams(width, height, width, 32));
	}
}
