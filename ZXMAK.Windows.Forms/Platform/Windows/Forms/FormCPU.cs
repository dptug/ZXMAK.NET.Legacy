using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using ZXMAK.Engine;
using ZXMAK.Engine.Z80;
using ZXMAK.Logging;
using ZXMAK.Platform.Windows.Forms.Controls;

namespace ZXMAK.Platform.Windows.Forms
{
	public partial class FormCPU : Form
	{
		public static FormCPU GetInstance(Spectrum spectrum)
		{
			if (FormCPU._instance != null && FormCPU._instance._spectrum == spectrum)
			{
				return FormCPU._instance;
			}
			FormCPU._instance = new FormCPU(spectrum);
			return FormCPU._instance;
		}

		private FormCPU(Spectrum spectrum)
		{
			this._spectrum = spectrum;
			this.InitializeComponent();
			this._spectrum.UpdateState += this.spectrum_OnUpdateState;
			this._spectrum.Breakpoint += this.spectrum_OnBreakpoint;
		}

		private void FormCPU_FormClosed(object sender, FormClosedEventArgs e)
		{
			FormCPU._instance = null;
		}

		private void FormCPU_Load(object sender, EventArgs e)
		{
			this.UpdateCPU(true);
		}

		private void FormCPU_Shown(object sender, EventArgs e)
		{
			base.Show();
			this.UpdateCPU(false);
			this.dasmPanel.Focus();
			base.Select();
		}

		private void spectrum_OnUpdateState(object sender, EventArgs args)
		{
			if (base.InvokeRequired)
			{
				base.Invoke(new EventHandler(this.spectrum_OnUpdateState), new object[]
				{
					sender,
					args
				});
				return;
			}
			this.UpdateCPU(true);
		}

		private void spectrum_OnBreakpoint(object sender, EventArgs args)
		{
			if (base.InvokeRequired)
			{
				base.Invoke(new EventHandler(this.spectrum_OnBreakpoint), new object[]
				{
					sender,
					args
				});
				return;
			}
			base.Show();
			this.UpdateCPU(true);
			this.dasmPanel.Focus();
			base.Select();
		}

		private void UpdateCPU(bool updatePC)
		{
			if (this._spectrum.IsRunning)
			{
				updatePC = false;
			}
			this.dasmPanel.ForeColor = (this._spectrum.IsRunning ? SystemColors.ControlDarkDark : SystemColors.ControlText);
			this.UpdateREGS();
			this.UpdateDASM(updatePC);
			this.UpdateDATA();
		}

