using System;
using System.Drawing;
using System.Windows.Forms;

namespace ZXMAK.Platform.Windows.Forms.Controls
{
	public class DataPanel : Control
	{
		public DataPanel()
		{
			base.TabStop = true;
			this.Font = new Font("Courier", 13f, FontStyle.Regular, GraphicsUnit.Pixel);
			base.Size = new Size(424, 99);
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
			this.fActiveColumn = 0;
			this.fColCount = 8;
			this.UpdateLines();
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

		public int ColCount
		{
			get
			{
				return this.fColCount;
			}
			set
			{
				this.fColCount = value;
				this.UpdateLines();
				base.Invalidate();
			}
		}

		public event DataPanel.ONGETDATACPU GetData;

		public event DataPanel.ONCLICKCPU DataClick;

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
				int num = 2;
				this.wa = (int)graphics.MeasureString("DDDD", this.Font).Width;
				this.wd = (int)graphics.MeasureString("DD", this.Font).Width + num * 2;
				this.wsymb = (int)graphics.MeasureString("D", this.Font).Width;
				this.wtab = 8;
				this.wsp = 8;
				int num2 = 0;
				graphics.FillRectangle(new SolidBrush(this.BackColor), 0, 0, this.bitmap.Width, this.bitmap.Height);
				for (int i = 0; i < this.fVisibleLineCount; i++)
				{
					Color color = this.ForeColor;
					Color color2 = this.BackColor;
					graphics.DrawString(this.fADDRS[i].ToString("X4"), this.Font, new SolidBrush(color), (float)(DataPanel.fGutterWidth + this.wsp), (float)num2);
					for (int j = 0; j < this.fColCount; j++)
					{
						color = this.ForeColor;
						color2 = this.BackColor;
						if (i == this.fActiveLine && j == this.fActiveColumn)
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
							graphics.FillRectangle(new SolidBrush(color2), new Rectangle(DataPanel.fGutterWidth + this.wsp + this.wa + this.wtab + j * this.wd, num2, this.wd, this.fLineHeight));
							graphics.FillRectangle(new SolidBrush(Color.Gray), new Rectangle(DataPanel.fGutterWidth + this.wsp + this.wa + this.wtab + this.fColCount * this.wd + this.wtab + j * this.wsymb, num2, this.wsymb, this.fLineHeight));
						}
						graphics.DrawString(this.fBytesDATAS[i][j].ToString("X2"), this.Font, new SolidBrush(color), (float)(DataPanel.fGutterWidth + this.wsp + this.wa + this.wtab + j * this.wd + num), (float)num2);
						string s = new string(DataPanel.zxencode[(int)this.fBytesDATAS[i][j]], 1);
						graphics.DrawString(s, this.Font, new SolidBrush(color), (float)(DataPanel.fGutterWidth + this.wsp + this.wa + this.wtab + this.fColCount * this.wd + this.wtab + j * this.wsymb), (float)num2);
					}
					num2 += this.fLineHeight;
				}
			}
			g.DrawImageUnscaled(this.bitmap, x, y);
		}

		public void UpdateLines()
		{
			this.fADDRS = new ushort[this.fLineCount];
			this.fBytesDATAS = new byte[this.fLineCount][];
			ushort num = this.fTopAddress;
			for (int i = 0; i < this.fLineCount; i++)
			{
				this.fADDRS[i] = num;
				if (this.GetData != null)
				{
					this.GetData(this, num, this.fColCount, out this.fBytesDATAS[i]);
				}
				else
				{
					this.fBytesDATAS[i] = new byte[this.fColCount];
					for (int j = 0; j < this.fColCount; j++)
					{
						this.fBytesDATAS[i][j] = (byte)((int)this.fTopAddress + i * this.fColCount + j & 255);
					}
				}
				num += (ushort)this.fColCount;
			}
		}

		private void ControlUp()
		{
			this.fActiveLine--;
			if (this.fActiveLine < 0)
			{
				this.fActiveLine++;
				this.fTopAddress -= (ushort)this.fColCount;
				this.UpdateLines();
			}
		}

		private void ControlDown()
		{
			this.fActiveLine++;
			if (this.fActiveLine >= this.fVisibleLineCount)
			{
				this.fTopAddress += (ushort)this.fColCount;
				this.fActiveLine--;
				this.UpdateLines();
			}
		}

		private void ControlLeft()
		{
			this.fActiveColumn--;
			if (this.fActiveColumn < 0)
			{
				this.fActiveColumn = this.fColCount - 1;
				this.ControlUp();
				return;
			}
			this.UpdateLines();
		}

		private void ControlRight()
		{
			this.fActiveColumn++;
			if (this.fActiveColumn >= this.fColCount)
			{
				this.fActiveColumn = 0;
				this.ControlDown();
				return;
			}
			this.UpdateLines();
		}

		private void ControlPageUp()
		{
			this.fTopAddress -= (ushort)(this.fColCount * this.fVisibleLineCount);
			this.UpdateLines();
		}

		private void ControlPageDown()
		{
			this.fTopAddress += (ushort)(this.fColCount * this.fVisibleLineCount);
			this.UpdateLines();
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			if ((e.Button & MouseButtons.Left) != MouseButtons.None)
			{
				int num = (e.Y - 1) / this.fLineHeight;
				if (num < this.fVisibleLineCount && num >= 0 && num != this.fActiveLine)
				{
					this.fActiveLine = num;
					base.Invalidate();
				}
				int num2;
				if (this.wd >= 0)
				{
					num2 = e.X - 1 - (DataPanel.fGutterWidth + this.wsp + this.wa + this.wtab);
					if (num2 >= 0)
					{
						num2 /= this.wd;
					}
					else
					{
						num2 = -1;
					}
				}
				else
				{
					num2 = 0;
				}
				if (num2 < this.fColCount && num2 >= 0 && num2 != this.fActiveColumn)
				{
					this.fActiveColumn = num2;
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
				if (num < this.fVisibleLineCount && num >= 0 && num != this.fActiveLine)
				{
					this.fActiveLine = num;
					base.Invalidate();
				}
				int num2;
				if (this.wd >= 0)
				{
					num2 = e.X - 1 - (DataPanel.fGutterWidth + this.wsp + this.wa + this.wtab);
					if (num2 >= 0)
					{
						num2 /= this.wd;
					}
					else
					{
						num2 = -1;
					}
				}
				else
				{
					num2 = 0;
				}
				if (num2 < this.fColCount && num2 >= 0 && num2 != this.fActiveColumn)
				{
					this.fActiveColumn = num2;
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
			Point point = base.PointToClient(Control.MousePosition);
			int num = point.Y - 1;
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
			int num2;
			if (this.wd >= 0)
			{
				num2 = point.X - 1 - (DataPanel.fGutterWidth + this.wsp + this.wa + this.wtab);
				if (num2 >= 0)
				{
					num2 /= this.wd;
				}
				else
				{
					num2 = -1;
				}
			}
			else
			{
				num2 = 0;
			}
			if (num2 < this.fColCount && num2 >= 0 && num2 != this.fActiveColumn)
			{
				this.fActiveColumn = num2;
				base.Invalidate();
			}
		}

		protected override void OnMouseDoubleClick(MouseEventArgs e)
		{
			if ((e.Button & MouseButtons.Left) != MouseButtons.None)
			{
				int num = e.Y / this.fLineHeight;
				int num2;
				if (this.wd >= 0)
				{
					num2 = (e.X - 1 - (DataPanel.fGutterWidth + this.wsp + this.wa + this.wtab)) / this.wd;
				}
				else
				{
					num2 = 0;
				}
				if (num < this.fVisibleLineCount && num >= 0 && num2 < this.fColCount && num2 >= 0)
				{
					if (this.DataClick != null)
					{
						this.DataClick(this, (ushort)((int)this.fADDRS[num] + num2));
					}
					this.UpdateLines();
					this.Refresh();
				}
			}
			base.OnMouseDoubleClick(e);
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
				case Keys.Left:
					this.ControlLeft();
					base.Invalidate();
					break;
				case Keys.Up:
					this.ControlUp();
					base.Invalidate();
					break;
				case Keys.Right:
					this.ControlRight();
					base.Invalidate();
					break;
				case Keys.Down:
					this.ControlDown();
					base.Invalidate();
					break;
				}
			}
			else
			{
				if (this.fVisibleLineCount > 0 && this.DataClick != null)
				{
					this.DataClick(this, (ushort)((int)this.fADDRS[this.fActiveLine] + this.fActiveColumn));
				}
				this.UpdateLines();
				this.Refresh();
			}
			base.OnKeyDown(e);
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
			}
			else if (this.fActiveLine < 0 && this.fVisibleLineCount > 0)
			{
				this.fActiveLine = 0;
			}
			this.DrawLines(e.Graphics, 0, 0, base.ClientRectangle.Width, base.ClientRectangle.Height);
		}

		private Timer mouseTimer;

		private ushort fTopAddress;

		private static int fGutterWidth = 30;

		private int fColCount;

		private int fVisibleLineCount;

		private int fLineHeight;

		private int fActiveLine;

		private int fActiveColumn;

		private ushort[] fADDRS;

		private byte[][] fBytesDATAS;

		private Bitmap bitmap;

		private int wa;

		private int wd;

		private int wtab;

		private int wsp;

		private int wsymb;

		private static char[] zxencode = new char[]
		{
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			'!',
			'"',
			'#',
			'$',
			'%',
			'&',
			'\'',
			'(',
			')',
			'*',
			'+',
			',',
			'-',
			'.',
			'/',
			'0',
			'1',
			'2',
			'3',
			'4',
			'5',
			'6',
			'7',
			'8',
			'9',
			':',
			';',
			'<',
			'=',
			'>',
			'?',
			'@',
			'A',
			'B',
			'C',
			'D',
			'E',
			'F',
			'G',
			'H',
			'I',
			'J',
			'K',
			'L',
			'M',
			'N',
			'O',
			'P',
			'Q',
			'R',
			'S',
			'T',
			'U',
			'V',
			'W',
			'X',
			'Y',
			'Z',
			'[',
			'\\',
			']',
			'↑',
			'_',
			'₤',
			'a',
			'b',
			'c',
			'd',
			'e',
			'f',
			'g',
			'h',
			'i',
			'j',
			'k',
			'l',
			'm',
			'n',
			'o',
			'p',
			'q',
			'r',
			's',
			't',
			'u',
			'v',
			'w',
			'x',
			'y',
			'z',
			'{',
			'|',
			'}',
			'~',
			'©',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' ',
			' '
		};

		public delegate void ONGETDATACPU(object Sender, ushort ADDR, int len, out byte[] data);

		public delegate void ONCLICKCPU(object Sender, ushort Addr);
	}
}
