using System;
using System.Drawing;
using System.Windows.Forms;

namespace ZXMAK.Platform.Windows.Forms.Controls
{
	public class DasmPanel : Control
	{
		public DasmPanel()
		{
			base.TabStop = true;
			this.Font = new Font("Courier", 13f, FontStyle.Regular, GraphicsUnit.Pixel);
			base.Size = new Size(424, 148);
			ControlStyles flag = ControlStyles.ContainerControl | ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.StandardClick | ControlStyles.Selectable | ControlStyles.UserMouse | ControlStyles.StandardDoubleClick;
			base.SetStyle(flag, true);
			this.mouseTimer = new Timer();
			this.mouseTimer.Enabled = false;
			this.mouseTimer.Interval = 50;
			this.mouseTimer.Tick += this.OnMouseTimer;
			this.fLineHeight = 1;
			this.fVisibleLineCount = 0;
			this.fTopAddress = 0;
			this.fActiveLine = 0;
			this.fBreakColor = Color.Red;
			this.fBreakForeColor = Color.Black;
			this.UpdateLines();
		}

		public Color BreakpointColor
		{
			get
			{
				return this.fBreakColor;
			}
			set
			{
				this.fBreakColor = value;
				this.Refresh();
			}
		}

		public Color BreakpointForeColor
		{
			get
			{
				return this.fBreakForeColor;
			}
			set
			{
				this.fBreakForeColor = value;
				this.Refresh();
			}
		}

		public ushort TopAddress
		{
			get
			{
				return this.fTopAddress;
			}
			set
			{
				this.fTopAddress = value;
				this.fActiveLine = 0;
				this.UpdateLines();
				base.Invalidate();
			}
		}

		public ushort ActiveAddress
		{
			get
			{
				if (this.fActiveLine >= 0 && this.fActiveLine < this.fLineCount)
				{
					return this.fADDRS[this.fActiveLine];
				}
				return 0;
			}
			set
			{
				for (int i = 0; i <= this.fVisibleLineCount; i++)
				{
					if (this.fADDRS[i] == value)
					{
						if (this.fActiveLine != i)
						{
							if (i == this.fVisibleLineCount)
							{
								this.fTopAddress = this.fADDRS[1];
								this.fActiveLine = i - 1;
							}
							else
							{
								this.fActiveLine = i;
							}
						}
						this.UpdateLines();
						this.Refresh();
						return;
					}
				}
				this.TopAddress = value;
			}
		}

		public event DasmPanel.ONCHECKCPU CheckBreakpoint;

		public event DasmPanel.ONCHECKCPU CheckExecuting;

		public event DasmPanel.ONGETDATACPU GetData;

		public event DasmPanel.ONGETDASMCPU GetDasm;

		public event DasmPanel.ONCLICKCPU BreakpointClick;

		public event DasmPanel.ONCLICKCPU DasmClick;

		private int fLineCount
		{
			get
			{
				return this.fVisibleLineCount + 3;
			}
		}

		public void DrawLines(Graphics g, int x, int y, int wid, int hei)
		{
			if (base.Height <= 0 || base.Width <= 0)
			{
				return;
			}
			if (!base.Visible)
			{
				return;
			}
			if (this.bitmap == null || this.bitmap.Width != wid || this.bitmap.Height != hei)
			{
				this.bitmap = new Bitmap(wid, hei);
			}
			using (Graphics graphics = Graphics.FromImage(this.bitmap))
			{
				int num = (int)graphics.MeasureString("DDDD", this.Font).Width;
				int num2 = (int)graphics.MeasureString("DDDDDDDDDDDDDDDD", this.Font).Width;
				int num3 = 8;
				int num4 = 8;
				int num5 = 0;
				graphics.FillRectangle(new SolidBrush(this.BackColor), 0, 0, this.bitmap.Width, this.bitmap.Height);
				for (int i = 0; i < this.fVisibleLineCount; i++)
				{
					Color color = this.ForeColor;
					Color color2 = this.BackColor;
					bool flag = this.fBreakpoints[i];
					bool flag2 = false;
					if (this.CheckExecuting != null)
					{
						flag2 = this.CheckExecuting(this, this.fADDRS[i]);
					}
					Rectangle rect = new Rectangle(DasmPanel.fGutterWidth, num5, this.bitmap.Width, this.fLineHeight);
					if (flag)
					{
						color = this.fBreakForeColor;
						color2 = this.fBreakColor;
						graphics.FillRectangle(new SolidBrush(color2), rect);
					}
					if (i == this.fActiveLine)
					{
						if (this.Focused)
						{
							color = Color.White;
							color2 = Color.Navy;
						}
						else
						{
							color = Color.Silver;
							color2 = Color.Gray;
						}
						graphics.FillRectangle(new SolidBrush(color2), rect);
					}
					if (flag2)
					{
						int num6 = 4;
						int num7 = 2 + num6;
						int num8 = num5 + this.fLineHeight / 2;
						Point[] points = new Point[]
						{
							new Point(num7, num8 - 5),
							new Point(num7, num8 - 2),
							new Point(num7 - 3, num8 - 2),
							new Point(num7 - 3, num8 + 2),
							new Point(num7, num8 + 2),
							new Point(num7, num8 + 5),
							new Point(num7 + 5, num8)
						};
						graphics.FillPolygon(new SolidBrush(Color.Lime), points);
						graphics.DrawPolygon(new Pen(Color.Black), points);
						Point[] points2 = new Point[]
						{
							new Point(num7 - 2, num8 + 1),
							new Point(num7 - 2, num8 - 1),
							new Point(num7 + 1, num8 - 1),
							new Point(num7 + 1, num8 - 3),
							new Point(num7 + 4, num8)
						};
						graphics.DrawLines(new Pen(Color.Yellow), points2);
					}
					if (flag)
					{
						int num9 = 4;
						int num10 = 2 + num9;
						int num11 = num5 + this.fLineHeight / 2;
						Rectangle rect2;
						if (!flag2)
						{
							rect2 = new Rectangle(num10 - num9, num11 - num9, num9 + num9 + 1, num9 + num9 + 1);
						}
						else
						{
							num10 += 16;
							rect2 = new Rectangle(num10 - num9, num11 - num9, num9 + num9 + 1, num9 + num9 + 1);
						}
						graphics.FillEllipse(new SolidBrush(this.fBreakColor), rect2);
						graphics.DrawEllipse(new Pen(Color.Black), rect2);
					}
					graphics.DrawString(this.fStrADDRS[i], this.Font, new SolidBrush(color), (float)(DasmPanel.fGutterWidth + num4), (float)num5);
					graphics.DrawString(this.fStrDATAS[i], this.Font, new SolidBrush(color), (float)(DasmPanel.fGutterWidth + num4 + num + num3), (float)num5);
					graphics.DrawString(this.fStrDASMS[i], this.Font, new SolidBrush(color), (float)(DasmPanel.fGutterWidth + num4 + num + num3 + num2 + num3), (float)num5);
					num5 += this.fLineHeight;
				}
			}
			g.DrawImageUnscaled(this.bitmap, x, y);
		}

