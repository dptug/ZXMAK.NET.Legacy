using System;
using System.Drawing;
using SdlDotNet.Input;

namespace ZXMAK.Platform.SDL
{
	public class Mouse
	{
		public Mouse()
		{
			Mouse.ShowCursor = false;
		}

		public void Scan()
		{
			Point mousePositionChange = Mouse.MousePositionChange;
			this.DeltaX = mousePositionChange.X / 3;
			this.DeltaY = mousePositionChange.Y / 3;
			this.Buttons = 0;
			if (Mouse.IsButtonPressed(1))
			{
				this.Buttons |= 1;
			}
			if (Mouse.IsButtonPressed(3))
			{
				this.Buttons |= 2;
			}
			if (Mouse.IsButtonPressed(2))
			{
				this.Buttons |= 3;
			}
		}

		public int DeltaX;

		public int DeltaY;

		public int Buttons;
	}
}
