using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using ZXMAK.Engine;

namespace ZXMAK.Platform.Windows.Forms;

public class dbgWD1793 : Form
{
	private IContainer components;

	private Label label1;

	private Timer timerUpdate;

	private IBetaDiskDevice _betaDiskDevice;

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		this.components = new System.ComponentModel.Container();
		this.label1 = new System.Windows.Forms.Label();
		this.timerUpdate = new System.Windows.Forms.Timer(this.components);
		base.SuspendLayout();
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(36, 39);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(35, 13);
		this.label1.TabIndex = 0;
		this.label1.Text = "label1";
		this.timerUpdate.Enabled = true;
		this.timerUpdate.Interval = 300;
		this.timerUpdate.Tick += new System.EventHandler(timerUpdate_Tick);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(292, 371);
		base.Controls.Add(this.label1);
		base.Name = "dbgWD1793";
		this.Text = "dbgWD1793";
		base.ResumeLayout(false);
		base.PerformLayout();
	}

	public dbgWD1793(Spectrum spectrum)
	{
		_betaDiskDevice = spectrum as IBetaDiskDevice;
		InitializeComponent();
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
		if (_betaDiskDevice != null)
		{
			label1.Text = _betaDiskDevice.BetaDisk.DumpState();
		}
		else
		{
			label1.Text = "Beta Disk interface not found";
		}
	}
}
