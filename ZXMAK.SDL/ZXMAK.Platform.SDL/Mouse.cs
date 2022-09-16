using System.Drawing;
using SdlDotNet.Input;

namespace ZXMAK.Platform.SDL;

public class Mouse
{
	public int DeltaX;

	public int DeltaY;

	public int Buttons;

	public Mouse()
	{
		Mouse.set_ShowCursor(false);
	}

	public void Scan()
	{
		Point mousePositionChange = Mouse.get_MousePositionChange();
		DeltaX = mousePositionChange.X / 3;
		DeltaY = mousePositionChange.Y / 3;
		Buttons = 0;
		if (Mouse.IsButtonPressed((MouseButton)1))
		{
			Buttons |= 1;
		}
		if (Mouse.IsButtonPressed((MouseButton)3))
		{
			Buttons |= 2;
		}
		if (Mouse.IsButtonPressed((MouseButton)2))
		{
			Buttons |= 3;
		}
	}
}
