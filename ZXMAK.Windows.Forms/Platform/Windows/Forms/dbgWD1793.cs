using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using ZXMAK.Engine;

namespace ZXMAK.Platform.Windows.Forms
{
	public partial class dbgWD1793 : Form
	{
		public dbgWD1793(Spectrum spectrum)
		{
			this._betaDiskDevice = (spectrum as IBetaDiskDevice);
			this.InitializeComponent();
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
		}

		protected override void OnFormClosed(FormClosedEventArgs e)
		{
			base.OnFormClosed(e);
		}

		private void timerUpdate_Tick(object sender, EventArgs e)
		{
			if (this._betaDiskDevice != null)
			{
				this.label1.Text = this._betaDiskDevice.BetaDisk.DumpState();
				return;
			}
			this.label1.Text = "Beta Disk interface not found";
		}

		private IBetaDiskDevice _betaDiskDevice;
	}
}
