using System;
using ZXMAK.Configuration;
using ZXMAK.Engine.Loaders;
using ZXMAK.Engine.Z80;
using ZXMAK.Platform;

namespace ZXMAK.Engine;

public abstract class Spectrum : IDisposable, IVideoRenderer
{
	private Config _config;

	private bool _isRunning;

	private VideoManager _videoManager;

	protected virtual Config Config => _config;

	public abstract Z80CPU CPU { get; }

	public abstract LoadManager Loader { get; }

	public virtual bool IsRunning
	{
		get
		{
			return _isRunning;
		}
		set
		{
			_isRunning = value;
			OnUpdateState();
		}
	}

	public abstract long KeyboardState { get; set; }

	public abstract int MouseX { get; set; }

	public abstract int MouseY { get; set; }

	public abstract int MouseButtons { get; set; }

	public event OnBreakpointDelegate Breakpoint;

	public event OnUpdateStateDelegate UpdateState;

	public event OnMaxTactExceedDelegate MaxTactExceed;

	protected void OnBreakpoint()
	{
		if (this.Breakpoint != null)
		{
			this.Breakpoint(this, new EventArgs());
		}
	}

	protected void OnUpdateState()
	{
		if (this.UpdateState != null)
		{
			this.UpdateState(this, new EventArgs());
		}
	}

	protected bool OnMaxTactExceed(ulong tactLimit)
	{
		MaxTactExceedEventArgs maxTactExceedEventArgs = new MaxTactExceedEventArgs(cancel: true, tactLimit);
		if (this.MaxTactExceed != null)
		{
			this.MaxTactExceed(this, maxTactExceedEventArgs);
		}
		return maxTactExceedEventArgs.Cancel;
	}

	protected void SetResolution(int width, int height)
	{
		if (_videoManager != null)
		{
			_videoManager.SetResolution(width, height);
		}
	}

	protected virtual void VideoParamsChanged(VideoManager sender, VideoParams value)
	{
	}

	protected virtual void Reset()
	{
		CPU.Reset();
	}

	protected virtual void StepInto()
	{
		do
		{
			CPU.ExecCycle();
		}
		while (CPU.FX != 0 || CPU.XFX != 0);
	}

	protected virtual void StepOver()
	{
		ulong num = Config.MaxStepOverTactCount;
		ulong tact = CPU.Tact;
		int MnemLength;
		string mnemonic = Z80CPU.GetMnemonic(ReadMemory, CPU.regs.PC, Hex: true, out MnemLength);
		ushort num2 = (ushort)((uint)(CPU.regs.PC + MnemLength) & 0xFFFFu);
		if (mnemonic.IndexOf("J") >= 0 || mnemonic.IndexOf("RET") >= 0)
		{
			StepInto();
			return;
		}
		do
		{
			if (CPU.Tact - tact >= num)
			{
				if (OnMaxTactExceed(num))
				{
					return;
				}
				tact = CPU.Tact;
				num *= 2;
			}
			StepInto();
			if (CPU.regs.PC == num2)
			{
				return;
			}
		}
		while (!CheckBreakpoint(CPU.regs.PC));
		OnBreakpoint();
	}

	public void Init(GenericPlatform platform)
	{
		_config = platform.Config;
		_videoManager = platform.VideoManager;
		_videoManager.SetVideoRenderer(this);
		OnInit();
	}

	public virtual void OnInit()
	{
	}

	public virtual void Dispose()
	{
	}

	public abstract bool SetRomImage(RomName romName, byte[] data, int startIndex, int length);

	public abstract bool SetRamImage(int page, byte[] data, int startIndex, int length);

	public abstract byte[] GetRamImage(int page);

	public abstract int GetRamImagePageCount();

	public abstract int GetFrameTact();

	public abstract int GetFrameTactCount();

	public abstract byte ReadMemory(ushort addr);

	public abstract void WriteMemory(ushort addr, byte value);

	public void DoReset()
	{
		Reset();
		if (CheckBreakpoint(CPU.regs.PC))
		{
			IsRunning = false;
			OnBreakpoint();
		}
		OnUpdateState();
	}

	public void DoStepInto()
	{
		if (!IsRunning)
		{
			StepInto();
			OnUpdateState();
		}
	}

	public void DoStepOver()
	{
		if (!IsRunning)
		{
			StepOver();
			OnUpdateState();
		}
	}

	public abstract void AddBreakpoint(ushort addr);

	public abstract void RemoveBreakpoint(ushort addr);

	public abstract ushort[] GetBreakpointList();

	public abstract bool CheckBreakpoint(ushort addr);

	public abstract void ClearBreakpoints();

	public abstract void ExecuteFrame(IntPtr videoPtr, IntPtr soundPtr);

	public abstract void DrawScreen(IntPtr videoPtr);

	void IVideoRenderer.SetVideoParams(VideoManager sender, VideoParams value)
	{
		VideoParamsChanged(sender, value);
	}
}
