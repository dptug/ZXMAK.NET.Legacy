namespace ZXMAK.Engine;

public class VideoManager
{
	private IVideoDevice _videoDevice;

	private IVideoRenderer _videoRenderer;

	private int _width;

	private int _height;

	private VideoParams _videoParams;

	public int Width => _width;

	public int Height => _height;

	public VideoParams VideoParams => _videoParams;

	public void SetResolution(int width, int height)
	{
		if (width != _width || height != _height)
		{
			_width = width;
			_height = height;
			if (_videoDevice != null)
			{
				_videoDevice.SetResolution(this, width, height);
			}
		}
	}

	public void SetVideoParams(VideoParams value)
	{
		if (value != _videoParams)
		{
			_videoParams = value;
			if (_videoRenderer != null)
			{
				_videoRenderer.SetVideoParams(this, value);
			}
		}
	}

	public void SetVideoDevice(IVideoDevice device)
	{
		_videoDevice = device;
		if (_width > 0 && _height > 0)
		{
			_videoDevice.SetResolution(this, _width, _height);
		}
	}

	public void SetVideoRenderer(IVideoRenderer renderer)
	{
		_videoRenderer = renderer;
		if (_videoParams != null)
		{
			_videoRenderer.SetVideoParams(this, _videoParams);
		}
	}
}