		private void UpdateREGS()
		{
			this.listREGS.Items.Clear();
			this.listREGS.Items.Add(" PC = " + this._spectrum.CPU.regs.PC.ToString("X4"));
			this.listREGS.Items.Add(" IR = " + this._spectrum.CPU.regs.IR.ToString("X4"));
			this.listREGS.Items.Add(" SP = " + this._spectrum.CPU.regs.SP.ToString("X4"));
			this.listREGS.Items.Add(" AF = " + this._spectrum.CPU.regs.AF.ToString("X4"));
			this.listREGS.Items.Add(" HL = " + this._spectrum.CPU.regs.HL.ToString("X4"));
			this.listREGS.Items.Add(" DE = " + this._spectrum.CPU.regs.DE.ToString("X4"));
			this.listREGS.Items.Add(" BC = " + this._spectrum.CPU.regs.BC.ToString("X4"));
			this.listREGS.Items.Add(" IX = " + this._spectrum.CPU.regs.IX.ToString("X4"));
			this.listREGS.Items.Add(" IY = " + this._spectrum.CPU.regs.IY.ToString("X4"));
			this.listREGS.Items.Add(" AF'= " + this._spectrum.CPU.regs._AF.ToString("X4"));
			this.listREGS.Items.Add(" HL'= " + this._spectrum.CPU.regs._HL.ToString("X4"));
			this.listREGS.Items.Add(" DE'= " + this._spectrum.CPU.regs._DE.ToString("X4"));
			this.listREGS.Items.Add(" BC'= " + this._spectrum.CPU.regs._BC.ToString("X4"));
			this.listREGS.Items.Add(" MW = " + this._spectrum.CPU.regs.MW.ToString("X4"));
			this.listF.Items.Clear();
			this.listF.Items.Add("  S = " + (((this._spectrum.CPU.regs.F & 128) != 0) ? "1" : "0"));
			this.listF.Items.Add("  Z = " + (((this._spectrum.CPU.regs.F & 64) != 0) ? "1" : "0"));
			this.listF.Items.Add(" F5 = " + (((this._spectrum.CPU.regs.F & 32) != 0) ? "1" : "0"));
			this.listF.Items.Add("  H = " + (((this._spectrum.CPU.regs.F & 16) != 0) ? "1" : "0"));
			this.listF.Items.Add(" F3 = " + (((this._spectrum.CPU.regs.F & 8) != 0) ? "1" : "0"));
			this.listF.Items.Add("P/V = " + (((this._spectrum.CPU.regs.F & 4) != 0) ? "1" : "0"));
			this.listF.Items.Add("  N = " + (((this._spectrum.CPU.regs.F & 2) != 0) ? "1" : "0"));
			this.listF.Items.Add("  C = " + (((this._spectrum.CPU.regs.F & 1) != 0) ? "1" : "0"));
			this.listState.Items.Clear();
			this.listState.Items.Add("IFF1=" + (this._spectrum.CPU.IFF1 ? "1" : "0") + " IFF2=" + (this._spectrum.CPU.IFF2 ? "1" : "0"));
			this.listState.Items.Add("HALT=" + (this._spectrum.CPU.HALTED ? "HALTED" : "") + " " + (this._spectrum.CPU.BlockINT ? "BINT" : ""));
			this.listState.Items.Add("  IM=" + this._spectrum.CPU.IM.ToString());
			this.listState.Items.Add("  FX=" + this._spectrum.CPU.FX.ToString());
			this.listState.Items.Add(" XFX=" + this._spectrum.CPU.XFX.ToString());
			this.listState.Items.Add("Tact=" + this._spectrum.CPU.Tact.ToString());
			this.listState.Items.Add("frmT=" + this._spectrum.GetFrameTact().ToString());
		}

		private void UpdateDASM(bool updatePC)
		{
			if (!this._spectrum.IsRunning && updatePC)
			{
				this.dasmPanel.ActiveAddress = this._spectrum.CPU.regs.PC;
				return;
			}
			this.dasmPanel.UpdateLines();
			this.dasmPanel.Refresh();
		}

		private void UpdateDATA()
		{
			this.dataPanel.UpdateLines();
			this.dataPanel.Refresh();
		}

		private bool dasmPanel_CheckExecuting(object Sender, ushort ADDR)
		{
			return !this._spectrum.IsRunning && ADDR == this._spectrum.CPU.regs.PC;
		}

		private void dasmPanel_GetDasm(object Sender, ushort ADDR, out string DASM, out int len)
		{
			DASM = Z80CPU.GetMnemonic(new Z80CPU.MEMREADER(this._spectrum.ReadMemory), (int)ADDR, true, out len);
		}

		private void dasmPanel_GetData(object Sender, ushort ADDR, int len, out byte[] data)
		{
			data = new byte[len];
			for (int i = 0; i < len; i++)
			{
				data[i] = this._spectrum.ReadMemory((ushort)((int)ADDR + i));
			}
		}

		private bool dasmPanel_CheckBreakpoint(object Sender, ushort ADDR)
		{
			return this._spectrum.CheckBreakpoint(ADDR);
		}

		private void dasmPanel_SetBreakpoint(object Sender, ushort Addr)
		{
			if (this._spectrum.CheckBreakpoint(Addr))
			{
				this._spectrum.RemoveBreakpoint(Addr);
				return;
			}
			this._spectrum.AddBreakpoint(Addr);
		}

