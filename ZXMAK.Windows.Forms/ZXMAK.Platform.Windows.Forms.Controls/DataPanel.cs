using System;
using System.Drawing;
using System.Windows.Forms;

namespace ZXMAK.Platform.Windows.Forms.Controls;

public class DataPanel : Control
{
	public delegate void ONGETDATACPU(object Sender, ushort ADDR, int len, out byte[] data);

	public delegate void ONCLICKCPU(object Sender, ushort Addr);

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

	private static char[] zxencode = new char[256]
	{
		' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ',
		' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ',
		' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ',
		' ', ' ', ' ', '!', '"', '#', '$', '%', '&', '\'',
		'(', ')', '*', '+', ',', '-', '.', '/', '0', '1',
		'2', '3', '4', '5', '6', '7', '8', '9', ':', ';',
		'<', '=', '>', '?', '@', 'A', 'B', 'C', 'D', 'E',
		'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O',
		'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y',
		'Z', '[', '\\', ']', '↑', '_', '₤', 'a', 'b', 'c',
		'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm',
		'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w',
		'x', 'y', 'z', '{', '|', '}', '~', '©', ' ', ' ',
		' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ',
		' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ',
		' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ',
		' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ',
		' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ',
		' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ',
		' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ',
		' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ',
		' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ',
		' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ',
		' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ',
		' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ',
		' ', ' ', ' ', ' ', ' ', ' '
	};

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

	public int ColCount
	{
		get
		{
			return fColCount;
		}
		set
		{
			fColCount = value;
			UpdateLines();
			Invalidate();
		}
	}

	private int fLineCount => fVisibleLineCount + 3;

	public event ONGETDATACPU GetData;

	public event ONCLICKCPU DataClick;

