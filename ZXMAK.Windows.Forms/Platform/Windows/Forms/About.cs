using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using ZXMAK.Platform.Windows.Forms.Properties;

namespace ZXMAK.Platform.Windows.Forms
{
	public partial class About : Form
	{
		public About()
		{
			this.InitializeComponent();
			this.labelVersionText.Text = this.labelVersionText.Text.Replace("0.0.0.0", Application.ProductVersion);
		}
	}
}
