using System;

namespace ZXMAK.Platform.Windows.Forms
{
	public interface IGameLoopForm
	{
		void Show();

		void UpdateState();

		void RenderFrame();

		bool Created { get; }
	}
}
