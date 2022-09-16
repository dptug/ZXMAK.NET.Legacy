using System;
using System.Windows.Forms;
using ZXMAK.Platform.Windows.Forms;
using ZXMAK.Windows.Forms;

namespace ZXMAK.Platform.MDX;

public class Platform : GenericPlatform
{
	private MainForm _form;

	public override void SetCaption(string text)
	{
		_form.Text = text;
	}

	public override void ShowFatalError(Exception ex)
	{
		ExceptionReport.Execute(ex);
	}

	public override void ShowWarning(string message, string title)
	{
		MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
	}

	public override void ShowNotification(string message, string title)
	{
		MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
	}

	public override QueryResult QueryDialog(string message, string title, QueryButtons buttons)
	{
		MessageBoxButtons buttons2 = MessageBoxButtons.OK;
		if (buttons == QueryButtons.YesNo)
		{
			buttons2 = MessageBoxButtons.YesNo;
		}
		DialogResult dialogResult = MessageBox.Show(message, title, buttons2, MessageBoxIcon.Question);
		QueryResult result = QueryResult.Yes;
		if (dialogResult != DialogResult.Yes)
		{
			result = QueryResult.No;
		}
		return result;
	}

	protected override void Running()
	{
		Application.EnableVisualStyles();
		Application.SetCompatibleTextRenderingDefault(defaultValue: false);
		ToolStripManager.Renderer = new ToolStripProfessionalRenderer(new TanColorTable());
		using (_form = new MainForm())
		{
			_form.Init(this);
			GameLoop.Run(_form);
		}
	}
}