	public DataPanel()
	{
		base.TabStop = true;
		Font = new Font("Courier", 13f, FontStyle.Regular, GraphicsUnit.Pixel);
		base.Size = new Size(424, 99);
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
		fActiveColumn = 0;
		fColCount = 8;
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
			int num = 2;
			wa = (int)graphics.MeasureString("DDDD", Font).Width;
			wd = (int)graphics.MeasureString("DD", Font).Width + num * 2;
			wsymb = (int)graphics.MeasureString("D", Font).Width;
			wtab = 8;
			wsp = 8;
			int num2 = 0;
			graphics.FillRectangle(new SolidBrush(BackColor), 0, 0, bitmap.Width, bitmap.Height);
			for (int i = 0; i < fVisibleLineCount; i++)
			{
				Color foreColor = ForeColor;
				Color backColor = BackColor;
				graphics.DrawString(fADDRS[i].ToString("X4"), Font, new SolidBrush(foreColor), fGutterWidth + wsp, num2);
				for (int j = 0; j < fColCount; j++)
				{
					foreColor = ForeColor;
					backColor = BackColor;
					if (i == fActiveLine && j == fActiveColumn)
					{
						if (Focused)
						{
							foreColor = Color.White;
							backColor = Color.Navy;
						}
						else
						{
							foreColor = Color.Silver;
							backColor = Color.Gray;
						}
						graphics.FillRectangle(new SolidBrush(backColor), new Rectangle(fGutterWidth + wsp + wa + wtab + j * wd, num2, wd, fLineHeight));
						graphics.FillRectangle(new SolidBrush(Color.Gray), new Rectangle(fGutterWidth + wsp + wa + wtab + fColCount * wd + wtab + j * wsymb, num2, wsymb, fLineHeight));
					}
					graphics.DrawString(fBytesDATAS[i][j].ToString("X2"), Font, new SolidBrush(foreColor), fGutterWidth + wsp + wa + wtab + j * wd + num, num2);
					string s = new string(zxencode[fBytesDATAS[i][j]], 1);
					graphics.DrawString(s, Font, new SolidBrush(foreColor), fGutterWidth + wsp + wa + wtab + fColCount * wd + wtab + j * wsymb, num2);
				}
				num2 += fLineHeight;
			}
		}
		g.DrawImageUnscaled(bitmap, x, y);
	}

	public void UpdateLines()
	{
		fADDRS = new ushort[fLineCount];
		fBytesDATAS = new byte[fLineCount][];
		ushort num = fTopAddress;
		for (int i = 0; i < fLineCount; i++)
		{
			fADDRS[i] = num;
			if (this.GetData != null)
			{
				this.GetData(this, num, fColCount, out fBytesDATAS[i]);
			}
			else
			{
				fBytesDATAS[i] = new byte[fColCount];
				for (int j = 0; j < fColCount; j++)
				{
					fBytesDATAS[i][j] = (byte)((uint)(fTopAddress + i * fColCount + j) & 0xFFu);
				}
			}
			num = (ushort)(num + (ushort)fColCount);
		}
	}

	private void ControlUp()
	{
		fActiveLine--;
		if (fActiveLine < 0)
		{
			fActiveLine++;
			fTopAddress -= (ushort)fColCount;
			UpdateLines();
		}
	}

	private void ControlDown()
	{
		fActiveLine++;
		if (fActiveLine >= fVisibleLineCount)
		{
			fTopAddress += (ushort)fColCount;
			fActiveLine--;
			UpdateLines();
		}
	}

	private void ControlLeft()
	{
		fActiveColumn--;
		if (fActiveColumn < 0)
		{
			fActiveColumn = fColCount - 1;
			ControlUp();
		}
		else
		{
			UpdateLines();
		}
	}

	private void ControlRight()
	{
		fActiveColumn++;
		if (fActiveColumn >= fColCount)
		{
			fActiveColumn = 0;
			ControlDown();
		}
		else
		{
			UpdateLines();
		}
	}

	private void ControlPageUp()
	{
		fTopAddress -= (ushort)(fColCount * fVisibleLineCount);
		UpdateLines();
	}

	private void ControlPageDown()
	{
		fTopAddress += (ushort)(fColCount * fVisibleLineCount);
		UpdateLines();
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		if ((e.Button & MouseButtons.Left) != 0)
		{
			int num = (e.Y - 1) / fLineHeight;
			if (num < fVisibleLineCount && num >= 0 && num != fActiveLine)
			{
				fActiveLine = num;
				Invalidate();
			}
			int num2;
			if (wd >= 0)
			{
				num2 = e.X - 1 - (fGutterWidth + wsp + wa + wtab);
				num2 = ((num2 < 0) ? (-1) : (num2 / wd));
			}
			else
			{
				num2 = 0;
			}
			if (num2 < fColCount && num2 >= 0 && num2 != fActiveColumn)
			{
				fActiveColumn = num2;
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
			if (num < fVisibleLineCount && num >= 0 && num != fActiveLine)
			{
				fActiveLine = num;
				Invalidate();
			}
			int num2;
			if (wd >= 0)
			{
				num2 = e.X - 1 - (fGutterWidth + wsp + wa + wtab);
				num2 = ((num2 < 0) ? (-1) : (num2 / wd));
			}
			else
			{
				num2 = 0;
			}
			if (num2 < fColCount && num2 >= 0 && num2 != fActiveColumn)
			{
				fActiveColumn = num2;
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
		Point point = PointToClient(Control.MousePosition);
		int num = point.Y - 1;
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
		int num2;
		if (wd >= 0)
		{
			num2 = point.X - 1 - (fGutterWidth + wsp + wa + wtab);
			num2 = ((num2 < 0) ? (-1) : (num2 / wd));
		}
		else
		{
			num2 = 0;
		}
		if (num2 < fColCount && num2 >= 0 && num2 != fActiveColumn)
		{
			fActiveColumn = num2;
			Invalidate();
		}
	}

	protected override void OnMouseDoubleClick(MouseEventArgs e)
	{
		if ((e.Button & MouseButtons.Left) != 0)
		{
			int num = e.Y / fLineHeight;
			int num2 = ((wd >= 0) ? ((e.X - 1 - (fGutterWidth + wsp + wa + wtab)) / wd) : 0);
			if (num < fVisibleLineCount && num >= 0 && num2 < fColCount && num2 >= 0)
			{
				if (this.DataClick != null)
				{
					this.DataClick(this, (ushort)(fADDRS[num] + num2));
				}
				UpdateLines();
				Refresh();
			}
		}
		base.OnMouseDoubleClick(e);
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
		case Keys.Left:
			ControlLeft();
			Invalidate();
			break;
		case Keys.Right:
			ControlRight();
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
			if (fVisibleLineCount > 0 && this.DataClick != null)
			{
				this.DataClick(this, (ushort)(fADDRS[fActiveLine] + fActiveColumn));
			}
			UpdateLines();
			Refresh();
			break;
		}
		base.OnKeyDown(e);
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
		}
		else if (fActiveLine < 0 && fVisibleLineCount > 0)
		{
			fActiveLine = 0;
		}
		DrawLines(e.Graphics, 0, 0, base.ClientRectangle.Width, base.ClientRectangle.Height);
	}
}
