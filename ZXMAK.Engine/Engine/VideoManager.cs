using System;

namespace ZXMAK.Engine
{
	public class VideoManager
	{
		public int Width
		{
			get
			{
				return this._width;
			}
		}

		public int Height
		{
			get
			{
				return this._height;
			}
		}

		public VideoParams VideoParams
		{
			get
			{
				return this._videoParams;
			}
		}

		public void SetResolution(int width, int height)
		{
			if (width != this._width || height != this._height)
			{
				this._width = width;
				this._height = height;
				if (this._videoDevice != null)
				{
					this._videoDevice.SetResolution(this, width, height);
				}
			}
		}

		public void SetVideoParams(VideoParams value)
		{
			if (value != this._videoParams)
			{
				this._videoParams = value;
				if (this._videoRenderer != null)
				{
					this._videoRenderer.SetVideoParams(this, value);
				}
			}
		}

		public void SetVideoDevice(IVideoDevice device)
		{
			this._videoDevice = device;
			if (this._width > 0 && this._height > 0)
			{
				this._videoDevice.SetResolution(this, this._width, this._height);
			}
		}

		public void SetVideoRenderer(IVideoRenderer renderer)
		{
			this._videoRenderer = renderer;
			if (this._videoParams != null)
			{
				this._videoRenderer.SetVideoParams(this, this._videoParams);
			}
		}

		private IVideoDevice _videoDevice;

		private IVideoRenderer _videoRenderer;

		private int _width;

		private int _height;

		private VideoParams _videoParams;
	}
}
