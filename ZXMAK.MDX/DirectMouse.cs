using System;
using System.Windows.Forms;
using Microsoft.DirectX.DirectInput;

namespace ZXMAK.Platform.MDX
{
	public class DirectMouse : IDisposable
	{
		public DirectMouse(Form mainForm)
		{
			this._form = mainForm;
			if (this._device == null)
			{
				this._device = new Device(SystemGuid.Mouse);
				mainForm.Deactivate += this.WndDeactivate;
			}
		}

		public void Dispose()
		{
			if (this._device != null)
			{
				this._active = false;
				this._device.Unacquire();
				this._device.Dispose();
				this._device = null;
			}
		}

		private void WndActivated(object sender, EventArgs e)
		{
		}

		private void WndDeactivate(object sender, EventArgs e)
		{
			this.StopCapture();
		}

		public void StartCapture()
		{
			if (this._device != null && !this._active)
			{
				try
				{
					this._device.SetCooperativeLevel(this._form, CooperativeLevelFlags.Foreground | CooperativeLevelFlags.Exclusive);
					this._device.Acquire();
					this._active = true;
				}
				catch
				{
					this.StopCapture();
				}
			}
		}

		public void StopCapture()
		{
			if (this._device != null)
			{
				try
				{
					if (this._active)
					{
						this._device.Unacquire();
					}
					this._device.SetCooperativeLevel(this._form, CooperativeLevelFlags.Foreground | CooperativeLevelFlags.NonExclusive);
					this._active = false;
				}
				catch
				{
				}
			}
		}

		public void Scan()
		{
			if (this._active)
			{
				MouseState currentMouseState;
				try
				{
					currentMouseState = this._device.CurrentMouseState;
				}
				catch (NotAcquiredException)
				{
					this.StopCapture();
					return;
				}
				this.DeltaX = currentMouseState.X;
				this.DeltaY = currentMouseState.Y;
				this.Buttons = 0;
				byte[] mouseButtons = currentMouseState.GetMouseButtons();
				if ((mouseButtons[0] & 128) != 0)
				{
					this.Buttons |= 1;
				}
				if ((mouseButtons[1] & 128) != 0)
				{
					this.Buttons |= 2;
				}
				if ((mouseButtons[2] & 128) != 0)
				{
					this.Buttons |= 3;
				}
			}
		}

		private Form _form;

		private bool _active;

		private Device _device;

		public int DeltaX;

		public int DeltaY;

		public int Buttons;
	}
}
