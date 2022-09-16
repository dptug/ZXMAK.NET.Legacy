using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using ZXMAK.Engine;
using ZXMAK.Engine.Tape;
using ZXMAK.Platform.Windows.Forms.Properties;

namespace ZXMAK.Platform.Windows.Forms;

public class TapeForm : Form
{
	private IContainer components;

	private ToolStrip toolBar;

	private Panel panelList;

	private ListBox blockList;

	private ToolStripButton toolButtonRewind;

	private ToolStripButton toolButtonPrev;

	private ToolStripButton toolButtonPlay;

	private ToolStripButton toolButtonNext;

	private ToolStripProgressBar toolProgressBar;

	private Timer timerProgress;

	private static TapeForm _instance;

	private Spectrum _spectrum;

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
		this.toolBar = new System.Windows.Forms.ToolStrip();
		this.toolButtonRewind = new System.Windows.Forms.ToolStripButton();
		this.toolButtonPrev = new System.Windows.Forms.ToolStripButton();
		this.toolButtonPlay = new System.Windows.Forms.ToolStripButton();
		this.toolButtonNext = new System.Windows.Forms.ToolStripButton();
		this.toolProgressBar = new System.Windows.Forms.ToolStripProgressBar();
		this.panelList = new System.Windows.Forms.Panel();
		this.blockList = new System.Windows.Forms.ListBox();
		this.timerProgress = new System.Windows.Forms.Timer(this.components);
		this.toolBar.SuspendLayout();
		this.panelList.SuspendLayout();
		base.SuspendLayout();
		this.toolBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[5] { this.toolButtonRewind, this.toolButtonPrev, this.toolButtonPlay, this.toolButtonNext, this.toolProgressBar });
		this.toolBar.Location = new System.Drawing.Point(0, 0);
		this.toolBar.Name = "toolBar";
		this.toolBar.Size = new System.Drawing.Size(292, 25);
		this.toolBar.TabIndex = 2;
		this.toolBar.Text = "toolBar";
		this.toolButtonRewind.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.toolButtonRewind.Image = ZXMAK.Platform.Windows.Forms.Properties.Resources.RewindIcon;
		this.toolButtonRewind.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.toolButtonRewind.Name = "toolButtonRewind";
		this.toolButtonRewind.Size = new System.Drawing.Size(23, 22);
		this.toolButtonRewind.Text = "Rewind";
		this.toolButtonRewind.Click += new System.EventHandler(toolButtonRewind_Click);
		this.toolButtonPrev.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.toolButtonPrev.Image = ZXMAK.Platform.Windows.Forms.Properties.Resources.PrevIcon;
		this.toolButtonPrev.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.toolButtonPrev.Name = "toolButtonPrev";
		this.toolButtonPrev.Size = new System.Drawing.Size(23, 22);
		this.toolButtonPrev.Text = "Previous block";
		this.toolButtonPrev.Click += new System.EventHandler(toolButtonPrev_Click);
		this.toolButtonPlay.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.toolButtonPlay.Image = ZXMAK.Platform.Windows.Forms.Properties.Resources.PlayIcon;
		this.toolButtonPlay.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.toolButtonPlay.Name = "toolButtonPlay";
		this.toolButtonPlay.Size = new System.Drawing.Size(23, 22);
		this.toolButtonPlay.Text = "Play/Stop";
		this.toolButtonPlay.Click += new System.EventHandler(toolButtonPlay_Click);
		this.toolButtonNext.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.toolButtonNext.Image = ZXMAK.Platform.Windows.Forms.Properties.Resources.NextIcon;
		this.toolButtonNext.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.toolButtonNext.Name = "toolButtonNext";
		this.toolButtonNext.Size = new System.Drawing.Size(23, 22);
		this.toolButtonNext.Text = "Next block";
		this.toolButtonNext.Click += new System.EventHandler(toolButtonNext_Click);
		this.toolProgressBar.Name = "toolProgressBar";
		this.toolProgressBar.Size = new System.Drawing.Size(170, 22);
		this.toolProgressBar.Step = 1;
		this.toolProgressBar.ToolTipText = "Loading progress";
		this.toolProgressBar.Value = 50;
		this.panelList.Controls.Add(this.blockList);
		this.panelList.Dock = System.Windows.Forms.DockStyle.Fill;
		this.panelList.Location = new System.Drawing.Point(0, 25);
		this.panelList.Name = "panelList";
		this.panelList.Size = new System.Drawing.Size(292, 237);
		this.panelList.TabIndex = 3;
		this.blockList.Dock = System.Windows.Forms.DockStyle.Fill;
		this.blockList.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.blockList.FormattingEnabled = true;
		this.blockList.IntegralHeight = false;
		this.blockList.ItemHeight = 16;
		this.blockList.Location = new System.Drawing.Point(0, 0);
		this.blockList.Name = "blockList";
		this.blockList.Size = new System.Drawing.Size(292, 237);
		this.blockList.TabIndex = 0;
		this.blockList.DoubleClick += new System.EventHandler(blockList_DoubleClick);
		this.blockList.Click += new System.EventHandler(blockList_Click);
		this.timerProgress.Enabled = true;
		this.timerProgress.Interval = 200;
		this.timerProgress.Tick += new System.EventHandler(timerProgress_Tick);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(292, 262);
		base.Controls.Add(this.panelList);
		base.Controls.Add(this.toolBar);
		base.Name = "TapeForm";
		this.Text = "Tape";
		base.FormClosed += new System.Windows.Forms.FormClosedEventHandler(TapeForm_FormClosed);
		this.toolBar.ResumeLayout(false);
		this.toolBar.PerformLayout();
		this.panelList.ResumeLayout(false);
		base.ResumeLayout(false);
		base.PerformLayout();
	}

	public static TapeForm GetInstance(Spectrum spec)
	{
		if (_instance == null)
		{
			_instance = new TapeForm(spec);
		}
		return _instance;
	}

	private TapeForm(Spectrum spec)
	{
		_spectrum = spec;
		InitializeComponent();
		if (_spectrum is ITapeDevice tapeDevice)
		{
			tapeDevice.Tape.TapeStateChanged += OnTapeStateChanged;
			OnTapeStateChanged(null, null);
		}
		OnTapeStateChanged(null, null);
	}

	private void TapeForm_FormClosed(object sender, FormClosedEventArgs e)
	{
		_instance = null;
	}

	private void toolButtonRewind_Click(object sender, EventArgs e)
	{
		if (_spectrum is ITapeDevice tapeDevice)
		{
			tapeDevice.Tape.Rewind(_spectrum.CPU.Tact);
		}
	}

	private void toolButtonPrev_Click(object sender, EventArgs e)
	{
		if (_spectrum is ITapeDevice tapeDevice)
		{
			tapeDevice.Tape.CurrentBlock++;
		}
	}

	private void toolButtonPlay_Click(object sender, EventArgs e)
	{
		if (_spectrum is ITapeDevice tapeDevice)
		{
			if (tapeDevice.Tape.IsPlay)
			{
				tapeDevice.Tape.Stop(_spectrum.CPU.Tact);
			}
			else
			{
				tapeDevice.Tape.Play(_spectrum.CPU.Tact);
			}
		}
	}

	private void toolButtonNext_Click(object sender, EventArgs e)
	{
		if (_spectrum is ITapeDevice tapeDevice)
		{
			tapeDevice.Tape.CurrentBlock--;
		}
	}

	private void OnTapeStateChanged(object sender, EventArgs args)
	{
		if (!(_spectrum is ITapeDevice tapeDevice))
		{
			ToolStripButton toolStripButton = toolButtonRewind;
			ToolStripButton toolStripButton2 = toolButtonPrev;
			ToolStripButton toolStripButton3 = toolButtonPlay;
			bool flag2 = (toolButtonNext.Enabled = false);
			bool flag4 = (toolStripButton3.Enabled = flag2);
			bool enabled = (toolStripButton2.Enabled = flag4);
			toolStripButton.Enabled = enabled;
			blockList.SelectedIndex = -1;
			return;
		}
		if (tapeDevice.Tape.Blocks.Count <= 0)
		{
			ToolStripButton toolStripButton4 = toolButtonRewind;
			ToolStripButton toolStripButton5 = toolButtonPrev;
			ToolStripButton toolStripButton6 = toolButtonPlay;
			bool flag7 = (toolButtonNext.Enabled = false);
			bool flag9 = (toolStripButton6.Enabled = flag7);
			bool enabled2 = (toolStripButton5.Enabled = flag9);
			toolStripButton4.Enabled = enabled2;
			blockList.SelectedIndex = -1;
		}
		else
		{
			ToolStripButton toolStripButton7 = toolButtonNext;
			bool enabled3 = (toolButtonPrev.Enabled = !tapeDevice.Tape.IsPlay);
			toolStripButton7.Enabled = enabled3;
			ToolStripButton toolStripButton8 = toolButtonRewind;
			bool enabled4 = (toolButtonPlay.Enabled = true);
			toolStripButton8.Enabled = enabled4;
			if (tapeDevice.Tape.IsPlay)
			{
				toolButtonPlay.Image = Resources.StopIcon;
			}
			else
			{
				toolButtonPlay.Image = Resources.PlayIcon;
			}
			blockList.Items.Clear();
			foreach (TapeBlock block in tapeDevice.Tape.Blocks)
			{
				blockList.Items.Add(block.Description);
			}
			blockList.SelectedIndex = tapeDevice.Tape.CurrentBlock;
		}
		blockList.Enabled = !tapeDevice.Tape.IsPlay;
	}

	private void timerProgress_Tick(object sender, EventArgs e)
	{
		if (!(_spectrum is ITapeDevice tapeDevice))
		{
			toolProgressBar.Maximum = 100;
			toolProgressBar.Value = 0;
			return;
		}
		toolProgressBar.Minimum = 0;
		if (tapeDevice.Tape.CurrentBlock >= 0 && tapeDevice.Tape.CurrentBlock < tapeDevice.Tape.Blocks.Count)
		{
			toolProgressBar.Maximum = tapeDevice.Tape.Blocks[tapeDevice.Tape.CurrentBlock].Periods.Count;
			toolProgressBar.Value = tapeDevice.Tape.Position;
		}
		else
		{
			toolProgressBar.Maximum = 65535;
			toolProgressBar.Value = 0;
		}
	}

	private void blockList_Click(object sender, EventArgs e)
	{
		if (_spectrum is ITapeDevice tapeDevice && blockList.Enabled && !tapeDevice.Tape.IsPlay)
		{
			tapeDevice.Tape.CurrentBlock = blockList.SelectedIndex;
		}
	}

	private void blockList_DoubleClick(object sender, EventArgs e)
	{
		if (_spectrum is ITapeDevice tapeDevice && blockList.Enabled && !tapeDevice.Tape.IsPlay)
		{
			tapeDevice.Tape.CurrentBlock = blockList.SelectedIndex;
			tapeDevice.Tape.Play(_spectrum.CPU.Tact);
		}
	}
}
