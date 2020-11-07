using System;
using System.Collections;
using System.Threading;
using System.Windows.Forms;
using Microsoft.DirectX.DirectSound;
using ZXMAK.Logging;

namespace ZXMAK.Platform.MDX
{
	public class DirectSound : IDisposable
	{
		public DirectSound(Control mainForm, int device, int samplesPerSecond, short bitsPerSample, short channels, int bufferSize, int bufferCount)
		{
			this._fillQueue = new Queue(bufferCount);
			this._playQueue = new Queue(bufferCount);
			for (int i = 0; i < bufferCount; i++)
			{
				this._fillQueue.Enqueue(new byte[bufferSize]);
			}
			this._bufferSize = bufferSize;
			this._bufferCount = bufferCount;
			this._zeroValue = ((bitsPerSample == 8) ? 128 : 0);
			this._device = new Device();
			this._device.SetCooperativeLevel(mainForm, CooperativeLevel.Priority);
			WaveFormat wfx = new WaveFormat();
			wfx.FormatTag = WaveFormatTag.Pcm;
			wfx.SamplesPerSecond = samplesPerSecond;
			wfx.BitsPerSample = bitsPerSample;
			wfx.Channels = channels;
			wfx.BlockAlign = wfx.Channels * (wfx.BitsPerSample / 8);
			wfx.AverageBytesPerSecond = wfx.SamplesPerSecond * (int)wfx.BlockAlign;
			this._soundBuffer = new SecondaryBuffer(new BufferDescription(wfx)
			{
				BufferBytes = this._bufferSize * this._bufferCount,
				ControlPositionNotify = true,
				GlobalFocus = true
			}, this._device);
			this._notify = new Notify(this._soundBuffer);
			BufferPositionNotify[] array = new BufferPositionNotify[this._bufferCount];
			for (int j = 0; j < array.Length; j++)
			{
				array[j] = new BufferPositionNotify();
				array[j].Offset = j * this._bufferSize;
				array[j].EventNotifyHandle = this._fillEvent.SafeWaitHandle.DangerousGetHandle();
			}
			this._notify.SetNotificationPositions(array);
			this._waveFillThread = new Thread(new ThreadStart(this.waveFillThreadProc));
			this._waveFillThread.IsBackground = true;
			this._waveFillThread.Name = "Wave fill thread";
			this._waveFillThread.Priority = ThreadPriority.Highest;
			this._waveFillThread.Start();
		}

		public void Dispose()
		{
			if (this._waveFillThread != null)
			{
				try
				{
					this._isFinished = true;
					if (this._soundBuffer != null && this._soundBuffer.Status.Playing)
					{
						this._soundBuffer.Stop();
					}
					this._fillEvent.Set();
					this._waveFillThread.Join();
					if (this._soundBuffer != null)
					{
						this._soundBuffer.Dispose();
					}
					if (this._notify != null)
					{
						this._notify.Dispose();
					}
					if (this._device != null)
					{
						this._device.Dispose();
					}
				}
				finally
				{
					this._waveFillThread = null;
					this._soundBuffer = null;
					this._notify = null;
					this._device = null;
				}
			}
		}

		private unsafe void waveFillThreadProc()
		{
			int num = -1;
			byte[] array = new byte[this._bufferSize];
			fixed (byte* ptr = array)
			{
				try
				{
					this._soundBuffer.Play(0, BufferPlayFlags.Looping);
					while (!this._isFinished)
					{
						this._fillEvent.WaitOne();
						for (int num2 = (num + 1) % this._bufferCount; num2 != this._soundBuffer.PlayPosition / this._bufferSize; num2 = (num2 + 1) % this._bufferCount)
						{
							this.OnBufferFill((IntPtr)((void*)ptr), array.Length);
							this._soundBuffer.Write(this._bufferSize * num2, array, LockFlag.None);
							num = num2;
						}
					}
				}
				catch (Exception ex)
				{
					Logger.GetLogger().LogError(ex);
				}
			}
		}

		protected unsafe void OnBufferFill(IntPtr buffer, int length)
		{
			byte[] array = null;
			lock (this._playQueue.SyncRoot)
			{
				if (this._playQueue.Count > 0)
				{
					array = (this._playQueue.Dequeue() as byte[]);
				}
			}
			if (array != null)
			{
				uint* ptr = (uint*)((void*)buffer);
				fixed (byte* ptr2 = array)
				{
					uint* ptr3 = (uint*)ptr2;
					for (int i = 0; i < length / 4; i++)
					{
						ptr[i] = ptr3[i];
					}
					this.lastSample = ptr[length / 4 - 1];
				}
				lock (this._fillQueue.SyncRoot)
				{
					this._fillQueue.Enqueue(array);
					return;
				}
			}
			uint* ptr4 = (uint*)((void*)buffer);
			for (int j = 0; j < length / 4; j++)
			{
				ptr4[j] = this.lastSample;
			}
		}

		public byte[] LockBuffer()
		{
			byte[] result = null;
			lock (this._fillQueue.SyncRoot)
			{
				if (this._fillQueue.Count > 0)
				{
					result = (this._fillQueue.Dequeue() as byte[]);
				}
			}
			return result;
		}

		public void UnlockBuffer(byte[] sndbuf)
		{
			lock (this._playQueue.SyncRoot)
			{
				this._playQueue.Enqueue(sndbuf);
			}
		}

		public int QueueLoadState
		{
			get
			{
				return (int)((double)this._playQueue.Count * 100.0 / (double)this._fillQueue.Count);
			}
		}

		private Device _device;

		private SecondaryBuffer _soundBuffer;

		private Notify _notify;

		private byte _zeroValue;

		private int _bufferSize;

		private int _bufferCount;

		private Thread _waveFillThread;

		private AutoResetEvent _fillEvent = new AutoResetEvent(true);

		private bool _isFinished;

		private Queue _fillQueue;

		private Queue _playQueue;

		private uint lastSample;
	}
}
