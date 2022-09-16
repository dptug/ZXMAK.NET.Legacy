namespace ZXMAK.Platform.Windows.Forms;

public interface IGameLoopForm
{
	bool Created { get; }

	void Show();

	void UpdateState();

	void RenderFrame();
}
