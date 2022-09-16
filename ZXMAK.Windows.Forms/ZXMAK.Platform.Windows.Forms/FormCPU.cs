using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using ZXMAK.Engine;
using ZXMAK.Engine.Z80;
using ZXMAK.Logging;
using ZXMAK.Platform.Windows.Forms.Controls;

namespace ZXMAK.Platform.Windows.Forms;

public class FormCPU : Form
{
	private static FormCPU _instance;

	private Spectrum _spectrum;

	private IContainer components;

	private Panel panelStatus;

	private Splitter splitter1;

	private Panel panelMem;

	private Splitter splitter2;

	private Panel panelDasm;

	private Panel panelRegs;

	private Panel panelState;

	private Splitter splitter3;

	private ListBox listREGS;

	private ListBox listF;

	private Splitter splitter4;

	private ListBox listState;

	private DasmPanel dasmPanel;

	private DataPanel dataPanel;

	private ContextMenu contextMenuDasm;

	private MenuItem menuItemDasmGotoADDR;

	private MenuItem menuItem2;

	private MenuItem menuItemDasmClearBreakpoints;

	private MenuItem menuItem4;

	private MenuItem menuItemDasmRefresh;

	private MenuItem menuItemDasmGotoPC;

	private ContextMenu contextMenuData;

	private MenuItem menuItemDataGotoADDR;

	private MenuItem menuItemDataSetColumnCount;

	private MenuItem menuItem5;

	private MenuItem menuItemDataRefresh;

	public static FormCPU GetInstance(Spectrum spectrum)
	{
		if (_instance != null && _instance._spectrum == spectrum)
		{
			return _instance;
		}
		_instance = new FormCPU(spectrum);
		return _instance;
	}

	private FormCPU(Spectrum spectrum)
	{
		_spectrum = spectrum;
		InitializeComponent();
		_spectrum.UpdateState += spectrum_OnUpdateState;
		_spectrum.Breakpoint += spectrum_OnBreakpoint;
	}

	private void FormCPU_FormClosed(object sender, FormClosedEventArgs e)
	{
		_instance = null;
	}

	private void FormCPU_Load(object sender, EventArgs e)
	{
		UpdateCPU(updatePC: true);
	}

	private void FormCPU_Shown(object sender, EventArgs e)
	{
		Show();
		UpdateCPU(updatePC: false);
		dasmPanel.Focus();
		Select();
	}

	private void spectrum_OnUpdateState(object sender, EventArgs args)
	{
		if (base.InvokeRequired)
		{
			Invoke(new EventHandler(spectrum_OnUpdateState), sender, args);
		}
		else
		{
			UpdateCPU(updatePC: true);
		}
	}

	private void spectrum_OnBreakpoint(object sender, EventArgs args)
	{
		if (base.InvokeRequired)
		{
			Invoke(new EventHandler(spectrum_OnBreakpoint), sender, args);
		}
		else
		{
			Show();
			UpdateCPU(updatePC: true);
			dasmPanel.Focus();
			Select();
		}
	}

	private void UpdateCPU(bool updatePC)
	{
		if (_spectrum.IsRunning)
		{
			updatePC = false;
		}
		dasmPanel.ForeColor = (_spectrum.IsRunning ? SystemColors.ControlDarkDark : SystemColors.ControlText);
		UpdateREGS();
		UpdateDASM(updatePC);
		UpdateDATA();
	}

