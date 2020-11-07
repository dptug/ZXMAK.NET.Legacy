using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace ZXMAK.Platform.Windows.Forms
{
	public partial class ExceptionReport : Form
	{
		public static void Execute(Exception ex)
		{
			using (ExceptionReport exceptionReport = new ExceptionReport(ex))
			{
				exceptionReport.ShowDialog();
			}
		}

		private ExceptionReport(Exception ex)
		{
			this._exception = ex;
			this.InitializeComponent();
			this.labelInfo.Text = this.labelInfo.Text.Replace("Exception", ex.GetType().ToString());
			this.textInfo.Text = "";
			this.parseException(0, ex);
			this.labelHelp.Text = ex.Message;
		}

		private void parseException(int tabSize, Exception ex)
		{
			string text = new string(' ', tabSize);
			TextBox textBox = this.textInfo;
			string text2 = textBox.Text;
			textBox.Text = string.Concat(new string[]
			{
				text2,
				text,
				"Type: ",
				ex.GetType().ToString(),
				"\r\n"
			});
			TextBox textBox2 = this.textInfo;
			string text3 = textBox2.Text;
			textBox2.Text = string.Concat(new string[]
			{
				text3,
				text,
				"Message: ",
				ex.Message,
				"\r\n"
			});
			TextBox textBox3 = this.textInfo;
			textBox3.Text = textBox3.Text + text + "Stack trace:\r\n";
			TextBox textBox4 = this.textInfo;
			textBox4.Text = textBox4.Text + text + ex.StackTrace + "\r\n";
			if (ex.InnerException != null)
			{
				TextBox textBox5 = this.textInfo;
				textBox5.Text = textBox5.Text + text + "======Inner exception======\r\n";
				this.parseException(tabSize + 4, ex.InnerException);
				TextBox textBox6 = this.textInfo;
				textBox6.Text = textBox6.Text + text + "===========================\r\n";
			}
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			e.Graphics.DrawIcon(this._reportIcon, 16, 16);
		}

		private Exception _exception;

		private Icon _reportIcon = SystemIcons.Error;
	}
}
