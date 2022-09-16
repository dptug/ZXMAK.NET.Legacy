using System;
using System.Drawing;
using System.Windows.Forms;

namespace ZXMAK.Platform.Windows.Forms.Controls;

public class DasmPanel : Control
{
	public delegate bool ONCHECKCPU(object Sender, ushort ADDR);

	public delegate void ONGETDATACPU(object Sender, ushort ADDR, int len, out byte[] data);

	public delegate void ONGETDASMCPU(object Sender, ushort ADDR, out string DASM, out int len);

	public delegate void ONCLICKCPU(object Sender, ushort Addr);

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

	public Color BreakpointColor
	{
		get
		{
			return fBreakColor;
		}
		set
		{
			fBreakColor = value;
			Refresh();
		}
	}

	public Color BreakpointForeColor
	{
		get
		{
			return fBreakForeColor;
		}
		set
		{
			fBreakForeColor = value;
			Refresh();
		}
	}

	public ushort TopAddress
	{
		get
		{
			return fTopAddress;
		}
		set
		{
			fTopAddress = value;
			fActiveLine = 0;
			UpdateLines();
			Invalidate();
		}
	}

	public ushort ActiveAddress
	{
		get
		{
			if (fActiveLine >= 0 && fActiveLine < fLineCount)
			{
				return fADDRS[fActiveLine];
			}
			return 0;
		}
		set
		{
			for (int i = 0; i <= fVisibleLineCount; i++)
			{
				if (fADDRS[i] != value)
				{
					continue;
				}
				if (fActiveLine != i)
				{
					if (i == fVisibleLineCount)
					{
						fTopAddress = fADDRS[1];
						fActiveLine = i - 1;
					}
					else
					{
						fActiveLine = i;
					}
				}
				UpdateLines();
				Refresh();
				return;
			}
			TopAddress = value;
		}
	}

	private int fLineCount => fVisibleLineCount + 3;

	public event ONCHECKCPU CheckBreakpoint;

	public event ONCHECKCPU CheckExecuting;

	public event ONGETDATACPU GetData;

	public event ONGETDASMCPU GetDasm;

	public event ONCLICKCPU BreakpointClick;

	public event ONCLICKCPU DasmClick;

	public DasmPanel()
	{
		base.TabStop = true;
		Font = new Font("Courier", 13f, FontStyle.Regular, GraphicsUnit.Pixel);
		base.Size = new Size(424, 148);
		ControlStyles flag = ControlStyles.ContainerControl | ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.StandardClick | ControlStyles.Selectable | ControlStyles.UserMouse | ControlStyles.StandardDoubleClick;
		SetStyle(flag, value: true);
		mouseTimer = new Timer();
		mouseTimer.Enabled = false;
		mouseTimer.Interval = 50;
		mouseTimer.Tick += OnMouseTimer;
		fLineHeight = 1;
		fVisibleLineCount = 0;
		fTopAddress = 0;
		fActiveLine = 0;
		fBreakColor = Color.Red;
		fBreakForeColor = Color.Black;
		UpdateLines();
	}