	private void UpdateREGS()
	{
		listREGS.Items.Clear();
		listREGS.Items.Add(" PC = " + _spectrum.CPU.regs.PC.ToString("X4"));
		listREGS.Items.Add(" IR = " + _spectrum.CPU.regs.IR.ToString("X4"));
		listREGS.Items.Add(" SP = " + _spectrum.CPU.regs.SP.ToString("X4"));
		listREGS.Items.Add(" AF = " + _spectrum.CPU.regs.AF.ToString("X4"));
		listREGS.Items.Add(" HL = " + _spectrum.CPU.regs.HL.ToString("X4"));
		listREGS.Items.Add(" DE = " + _spectrum.CPU.regs.DE.ToString("X4"));
		listREGS.Items.Add(" BC = " + _spectrum.CPU.regs.BC.ToString("X4"));
		listREGS.Items.Add(" IX = " + _spectrum.CPU.regs.IX.ToString("X4"));
		listREGS.Items.Add(" IY = " + _spectrum.CPU.regs.IY.ToString("X4"));
		listREGS.Items.Add(" AF'= " + _spectrum.CPU.regs._AF.ToString("X4"));
		listREGS.Items.Add(" HL'= " + _spectrum.CPU.regs._HL.ToString("X4"));
		listREGS.Items.Add(" DE'= " + _spectrum.CPU.regs._DE.ToString("X4"));
		listREGS.Items.Add(" BC'= " + _spectrum.CPU.regs._BC.ToString("X4"));
		listREGS.Items.Add(" MW = " + _spectrum.CPU.regs.MW.ToString("X4"));
		listF.Items.Clear();
		listF.Items.Add("  S = " + (((_spectrum.CPU.regs.F & 0x80u) != 0) ? "1" : "0"));
		listF.Items.Add("  Z = " + (((_spectrum.CPU.regs.F & 0x40u) != 0) ? "1" : "0"));
		listF.Items.Add(" F5 = " + (((_spectrum.CPU.regs.F & 0x20u) != 0) ? "1" : "0"));
		listF.Items.Add("  H = " + (((_spectrum.CPU.regs.F & 0x10u) != 0) ? "1" : "0"));
		listF.Items.Add(" F3 = " + (((_spectrum.CPU.regs.F & 8u) != 0) ? "1" : "0"));
		listF.Items.Add("P/V = " + (((_spectrum.CPU.regs.F & 4u) != 0) ? "1" : "0"));
		listF.Items.Add("  N = " + (((_spectrum.CPU.regs.F & 2u) != 0) ? "1" : "0"));
		listF.Items.Add("  C = " + ((((uint)_spectrum.CPU.regs.F & (true ? 1u : 0u)) != 0) ? "1" : "0"));
		listState.Items.Clear();
		listState.Items.Add("IFF1=" + (_spectrum.CPU.IFF1 ? "1" : "0") + " IFF2=" + (_spectrum.CPU.IFF2 ? "1" : "0"));
		listState.Items.Add("HALT=" + (_spectrum.CPU.HALTED ? "HALTED" : "") + " " + (_spectrum.CPU.BlockINT ? "BINT" : ""));
		listState.Items.Add("  IM=" + _spectrum.CPU.IM);
		listState.Items.Add("  FX=" + _spectrum.CPU.FX);
		listState.Items.Add(" XFX=" + _spectrum.CPU.XFX);
		listState.Items.Add("Tact=" + _spectrum.CPU.Tact);
		listState.Items.Add("frmT=" + _spectrum.GetFrameTact());
	}

	private void UpdateDASM(bool updatePC)
	{
		if (!_spectrum.IsRunning && updatePC)
		{
			dasmPanel.ActiveAddress = _spectrum.CPU.regs.PC;
			return;
		}
		dasmPanel.UpdateLines();
		dasmPanel.Refresh();
	}

	private void UpdateDATA()
	{
		dataPanel.UpdateLines();
		dataPanel.Refresh();
	}

	private bool dasmPanel_CheckExecuting(object Sender, ushort ADDR)
	{
		if (_spectrum.IsRunning)
		{
			return false;
		}
		if (ADDR == _spectrum.CPU.regs.PC)
		{
			return true;
		}
		return false;
	}

	private void dasmPanel_GetDasm(object Sender, ushort ADDR, out string DASM, out int len)
	{
		DASM = Z80CPU.GetMnemonic(_spectrum.ReadMemory, ADDR, Hex: true, out len);
	}

	private void dasmPanel_GetData(object Sender, ushort ADDR, int len, out byte[] data)
	{
		data = new byte[len];
		for (int i = 0; i < len; i++)
		{
			data[i] = _spectrum.ReadMemory((ushort)(ADDR + i));
		}
	}

	private bool dasmPanel_CheckBreakpoint(object Sender, ushort ADDR)
	{
		return _spectrum.CheckBreakpoint(ADDR);
	}

	private void dasmPanel_SetBreakpoint(object Sender, ushort Addr)
	{
		if (_spectrum.CheckBreakpoint(Addr))
		{
			_spectrum.RemoveBreakpoint(Addr);
		}
		else
		{
			_spectrum.AddBreakpoint(Addr);
		}
	}

