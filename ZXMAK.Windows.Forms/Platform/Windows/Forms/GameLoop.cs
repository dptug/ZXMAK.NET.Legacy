using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Forms;

namespace ZXMAK.Platform.Windows.Forms
{
	public sealed class GameLoop
	{
		public static void Run(IGameLoopForm form)
		{
			form.Show();
			while (form.Created)
			{
				if (!GameLoop.AppStillIdle)
				{
					Application.DoEvents();
				}
				form.UpdateState();
				form.RenderFrame();
			}
		}

		private static bool AppStillIdle
		{
			get
			{
				GameLoop.MSG msg = default(GameLoop.MSG);
				return !GameLoop.PeekMessage(ref msg, IntPtr.Zero, 0, 0, 0);
			}
		}

		[SuppressUnmanagedCodeSecurity]
		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		private static extern bool PeekMessage(ref GameLoop.MSG msg, IntPtr hwnd, int msgMin, int msgMax, int remove);

		private struct MSG
		{
			public IntPtr hwnd;

			public int message;

			public IntPtr wParam;

			public IntPtr lParam;

			public int time;

			public int pt_x;

			public int pt_y;
		}
	}
}