		public void UpdateLines()
		{
			this.fADDRS = new ushort[this.fLineCount];
			this.fStrADDRS = new string[this.fLineCount];
			this.fStrDATAS = new string[this.fLineCount];
			this.fStrDASMS = new string[this.fLineCount];
			this.fBreakpoints = new bool[this.fLineCount];
			ushort num = this.fTopAddress;
			for (int i = 0; i < this.fLineCount; i++)
			{
				this.fADDRS[i] = num;
				this.fStrADDRS[i] = num.ToString("X4");
				string text;
				int num2;
				if (this.GetDasm != null)
				{
					this.GetDasm(this, num, out text, out num2);
				}
				else
				{
					text = "???";
					num2 = 1;
				}
				byte[] array;
				if (this.GetData != null)
				{
					this.GetData(this, num, num2, out array);
				}
				else
				{
					byte[] array2 = new byte[1];
					array = array2;
				}
				this.fStrDASMS[i] = text;
				string text2 = "";
				int num3 = array.Length;
				if (num3 > 7)
				{
					num3 = 7;
				}
				for (int j = 0; j < num3; j++)
				{
					text2 += array[j].ToString("X2");
				}
				if (num3 < array.Length)
				{
					text2 += "..";
				}
				this.fStrDATAS[i] = text2;
				if (this.CheckBreakpoint != null)
				{
					if (this.CheckBreakpoint(this, num))
					{
						this.fBreakpoints[i] = true;
					}
				}
				else
				{
					this.fBreakpoints[i] = false;
				}
				num += (ushort)num2;
			}
		}

		private void ControlUp()
		{
			this.fActiveLine--;
			if (this.fActiveLine < 0)
			{
				this.fActiveLine++;
				this.fTopAddress = this.fADDRS[0] - 1;
				this.UpdateLines();
			}
		}

		private void ControlDown()
		{
			this.fActiveLine++;
			if (this.fActiveLine >= this.fVisibleLineCount)
			{
				this.fTopAddress = this.fADDRS[1];
				this.fActiveLine--;
				this.UpdateLines();
			}
		}

		private void ControlPageUp()
		{
			for (int i = 0; i < this.fVisibleLineCount - 1; i++)
			{
				this.fTopAddress -= 1;
				this.UpdateLines();
			}
		}