	public void DrawLines(Graphics g, int x, int y, int wid, int hei)
	{
		if (base.Height <= 0 || base.Width <= 0 || !base.Visible)
		{
			return;
		}
		if (bitmap == null || bitmap.Width != wid || bitmap.Height != hei)
		{
			bitmap = new Bitmap(wid, hei);
		}
		using (Graphics graphics = Graphics.FromImage(bitmap))
		{
			int num = (int)graphics.MeasureString("DDDD", Font).Width;
			int num2 = (int)graphics.MeasureString("DDDDDDDDDDDDDDDD", Font).Width;
			int num3 = 8;
			int num4 = 8;
			int num5 = 0;
			graphics.FillRectangle(new SolidBrush(BackColor), 0, 0, bitmap.Width, bitmap.Height);
			for (int i = 0; i < fVisibleLineCount; i++)
			{
				Color color = ForeColor;
				Color backColor = BackColor;
				bool flag = fBreakpoints[i];
				bool flag2 = false;
				if (this.CheckExecuting != null)
				{
					flag2 = this.CheckExecuting(this, fADDRS[i]);
				}
				Rectangle rect = new Rectangle(fGutterWidth, num5, bitmap.Width, fLineHeight);
				if (flag)
				{
					color = fBreakForeColor;
					backColor = fBreakColor;
					graphics.FillRectangle(new SolidBrush(backColor), rect);
				}
				if (i == fActiveLine)
				{
					if (Focused)
					{
						color = Color.White;
						backColor = Color.Navy;
					}
					else
					{
						color = Color.Silver;
						backColor = Color.Gray;
					}
					graphics.FillRectangle(new SolidBrush(backColor), rect);
				}
				if (flag2)
				{
					int num6 = 4;
					int num7 = 2 + num6;
					int num8 = num5 + fLineHeight / 2;
					Point[] points = new Point[7]
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
					Point[] points2 = new Point[5]
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
					int num11 = num5 + fLineHeight / 2;
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
					graphics.FillEllipse(new SolidBrush(fBreakColor), rect2);
					graphics.DrawEllipse(new Pen(Color.Black), rect2);
				}
				graphics.DrawString(fStrADDRS[i], Font, new SolidBrush(color), fGutterWidth + num4, num5);
				graphics.DrawString(fStrDATAS[i], Font, new SolidBrush(color), fGutterWidth + num4 + num + num3, num5);
				graphics.DrawString(fStrDASMS[i], Font, new SolidBrush(color), fGutterWidth + num4 + num + num3 + num2 + num3, num5);
				num5 += fLineHeight;
			}
		}
		g.DrawImageUnscaled(bitmap, x, y);
	}

	public void UpdateLines()
	{
		fADDRS = new ushort[fLineCount];
		fStrADDRS = new string[fLineCount];
		fStrDATAS = new string[fLineCount];
		fStrDASMS = new string[fLineCount];
		fBreakpoints = new bool[fLineCount];
		ushort num = fTopAddress;
		for (int i = 0; i < fLineCount; i++)
		{
			fADDRS[i] = num;
			fStrADDRS[i] = num.ToString("X4");
			string DASM;
			int len;
			if (this.GetDasm != null)
			{
				this.GetDasm(this, num, out DASM, out len);
			}
			else
			{
				DASM = "???";
				len = 1;
			}
			byte[] data;
			if (this.GetData != null)
			{
				this.GetData(this, num, len, out data);
			}
			else
			{
				byte[] array = new byte[1];
				data = array;
			}
			fStrDASMS[i] = DASM;
			string text = "";
			int num2 = data.Length;
			if (num2 > 7)
			{
				num2 = 7;
			}
			for (int j = 0; j < num2; j++)
			{
				text += data[j].ToString("X2");
			}
			if (num2 < data.Length)
			{
				text += "..";
			}
			fStrDATAS[i] = text;
			if (this.CheckBreakpoint != null)
			{
				if (this.CheckBreakpoint(this, num))
				{
					fBreakpoints[i] = true;
				}
			}
			else
			{
				fBreakpoints[i] = false;
			}
			num = (ushort)(num + (ushort)len);
		}
	}

	private void ControlUp()
	{
		fActiveLine--;
		if (fActiveLine < 0)
		{
			fActiveLine++;
			fTopAddress = (ushort)(fADDRS[0] - 1);
			UpdateLines();
		}
	}

	private void ControlDown()
	{
		fActiveLine++;
		if (fActiveLine >= fVisibleLineCount)
		{
			fTopAddress = fADDRS[1];
			fActiveLine--;
			UpdateLines();
		}
	}

	private void ControlPageUp()
	{
		for (int i = 0; i < fVisibleLineCount - 1; i++)
		{
			fTopAddress--;
			UpdateLines();
		}
	}

