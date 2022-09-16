using System;
using System.Windows.Forms;
using Microsoft.DirectX.DirectInput;

namespace ZXMAK.Platform.MDX;

public class DirectMouse : IDisposable
{
	private Form _form;

	private bool _active;

	private Device _device;

	public int DeltaX;

	public int DeltaY;

	public int Buttons;

	public DirectMouse(Form mainForm)
	{
		_form = mainForm;
		if (_device == null)
		{
			_device = new Device(SystemGuid.Mouse);
			mainForm.Deactivate += WndDeactivate;
		}
	}

	public void Dispose()
	{
		if (_device != null)
		{
			_active = false;
			_device.Unacquire();
			_device.Dispose();
			_device = null;
		}
	}

	private void WndActivated(object sender, EventArgs e)
	{
	}

	private void WndDeactivate(object sender, EventArgs e)
	{
		StopCapture();
	}

	public void StartCapture()
	{
		if (_device != null && !_active)
		{
			try
			{
				_device.SetCooperativeLevel(_form, CooperativeLevelFlags.Foreground | CooperativeLevelFlags.Exclusive);
				_device.Acquire();
				_active = true;
			}
			catch
			{
				StopCapture();
			}
		}
	}

	public void StopCapture()
	{
		if (!(_device != null))
		{
			return;
		}
		try
		{
			if (_active)
			{
				_device.Unacquire();
			}
			_device.SetCooperativeLevel(_form, CooperativeLevelFlags.Foreground | CooperativeLevelFlags.NonExclusive);
			_active = false;
		}
		catch
		{
		}
	}

	public void Scan()
	{
		if (_active)
		{
			MouseState currentMouseState;
			try
			{
				currentMouseState = _device.CurrentMouseState;
			}
			catch (NotAcquiredException)
			{
				StopCapture();
				return;
			}
			DeltaX = currentMouseState.X;
			DeltaY = currentMouseState.Y;
			Buttons = 0;
			byte[] mouseButtons = currentMouseState.GetMouseButtons();
			if ((mouseButtons[0] & 0x80u) != 0)
			{
				Buttons |= 1;
			}
			if ((mouseButtons[1] & 0x80u) != 0)
			{
				Buttons |= 2;
			}
			if ((mouseButtons[2] & 0x80u) != 0)
			{
				Buttons |= 3;
			}
		}
	}
}
