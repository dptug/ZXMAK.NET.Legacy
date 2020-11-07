using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using ZXMAK.Engine;
using ZXMAK.Engine.Tape;
using ZXMAK.Platform.Windows.Forms.Properties;

namespace ZXMAK.Platform.Windows.Forms
{
	public partial class TapeForm : Form
	{
		public static TapeForm GetInstance(Spectrum spec)
		{
			if (TapeForm._instance == null)
			{
				TapeForm._instance = new TapeForm(spec);
			}
			return TapeForm._instance;
		}

		private TapeForm(Spectrum spec)
		{
			this._spectrum = spec;
			this.InitializeComponent();
			ITapeDevice tapeDevice = this._spectrum as ITapeDevice;
			if (tapeDevice != null)
			{
				tapeDevice.Tape.TapeStateChanged += this.OnTapeStateChanged;
				this.OnTapeStateChanged(null, null);
			}
			this.OnTapeStateChanged(null, null);
		}

		private void TapeForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			TapeForm._instance = null;
		}

		private void toolButtonRewind_Click(object sender, EventArgs e)
		{
			ITapeDevice tapeDevice = this._spectrum as ITapeDevice;
			if (tapeDevice != null)
			{
				tapeDevice.Tape.Rewind(this._spectrum.CPU.Tact);
			}
		}

		private void toolButtonPrev_Click(object sender, EventArgs e)
		{
			ITapeDevice tapeDevice = this._spectrum as ITapeDevice;
			if (tapeDevice != null)
			{
				tapeDevice.Tape.CurrentBlock++;
			}
		}

		private void toolButtonPlay_Click(object sender, EventArgs e)
		{
			ITapeDevice tapeDevice = this._spectrum as ITapeDevice;
			if (tapeDevice != null)
			{
				if (tapeDevice.Tape.IsPlay)
				{
					tapeDevice.Tape.Stop(this._spectrum.CPU.Tact);
					return;
				}
				tapeDevice.Tape.Play(this._spectrum.CPU.Tact);
			}
		}

		private void toolButtonNext_Click(object sender, EventArgs e)
		{
			ITapeDevice tapeDevice = this._spectrum as ITapeDevice;
			if (tapeDevice != null)
			{
				tapeDevice.Tape.CurrentBlock--;
			}
		}

		private void OnTapeStateChanged(object sender, EventArgs args)
		{
			ITapeDevice tapeDevice = this._spectrum as ITapeDevice;
			if (tapeDevice == null)
			{
				this.toolButtonRewind.Enabled = (this.toolButtonPrev.Enabled = (this.toolButtonPlay.Enabled = (this.toolButtonNext.Enabled = false)));
				this.blockList.SelectedIndex = -1;
				return;
			}
			if (tapeDevice.Tape.Blocks.Count <= 0)
			{
				this.toolButtonRewind.Enabled = (this.toolButtonPrev.Enabled = (this.toolButtonPlay.Enabled = (this.toolButtonNext.Enabled = false)));
				this.blockList.SelectedIndex = -1;
			}
			else
			{
				this.toolButtonNext.Enabled = (this.toolButtonPrev.Enabled = !tapeDevice.Tape.IsPlay);
				this.toolButtonRewind.Enabled = (this.toolButtonPlay.Enabled = true);
				if (tapeDevice.Tape.IsPlay)
				{
					this.toolButtonPlay.Image = Resources.StopIcon;
				}
				else
				{
					this.toolButtonPlay.Image = Resources.PlayIcon;
				}
				this.blockList.Items.Clear();
				foreach (TapeBlock tapeBlock in tapeDevice.Tape.Blocks)
				{
					this.blockList.Items.Add(tapeBlock.Description);
				}
				this.blockList.SelectedIndex = tapeDevice.Tape.CurrentBlock;
			}
			this.blockList.Enabled = !tapeDevice.Tape.IsPlay;
		}

		private void timerProgress_Tick(object sender, EventArgs e)
		{
			ITapeDevice tapeDevice = this._spectrum as ITapeDevice;
			if (tapeDevice == null)
			{
				this.toolProgressBar.Maximum = 100;
				this.toolProgressBar.Value = 0;
				return;
			}
			this.toolProgressBar.Minimum = 0;
			if (tapeDevice.Tape.CurrentBlock >= 0 && tapeDevice.Tape.CurrentBlock < tapeDevice.Tape.Blocks.Count)
			{
				this.toolProgressBar.Maximum = tapeDevice.Tape.Blocks[tapeDevice.Tape.CurrentBlock].Periods.Count;
				this.toolProgressBar.Value = tapeDevice.Tape.Position;
				return;
			}
			this.toolProgressBar.Maximum = 65535;
			this.toolProgressBar.Value = 0;
		}

		private void blockList_Click(object sender, EventArgs e)
		{
			ITapeDevice tapeDevice = this._spectrum as ITapeDevice;
			if (tapeDevice == null)
			{
				return;
			}
			if (!this.blockList.Enabled || tapeDevice.Tape.IsPlay)
			{
				return;
			}
			tapeDevice.Tape.CurrentBlock = this.blockList.SelectedIndex;
		}

		private void blockList_DoubleClick(object sender, EventArgs e)
		{
			ITapeDevice tapeDevice = this._spectrum as ITapeDevice;
			if (tapeDevice == null)
			{
				return;
			}
			if (!this.blockList.Enabled || tapeDevice.Tape.IsPlay)
			{
				return;
			}
			tapeDevice.Tape.CurrentBlock = this.blockList.SelectedIndex;
			tapeDevice.Tape.Play(this._spectrum.CPU.Tact);
		}

		private static TapeForm _instance;

		private Spectrum _spectrum;
	}
}
