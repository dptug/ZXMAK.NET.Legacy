using SdlDotNet.Core;
using SdlDotNet.Graphics;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security;

namespace ZXMAK.Platform.SDL
{
    public class SurfaceEx : Surface
    {
        private const string SDL_NATIVE_LIBRARY = "SDL.dll";
        private const CallingConvention CALLING_CONVENTION = CallingConvention.Cdecl;

        private static Tao.Sdl.Sdl.SDL_Rect ConvertRecttoSDLRect(Rectangle rect) => new((short)rect.X, (short)rect.Y, (short)rect.Width, (short)rect.Height);

        [SuppressUnmanagedCodeSecurity]
        [DllImport("SDL.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int SDL_SoftStretch(
          IntPtr src,
          ref Tao.Sdl.Sdl.SDL_Rect srcrect,
          IntPtr dst,
          ref Tao.Sdl.Sdl.SDL_Rect dstrect);

        public SurfaceEx(int width, int height)
          : base(width, height, 32)
        {
        }

        public void SoftStretch(
          SurfaceEx sourceSurface,
          Rectangle destinationRectangle,
          Rectangle sourceRectangle)
        {
            Tao.Sdl.Sdl.SDL_Rect srcrect = SurfaceEx.ConvertRecttoSDLRect(sourceRectangle);
            Tao.Sdl.Sdl.SDL_Rect dstrect = SurfaceEx.ConvertRecttoSDLRect(destinationRectangle);
            int num = SurfaceEx.SDL_SoftStretch(sourceSurface.Handle, ref srcrect, this.Handle, ref dstrect);
            GC.KeepAlive((object)this);
            if (num != 0)
                throw SdlException.Generate();
        }
    }
}
