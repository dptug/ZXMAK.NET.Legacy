
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using ZXMAK.Engine;

namespace ZXMAK.Platform.XNA2.Win
{
    public class XnaGame : Game, IVideoDevice
    {
        private GenericPlatform _platform;
        private GraphicsDeviceManager _graphicsDeviceManager;
        private Texture2D _targetTexture;
        private SpriteBatch _spriteBatch;
        private uint[] _videoBuffer = new uint[76800];

        public XnaGame(GenericPlatform platform)
        {
            this._platform = platform;
            this._graphicsDeviceManager = new GraphicsDeviceManager((Game)this);
        }

        protected override void Initialize()
        {
            base.Initialize();
            this._spriteBatch = new SpriteBatch(this._graphicsDeviceManager.GraphicsDevice);
            this._platform.VideoManager.SetVideoDevice((IVideoDevice)this);
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

        protected override unsafe void Update(GameTime gameTime)
        {
            this._platform.Spectrum.KeyboardState = this.parseKeyboard(Keyboard.GetState());
            MouseState state = Mouse.GetState();
            int num = 0;
            if (state.LeftButton == ButtonState.Pressed)
                num |= 1;
            if (state.RightButton == ButtonState.Pressed)
                num |= 2;
            if (state.MiddleButton == ButtonState.Pressed)
                num |= 4;
            this._platform.Spectrum.MouseX = state.X;
            this._platform.Spectrum.MouseY = state.Y;
            this._platform.Spectrum.MouseButtons = num;
            fixed (uint* videoPtr = this._videoBuffer)
                this._platform.Spectrum.ExecuteFrame((IntPtr)(void*)videoPtr, IntPtr.Zero);
            if (this._targetTexture != null)
            {
                Texture2D texture2D = new Texture2D(this._graphicsDeviceManager.GraphicsDevice, this._targetTexture.Width, this._targetTexture.Height, 1, TextureUsage.None, SurfaceFormat.Bgr32);
                texture2D.SetData<uint>(this._videoBuffer, 0, texture2D.Width * texture2D.Height, SetDataOptions.None);
                this._targetTexture.Dispose();
                this._targetTexture = texture2D;
            }
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            this._graphicsDeviceManager.GraphicsDevice.Clear(Color.Black);
            this._spriteBatch.Begin(SpriteBlendMode.AlphaBlend);
            if (this._targetTexture != null)
                this._spriteBatch.Draw(this._targetTexture, new Rectangle(0, 0, this._graphicsDeviceManager.PreferredBackBufferWidth, this._graphicsDeviceManager.PreferredBackBufferHeight), Color.White);
            this._spriteBatch.End();
            base.Draw(gameTime);
        }

        private long parseKeyboard(KeyboardState state) => (((((((0L << 5 | (long)XnaGame.parse_7FFE(state)) << 5 | (long)XnaGame.parse_BFFE(state)) << 5 | (long)XnaGame.parse_DFFE(state)) << 5 | (long)XnaGame.parse_EFFE(state)) << 5 | (long)XnaGame.parse_F7FE(state)) << 5 | (long)XnaGame.parse_FBFE(state)) << 5 | (long)XnaGame.parse_FDFE(state)) << 5 | (long)XnaGame.parse_FEFE(state);

        private static byte parse_7FFE(KeyboardState state)
        {
            byte num = 0;
            if (state[Keys.Space] == KeyState.Down)
                num |= (byte)1;
            if (state[Keys.RightShift] == KeyState.Down)
                num |= (byte)2;
            if (state[Keys.M] == KeyState.Down)
                num |= (byte)4;
            if (state[Keys.N] == KeyState.Down)
                num |= (byte)8;
            if (state[Keys.B] == KeyState.Down)
                num |= (byte)16;
            return num;
        }

        private static byte parse_BFFE(KeyboardState state)
        {
            byte bffe = 0;
            if (state[Keys.Enter] == KeyState.Down)
                bffe |= (byte)1;
            if (state[Keys.L] == KeyState.Down)
                bffe |= (byte)2;
            if (state[Keys.K] == KeyState.Down)
                bffe |= (byte)4;
            if (state[Keys.J] == KeyState.Down)
                bffe |= (byte)8;
            if (state[Keys.H] == KeyState.Down)
                bffe |= (byte)16;
            return bffe;
        }

        private static byte parse_DFFE(KeyboardState state)
        {
            byte dffe = 0;
            if (state[Keys.P] == KeyState.Down)
                dffe |= (byte)1;
            if (state[Keys.O] == KeyState.Down)
                dffe |= (byte)2;
            if (state[Keys.I] == KeyState.Down)
                dffe |= (byte)4;
            if (state[Keys.U] == KeyState.Down)
                dffe |= (byte)8;
            if (state[Keys.Y] == KeyState.Down)
                dffe |= (byte)16;
            return dffe;
        }

        private static byte parse_EFFE(KeyboardState state)
        {
            byte effe = 0;
            if (state[Keys.D0] == KeyState.Down)
                effe |= (byte)1;
            if (state[Keys.D9] == KeyState.Down)
                effe |= (byte)2;
            if (state[Keys.D8] == KeyState.Down)
                effe |= (byte)4;
            if (state[Keys.D7] == KeyState.Down)
                effe |= (byte)8;
            if (state[Keys.D6] == KeyState.Down)
                effe |= (byte)16;
            if (state[Keys.Right] == KeyState.Down)
                effe |= (byte)4;
            if (state[Keys.Up] == KeyState.Down)
                effe |= (byte)8;
            if (state[Keys.Down] == KeyState.Down)
                effe |= (byte)16;
            if (state[Keys.Back] == KeyState.Down)
                effe |= (byte)1;
            return effe;
        }

        private static byte parse_F7FE(KeyboardState state)
        {
            byte f7Fe = 0;
            if (state[Keys.D1] == KeyState.Down)
                f7Fe |= (byte)1;
            if (state[Keys.D2] == KeyState.Down)
                f7Fe |= (byte)2;
            if (state[Keys.D3] == KeyState.Down)
                f7Fe |= (byte)4;
            if (state[Keys.D4] == KeyState.Down)
                f7Fe |= (byte)8;
            if (state[Keys.D5] == KeyState.Down)
                f7Fe |= (byte)16;
            if (state[Keys.Left] == KeyState.Down)
                f7Fe |= (byte)16;
            return f7Fe;
        }

        private static byte parse_FBFE(KeyboardState state)
        {
            byte fbfe = 0;
            if (state[Keys.Q] == KeyState.Down)
                fbfe |= (byte)1;
            if (state[Keys.W] == KeyState.Down)
                fbfe |= (byte)2;
            if (state[Keys.E] == KeyState.Down)
                fbfe |= (byte)4;
            if (state[Keys.R] == KeyState.Down)
                fbfe |= (byte)8;
            if (state[Keys.T] == KeyState.Down)
                fbfe |= (byte)16;
            return fbfe;
        }

        private static byte parse_FDFE(KeyboardState state)
        {
            byte fdfe = 0;
            if (state[Keys.A] == KeyState.Down)
                fdfe |= (byte)1;
            if (state[Keys.S] == KeyState.Down)
                fdfe |= (byte)2;
            if (state[Keys.D] == KeyState.Down)
                fdfe |= (byte)4;
            if (state[Keys.F] == KeyState.Down)
                fdfe |= (byte)8;
            if (state[Keys.G] == KeyState.Down)
                fdfe |= (byte)16;
            return fdfe;
        }

        private static byte parse_FEFE(KeyboardState state)
        {
            byte fefe = 0;
            if (state[Keys.LeftShift] == KeyState.Down)
                fefe |= (byte)1;
            if (state[Keys.Z] == KeyState.Down)
                fefe |= (byte)2;
            if (state[Keys.X] == KeyState.Down)
                fefe |= (byte)4;
            if (state[Keys.C] == KeyState.Down)
                fefe |= (byte)8;
            if (state[Keys.V] == KeyState.Down)
                fefe |= (byte)16;
            if (state[Keys.Left] == KeyState.Down)
                fefe |= (byte)1;
            if (state[Keys.Right] == KeyState.Down)
                fefe |= (byte)1;
            if (state[Keys.Up] == KeyState.Down)
                fefe |= (byte)1;
            if (state[Keys.Down] == KeyState.Down)
                fefe |= (byte)1;
            if (state[Keys.Back] == KeyState.Down)
                fefe |= (byte)1;
            return fefe;
        }

        void IVideoDevice.SetResolution(VideoManager sender, int width, int height)
        {
            this._targetTexture = new Texture2D(this._graphicsDeviceManager.GraphicsDevice, width, height, 1, TextureUsage.None, SurfaceFormat.Bgr32);
            this._platform.VideoManager.SetVideoParams(new VideoParams(width, height, width, 32));
        }
    }
}
