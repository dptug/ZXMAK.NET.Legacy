using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Forms;

namespace ZXMAK.Platform.Windows.Forms;

public sealed class GameLoop
{
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

	private static bool AppStillIdle
	{
		get
		{
			MSG msg = default(MSG);
			return !PeekMessage(ref msg, IntPtr.Zero, 0, 0, 0);
		}
	}

	public static void Run(IGameLoopForm form)
	{
		form.Show();
		while (form.Created)
		{
			if (!AppStillIdle)
			{
				Application.DoEvents();
			}
			form.UpdateState();
			form.RenderFrame();
		}
	}

	[DllImport("user32.dll", CharSet = CharSet.Auto)]
	[SuppressUnmanagedCodeSecurity]
	private static extern bool PeekMessage(ref MSG msg, IntPtr hwnd, int msgMin, int msgMax, int remove);
}
