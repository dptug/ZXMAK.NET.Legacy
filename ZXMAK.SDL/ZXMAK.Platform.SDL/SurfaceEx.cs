using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security;
using SdlDotNet.Core;
using SdlDotNet.Graphics;
using Tao.Sdl;

namespace ZXMAK.Platform.SDL;

public class SurfaceEx : Surface
{
	private const string SDL_NATIVE_LIBRARY = "SDL.dll";

	private const CallingConvention CALLING_CONVENTION = CallingConvention.Cdecl;

	private static SDL_Rect ConvertRecttoSDLRect(Rectangle rect)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		return new SDL_Rect((short)rect.X, (short)rect.Y, (short)rect.Width, (short)rect.Height);
	}

	[DllImport("SDL.dll", CallingConvention = CallingConvention.Cdecl)]
	[SuppressUnmanagedCodeSecurity]
	public static extern int SDL_SoftStretch(IntPtr src, ref SDL_Rect srcrect, IntPtr dst, ref SDL_Rect dstrect);

	public SurfaceEx(int width, int height)
		: base(width, height, 32)
	{
	}

	public void SoftStretch(SurfaceEx sourceSurface, Rectangle destinationRectangle, Rectangle sourceRectangle)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		SDL_Rect srcrect = ConvertRecttoSDLRect(sourceRectangle);
		SDL_Rect dstrect = ConvertRecttoSDLRect(destinationRectangle);
		int num = SDL_SoftStretch(((BaseSdlResource)sourceSurface).get_Handle(), ref srcrect, ((BaseSdlResource)this).get_Handle(), ref dstrect);
		GC.KeepAlive(this);
		if (num != 0)
		{
			throw SdlException.Generate();
		}
	}
}
