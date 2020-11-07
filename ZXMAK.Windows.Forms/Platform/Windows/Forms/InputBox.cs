using System;
using System.Drawing;
using System.Windows.Forms;

namespace ZXMAK.Platform.Windows.Forms
{
	public partial class InputBox : Form
	{
		private InputBox(string Caption, string Text)
		{
			this.label = new Label();
			this.textValue = new TextBox();
			this.buttonOK = new Button();
			this.buttonCancel = new Button();
			base.SuspendLayout();
			this.label.AutoSize = true;
			this.label.Location = new Point(9, 13);
			this.label.Name = "label";
			this.label.Size = new Size(31, 13);
			this.label.TabIndex = 1;
			this.label.Text = Text;
			this.textValue.Location = new Point(12, 31);
			this.textValue.Name = "textValue";
			this.textValue.Size = new Size(245, 20);
			this.textValue.TabIndex = 2;
			this.textValue.WordWrap = false;
			this.buttonOK.DialogResult = DialogResult.OK;
			this.buttonOK.Location = new Point(57, 67);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new Size(75, 23);
			this.buttonOK.TabIndex = 3;
			this.buttonOK.Text = "OK";
			this.buttonOK.UseVisualStyleBackColor = true;
			this.buttonCancel.DialogResult = DialogResult.Cancel;
			this.buttonCancel.Location = new Point(138, 67);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new Size(75, 23);
			this.buttonCancel.TabIndex = 4;
			this.buttonCancel.Text = "Cancel";
			this.buttonCancel.UseVisualStyleBackColor = true;
			base.AcceptButton = this.buttonOK;
			base.AutoScaleDimensions = new SizeF(6f, 13f);
			base.AutoScaleMode = AutoScaleMode.Font;
			base.CancelButton = this.buttonCancel;
			base.ClientSize = new Size(270, 103);
			base.Controls.Add(this.buttonCancel);
			base.Controls.Add(this.buttonOK);
			base.Controls.Add(this.textValue);
			base.Controls.Add(this.label);
			base.FormBorderStyle = FormBorderStyle.FixedSingle;
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			base.Name = "InputBox";
			base.ShowIcon = false;
			base.ShowInTaskbar = false;
			base.StartPosition = FormStartPosition.CenterScreen;
			this.Text = Caption;
			base.ResumeLayout(false);
			base.PerformLayout();
		}

		public static bool Query(string Caption, string Text, ref string s_val)
		{
			InputBox inputBox = new InputBox(Caption, Text);
			inputBox.textValue.Text = s_val;
			if (inputBox.ShowDialog() != DialogResult.OK)
			{
				return false;
			}
			s_val = inputBox.textValue.Text;
			return true;
		}

		public static bool InputValue(string Caption, string Text, string prefix, string format, ref int value, int min, int max)
		{
			int num = value;
			string text = prefix + value.ToString(format);
			for (;;)
			{
				bool flag = true;
				if (!InputBox.Query(Caption, Text, ref text))
				{
					break;
				}
				try
				{
					string text2 = text.Trim();
					if (text2.Length > 0 && text2[0] == '#')
					{
						text2 = text2.Remove(0, 1);
						num = Convert.ToInt32(text2, 16);
					}
					else if (text2.Length > 1 && text2[1] == 'x' && text2[0] == '0')
					{
						text2 = text2.Remove(0, 2);
						num = Convert.ToInt32(text2, 16);
					}
					else
					{
						num = Convert.ToInt32(text2, 10);
					}
				}
				catch
				{
					MessageBox.Show("Требуется ввести число!");
					flag = false;
				}
				if (num < min || num > max)
				{
					MessageBox.Show(string.Concat(new string[]
					{
						"Требуется число в диапазоне ",
						min.ToString(),
						"...",
						max.ToString(),
						" !"
					}));
					flag = false;
				}
				if (flag)
				{
					goto Block_4;
				}
			}
			return false;
			Block_4:
			value = num;
			return true;
		}

		private Label label;

		private TextBox textValue;

		private Button buttonOK;

		private Button buttonCancel;
	}
}
