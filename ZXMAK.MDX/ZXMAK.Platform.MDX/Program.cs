using System;

namespace ZXMAK.Platform.MDX;

internal class Program
{
	[STAThread]
	private static void Main(string[] args)
	{
		PlatformFactory.Execute(args, new Platform());
	}
}
