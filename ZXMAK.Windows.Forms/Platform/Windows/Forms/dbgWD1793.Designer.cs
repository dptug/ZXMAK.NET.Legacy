namespace ZXMAK.Platform.Windows.Forms
{
	public partial class dbgWD1793 : global::System.Windows.Forms.Form
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
			this.components = new global::System.ComponentModel.Container();
			this.label1 = new global::System.Windows.Forms.Label();
			this.timerUpdate = new global::System.Windows.Forms.Timer(this.components);
			base.SuspendLayout();
			this.label1.AutoSize = true;
			this.label1.Location = new global::System.Drawing.Point(36, 39);
			this.label1.Name = "label1";
			this.label1.Size = new global::System.Drawing.Size(35, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "label1";
			this.timerUpdate.Enabled = true;
			this.timerUpdate.Interval = 300;
			this.timerUpdate.Tick += new global::System.EventHandler(this.timerUpdate_Tick);
			base.AutoScaleDimensions = new global::System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new global::System.Drawing.Size(292, 371);
			base.Controls.Add(this.label1);
			base.Name = "dbgWD1793";
			this.Text = "dbgWD1793";
			base.ResumeLayout(false);
			base.PerformLayout();
		}

		private global::System.ComponentModel.IContainer components;

		private global::System.Windows.Forms.Label label1;

		private global::System.Windows.Forms.Timer timerUpdate;
	}
}
