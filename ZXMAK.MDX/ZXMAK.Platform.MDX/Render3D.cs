using System;
using System.ComponentModel;
using System.Windows.Forms;
using Microsoft.DirectX.Direct3D;
using ZXMAK.Logging;

namespace ZXMAK.Platform.MDX;

public class Render3D : Control
{
	protected const int WM_SYSCHAR = 262;

	private int _frameCounter;

	protected Device D3D;

	private PresentParameters _presentParams;

	public int FrameCounter => _frameCounter;

	public int FrameRate
	{
		get
		{
			if (D3D != null)
			{
				return D3D.DisplayMode.RefreshRate;
			}
			return 0;
		}
	}

	public Render3D()
	{
		SetStyle(ControlStyles.UserPaint | ControlStyles.Opaque | ControlStyles.ResizeRedraw, value: true);
	}

	protected override void OnHandleCreated(EventArgs e)
	{
		base.OnHandleCreated(e);
		init();
	}

	protected override void Dispose(bool disposing)
	{
		free();
		base.Dispose(disposing);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		RenderScene();
	}

	protected override void WndProc(ref Message m)
	{
		if (m.Msg != 262 || (int)m.WParam != 13)
		{
			base.WndProc(ref m);
		}
	}

	private void init()
	{
		CreateFlags behaviorFlags = CreateFlags.SoftwareVertexProcessing;
		_presentParams = new PresentParameters();
		_presentParams.Windowed = true;
		_presentParams.SwapEffect = SwapEffect.Discard;
		_presentParams.BackBufferFormat = Manager.Adapters.Default.CurrentDisplayMode.Format;
		_presentParams.BackBufferCount = 1;
		_presentParams.PresentationInterval = PresentInterval.One;
		_presentParams.BackBufferWidth = 640;
		_presentParams.BackBufferHeight = 480;
		D3D = new Device(0, DeviceType.Hardware, base.Handle, behaviorFlags, _presentParams);
		D3D.DeviceResizing += D3D_DeviceResizing;
		D3D.DeviceReset += D3D_DeviceReset;
		OnCreateDevice();
	}

	private void free()
	{
		if (D3D != null)
		{
			OnDestroyDevice();
			D3D.Dispose();
			D3D = null;
		}
	}

	private void D3D_DeviceReset(object sender, EventArgs e)
	{
	}

	private void D3D_DeviceResizing(object sender, CancelEventArgs e)
	{
		e.Cancel = true;
	}

	protected virtual void OnCreateDevice()
	{
	}

	protected virtual void OnDestroyDevice()
	{
	}

	protected virtual void OnUpdateScene()
	{
	}

	protected virtual void OnRenderScene()
	{
	}

	public void UpdateScene()
	{
		if (D3D != null)
		{
			OnUpdateScene();
		}
	}

	public void RenderScene()
	{
		if (D3D != null && base.Visible && base.ClientSize.Width > 0 && base.ClientSize.Height > 0)
		{
			D3D.CheckCooperativeLevel(out var result);
			switch (result)
			{
			case -2005530519:
				D3D.Reset(_presentParams);
				break;
			case 0:
				D3D.Clear(ClearFlags.Target, 0, 1f, 0);
				D3D.BeginScene();
				OnRenderScene();
				D3D.EndScene();
				D3D.Present();
				break;
			default:
				Logger.GetLogger().LogMessage("CheckCooperativeLevel = " + (ResultCode)result);
				break;
			case -2005530520:
				break;
			}
			_frameCounter++;
		}
	}
}
