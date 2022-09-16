using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using ZXMAK.Engine;

namespace ZXMAK.Platform.MDX;

public class RenderVideo : Render3D, IVideoDevice
{
	private Sprite _sprite;

	private Texture _texture;

	private VideoManager _videoManager;

	public bool AntiAlias;

	public VideoManager VideoManager
	{
		get
		{
			return _videoManager;
		}
		set
		{
			_videoManager = value;
			if (_videoManager != null)
			{
				_videoManager.SetVideoDevice(this);
			}
		}
	}

	public event DrawFrameDelegate DrawFrame;

	protected override void OnCreateDevice()
	{
		_sprite = new Sprite(D3D);
		if (_videoManager != null)
		{
			ResolutionChanged(_videoManager, _videoManager.Width, _videoManager.Height);
		}
	}

	protected override void OnDestroyDevice()
	{
		if (_texture != null)
		{
			_texture.Dispose();
		}
		_sprite.Dispose();
	}

	protected override void OnUpdateScene()
	{
		if (_texture != null)
		{
			using (GraphicsStream graphicsStream = _texture.LockRectangle(0, LockFlags.None))
			{
				OnDrawFrame(graphicsStream.InternalData);
			}
			_texture.UnlockRectangle(0);
		}
	}

	protected override void OnRenderScene()
	{
		if (_texture != null)
		{
			_sprite.Begin(SpriteFlags.None);
			if (!AntiAlias)
			{
				D3D.SamplerState[0].MinFilter = TextureFilter.None;
				D3D.SamplerState[0].MagFilter = TextureFilter.None;
			}
			_sprite.Draw2D(_texture, new Rectangle(0, 0, _videoManager.Width, _videoManager.Height), new SizeF(D3D.PresentationParameters.BackBufferWidth, D3D.PresentationParameters.BackBufferHeight), new PointF(0f, 0f), 16777215);
			_sprite.End();
		}
	}

	protected unsafe virtual void OnDrawFrame(IntPtr ptr)
	{
		if (this.DrawFrame != null)
		{
			this.DrawFrame(this, ptr);
			return;
		}
		uint* ptr2 = (uint*)(void*)ptr;
		for (int i = 0; i < 76800; i++)
		{
			ptr2[i] = 0u;
		}
	}

	private void ResolutionChanged(VideoManager sender, int width, int height)
	{
		if (!(D3D == null) && width >= 1 && height >= 1)
		{
			int num = ((width > height) ? width : height);
			int num2 = 1;
			int pitch;
			while ((pitch = pow(2, num2)) < num)
			{
				num2++;
			}
			if (_texture != null)
			{
				_texture.Dispose();
				_texture = null;
			}
			_texture = new Texture(D3D, pitch, pitch, 1, Usage.None, Format.A8R8G8B8, Pool.Managed);
			_videoManager.SetVideoParams(new VideoParams(width, height, pitch, 32));
		}
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
		try
		{
			ResolutionChanged(sender, width, height);
		}
		catch (Exception ex)
		{
			try
			{
				using (StreamWriter streamWriter = new StreamWriter("client-crashlog.txt", append: true))
				{
					streamWriter.WriteLine(DateTime.Now);
					streamWriter.WriteLine(ex);
					streamWriter.WriteLine("");
				}
				MessageBox.Show(ex.ToString(), "ZXMAK: Error");
			}
			catch
			{
			}
		}
    }
}
