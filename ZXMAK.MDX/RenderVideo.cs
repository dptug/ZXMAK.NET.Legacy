using System;
using System.Drawing;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D9;
using ZXMAK.Engine;

namespace ZXMAK.Platform.MDX
{
	public class RenderVideo : Render3D, IVideoDevice
	{
		public event DrawFrameDelegate DrawFrame;

		public VideoManager VideoManager
		{
			get
			{
				return this._videoManager;
			}
			set
			{
				this._videoManager = value;
				if (this._videoManager != null)
				{
					this._videoManager.SetVideoDevice(this);
				}
			}
		}

		protected override void OnCreateDevice()
		{
			this._sprite = new Sprite(this.D3D);
			if (this._videoManager != null)
			{
				this.ResolutionChanged(this._videoManager, this._videoManager.Width, this._videoManager.Height);
			}
		}

		protected override void OnDestroyDevice()
		{
			if (this._texture != null)
			{
				this._texture.Dispose();
			}
			this._sprite.Dispose();
		}

		protected override void OnUpdateScene()
		{
			if (this._texture != null)
			{
				using (GraphicsStream graphicsStream = this._texture.LockRectangle(0, LockFlags.None))
				{
					this.OnDrawFrame(graphicsStream.InternalData);
				}
				this._texture.UnlockRectangle(0);
			}
		}

		protected override void OnRenderScene()
		{
			if (this._texture != null)
			{
				this._sprite.Begin(SpriteFlags.None);
				if (!this.AntiAlias)
				{
					this.D3D.SamplerState[0].MinFilter = TextureFilter.None;
					this.D3D.SamplerState[0].MagFilter = TextureFilter.None;
				}
				this._sprite.Draw2D(this._texture, new Rectangle(0, 0, this._videoManager.Width, this._videoManager.Height), new SizeF((float)this.D3D.PresentationParameters.BackBufferWidth, (float)this.D3D.PresentationParameters.BackBufferHeight), new PointF(0f, 0f), 16777215);
				this._sprite.End();
			}
		}

		protected unsafe virtual void OnDrawFrame(IntPtr ptr)
		{
			if (this.DrawFrame != null)
			{
				this.DrawFrame(this, ptr);
				return;
			}
			uint* ptr2 = (uint*)((void*)ptr);
			for (int i = 0; i < 76800; i++)
			{
				ptr2[i] = 0U;
			}
		}

		private void ResolutionChanged(VideoManager sender, int width, int height)
		{
			if (this.D3D == null || width < 1 || height < 1)
			{
				return;
			}
			int num = (width > height) ? width : height;
			int num2 = 1;
			int num3;
			while ((num3 = RenderVideo.pow(2, num2)) < num)
			{
				num2++;
			}
			if (this._texture != null)
			{
				this._texture.Dispose();
				this._texture = null;
			}
			this._texture = new Texture(this.D3D, num3, num3, 1, Usage.None, Format.A8R8G8B8, Pool.Managed);
			this._videoManager.SetVideoParams(new VideoParams(width, height, num3, 32));
		}

		private static int pow(int value, int power)
		{
			int num = value;
			for (int i = 0; i < power; i++)
			{
				num *= value;
			}
			return num;
		}

		void IVideoDevice.SetResolution(VideoManager sender, int width, int height)
		{
			this.ResolutionChanged(sender, width, height);
		}

		private Sprite _sprite;

		private Texture _texture;

		private VideoManager _videoManager;

		public bool AntiAlias;
	}
}
