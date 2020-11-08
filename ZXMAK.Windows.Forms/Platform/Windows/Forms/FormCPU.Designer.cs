namespace ZXMAK.Platform.Windows.Forms
{
	public partial class FormCPU : global::System.Windows.Forms.Form
	{
		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			this.panelStatus = new global::System.Windows.Forms.Panel();
			this.panelState = new global::System.Windows.Forms.Panel();
			this.listState = new global::System.Windows.Forms.ListBox();
			this.splitter3 = new global::System.Windows.Forms.Splitter();
			this.panelRegs = new global::System.Windows.Forms.Panel();
			this.listF = new global::System.Windows.Forms.ListBox();
			this.splitter4 = new global::System.Windows.Forms.Splitter();
			this.listREGS = new global::System.Windows.Forms.ListBox();
			this.splitter1 = new global::System.Windows.Forms.Splitter();
			this.panelMem = new global::System.Windows.Forms.Panel();
			this.dataPanel = new global::ZXMAK.Platform.Windows.Forms.Controls.DataPanel();
			this.splitter2 = new global::System.Windows.Forms.Splitter();
			this.panelDasm = new global::System.Windows.Forms.Panel();
			this.dasmPanel = new global::ZXMAK.Platform.Windows.Forms.Controls.DasmPanel();
			this.contextMenuDasm = new global::System.Windows.Forms.ContextMenu();
			this.contextMenuDasmMI = new global::System.Windows.Forms.MenuStrip();
			this.menuItemDasmGotoADDR = new global::System.Windows.Forms.ToolStripMenuItem();
			this.menuItemDasmGotoPC = new global::System.Windows.Forms.ToolStripMenuItem();
			this.menuItem2 = new global::System.Windows.Forms.ToolStripMenuItem();
			this.menuItemDasmClearBreakpoints = new global::System.Windows.Forms.ToolStripMenuItem();
			this.menuItem4 = new global::System.Windows.Forms.ToolStripMenuItem();
			this.menuItemDasmRefresh = new global::System.Windows.Forms.ToolStripMenuItem();
			this.contextMenuData = new global::System.Windows.Forms.ContextMenuStrip();
			this.menuItemDataGotoADDR = new global::System.Windows.Forms.ToolStripMenuItem();
			this.menuItemDataSetColumnCount = new global::System.Windows.Forms.ToolStripMenuItem();
			this.menuItem5 = new global::System.Windows.Forms.ToolStripMenuItem();
			this.menuItemDataRefresh = new global::System.Windows.Forms.ToolStripMenuItem();
			this.panelStatus.SuspendLayout();
			this.panelState.SuspendLayout();
			this.panelRegs.SuspendLayout();
			this.panelMem.SuspendLayout();
			this.panelDasm.SuspendLayout();
			base.SuspendLayout();
			this.panelStatus.Controls.Add(this.panelState);
			this.panelStatus.Controls.Add(this.splitter3);
			this.panelStatus.Controls.Add(this.panelRegs);
			this.panelStatus.Dock = global::System.Windows.Forms.DockStyle.Right;
			this.panelStatus.Location = new global::System.Drawing.Point(451, 0);
			this.panelStatus.Name = "panelStatus";
			this.panelStatus.Size = new global::System.Drawing.Size(168, 345);
			this.panelStatus.TabIndex = 0;
			this.panelState.BorderStyle = global::System.Windows.Forms.BorderStyle.Fixed3D;
			this.panelState.Controls.Add(this.listState);
			this.panelState.Dock = global::System.Windows.Forms.DockStyle.Fill;
			this.panelState.Location = new global::System.Drawing.Point(0, 224);
			this.panelState.Name = "panelState";
			this.panelState.Size = new global::System.Drawing.Size(168, 121);
			this.panelState.TabIndex = 2;
			this.listState.Dock = global::System.Windows.Forms.DockStyle.Fill;
			this.listState.Font = new global::System.Drawing.Font("Courier New", 8.25f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 204);
			this.listState.FormattingEnabled = true;
			this.listState.IntegralHeight = false;
			this.listState.ItemHeight = 14;
			this.listState.Location = new global::System.Drawing.Point(0, 0);
			this.listState.Name = "listState";
			this.listState.Size = new global::System.Drawing.Size(164, 117);
			this.listState.TabIndex = 3;
			this.listState.DoubleClick += new global::System.EventHandler(this.listState_DoubleClick);
			this.splitter3.Dock = global::System.Windows.Forms.DockStyle.Top;
			this.splitter3.Location = new global::System.Drawing.Point(0, 221);
			this.splitter3.Name = "splitter3";
			this.splitter3.Size = new global::System.Drawing.Size(168, 3);
			this.splitter3.TabIndex = 1;
			this.splitter3.TabStop = false;
			this.panelRegs.BorderStyle = global::System.Windows.Forms.BorderStyle.Fixed3D;
			this.panelRegs.Controls.Add(this.listF);
			this.panelRegs.Controls.Add(this.splitter4);
			this.panelRegs.Controls.Add(this.listREGS);
			this.panelRegs.Dock = global::System.Windows.Forms.DockStyle.Top;
			this.panelRegs.Location = new global::System.Drawing.Point(0, 0);
			this.panelRegs.Name = "panelRegs";
			this.panelRegs.Size = new global::System.Drawing.Size(168, 221);
			this.panelRegs.TabIndex = 0;
			this.listF.BackColor = global::System.Drawing.SystemColors.ButtonFace;
			this.listF.Dock = global::System.Windows.Forms.DockStyle.Fill;
			this.listF.Font = new global::System.Drawing.Font("Courier New", 9f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 204);
			this.listF.FormattingEnabled = true;
			this.listF.IntegralHeight = false;
			this.listF.ItemHeight = 15;
			this.listF.Items.AddRange(new object[]
			{
				"  S = 0",
				"  Z = 0",
				" F5 = 0",
				"  H = 1",
				" F3 = 0",
				"P/V = 0",
				"  N = 0",
				"  C = 0"
			});
			this.listF.Location = new global::System.Drawing.Point(101, 0);
			this.listF.Name = "listF";
			this.listF.Size = new global::System.Drawing.Size(63, 217);
			this.listF.TabIndex = 2;
			this.listF.MouseDoubleClick += new global::System.Windows.Forms.MouseEventHandler(this.listF_MouseDoubleClick);
			this.splitter4.Location = new global::System.Drawing.Point(98, 0);
			this.splitter4.Name = "splitter4";
			this.splitter4.Size = new global::System.Drawing.Size(3, 217);
			this.splitter4.TabIndex = 1;
			this.splitter4.TabStop = false;
			this.listREGS.BackColor = global::System.Drawing.SystemColors.ButtonFace;
			this.listREGS.Dock = global::System.Windows.Forms.DockStyle.Left;
			this.listREGS.Font = new global::System.Drawing.Font("Courier New", 9f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 204);
			this.listREGS.FormattingEnabled = true;
			this.listREGS.IntegralHeight = false;
			this.listREGS.ItemHeight = 15;
			this.listREGS.Items.AddRange(new object[]
			{
				" PC = 0000",
				" IR = 0000",
				" SP = 0000",
				" AF = 0000",
				" HL = 0000",
				" DE = 0000",
				" BC = 0000",
				" IX = 0000",
				" IY = 0000",
				"AF' = 0000",
				"HL' = 0000",
				"DE' = 0000",
				"BC' = 0000",
				" MW = 0000"
			});
			this.listREGS.Location = new global::System.Drawing.Point(0, 0);
			this.listREGS.Name = "listREGS";
			this.listREGS.Size = new global::System.Drawing.Size(98, 217);
			this.listREGS.TabIndex = 1;
			this.listREGS.MouseDoubleClick += new global::System.Windows.Forms.MouseEventHandler(this.listREGS_MouseDoubleClick);
			this.splitter1.Dock = global::System.Windows.Forms.DockStyle.Right;
			this.splitter1.Location = new global::System.Drawing.Point(448, 0);
			this.splitter1.Name = "splitter1";
			this.splitter1.Size = new global::System.Drawing.Size(3, 345);
			this.splitter1.TabIndex = 1;
			this.splitter1.TabStop = false;
			this.panelMem.BorderStyle = global::System.Windows.Forms.BorderStyle.Fixed3D;
			this.panelMem.Controls.Add(this.dataPanel);
			this.panelMem.Dock = global::System.Windows.Forms.DockStyle.Bottom;
			this.panelMem.Location = new global::System.Drawing.Point(0, 223);
			this.panelMem.Name = "panelMem";
			this.panelMem.Size = new global::System.Drawing.Size(448, 122);
			this.panelMem.TabIndex = 2;
			this.dataPanel.ColCount = 8;
			this.dataPanel.Dock = global::System.Windows.Forms.DockStyle.Fill;
			this.dataPanel.Font = new global::System.Drawing.Font("Courier New", 9f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 204);
			this.dataPanel.Location = new global::System.Drawing.Point(0, 0);
			this.dataPanel.Name = "dataPanel";
			this.dataPanel.Size = new global::System.Drawing.Size(444, 118);
			this.dataPanel.TabIndex = 0;
			this.dataPanel.Text = "dataPanel1";
			this.dataPanel.TopAddress = 0;
			this.dataPanel.GetData += new global::ZXMAK.Platform.Windows.Forms.Controls.DataPanel.ONGETDATACPU(this.dasmPanel_GetData);
			this.dataPanel.MouseClick += new global::System.Windows.Forms.MouseEventHandler(this.dataPanel_MouseClick);
			this.dataPanel.DataClick += new global::ZXMAK.Platform.Windows.Forms.Controls.DataPanel.ONCLICKCPU(this.dataPanel_DataClick);
			this.splitter2.Dock = global::System.Windows.Forms.DockStyle.Bottom;
			this.splitter2.Location = new global::System.Drawing.Point(0, 220);
			this.splitter2.Name = "splitter2";
			this.splitter2.Size = new global::System.Drawing.Size(448, 3);
			this.splitter2.TabIndex = 3;
			this.splitter2.TabStop = false;
			this.panelDasm.BorderStyle = global::System.Windows.Forms.BorderStyle.Fixed3D;
			this.panelDasm.Controls.Add(this.dasmPanel);
			this.panelDasm.Dock = global::System.Windows.Forms.DockStyle.Fill;
			this.panelDasm.Location = new global::System.Drawing.Point(0, 0);
			this.panelDasm.Name = "panelDasm";
			this.panelDasm.Size = new global::System.Drawing.Size(448, 220);
			this.panelDasm.TabIndex = 4;
			this.dasmPanel.ActiveAddress = 0;
			this.dasmPanel.BreakpointColor = global::System.Drawing.Color.Red;
			this.dasmPanel.BreakpointForeColor = global::System.Drawing.Color.Black;
			this.dasmPanel.Dock = global::System.Windows.Forms.DockStyle.Fill;
			this.dasmPanel.Font = new global::System.Drawing.Font("Courier New", 9f);
			this.dasmPanel.ForeColor = global::System.Drawing.SystemColors.ControlText;
			this.dasmPanel.Location = new global::System.Drawing.Point(0, 0);
			this.dasmPanel.Name = "dasmPanel";
			this.dasmPanel.Size = new global::System.Drawing.Size(444, 216);
			this.dasmPanel.TabIndex = 0;
			this.dasmPanel.Text = "dasmPanel1";
			this.dasmPanel.TopAddress = 0;
			this.dasmPanel.CheckExecuting += new global::ZXMAK.Platform.Windows.Forms.Controls.DasmPanel.ONCHECKCPU(this.dasmPanel_CheckExecuting);
			this.dasmPanel.GetData += new global::ZXMAK.Platform.Windows.Forms.Controls.DasmPanel.ONGETDATACPU(this.dasmPanel_GetData);
			this.dasmPanel.MouseClick += new global::System.Windows.Forms.MouseEventHandler(this.dasmPanel_MouseClick);
			this.dasmPanel.CheckBreakpoint += new global::ZXMAK.Platform.Windows.Forms.Controls.DasmPanel.ONCHECKCPU(this.dasmPanel_CheckBreakpoint);
			this.dasmPanel.BreakpointClick += new global::ZXMAK.Platform.Windows.Forms.Controls.DasmPanel.ONCLICKCPU(this.dasmPanel_SetBreakpoint);
			this.dasmPanel.GetDasm += new global::ZXMAK.Platform.Windows.Forms.Controls.DasmPanel.ONGETDASMCPU(this.dasmPanel_GetDasm);
			this.contextMenuDasmMI.Items.AddRange(new global::System.Windows.Forms.ToolStripMenuItem[]
			{
				this.menuItemDasmGotoADDR,
				this.menuItemDasmGotoPC,
				this.menuItem2,
				this.menuItemDasmClearBreakpoints,
				this.menuItem4,
				this.menuItemDasmRefresh
			});
			this.contextMenuDasm.Popup += new global::System.Windows.Forms.PopupEventHandler(this.contextMenuDasm_Popup);
			this.menuItemDasmGotoADDR.ImageIndex = 0;
			this.menuItemDasmGotoADDR.Text = "Goto address...";
			this.menuItemDasmGotoADDR.Click += new global::System.EventHandler(this.menuItemDasmGotoADDR_Click);
			this.menuItemDasmGotoPC.ImageIndex = 1;
			this.menuItemDasmGotoPC.Text = "Goto PC";
			this.menuItemDasmGotoPC.Click += new global::System.EventHandler(this.menuItemDasmGotoPC_Click);
			this.menuItem2.ImageIndex = 2;
			this.menuItem2.Text = "-";
			this.menuItemDasmClearBreakpoints.ImageIndex = 3;
			this.menuItemDasmClearBreakpoints.Text = "Reset breakpoints";
			this.menuItemDasmClearBreakpoints.Click += new global::System.EventHandler(this.menuItemDasmClearBP_Click);
			this.menuItem4.ImageIndex = 4;
			this.menuItem4.Text = "-";
			this.menuItemDasmRefresh.ImageIndex = 5;
			this.menuItemDasmRefresh.Text = "Refresh";
			this.menuItemDasmRefresh.Click += new global::System.EventHandler(this.menuItemDasmRefresh_Click);
			this.contextMenuDataMI.Items.AddRange(new global::System.Windows.Forms.ToolStripMenuItem[]
			{
				this.menuItemDataGotoADDR,
				this.menuItemDataSetColumnCount,
				this.menuItem5,
				this.menuItemDataRefresh
			});
			this.menuItemDataGotoADDR.ImageIndex = 0;
			this.menuItemDataGotoADDR.Text = "Goto Address...";
			this.menuItemDataGotoADDR.Click += new global::System.EventHandler(this.menuItemDataGotoADDR_Click);
			this.menuItemDataSetColumnCount.ImageIndex = 1;
			this.menuItemDataSetColumnCount.Text = "Set column count...";
			this.menuItemDataSetColumnCount.Click += new global::System.EventHandler(this.menuItemDataSetColumnCount_Click);
			this.menuItem5.ImageIndex = 2;
			this.menuItem5.Text = "-";
			this.menuItemDataRefresh.ImageIndex = 3;
			this.menuItemDataRefresh.Text = "Refresh";
			this.menuItemDataRefresh.Click += new global::System.EventHandler(this.menuItemDataRefresh_Click);
			base.AutoScaleDimensions = new global::System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new global::System.Drawing.Size(619, 345);
			base.Controls.Add(this.panelDasm);
			base.Controls.Add(this.splitter2);
			base.Controls.Add(this.panelMem);
			base.Controls.Add(this.splitter1);
			base.Controls.Add(this.panelStatus);
			base.FormBorderStyle = global::System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			base.KeyPreview = true;
			base.Name = "FormCPU";
			this.Text = "Z80 CPU";
			base.FormClosed += new global::System.Windows.Forms.FormClosedEventHandler(this.FormCPU_FormClosed);
			base.Shown += new global::System.EventHandler(this.FormCPU_Shown);
			base.FormClosing += new global::System.Windows.Forms.FormClosingEventHandler(this.FormCPU_FormClosing);
			base.KeyDown += new global::System.Windows.Forms.KeyEventHandler(this.FormCPU_KeyDown);
			base.Load += new global::System.EventHandler(this.FormCPU_Load);
			this.panelStatus.ResumeLayout(false);
			this.panelState.ResumeLayout(false);
			this.panelRegs.ResumeLayout(false);
			this.panelMem.ResumeLayout(false);
			this.panelDasm.ResumeLayout(false);
			base.ResumeLayout(false);
		}

		private global::System.ComponentModel.IContainer components;

		private global::System.Windows.Forms.Panel panelStatus;

		private global::System.Windows.Forms.Splitter splitter1;

		private global::System.Windows.Forms.Panel panelMem;

		private global::System.Windows.Forms.Splitter splitter2;

		private global::System.Windows.Forms.Panel panelDasm;

		private global::System.Windows.Forms.Panel panelRegs;

		private global::System.Windows.Forms.Panel panelState;

		private global::System.Windows.Forms.Splitter splitter3;

		private global::System.Windows.Forms.ListBox listREGS;

		private global::System.Windows.Forms.ListBox listF;

		private global::System.Windows.Forms.Splitter splitter4;

		private global::System.Windows.Forms.ListBox listState;

		private global::ZXMAK.Platform.Windows.Forms.Controls.DasmPanel dasmPanel;

		private global::ZXMAK.Platform.Windows.Forms.Controls.DataPanel dataPanel;

		private global::System.Windows.Forms.ToolTip contextMenuDasm;

		private global::System.Windows.Forms.MenuStrip contextMenuDasmMI;
		
		private global::System.Windows.Forms.ToolStripMenuItem menuItemDasmGotoADDR;

		private global::System.Windows.Forms.ToolStripMenuItem menuItem2;

		private global::System.Windows.Forms.ToolStripMenuItem menuItemDasmClearBreakpoints;

		private global::System.Windows.Forms.ToolStripMenuItem menuItem4;

		private global::System.Windows.Forms.ToolStripMenuItem menuItemDasmRefresh;

		private global::System.Windows.Forms.ToolStripMenuItem menuItemDasmGotoPC;

		private global::System.Windows.Forms.ContextMenuStrip contextMenuData;

		private global::System.Windows.Forms.MenuStrip contextMenuDataMI;

		private global::System.Windows.Forms.ToolStripMenuItem menuItemDataGotoADDR;

		private global::System.Windows.Forms.ToolStripMenuItem menuItemDataSetColumnCount;

		private global::System.Windows.Forms.ToolStripMenuItem menuItem5;

		private global::System.Windows.Forms.ToolStripMenuItem menuItemDataRefresh;
	}
}
