namespace ZXMAK.Platform.Windows.Forms
{
	public partial class About : global::System.Windows.Forms.Form
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
			this.buttonOk = new global::System.Windows.Forms.Button();
			this.buttonUpdate = new global::System.Windows.Forms.Button();
			this.labelVersionText = new global::System.Windows.Forms.Label();
			this.labelCopyright = new global::System.Windows.Forms.Label();
			this.labelAmstrad = new global::System.Windows.Forms.Label();
			this.labelLogo = new global::System.Windows.Forms.Label();
			this.pictureBox1 = new global::System.Windows.Forms.PictureBox();
			((global::System.ComponentModel.ISupportInitialize)this.pictureBox1).BeginInit();
			base.SuspendLayout();
			this.buttonOk.Anchor = (global::System.Windows.Forms.AnchorStyles.Top | global::System.Windows.Forms.AnchorStyles.Right);
			this.buttonOk.DialogResult = global::System.Windows.Forms.DialogResult.OK;
			this.buttonOk.Location = new global::System.Drawing.Point(406, 12);
			this.buttonOk.Name = "buttonOk";
			this.buttonOk.Size = new global::System.Drawing.Size(75, 23);
			this.buttonOk.TabIndex = 0;
			this.buttonOk.Text = "OK";
			this.buttonOk.UseVisualStyleBackColor = true;
			this.buttonUpdate.Anchor = (global::System.Windows.Forms.AnchorStyles.Top | global::System.Windows.Forms.AnchorStyles.Right);
			this.buttonUpdate.Location = new global::System.Drawing.Point(406, 41);
			this.buttonUpdate.Name = "buttonUpdate";
			this.buttonUpdate.Size = new global::System.Drawing.Size(75, 23);
			this.buttonUpdate.TabIndex = 1;
			this.buttonUpdate.Text = "Update";
			this.buttonUpdate.UseVisualStyleBackColor = true;
			this.buttonUpdate.Visible = false;
			this.labelVersionText.AutoSize = true;
			this.labelVersionText.Location = new global::System.Drawing.Point(53, 88);
			this.labelVersionText.Name = "labelVersionText";
			this.labelVersionText.Size = new global::System.Drawing.Size(81, 13);
			this.labelVersionText.TabIndex = 3;
			this.labelVersionText.Text = "Version 0.0.0.0";
			this.labelCopyright.Anchor = (global::System.Windows.Forms.AnchorStyles.Bottom | global::System.Windows.Forms.AnchorStyles.Left | global::System.Windows.Forms.AnchorStyles.Right);
			this.labelCopyright.AutoSize = true;
			this.labelCopyright.Location = new global::System.Drawing.Point(53, 112);
			this.labelCopyright.Name = "labelCopyright";
			this.labelCopyright.Size = new global::System.Drawing.Size(218, 26);
			this.labelCopyright.TabIndex = 4;
			this.labelCopyright.Text = "Copyright © 2001-2007 Alexander Makeev.\nAll rights reserved.";
			this.labelAmstrad.Anchor = (global::System.Windows.Forms.AnchorStyles.Bottom | global::System.Windows.Forms.AnchorStyles.Left | global::System.Windows.Forms.AnchorStyles.Right);
			this.labelAmstrad.AutoSize = true;
			this.labelAmstrad.Location = new global::System.Drawing.Point(53, 150);
			this.labelAmstrad.Name = "labelAmstrad";
			this.labelAmstrad.Size = new global::System.Drawing.Size(428, 39);
			this.labelAmstrad.TabIndex = 6;
			this.labelAmstrad.Text = "Portions of this software are copyright © Amstrad Consumer Electronics plc. Amstrad\nhave kindly given their permission for the redistribution of their copyrighted material but\nretain that copyright.";
			this.labelLogo.AutoSize = true;
			this.labelLogo.Font = new global::System.Drawing.Font("Courier New", 21.75f, global::System.Drawing.FontStyle.Bold, global::System.Drawing.GraphicsUnit.Point, 204);
			this.labelLogo.Location = new global::System.Drawing.Point(90, 31);
			this.labelLogo.Name = "labelLogo";
			this.labelLogo.Size = new global::System.Drawing.Size(168, 33);
			this.labelLogo.TabIndex = 9;
			this.labelLogo.Text = "ZXMAK.NET";
			this.pictureBox1.Image = global::ZXMAK.Platform.Windows.Forms.Properties.Resources.zxmak;
			this.pictureBox1.Location = new global::System.Drawing.Point(12, 12);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new global::System.Drawing.Size(72, 73);
			this.pictureBox1.TabIndex = 10;
			this.pictureBox1.TabStop = false;
			base.AutoScaleDimensions = new global::System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = global::System.Drawing.SystemColors.Window;
			base.ClientSize = new global::System.Drawing.Size(497, 207);
			base.Controls.Add(this.pictureBox1);
			base.Controls.Add(this.labelLogo);
			base.Controls.Add(this.labelAmstrad);
			base.Controls.Add(this.labelCopyright);
			base.Controls.Add(this.labelVersionText);
			base.Controls.Add(this.buttonUpdate);
			base.Controls.Add(this.buttonOk);
			this.Font = new global::System.Drawing.Font("Tahoma", 8.25f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 204);
			base.FormBorderStyle = global::System.Windows.Forms.FormBorderStyle.FixedSingle;
			base.Margin = new global::System.Windows.Forms.Padding(3, 4, 3, 4);
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			base.Name = "About";
			base.ShowIcon = false;
			base.ShowInTaskbar = false;
			base.StartPosition = global::System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "About ZXMAK.NET";
			((global::System.ComponentModel.ISupportInitialize)this.pictureBox1).EndInit();
			base.ResumeLayout(false);
			base.PerformLayout();
		}

		private global::System.ComponentModel.IContainer components;

		private global::System.Windows.Forms.Label labelVersionText;

		private global::System.Windows.Forms.Label labelCopyright;

		private global::System.Windows.Forms.Label labelAmstrad;

		private global::System.Windows.Forms.Button buttonOk;

		private global::System.Windows.Forms.Button buttonUpdate;

		private global::System.Windows.Forms.Label labelLogo;

		private global::System.Windows.Forms.PictureBox pictureBox1;
	}
}
