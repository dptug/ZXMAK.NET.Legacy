using System;

namespace ZXMAK.Engine
{
	public class VideoParams
	{
		public VideoParams(int width, int height, int pitch, int bpp)
		{
			this.Width = width;
			this.Height = height;
			this.LinePitch = pitch;
			this.BPP = bpp;
		}

		public int Width;

		public int Height;

		public int LinePitch;

		public int BPP;
	}
}