		private void ControlPageDown()
		{
			if (this.fVisibleLineCount > 0)
			{
				this.fTopAddress = this.fADDRS[this.fVisibleLineCount - 1];
				this.UpdateLines();
			}
			this.fTopAddress = this.fADDRS[1];
			this.UpdateLines();
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			Keys keyCode = e.KeyCode;
			if (keyCode != Keys.Return)
			{
				switch (keyCode)
				{
				case Keys.Prior:
					this.ControlPageUp();
					base.Invalidate();
					break;
				case Keys.Next:
					this.ControlPageDown();
					base.Invalidate();
					break;
				default:
					switch (keyCode)
					{
					case Keys.Up:
						this.ControlUp();
						base.Invalidate();
						break;
					case Keys.Down:
						this.ControlDown();
						base.Invalidate();
						break;
					}
					break;
				}
			}
			else
			{
				if (this.DasmClick != null)
				{
					this.DasmClick(this, this.fADDRS[this.fActiveLine]);
				}
				this.UpdateLines();
				this.Refresh();
			}
			base.OnKeyDown(e);
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			if ((e.Button & MouseButtons.Left) != MouseButtons.None)
			{
				int num = (e.Y - 1) / this.fLineHeight;
				if (num < this.fVisibleLineCount && num != this.fActiveLine)
				{
					this.fActiveLine = num;
					base.Invalidate();
				}
				this.mouseTimer.Enabled = true;
			}
			base.OnMouseDown(e);
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			if ((e.Button & MouseButtons.Left) != MouseButtons.None)
			{
				int num = (e.Y - 1) / this.fLineHeight;
				if (num < 0)
				{
					return;
				}
				if (num < this.fVisibleLineCount && num != this.fActiveLine)
				{
					this.fActiveLine = num;
					base.Invalidate();
				}
			}
			base.OnMouseMove(e);
		}

		protected override void OnMouseWheel(MouseEventArgs e)
		{
			int num = -e.Delta / 120;
			if (num < 0)
			{
				for (int i = 0; i < -num; i++)
				{
					this.ControlUp();
				}
			}
			else
			{
				for (int j = 0; j < num; j++)
				{
					this.ControlDown();
				}
			}
			base.Invalidate();
			base.OnMouseWheel(e);
		}

		protected override void OnMouseCaptureChanged(EventArgs e)
		{
			this.mouseTimer.Enabled = false;
			base.OnMouseCaptureChanged(e);
		}

		private void OnMouseTimer(object sender, EventArgs e)
		{
			int num = base.PointToClient(Control.MousePosition).Y - 1;
			if (num < 0)
			{
				this.ControlUp();
				base.Invalidate();
			}
			if (num >= this.fVisibleLineCount * this.fLineHeight)
			{
				this.ControlDown();
				base.Invalidate();
			}
		}

		protected override void OnPaintBackground(PaintEventArgs e)
		{
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			int num = (int)e.Graphics.MeasureString("3D,", this.Font).Height;
			int num2 = (base.Height - 2) / num;
			if (num2 < 0)
			{
				num2 = 0;
			}
			if (this.fVisibleLineCount != num2 || this.fLineHeight != num)
			{
				this.fLineHeight = num;
				this.fVisibleLineCount = num2;
				this.UpdateLines();
			}
			if (this.fActiveLine >= this.fVisibleLineCount && this.fVisibleLineCount > 0)
			{
				this.fActiveLine = this.fVisibleLineCount - 1;
				base.Invalidate();
			}
			else if (this.fActiveLine < 0 && this.fVisibleLineCount > 0)
			{
				this.fActiveLine = 0;
				base.Invalidate();
			}
			this.DrawLines(e.Graphics, 0, 0, base.ClientRectangle.Width, base.ClientRectangle.Height);
		}

		protected override void OnGotFocus(EventArgs e)
		{
			base.OnGotFocus(e);
			base.Invalidate();
		}

		protected override void OnLostFocus(EventArgs e)
		{
			base.OnLostFocus(e);
			base.Invalidate();
		}

		protected override bool IsInputKey(Keys keyData)
		{
			switch (keyData & Keys.KeyCode)
			{
			case Keys.Left:
			case Keys.Up:
			case Keys.Right:
			case Keys.Down:
				return true;
			default:
				return base.IsInputKey(keyData);
			}
		}

		protected override void OnMouseDoubleClick(MouseEventArgs e)
		{
			if ((e.Button & MouseButtons.Left) != MouseButtons.None)
			{
				int num = e.Y / this.fLineHeight;
				if (num < this.fVisibleLineCount)
				{
					if (e.X <= DasmPanel.fGutterWidth)
					{
						if (this.BreakpointClick != null)
						{
							this.BreakpointClick(this, this.fADDRS[num]);
						}
						this.UpdateLines();
						this.Refresh();
					}
					else
					{
						if (this.DasmClick != null)
						{
							this.DasmClick(this, this.fADDRS[num]);
						}
						this.UpdateLines();
						this.Refresh();
					}
				}
			}
			base.OnMouseDoubleClick(e);
		}

		private Timer mouseTimer;

		private ushort fTopAddress;

		private Color fBreakColor;

		private Color fBreakForeColor;

		private static int fGutterWidth = 30;

		private int fVisibleLineCount;

		private int fLineHeight;

		private int fActiveLine;

		private ushort[] fADDRS;

		private string[] fStrADDRS;

		private string[] fStrDATAS;

		private string[] fStrDASMS;

		private bool[] fBreakpoints;

		private Bitmap bitmap;

		public delegate bool ONCHECKCPU(object Sender, ushort ADDR);

		public delegate void ONGETDATACPU(object Sender, ushort ADDR, int len, out byte[] data);

		public delegate void ONGETDASMCPU(object Sender, ushort ADDR, out string DASM, out int len);

		public delegate void ONCLICKCPU(object Sender, ushort Addr);
	}
}