	private void FormCPU_KeyDown(object sender, KeyEventArgs e)
	{
		switch (e.KeyCode)
		{
		case Keys.F3:
			if (!_spectrum.IsRunning)
			{
				_spectrum.DoReset();
				UpdateCPU(updatePC: true);
			}
			break;
		case Keys.F7:
			if (!_spectrum.IsRunning)
			{
				try
				{
					_spectrum.DoStepInto();
				}
				catch (Exception ex2)
				{
					Logger.GetLogger().LogError(ex2);
					PlatformFactory.Platform.ShowFatalError(ex2);
				}
				UpdateCPU(updatePC: true);
			}
			break;
		case Keys.F8:
			if (!_spectrum.IsRunning)
			{
				try
				{
					_spectrum.DoStepOver();
				}
				catch (Exception ex)
				{
					Logger.GetLogger().LogError(ex);
					PlatformFactory.Platform.ShowFatalError(ex);
				}
				UpdateCPU(updatePC: true);
			}
			break;
		case Keys.F9:
			_spectrum.IsRunning = true;
			UpdateCPU(updatePC: false);
			break;
		case Keys.F5:
			_spectrum.IsRunning = false;
			UpdateCPU(updatePC: true);
			break;
		case Keys.F4:
		case Keys.F6:
			break;
		}
	}

	private void menuItemDasmGotoADDR_Click(object sender, EventArgs e)
	{
		int value = 0;
		if (InputBox.InputValue("Адрес дизассемблирования", "Новое значение:", "#", "X4", ref value, 0, 65535))
		{
			dasmPanel.TopAddress = (ushort)value;
		}
	}

	private void menuItemDasmGotoPC_Click(object sender, EventArgs e)
	{
		dasmPanel.ActiveAddress = _spectrum.CPU.regs.PC;
		dasmPanel.UpdateLines();
		Refresh();
	}

	private void menuItemDasmClearBP_Click(object sender, EventArgs e)
	{
		_spectrum.ClearBreakpoints();
		UpdateCPU(updatePC: false);
	}

	private void menuItemDasmRefresh_Click(object sender, EventArgs e)
	{
		dasmPanel.UpdateLines();
		Refresh();
	}

	private void listF_MouseDoubleClick(object sender, MouseEventArgs e)
	{
		if (listF.SelectedIndex >= 0 && !_spectrum.IsRunning)
		{
			_spectrum.CPU.regs.F ^= (byte)(128 >> listF.SelectedIndex);
			UpdateREGS();
		}
	}

	private void listREGS_MouseDoubleClick(object sender, MouseEventArgs e)
	{
		if (listREGS.SelectedIndex >= 0 && !_spectrum.IsRunning)
		{
			ChangeRegByIndex(listREGS.SelectedIndex);
		}
	}

	private void ChangeRegByIndex(int index)
	{
		switch (index)
		{
		case 0:
			ChangeReg(ref _spectrum.CPU.regs.PC, "PC");
			break;
		case 1:
			ChangeReg(ref _spectrum.CPU.regs.IR, "IR");
			break;
		case 2:
			ChangeReg(ref _spectrum.CPU.regs.SP, "SP");
			break;
		case 3:
			ChangeReg(ref _spectrum.CPU.regs.AF, "AF");
			break;
		case 4:
			ChangeReg(ref _spectrum.CPU.regs.HL, "HL");
			break;
		case 5:
			ChangeReg(ref _spectrum.CPU.regs.DE, "DE");
			break;
		case 6:
			ChangeReg(ref _spectrum.CPU.regs.BC, "BC");
			break;
		case 7:
			ChangeReg(ref _spectrum.CPU.regs.IX, "IX");
			break;
		case 8:
			ChangeReg(ref _spectrum.CPU.regs.IY, "IY");
			break;
		case 9:
			ChangeReg(ref _spectrum.CPU.regs._AF, "AF'");
			break;
		case 10:
			ChangeReg(ref _spectrum.CPU.regs._HL, "HL'");
			break;
		case 11:
			ChangeReg(ref _spectrum.CPU.regs._DE, "DE'");
			break;
		case 12:
			ChangeReg(ref _spectrum.CPU.regs._BC, "BC'");
			break;
		case 13:
			ChangeReg(ref _spectrum.CPU.regs.MW, "MW (Memptr Word)");
			break;
		}
	}

