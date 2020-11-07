using System;
using System.ComponentModel;
using System.Windows.Forms;
using Microsoft.DirectX.Direct3D;
using ZXMAK.Logging;

namespace ZXMAK.Platform.MDX
{
	public class Render3D : Control
	{
		public Render3D()
		{
			base.SetStyle(ControlStyles.UserPaint | ControlStyles.Opaque | ControlStyles.ResizeRedraw, true);
		}

		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);
			this.init();
		}

		protected override void Dispose(bool disposing)
		{
			this.free();
			base.Dispose(disposing);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			this.RenderScene();
		}

		protected override void WndProc(ref Message m)
		{
			if (m.Msg == 262 && (int)m.WParam == 13)
			{
				return;
			}
			base.WndProc(ref m);
		}

		private void init()
		{
			CreateFlags behaviorFlags = CreateFlags.SoftwareVertexProcessing;
			this._presentParams = new PresentParameters();
			this._presentParams.Windowed = true;
			this._presentParams.SwapEffect = SwapEffect.Discard;
			this._presentParams.BackBufferFormat = Manager.Adapters.Default.CurrentDisplayMode.Format;
			this._presentParams.BackBufferCount = 1;
			this._presentParams.PresentationInterval = PresentInterval.One;
			this._presentParams.BackBufferWidth = 640;
			this._presentParams.BackBufferHeight = 480;
			this.D3D = new Device(0, DeviceType.Hardware, base.Handle, behaviorFlags, new PresentParameters[]
			{
				this._presentParams
			});
			this.D3D.DeviceResizing += this.D3D_DeviceResizing;
			this.D3D.DeviceReset += this.D3D_DeviceReset;
			this.OnCreateDevice();
		}

		private void free()
		{
			if (this.D3D != null)
			{
				this.OnDestroyDevice();
				this.D3D.Dispose();
				this.D3D = null;
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
			if (this.D3D != null)
			{
				this.OnUpdateScene();
			}
		}

		public void RenderScene()
		{
			if (this.D3D != null && base.Visible && base.ClientSize.Width > 0 && base.ClientSize.Height > 0)
			{
				int num;
				this.D3D.CheckCooperativeLevel(out num);
				ResultCode resultCode = (ResultCode)num;
				switch (resultCode)
				{
				case ResultCode.DeviceLost:
					break;
				case ResultCode.DeviceNotReset:
					this.D3D.Reset(new PresentParameters[]
					{
						this._presentParams
					});
					break;
				default:
					if (resultCode != ResultCode.Success)
					{
						Logger.GetLogger().LogMessage("CheckCooperativeLevel = " + ((ResultCode)num).ToString());
					}
					else
					{
						this.D3D.Clear(ClearFlags.Target, 0, 1f, 0);
						this.D3D.BeginScene();
						this.OnRenderScene();
						this.D3D.EndScene();
						this.D3D.Present();
					}
					break;
				}
				this._frameCounter++;
			}
		}

		public int FrameCounter
		{
			get
			{
				return this._frameCounter;
			}
		}

		public int FrameRate
		{
			get
			{
				if (this.D3D != null)
				{
					return this.D3D.DisplayMode.RefreshRate;
				}
				return 0;
			}
		}

		protected const int WM_SYSCHAR = 262;

		private int _frameCounter;

		protected Device D3D;

		private PresentParameters _presentParams;
	}
}
