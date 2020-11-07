using System;

namespace ZXMAK.Platform.XNA2.Win
{
	public class Platform : GenericPlatform
	{
		protected override void Running()
		{
			using (XnaGame xnaGame = new XnaGame(this))
			{
				xnaGame.Run();
			}
		}

		public override void ShowFatalError(Exception ex)
		{
		}

		public override void ShowWarning(string message, string title)
		{
		}

		public override void ShowNotification(string message, string title)
		{
		}

		public override void SetCaption(string text)
		{
		}

		public override QueryResult QueryDialog(string message, string title, QueryButtons buttons)
		{
			return QueryResult.No;
		}
	}
}