	private void ChangeReg(ref ushort p, string reg)
	{
		int value = p;
		if (InputBox.InputValue("Изменить регистр " + reg, "Новое значение:", "#", "X4", ref value, 0, 65535))
		{
			p = (ushort)value;
			UpdateCPU(updatePC: false);
		}
	}

	private void contextMenuDasm_Popup(object sender, EventArgs e)
	{
		if (_spectrum.IsRunning)
		{
			menuItemDasmClearBreakpoints.Enabled = false;
		}
		else
		{
			menuItemDasmClearBreakpoints.Enabled = true;
		}
	}

	private void dataPanel_DataClick(object Sender, ushort Addr)
	{
		int value = _spectrum.ReadMemory(Addr);
		if (InputBox.InputValue("POKE #" + Addr.ToString("X4"), "Значение:", "#", "X2", ref value, 0, 255))
		{
			_spectrum.WriteMemory(Addr, (byte)value);
			UpdateCPU(updatePC: false);
		}
	}

	private void menuItemDataGotoADDR_Click(object sender, EventArgs e)
	{
		int value = dataPanel.TopAddress;
		if (InputBox.InputValue("Адрес дампа", "Новое значение:", "#", "X4", ref value, 0, 65535))
		{
			dataPanel.TopAddress = (ushort)value;
		}
	}

	private void menuItemDataRefresh_Click(object sender, EventArgs e)
	{
		dataPanel.UpdateLines();
		Refresh();
	}

	private void menuItemDataSetColumnCount_Click(object sender, EventArgs e)
	{
		int value = dataPanel.ColCount;
		if (InputBox.InputValue("Число столбцов", "Новое значение:", "", "", ref value, 1, 32))
		{
			dataPanel.ColCount = value;
		}
	}

