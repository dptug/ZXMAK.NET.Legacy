using System;

namespace ZXMAK.Platform.XNA2.Win
{
	internal static class Program
	{
		[STAThread]
		private static void Main(string[] args)
		{
			PlatformFactory.Execute(args, new Platform());
		}
	}
}
