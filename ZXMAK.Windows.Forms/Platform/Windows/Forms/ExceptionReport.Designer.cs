namespace ZXMAK.Platform.Windows.Forms
{
	public partial class ExceptionReport : global::System.Windows.Forms.Form
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
			this.textInfo = new global::System.Windows.Forms.TextBox();
			this.labelInfo = new global::System.Windows.Forms.Label();
			this.btnOK = new global::System.Windows.Forms.Button();
			this.labelHelp = new global::System.Windows.Forms.Label();
			base.SuspendLayout();
			this.textInfo.Anchor = (global::System.Windows.Forms.AnchorStyles.Top | global::System.Windows.Forms.AnchorStyles.Bottom | global::System.Windows.Forms.AnchorStyles.Left | global::System.Windows.Forms.AnchorStyles.Right);
			this.textInfo.BackColor = global::System.Drawing.SystemColors.Control;
			this.textInfo.Font = new global::System.Drawing.Font("Courier New", 9f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 204);
			this.textInfo.Location = new global::System.Drawing.Point(8, 64);
			this.textInfo.Multiline = true;
			this.textInfo.Name = "textInfo";
			this.textInfo.ReadOnly = true;
			this.textInfo.ScrollBars = global::System.Windows.Forms.ScrollBars.Both;
			this.textInfo.Size = new global::System.Drawing.Size(458, 175);
			this.textInfo.TabIndex = 0;
			this.textInfo.WordWrap = false;
			this.labelInfo.Anchor = (global::System.Windows.Forms.AnchorStyles.Top | global::System.Windows.Forms.AnchorStyles.Left | global::System.Windows.Forms.AnchorStyles.Right);
			this.labelInfo.Font = new global::System.Drawing.Font("Microsoft Sans Serif", 9.75f, global::System.Drawing.FontStyle.Bold, global::System.Drawing.GraphicsUnit.Point, 204);
			this.labelInfo.Location = new global::System.Drawing.Point(64, 8);
			this.labelInfo.Name = "labelInfo";
			this.labelInfo.Size = new global::System.Drawing.Size(402, 18);
			this.labelInfo.TabIndex = 1;
			this.labelInfo.Text = "Exception was unhandled";
			this.btnOK.Anchor = (global::System.Windows.Forms.AnchorStyles.Bottom | global::System.Windows.Forms.AnchorStyles.Right);
			this.btnOK.DialogResult = global::System.Windows.Forms.DialogResult.OK;
			this.btnOK.Font = new global::System.Drawing.Font("Microsoft Sans Serif", 8.25f, global::System.Drawing.FontStyle.Bold, global::System.Drawing.GraphicsUnit.Point, 204);
			this.btnOK.Location = new global::System.Drawing.Point(388, 244);
			this.btnOK.Name = "btnOK";
			this.btnOK.Size = new global::System.Drawing.Size(74, 28);
			this.btnOK.TabIndex = 2;
			this.btnOK.Text = "OK";
			this.btnOK.UseVisualStyleBackColor = true;
			this.labelHelp.Location = new global::System.Drawing.Point(64, 26);
			this.labelHelp.Name = "labelHelp";
			this.labelHelp.Size = new global::System.Drawing.Size(402, 35);
			this.labelHelp.TabIndex = 3;
			this.labelHelp.Text = "label1";
			base.AutoScaleDimensions = new global::System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new global::System.Drawing.Size(474, 284);
			base.Controls.Add(this.labelHelp);
			base.Controls.Add(this.btnOK);
			base.Controls.Add(this.labelInfo);
			base.Controls.Add(this.textInfo);
			base.FormBorderStyle = global::System.Windows.Forms.FormBorderStyle.FixedDialog;
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			base.Name = "ExceptionReport";
			base.StartPosition = global::System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Exception Report";
			base.ResumeLayout(false);
			base.PerformLayout();
		}

		private global::System.ComponentModel.IContainer components;

		private global::System.Windows.Forms.TextBox textInfo;

		private global::System.Windows.Forms.Label labelInfo;

		private global::System.Windows.Forms.Button btnOK;

		private global::System.Windows.Forms.Label labelHelp;
	}
}