	private void dasmPanel_MouseClick(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Right)
		{
			contextMenuDasm.Show(dasmPanel, e.Location);
		}
	}

	private void dataPanel_MouseClick(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Right)
		{
			contextMenuData.Show(dataPanel, e.Location);
		}
	}

	private void FormCPU_FormClosing(object sender, FormClosingEventArgs e)
	{
		e.Cancel = true;
		Hide();
	}

	private void listState_DoubleClick(object sender, EventArgs e)
	{
		if (listState.SelectedIndex < 0 || _spectrum.IsRunning)
		{
			return;
		}
		switch (listState.SelectedIndex)
		{
		case 0:
			_spectrum.CPU.IFF1 = (_spectrum.CPU.IFF2 = !_spectrum.CPU.IFF1);
			break;
		case 1:
			_spectrum.CPU.HALTED = !_spectrum.CPU.HALTED;
			break;
		case 2:
			_spectrum.CPU.IM++;
			if (_spectrum.CPU.IM > 2)
			{
				_spectrum.CPU.IM = 0;
			}
			break;
		}
		UpdateCPU(updatePC: false);
	}

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
		this.panelStatus = new System.Windows.Forms.Panel();
		this.panelState = new System.Windows.Forms.Panel();
		this.listState = new System.Windows.Forms.ListBox();
		this.splitter3 = new System.Windows.Forms.Splitter();
		this.panelRegs = new System.Windows.Forms.Panel();
		this.listF = new System.Windows.Forms.ListBox();
		this.splitter4 = new System.Windows.Forms.Splitter();
		this.listREGS = new System.Windows.Forms.ListBox();
		this.splitter1 = new System.Windows.Forms.Splitter();
		this.panelMem = new System.Windows.Forms.Panel();
		this.dataPanel = new ZXMAK.Platform.Windows.Forms.Controls.DataPanel();
		this.splitter2 = new System.Windows.Forms.Splitter();
		this.panelDasm = new System.Windows.Forms.Panel();
		this.dasmPanel = new ZXMAK.Platform.Windows.Forms.Controls.DasmPanel();
		this.contextMenuDasm = new System.Windows.Forms.ContextMenu();
		this.menuItemDasmGotoADDR = new System.Windows.Forms.MenuItem();
		this.menuItemDasmGotoPC = new System.Windows.Forms.MenuItem();
		this.menuItem2 = new System.Windows.Forms.MenuItem();
		this.menuItemDasmClearBreakpoints = new System.Windows.Forms.MenuItem();
		this.menuItem4 = new System.Windows.Forms.MenuItem();
		this.menuItemDasmRefresh = new System.Windows.Forms.MenuItem();
		this.contextMenuData = new System.Windows.Forms.ContextMenu();
		this.menuItemDataGotoADDR = new System.Windows.Forms.MenuItem();
		this.menuItemDataSetColumnCount = new System.Windows.Forms.MenuItem();
		this.menuItem5 = new System.Windows.Forms.MenuItem();
		this.menuItemDataRefresh = new System.Windows.Forms.MenuItem();
		this.panelStatus.SuspendLayout();
		this.panelState.SuspendLayout();
		this.panelRegs.SuspendLayout();
		this.panelMem.SuspendLayout();
		this.panelDasm.SuspendLayout();
		base.SuspendLayout();
		this.panelStatus.Controls.Add(this.panelState);
		this.panelStatus.Controls.Add(this.splitter3);
		this.panelStatus.Controls.Add(this.panelRegs);
		this.panelStatus.Dock = System.Windows.Forms.DockStyle.Right;
		this.panelStatus.Location = new System.Drawing.Point(451, 0);
		this.panelStatus.Name = "panelStatus";
		this.panelStatus.Size = new System.Drawing.Size(168, 345);
		this.panelStatus.TabIndex = 0;
		this.panelState.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.panelState.Controls.Add(this.listState);
		this.panelState.Dock = System.Windows.Forms.DockStyle.Fill;
		this.panelState.Location = new System.Drawing.Point(0, 224);
		this.panelState.Name = "panelState";
		this.panelState.Size = new System.Drawing.Size(168, 121);
		this.panelState.TabIndex = 2;
		this.listState.Dock = System.Windows.Forms.DockStyle.Fill;
		this.listState.Font = new System.Drawing.Font("Courier New", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.listState.FormattingEnabled = true;
		this.listState.IntegralHeight = false;
		this.listState.ItemHeight = 14;
		this.listState.Location = new System.Drawing.Point(0, 0);
		this.listState.Name = "listState";
		this.listState.Size = new System.Drawing.Size(164, 117);
		this.listState.TabIndex = 3;
		this.listState.DoubleClick += new System.EventHandler(listState_DoubleClick);
		this.splitter3.Dock = System.Windows.Forms.DockStyle.Top;
		this.splitter3.Location = new System.Drawing.Point(0, 221);
		this.splitter3.Name = "splitter3";
		this.splitter3.Size = new System.Drawing.Size(168, 3);
		this.splitter3.TabIndex = 1;
		this.splitter3.TabStop = false;
		this.panelRegs.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.panelRegs.Controls.Add(this.listF);
		this.panelRegs.Controls.Add(this.splitter4);
		this.panelRegs.Controls.Add(this.listREGS);
		this.panelRegs.Dock = System.Windows.Forms.DockStyle.Top;
		this.panelRegs.Location = new System.Drawing.Point(0, 0);
		this.panelRegs.Name = "panelRegs";
		this.panelRegs.Size = new System.Drawing.Size(168, 221);
		this.panelRegs.TabIndex = 0;
		this.listF.BackColor = System.Drawing.SystemColors.ButtonFace;
		this.listF.Dock = System.Windows.Forms.DockStyle.Fill;
		this.listF.Font = new System.Drawing.Font("Courier New", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.listF.FormattingEnabled = true;
		this.listF.IntegralHeight = false;
		this.listF.ItemHeight = 15;
		this.listF.Items.AddRange(new object[8] { "  S = 0", "  Z = 0", " F5 = 0", "  H = 1", " F3 = 0", "P/V = 0", "  N = 0", "  C = 0" });
		this.listF.Location = new System.Drawing.Point(101, 0);
		this.listF.Name = "listF";
		this.listF.Size = new System.Drawing.Size(63, 217);
		this.listF.TabIndex = 2;
		this.listF.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(listF_MouseDoubleClick);
		this.splitter4.Location = new System.Drawing.Point(98, 0);
		this.splitter4.Name = "splitter4";
		this.splitter4.Size = new System.Drawing.Size(3, 217);
		this.splitter4.TabIndex = 1;
		this.splitter4.TabStop = false;
		this.listREGS.BackColor = System.Drawing.SystemColors.ButtonFace;
		this.listREGS.Dock = System.Windows.Forms.DockStyle.Left;
		this.listREGS.Font = new System.Drawing.Font("Courier New", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.listREGS.FormattingEnabled = true;
		this.listREGS.IntegralHeight = false;
		this.listREGS.ItemHeight = 15;
		this.listREGS.Items.AddRange(new object[14]
		{
			" PC = 0000", " IR = 0000", " SP = 0000", " AF = 0000", " HL = 0000", " DE = 0000", " BC = 0000", " IX = 0000", " IY = 0000", "AF' = 0000",
			"HL' = 0000", "DE' = 0000", "BC' = 0000", " MW = 0000"
		});
		this.listREGS.Location = new System.Drawing.Point(0, 0);
		this.listREGS.Name = "listREGS";
		this.listREGS.Size = new System.Drawing.Size(98, 217);
		this.listREGS.TabIndex = 1;
		this.listREGS.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(listREGS_MouseDoubleClick);
		this.splitter1.Dock = System.Windows.Forms.DockStyle.Right;
		this.splitter1.Location = new System.Drawing.Point(448, 0);
		this.splitter1.Name = "splitter1";
		this.splitter1.Size = new System.Drawing.Size(3, 345);
		this.splitter1.TabIndex = 1;
		this.splitter1.TabStop = false;
		this.panelMem.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.panelMem.Controls.Add(this.dataPanel);
		this.panelMem.Dock = System.Windows.Forms.DockStyle.Bottom;
		this.panelMem.Location = new System.Drawing.Point(0, 223);
		this.panelMem.Name = "panelMem";
		this.panelMem.Size = new System.Drawing.Size(448, 122);
		this.panelMem.TabIndex = 2;
		this.dataPanel.ColCount = 8;
		this.dataPanel.Dock = System.Windows.Forms.DockStyle.Fill;
		this.dataPanel.Font = new System.Drawing.Font("Courier New", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.dataPanel.Location = new System.Drawing.Point(0, 0);
		this.dataPanel.Name = "dataPanel";
		this.dataPanel.Size = new System.Drawing.Size(444, 118);
		this.dataPanel.TabIndex = 0;
		this.dataPanel.Text = "dataPanel1";
		this.dataPanel.TopAddress = 0;
		this.dataPanel.GetData += new ZXMAK.Platform.Windows.Forms.Controls.DataPanel.ONGETDATACPU(dasmPanel_GetData);
		this.dataPanel.MouseClick += new System.Windows.Forms.MouseEventHandler(dataPanel_MouseClick);
		this.dataPanel.DataClick += new ZXMAK.Platform.Windows.Forms.Controls.DataPanel.ONCLICKCPU(dataPanel_DataClick);
		this.splitter2.Dock = System.Windows.Forms.DockStyle.Bottom;
		this.splitter2.Location = new System.Drawing.Point(0, 220);
		this.splitter2.Name = "splitter2";
		this.splitter2.Size = new System.Drawing.Size(448, 3);
		this.splitter2.TabIndex = 3;
		this.splitter2.TabStop = false;
		this.panelDasm.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.panelDasm.Controls.Add(this.dasmPanel);
		this.panelDasm.Dock = System.Windows.Forms.DockStyle.Fill;
		this.panelDasm.Location = new System.Drawing.Point(0, 0);
		this.panelDasm.Name = "panelDasm";
		this.panelDasm.Size = new System.Drawing.Size(448, 220);
		this.panelDasm.TabIndex = 4;
		this.dasmPanel.ActiveAddress = 0;
		this.dasmPanel.BreakpointColor = System.Drawing.Color.Red;
		this.dasmPanel.BreakpointForeColor = System.Drawing.Color.Black;
		this.dasmPanel.Dock = System.Windows.Forms.DockStyle.Fill;
		this.dasmPanel.Font = new System.Drawing.Font("Courier New", 9f);
		this.dasmPanel.ForeColor = System.Drawing.SystemColors.ControlText;
		this.dasmPanel.Location = new System.Drawing.Point(0, 0);
		this.dasmPanel.Name = "dasmPanel";
		this.dasmPanel.Size = new System.Drawing.Size(444, 216);
		this.dasmPanel.TabIndex = 0;
		this.dasmPanel.Text = "dasmPanel1";
		this.dasmPanel.TopAddress = 0;
		this.dasmPanel.CheckExecuting += new ZXMAK.Platform.Windows.Forms.Controls.DasmPanel.ONCHECKCPU(dasmPanel_CheckExecuting);
		this.dasmPanel.GetData += new ZXMAK.Platform.Windows.Forms.Controls.DasmPanel.ONGETDATACPU(dasmPanel_GetData);
		this.dasmPanel.MouseClick += new System.Windows.Forms.MouseEventHandler(dasmPanel_MouseClick);
		this.dasmPanel.CheckBreakpoint += new ZXMAK.Platform.Windows.Forms.Controls.DasmPanel.ONCHECKCPU(dasmPanel_CheckBreakpoint);
		this.dasmPanel.BreakpointClick += new ZXMAK.Platform.Windows.Forms.Controls.DasmPanel.ONCLICKCPU(dasmPanel_SetBreakpoint);
		this.dasmPanel.GetDasm += new ZXMAK.Platform.Windows.Forms.Controls.DasmPanel.ONGETDASMCPU(dasmPanel_GetDasm);
		this.contextMenuDasm.MenuItems.AddRange(new System.Windows.Forms.MenuItem[6] { this.menuItemDasmGotoADDR, this.menuItemDasmGotoPC, this.menuItem2, this.menuItemDasmClearBreakpoints, this.menuItem4, this.menuItemDasmRefresh });
		this.contextMenuDasm.Popup += new System.EventHandler(contextMenuDasm_Popup);
		this.menuItemDasmGotoADDR.Index = 0;
		this.menuItemDasmGotoADDR.Text = "Goto address...";
		this.menuItemDasmGotoADDR.Click += new System.EventHandler(menuItemDasmGotoADDR_Click);
		this.menuItemDasmGotoPC.Index = 1;
		this.menuItemDasmGotoPC.Text = "Goto PC";
		this.menuItemDasmGotoPC.Click += new System.EventHandler(menuItemDasmGotoPC_Click);
		this.menuItem2.Index = 2;
		this.menuItem2.Text = "-";
		this.menuItemDasmClearBreakpoints.Index = 3;
		this.menuItemDasmClearBreakpoints.Text = "Reset breakpoints";
		this.menuItemDasmClearBreakpoints.Click += new System.EventHandler(menuItemDasmClearBP_Click);
		this.menuItem4.Index = 4;
		this.menuItem4.Text = "-";
		this.menuItemDasmRefresh.Index = 5;
		this.menuItemDasmRefresh.Text = "Refresh";
		this.menuItemDasmRefresh.Click += new System.EventHandler(menuItemDasmRefresh_Click);
		this.contextMenuData.MenuItems.AddRange(new System.Windows.Forms.MenuItem[4] { this.menuItemDataGotoADDR, this.menuItemDataSetColumnCount, this.menuItem5, this.menuItemDataRefresh });
		this.menuItemDataGotoADDR.Index = 0;
		this.menuItemDataGotoADDR.Text = "Goto Address...";
		this.menuItemDataGotoADDR.Click += new System.EventHandler(menuItemDataGotoADDR_Click);
		this.menuItemDataSetColumnCount.Index = 1;
		this.menuItemDataSetColumnCount.Text = "Set column count...";
		this.menuItemDataSetColumnCount.Click += new System.EventHandler(menuItemDataSetColumnCount_Click);
		this.menuItem5.Index = 2;
		this.menuItem5.Text = "-";
		this.menuItemDataRefresh.Index = 3;
		this.menuItemDataRefresh.Text = "Refresh";
		this.menuItemDataRefresh.Click += new System.EventHandler(menuItemDataRefresh_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(619, 345);
		base.Controls.Add(this.panelDasm);
		base.Controls.Add(this.splitter2);
		base.Controls.Add(this.panelMem);
		base.Controls.Add(this.splitter1);
		base.Controls.Add(this.panelStatus);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
		base.KeyPreview = true;
		base.Name = "FormCPU";
		this.Text = "Z80 CPU";
		base.FormClosed += new System.Windows.Forms.FormClosedEventHandler(FormCPU_FormClosed);
		base.Shown += new System.EventHandler(FormCPU_Shown);
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(FormCPU_FormClosing);
		base.KeyDown += new System.Windows.Forms.KeyEventHandler(FormCPU_KeyDown);
		base.Load += new System.EventHandler(FormCPU_Load);
		this.panelStatus.ResumeLayout(false);
		this.panelState.ResumeLayout(false);
		this.panelRegs.ResumeLayout(false);
		this.panelMem.ResumeLayout(false);
		this.panelDasm.ResumeLayout(false);
		base.ResumeLayout(false);
	}
}
