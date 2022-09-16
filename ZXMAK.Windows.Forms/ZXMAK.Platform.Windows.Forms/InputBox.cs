using System;
using System.Drawing;
using System.Windows.Forms;

namespace ZXMAK.Platform.Windows.Forms;

public class InputBox : Form
{
	private Label label;

	private TextBox textValue;

	private Button buttonOK;

	private Button buttonCancel;

	private InputBox(string Caption, string Text)
	{
		label = new Label();
		textValue = new TextBox();
		buttonOK = new Button();
		buttonCancel = new Button();
		SuspendLayout();
		label.AutoSize = true;
		label.Location = new Point(9, 13);
		label.Name = "label";
		label.Size = new Size(31, 13);
		label.TabIndex = 1;
		label.Text = Text;
		textValue.Location = new Point(12, 31);
		textValue.Name = "textValue";
		textValue.Size = new Size(245, 20);
		textValue.TabIndex = 2;
		textValue.WordWrap = false;
		buttonOK.DialogResult = DialogResult.OK;
		buttonOK.Location = new Point(57, 67);
		buttonOK.Name = "buttonOK";
		buttonOK.Size = new Size(75, 23);
		buttonOK.TabIndex = 3;
		buttonOK.Text = "OK";
		buttonOK.UseVisualStyleBackColor = true;
		buttonCancel.DialogResult = DialogResult.Cancel;
		buttonCancel.Location = new Point(138, 67);
		buttonCancel.Name = "buttonCancel";
		buttonCancel.Size = new Size(75, 23);
		buttonCancel.TabIndex = 4;
		buttonCancel.Text = "Cancel";
		buttonCancel.UseVisualStyleBackColor = true;
		base.AcceptButton = buttonOK;
		base.AutoScaleDimensions = new SizeF(6f, 13f);
		base.AutoScaleMode = AutoScaleMode.Font;
		base.CancelButton = buttonCancel;
		base.ClientSize = new Size(270, 103);
		base.Controls.Add(buttonCancel);
		base.Controls.Add(buttonOK);
		base.Controls.Add(textValue);
		base.Controls.Add(label);
		base.FormBorderStyle = FormBorderStyle.FixedSingle;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "InputBox";
		base.ShowIcon = false;
		base.ShowInTaskbar = false;
		base.StartPosition = FormStartPosition.CenterScreen;
		this.Text = Caption;
		ResumeLayout(performLayout: false);
		PerformLayout();
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
		string s_val = prefix + value.ToString(format);
		bool flag;
		do
		{
			flag = true;
			if (!Query(Caption, Text, ref s_val))
			{
				return false;
			}
			try
			{
				string text = s_val.Trim();
				if (text.Length > 0 && text[0] == '#')
				{
					text = text.Remove(0, 1);
					num = Convert.ToInt32(text, 16);
				}
				else if (text.Length > 1 && text[1] == 'x' && text[0] == '0')
				{
					text = text.Remove(0, 2);
					num = Convert.ToInt32(text, 16);
				}
				else
				{
					num = Convert.ToInt32(text, 10);
				}
			}
			catch
			{
				MessageBox.Show("Требуется ввести число!");
				flag = false;
			}
			if (num < min || num > max)
			{
				MessageBox.Show("Требуется число в диапазоне " + min + "..." + max + " !");
				flag = false;
			}
		}
		while (!flag);
		value = num;
		return true;
	}
}