		private void FormCPU_KeyDown(object sender, KeyEventArgs e)
		{
			switch (e.KeyCode)
			{
			case Keys.F3:
				if (this._spectrum.IsRunning)
				{
					return;
				}
				this._spectrum.DoReset();
				this.UpdateCPU(true);
				return;
			case Keys.F4:
			case Keys.F6:
				break;
			case Keys.F5:
				this._spectrum.IsRunning = false;
				this.UpdateCPU(true);
				break;
			case Keys.F7:
				if (this._spectrum.IsRunning)
				{
					return;
				}
				try
				{
					this._spectrum.DoStepInto();
				}
				catch (Exception ex)
				{
					Logger.GetLogger().LogError(ex);
					PlatformFactory.Platform.ShowFatalError(ex);
				}
				this.UpdateCPU(true);
				return;
			case Keys.F8:
				if (this._spectrum.IsRunning)
				{
					return;
				}
				try
				{
					this._spectrum.DoStepOver();
				}
				catch (Exception ex2)
				{
					Logger.GetLogger().LogError(ex2);
					PlatformFactory.Platform.ShowFatalError(ex2);
				}
				this.UpdateCPU(true);
				return;
			case Keys.F9:
				this._spectrum.IsRunning = true;
				this.UpdateCPU(false);
				return;
			default:
				return;
			}
		}

		private void menuItemDasmGotoADDR_Click(object sender, EventArgs e)
		{
			int num = 0;
			if (!InputBox.InputValue("Адрес дизассемблирования", "Новое значение:", "#", "X4", ref num, 0, 65535))
			{
				return;
			}
			this.dasmPanel.TopAddress = (ushort)num;
		}

		private void menuItemDasmGotoPC_Click(object sender, EventArgs e)
		{
			this.dasmPanel.ActiveAddress = this._spectrum.CPU.regs.PC;
			this.dasmPanel.UpdateLines();
			this.Refresh();
		}

		private void menuItemDasmClearBP_Click(object sender, EventArgs e)
		{
			this._spectrum.ClearBreakpoints();
			this.UpdateCPU(false);
		}

		private void menuItemDasmRefresh_Click(object sender, EventArgs e)
		{
			this.dasmPanel.UpdateLines();
			this.Refresh();
		}

		private void listF_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			if (this.listF.SelectedIndex < 0)
			{
				return;
			}
			if (this._spectrum.IsRunning)
			{
				return;
			}
			Registers regs = this._spectrum.CPU.regs;
			regs.F ^= (byte)(128 >> this.listF.SelectedIndex);
			this.UpdateREGS();
		}

