using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using ZXMAK.Platform.Windows.Forms.Properties;

namespace ZXMAK.Platform.Windows.Forms;

public class About : Form
{
	private readonly IContainer components;

	private Label labelVersionText;

	private Label labelCopyright;

	private Label labelAmstrad;

	private Button buttonOk;

	private Button buttonUpdate;

	private Label labelLogo;

	private PictureBox pictureBox1;

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
		this.buttonOk = new System.Windows.Forms.Button();
		this.buttonUpdate = new System.Windows.Forms.Button();
		this.labelVersionText = new System.Windows.Forms.Label();
		this.labelCopyright = new System.Windows.Forms.Label();
		this.labelAmstrad = new System.Windows.Forms.Label();
		this.labelLogo = new System.Windows.Forms.Label();
		this.pictureBox1 = new System.Windows.Forms.PictureBox();
		((System.ComponentModel.ISupportInitialize)this.pictureBox1).BeginInit();
		base.SuspendLayout();
		this.buttonOk.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.buttonOk.Location = new System.Drawing.Point(406, 12);
		this.buttonOk.Name = "buttonOk";
		this.buttonOk.Size = new System.Drawing.Size(75, 23);
		this.buttonOk.TabIndex = 0;
		this.buttonOk.Text = "OK";
		this.buttonOk.UseVisualStyleBackColor = true;
		this.buttonUpdate.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.buttonUpdate.Location = new System.Drawing.Point(406, 41);
		this.buttonUpdate.Name = "buttonUpdate";
		this.buttonUpdate.Size = new System.Drawing.Size(75, 23);
		this.buttonUpdate.TabIndex = 1;
		this.buttonUpdate.Text = "Update";
		this.buttonUpdate.UseVisualStyleBackColor = true;
		this.buttonUpdate.Visible = false;
		this.labelVersionText.AutoSize = true;
		this.labelVersionText.Location = new System.Drawing.Point(53, 88);
		this.labelVersionText.Name = "labelVersionText";
		this.labelVersionText.Size = new System.Drawing.Size(81, 13);
		this.labelVersionText.TabIndex = 3;
		this.labelVersionText.Text = "Version 0.0.0.0";
		this.labelCopyright.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.labelCopyright.AutoSize = true;
		this.labelCopyright.Location = new System.Drawing.Point(53, 112);
		this.labelCopyright.Name = "labelCopyright";
		this.labelCopyright.Size = new System.Drawing.Size(218, 26);
		this.labelCopyright.TabIndex = 4;
		this.labelCopyright.Text = "Copyright © 2001-2007 Alexander Makeev.\nAll rights reserved.";
		this.labelAmstrad.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.labelAmstrad.AutoSize = true;
		this.labelAmstrad.Location = new System.Drawing.Point(53, 150);
		this.labelAmstrad.Name = "labelAmstrad";
		this.labelAmstrad.Size = new System.Drawing.Size(428, 39);
		this.labelAmstrad.TabIndex = 6;
		this.labelAmstrad.Text = "Portions of this software are copyright © Amstrad Consumer Electronics plc. Amstrad\nhave kindly given their permission for the redistribution of their copyrighted material but\nretain that copyright.";
		this.labelLogo.AutoSize = true;
		this.labelLogo.Font = new System.Drawing.Font("Courier New", 21.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 204);
		this.labelLogo.Location = new System.Drawing.Point(90, 31);
		this.labelLogo.Name = "labelLogo";
		this.labelLogo.Size = new System.Drawing.Size(168, 33);
		this.labelLogo.TabIndex = 9;
		this.labelLogo.Text = "ZXMAK.NET";
		this.pictureBox1.Image = ZXMAK.Platform.Windows.Forms.Properties.Resources.zxmak;
		this.pictureBox1.Location = new System.Drawing.Point(12, 12);
		this.pictureBox1.Name = "pictureBox1";
		this.pictureBox1.Size = new System.Drawing.Size(72, 73);
		this.pictureBox1.TabIndex = 10;
		this.pictureBox1.TabStop = false;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackColor = System.Drawing.SystemColors.Window;
		base.ClientSize = new System.Drawing.Size(497, 207);
		base.Controls.Add(this.pictureBox1);
		base.Controls.Add(this.labelLogo);
		base.Controls.Add(this.labelAmstrad);
		base.Controls.Add(this.labelCopyright);
		base.Controls.Add(this.labelVersionText);
		base.Controls.Add(this.buttonUpdate);
		base.Controls.Add(this.buttonOk);
		this.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
		base.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "About";
		base.ShowIcon = false;
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "About ZXMAK.NET";
		((System.ComponentModel.ISupportInitialize)this.pictureBox1).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}

	public About()
	{
		InitializeComponent();
		labelVersionText.Text = labelVersionText.Text.Replace("0.0.0.0", Application.ProductVersion);
	}
}
