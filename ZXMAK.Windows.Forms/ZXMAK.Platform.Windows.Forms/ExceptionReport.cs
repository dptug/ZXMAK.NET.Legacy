using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace ZXMAK.Platform.Windows.Forms;

public class ExceptionReport : Form
{
	private Exception _exception;

	private Icon _reportIcon = SystemIcons.Error;

	private IContainer components;

	private TextBox textInfo;

	private Label labelInfo;

	private Button btnOK;

	private Label labelHelp;

	public static void Execute(Exception ex)
	{
		using ExceptionReport exceptionReport = new ExceptionReport(ex);
		exceptionReport.ShowDialog();
	}

	private ExceptionReport(Exception ex)
	{
		_exception = ex;
		InitializeComponent();
		labelInfo.Text = labelInfo.Text.Replace("Exception", ex.GetType().ToString());
		textInfo.Text = "";
		parseException(0, ex);
		labelHelp.Text = ex.Message;
	}

	private void parseException(int tabSize, Exception ex)
	{
		string text = new string(' ', tabSize);
		TextBox textBox = textInfo;
		string text2 = textBox.Text;
		textBox.Text = text2 + text + "Type: " + ex.GetType().ToString() + "\r\n";
		TextBox textBox2 = textInfo;
		string text3 = textBox2.Text;
		textBox2.Text = text3 + text + "Message: " + ex.Message + "\r\n";
		TextBox textBox3 = textInfo;
		textBox3.Text = textBox3.Text + text + "Stack trace:\r\n";
		TextBox textBox4 = textInfo;
		textBox4.Text = textBox4.Text + text + ex.StackTrace + "\r\n";
		if (ex.InnerException != null)
		{
			TextBox textBox5 = textInfo;
			textBox5.Text = textBox5.Text + text + "======Inner exception======\r\n";
			parseException(tabSize + 4, ex.InnerException);
			TextBox textBox6 = textInfo;
			textBox6.Text = textBox6.Text + text + "===========================\r\n";
		}
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		base.OnPaint(e);
		e.Graphics.DrawIcon(_reportIcon, 16, 16);
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
		this.textInfo = new System.Windows.Forms.TextBox();
		this.labelInfo = new System.Windows.Forms.Label();
		this.btnOK = new System.Windows.Forms.Button();
		this.labelHelp = new System.Windows.Forms.Label();
		base.SuspendLayout();
		this.textInfo.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.textInfo.BackColor = System.Drawing.SystemColors.Control;
		this.textInfo.Font = new System.Drawing.Font("Courier New", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.textInfo.Location = new System.Drawing.Point(8, 64);
		this.textInfo.Multiline = true;
		this.textInfo.Name = "textInfo";
		this.textInfo.ReadOnly = true;
		this.textInfo.ScrollBars = System.Windows.Forms.ScrollBars.Both;
		this.textInfo.Size = new System.Drawing.Size(458, 175);
		this.textInfo.TabIndex = 0;
		this.textInfo.WordWrap = false;
		this.labelInfo.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.labelInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 204);
		this.labelInfo.Location = new System.Drawing.Point(64, 8);
		this.labelInfo.Name = "labelInfo";
		this.labelInfo.Size = new System.Drawing.Size(402, 18);
		this.labelInfo.TabIndex = 1;
		this.labelInfo.Text = "Exception was unhandled";
		this.btnOK.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.btnOK.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 204);
		this.btnOK.Location = new System.Drawing.Point(388, 244);
		this.btnOK.Name = "btnOK";
		this.btnOK.Size = new System.Drawing.Size(74, 28);
		this.btnOK.TabIndex = 2;
		this.btnOK.Text = "OK";
		this.btnOK.UseVisualStyleBackColor = true;
		this.labelHelp.Location = new System.Drawing.Point(64, 26);
		this.labelHelp.Name = "labelHelp";
		this.labelHelp.Size = new System.Drawing.Size(402, 35);
		this.labelHelp.TabIndex = 3;
		this.labelHelp.Text = "label1";
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(474, 284);
		base.Controls.Add(this.labelHelp);
		base.Controls.Add(this.btnOK);
		base.Controls.Add(this.labelInfo);
		base.Controls.Add(this.textInfo);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "ExceptionReport";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "Exception Report";
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
