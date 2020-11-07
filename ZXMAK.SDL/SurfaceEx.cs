using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security;
using SdlDotNet.Core;
using SdlDotNet.Graphics;
using Tao.Sdl;

namespace ZXMAK.Platform.SDL
{
	public class SurfaceEx : Surface
	{
		private static Sdl.SDL_Rect ConvertRecttoSDLRect(Rectangle rect)
		{
			return new Sdl.SDL_Rect((short)rect.X, (short)rect.Y, (short)rect.Width, (short)rect.Height);
		}

		[SuppressUnmanagedCodeSecurity]
		[DllImport("SDL.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_SoftStretch(IntPtr src, ref Sdl.SDL_Rect srcrect, IntPtr dst, ref Sdl.SDL_Rect dstrect);

		public SurfaceEx(int width, int height) : base(width, height, 32)
		{
		}

		public void SoftStretch(SurfaceEx sourceSurface, Rectangle destinationRectangle, Rectangle sourceRectangle)
		{
			Sdl.SDL_Rect sdl_Rect = SurfaceEx.ConvertRecttoSDLRect(sourceRectangle);
			Sdl.SDL_Rect sdl_Rect2 = SurfaceEx.ConvertRecttoSDLRect(destinationRectangle);
			int num = SurfaceEx.SDL_SoftStretch(sourceSurface.Handle, ref sdl_Rect, base.Handle, ref sdl_Rect2);
			GC.KeepAlive(this);
			if (num != 0)
			{
				throw SdlException.Generate();
			}
		}

		private const string SDL_NATIVE_LIBRARY = "SDL.dll";

		private const CallingConvention CALLING_CONVENTION = CallingConvention.Cdecl;
	}
}