		private void listREGS_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			if (this.listREGS.SelectedIndex < 0)
			{
				return;
			}
			if (this._spectrum.IsRunning)
			{
				return;
			}
			this.ChangeRegByIndex(this.listREGS.SelectedIndex);
		}

		private void ChangeRegByIndex(int index)
		{
			switch (index)
			{
			case 0:
				this.ChangeReg(ref this._spectrum.CPU.regs.PC, "PC");
				return;
			case 1:
				this.ChangeReg(ref this._spectrum.CPU.regs.IR, "IR");
				return;
			case 2:
				this.ChangeReg(ref this._spectrum.CPU.regs.SP, "SP");
				return;
			case 3:
				this.ChangeReg(ref this._spectrum.CPU.regs.AF, "AF");
				return;
			case 4:
				this.ChangeReg(ref this._spectrum.CPU.regs.HL, "HL");
				return;
			case 5:
				this.ChangeReg(ref this._spectrum.CPU.regs.DE, "DE");
				return;
			case 6:
				this.ChangeReg(ref this._spectrum.CPU.regs.BC, "BC");
				return;
			case 7:
				this.ChangeReg(ref this._spectrum.CPU.regs.IX, "IX");
				return;
			case 8:
				this.ChangeReg(ref this._spectrum.CPU.regs.IY, "IY");
				return;
			case 9:
				this.ChangeReg(ref this._spectrum.CPU.regs._AF, "AF'");
				return;
			case 10:
				this.ChangeReg(ref this._spectrum.CPU.regs._HL, "HL'");
				return;
			case 11:
				this.ChangeReg(ref this._spectrum.CPU.regs._DE, "DE'");
				return;
			case 12:
				this.ChangeReg(ref this._spectrum.CPU.regs._BC, "BC'");
				return;
			case 13:
				this.ChangeReg(ref this._spectrum.CPU.regs.MW, "MW (Memptr Word)");
				return;
			default:
				return;
			}
		}

		private void ChangeReg(ref ushort p, string reg)
		{
			int num = (int)p;
			if (!InputBox.InputValue("Изменить регистр " + reg, "Новое значение:", "#", "X4", ref num, 0, 65535))
			{
				return;
			}
			p = (ushort)num;
			this.UpdateCPU(false);
		}

		private void contextMenuDasm_Popup(object sender, EventArgs e)
		{
			if (this._spectrum.IsRunning)
			{
				this.menuItemDasmClearBreakpoints.Enabled = false;
				return;
			}
			this.menuItemDasmClearBreakpoints.Enabled = true;
		}

		private void dataPanel_DataClick(object Sender, ushort Addr)
		{
			int num = (int)this._spectrum.ReadMemory(Addr);
			if (!InputBox.InputValue("POKE #" + Addr.ToString("X4"), "Значение:", "#", "X2", ref num, 0, 255))
			{
				return;
			}
			this._spectrum.WriteMemory(Addr, (byte)num);
			this.UpdateCPU(false);
		}

		private void menuItemDataGotoADDR_Click(object sender, EventArgs e)
		{
			int topAddress = (int)this.dataPanel.TopAddress;
			if (!InputBox.InputValue("Адрес дампа", "Новое значение:", "#", "X4", ref topAddress, 0, 65535))
			{
				return;
			}
			this.dataPanel.TopAddress = (ushort)topAddress;
		}

		private void menuItemDataRefresh_Click(object sender, EventArgs e)
		{
			this.dataPanel.UpdateLines();
			this.Refresh();
		}

		private void menuItemDataSetColumnCount_Click(object sender, EventArgs e)
		{
			int colCount = this.dataPanel.ColCount;
			if (!InputBox.InputValue("Число столбцов", "Новое значение:", "", "", ref colCount, 1, 32))
			{
				return;
			}
			this.dataPanel.ColCount = colCount;
		}

		private void dasmPanel_MouseClick(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				this.contextMenuDasm.Show(this.dasmPanel.ToString(), (System.Windows.Forms.IWin32Window)(object)e.Location);
			}
		}

		private void dataPanel_MouseClick(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				this.contextMenuData.Show(this.dataPanel, e.Location);
			}
		}

		private void FormCPU_FormClosing(object sender, FormClosingEventArgs e)
		{
			e.Cancel = true;
			base.Hide();
		}

		private void listState_DoubleClick(object sender, EventArgs e)
		{
			if (this.listState.SelectedIndex < 0)
			{
				return;
			}
			if (this._spectrum.IsRunning)
			{
				return;
			}
			switch (this.listState.SelectedIndex)
			{
			case 0:
				this._spectrum.CPU.IFF1 = (this._spectrum.CPU.IFF2 = !this._spectrum.CPU.IFF1);
				break;
			case 1:
				this._spectrum.CPU.HALTED = !this._spectrum.CPU.HALTED;
				break;
			case 2:
			{
				Z80CPU cpu = this._spectrum.CPU;
				cpu.IM += 1;
				if (this._spectrum.CPU.IM > 2)
				{
					this._spectrum.CPU.IM = 0;
				}
				break;
			}
			}
			this.UpdateCPU(false);
		}

		private static FormCPU _instance;

		private Spectrum _spectrum;
	}
}
