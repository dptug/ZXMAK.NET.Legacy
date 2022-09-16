namespace ZXMAK.Engine;

public class VideoParams
{
	public int Width;

	public int Height;

	public int LinePitch;

	public int BPP;

	public VideoParams(int width, int height, int pitch, int bpp)
	{
		Width = width;
		Height = height;
		LinePitch = pitch;
		BPP = bpp;
	}
}
