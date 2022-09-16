using SdlDotNet.Input;
using System.Drawing;

namespace ZXMAK.Platform.SDL
{
    public class Mouse
    {
        public int DeltaX;
        public int DeltaY;
        public int Buttons;

        public Mouse() => SdlDotNet.Input.Mouse.ShowCursor = false;

        public void Scan()
        {
            Point mousePositionChange = SdlDotNet.Input.Mouse.MousePositionChange;
            this.DeltaX = mousePositionChange.X / 3;
            this.DeltaY = mousePositionChange.Y / 3;
            this.Buttons = 0;
            if (SdlDotNet.Input.Mouse.IsButtonPressed(MouseButton.PrimaryButton))
                this.Buttons |= 1;
            if (SdlDotNet.Input.Mouse.IsButtonPressed(MouseButton.SecondaryButton))
                this.Buttons |= 2;
            if (!SdlDotNet.Input.Mouse.IsButtonPressed(MouseButton.MiddleButton))
                return;
            this.Buttons |= 3;
        }
    }
}
