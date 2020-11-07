using System;
using ZXMAK.Configuration;
using ZXMAK.Engine.Loaders;
using ZXMAK.Engine.Z80;
using ZXMAK.Platform;

namespace ZXMAK.Engine
{
	public abstract class Spectrum : IDisposable, IVideoRenderer
	{
		protected virtual Config Config
		{
			get
			{
				return this._config;
			}
		}

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
			MaxTactExceedEventArgs maxTactExceedEventArgs = new MaxTactExceedEventArgs(true, tactLimit);
			if (this.MaxTactExceed != null)
			{
				this.MaxTactExceed(this, maxTactExceedEventArgs);
			}
			return maxTactExceedEventArgs.Cancel;
		}

		protected void SetResolution(int width, int height)
		{
			if (this._videoManager != null)
			{
				this._videoManager.SetResolution(width, height);
			}
		}

		protected virtual void VideoParamsChanged(VideoManager sender, VideoParams value)
		{
		}

		protected virtual void Reset()
		{
			this.CPU.Reset();
		}

		protected virtual void StepInto()
		{
			do
			{
				this.CPU.ExecCycle();
			}
			while (this.CPU.FX != OPFX.NONE || this.CPU.XFX != OPXFX.NONE);
		}

		protected virtual void StepOver()
		{
			ulong num = this.Config.MaxStepOverTactCount;
			ulong tact = this.CPU.Tact;
			int num2;
			string mnemonic = Z80CPU.GetMnemonic(new Z80CPU.MEMREADER(this.ReadMemory), (int)this.CPU.regs.PC, true, out num2);
			ushort num3 = (ushort)((int)this.CPU.regs.PC + num2 & 65535);
			bool flag = mnemonic.IndexOf("J") >= 0 || mnemonic.IndexOf("RET") >= 0;
			if (flag)
			{
				this.StepInto();
				return;
			}
			for (;;)
			{
				if (this.CPU.Tact - tact >= num)
				{
					if (this.OnMaxTactExceed(num))
					{
						break;
					}
					tact = this.CPU.Tact;
					num *= 2UL;
				}
				this.StepInto();
				if (this.CPU.regs.PC == num3)
				{
					return;
				}
				if (this.CheckBreakpoint(this.CPU.regs.PC))
				{
					goto Block_6;
				}
			}
			return;
			Block_6:
			this.OnBreakpoint();
		}

		public abstract Z80CPU CPU { get; }

		public abstract LoadManager Loader { get; }

		public virtual bool IsRunning
		{
			get
			{
				return this._isRunning;
			}
			set
			{
				this._isRunning = value;
				this.OnUpdateState();
			}
		}

		public event OnBreakpointDelegate Breakpoint;

		public event OnUpdateStateDelegate UpdateState;

		public event OnMaxTactExceedDelegate MaxTactExceed;

		public void Init(GenericPlatform platform)
		{
			this._config = platform.Config;
			this._videoManager = platform.VideoManager;
			this._videoManager.SetVideoRenderer(this);
			this.OnInit();
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
			this.Reset();
			if (this.CheckBreakpoint(this.CPU.regs.PC))
			{
				this.IsRunning = false;
				this.OnBreakpoint();
			}
			this.OnUpdateState();
		}

		public void DoStepInto()
		{
			if (this.IsRunning)
			{
				return;
			}
			this.StepInto();
			this.OnUpdateState();
		}

		public void DoStepOver()
		{
			if (this.IsRunning)
			{
				return;
			}
			this.StepOver();
			this.OnUpdateState();
		}

		public abstract void AddBreakpoint(ushort addr);

		public abstract void RemoveBreakpoint(ushort addr);

		public abstract ushort[] GetBreakpointList();

		public abstract bool CheckBreakpoint(ushort addr);

		public abstract void ClearBreakpoints();

		public abstract long KeyboardState { get; set; }

		public abstract int MouseX { get; set; }

		public abstract int MouseY { get; set; }

		public abstract int MouseButtons { get; set; }

		public abstract void ExecuteFrame(IntPtr videoPtr, IntPtr soundPtr);

		public abstract void DrawScreen(IntPtr videoPtr);

		void IVideoRenderer.SetVideoParams(VideoManager sender, VideoParams value)
		{
			this.VideoParamsChanged(sender, value);
		}

		private Config _config;

		private bool _isRunning;

		private VideoManager _videoManager;
	}
}
