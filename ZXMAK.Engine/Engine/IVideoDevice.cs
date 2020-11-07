using System;

namespace ZXMAK.Engine
{
	public interface IVideoDevice
	{
		void SetResolution(VideoManager sender, int width, int height);
	}
}