	private void ControlPageDown()
	{
		if (fVisibleLineCount > 0)
		{
			fTopAddress = fADDRS[fVisibleLineCount - 1];
			UpdateLines();
		}
		fTopAddress = fADDRS[1];
		UpdateLines();
	}

	protected override void OnKeyDown(KeyEventArgs e)
	{
		switch (e.KeyCode)
		{
		case Keys.Down:
			ControlDown();
			Invalidate();
			break;
		case Keys.Up:
			ControlUp();
			Invalidate();
			break;
		case Keys.Next:
			ControlPageDown();
			Invalidate();
			break;
		case Keys.Prior:
			ControlPageUp();
			Invalidate();
			break;
		case Keys.Return:
			if (this.DasmClick != null)
			{
				this.DasmClick(this, fADDRS[fActiveLine]);
			}
			UpdateLines();
			Refresh();
			break;
		}
		base.OnKeyDown(e);
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		if ((e.Button & MouseButtons.Left) != 0)
		{
			int num = (e.Y - 1) / fLineHeight;
			if (num < fVisibleLineCount && num != fActiveLine)
			{
				fActiveLine = num;
				Invalidate();
			}
			mouseTimer.Enabled = true;
		}
		base.OnMouseDown(e);
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		if ((e.Button & MouseButtons.Left) != 0)
		{
			int num = (e.Y - 1) / fLineHeight;
			if (num < 0)
			{
				return;
			}
			if (num < fVisibleLineCount && num != fActiveLine)
			{
				fActiveLine = num;
				Invalidate();
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
				ControlUp();
			}
		}
		else
		{
			for (int j = 0; j < num; j++)
			{
				ControlDown();
			}
		}
		Invalidate();
		base.OnMouseWheel(e);
	}

	protected override void OnMouseCaptureChanged(EventArgs e)
	{
		mouseTimer.Enabled = false;
		base.OnMouseCaptureChanged(e);
	}

	private void OnMouseTimer(object sender, EventArgs e)
	{
		int num = PointToClient(Control.MousePosition).Y - 1;
		if (num < 0)
		{
			ControlUp();
			Invalidate();
		}
		if (num >= fVisibleLineCount * fLineHeight)
		{
			ControlDown();
			Invalidate();
		}
	}

	protected override void OnPaintBackground(PaintEventArgs e)
	{
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		int num = (int)e.Graphics.MeasureString("3D,", Font).Height;
		int num2 = (base.Height - 2) / num;
		if (num2 < 0)
		{
			num2 = 0;
		}
		if (fVisibleLineCount != num2 || fLineHeight != num)
		{
			fLineHeight = num;
			fVisibleLineCount = num2;
			UpdateLines();
		}
		if (fActiveLine >= fVisibleLineCount && fVisibleLineCount > 0)
		{
			fActiveLine = fVisibleLineCount - 1;
			Invalidate();
		}
		else if (fActiveLine < 0 && fVisibleLineCount > 0)
		{
			fActiveLine = 0;
			Invalidate();
		}
		DrawLines(e.Graphics, 0, 0, base.ClientRectangle.Width, base.ClientRectangle.Height);
	}

	protected override void OnGotFocus(EventArgs e)
	{
		base.OnGotFocus(e);
		Invalidate();
	}

	protected override void OnLostFocus(EventArgs e)
	{
		base.OnLostFocus(e);
		Invalidate();
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
		if ((e.Button & MouseButtons.Left) != 0)
		{
			int num = e.Y / fLineHeight;
			if (num < fVisibleLineCount)
			{
				if (e.X <= fGutterWidth)
				{
					if (this.BreakpointClick != null)
					{
						this.BreakpointClick(this, fADDRS[num]);
					}
					UpdateLines();
					Refresh();
				}
				else
				{
					if (this.DasmClick != null)
					{
						this.DasmClick(this, fADDRS[num]);
					}
					UpdateLines();
					Refresh();
				}
			}
		}
		base.OnMouseDoubleClick(e);
	}
}
