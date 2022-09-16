using System;
using System.Collections;
using System.Threading;
using System.Windows.Forms;
using Microsoft.DirectX.DirectSound;
using ZXMAK.Logging;

namespace ZXMAK.Platform.MDX;

public class DirectSound : IDisposable
{
	private Device _device;

	private SecondaryBuffer _soundBuffer;

	private Notify _notify;

	private readonly byte _zeroValue;

	private readonly int _bufferSize;

	private readonly int _bufferCount;

	private Thread _waveFillThread;

	private readonly AutoResetEvent _fillEvent = new(initialState: true);

	private bool _isFinished;

	private readonly Queue _fillQueue;

	private readonly Queue _playQueue;

	private uint lastSample;

	public int QueueLoadState => (int)((double)_playQueue.Count * 100.0 / (double)_fillQueue.Count);

	public DirectSound(Control mainForm, int device, int samplesPerSecond, short bitsPerSample, short channels, int bufferSize, int bufferCount)
	{
		_fillQueue = new Queue(bufferCount);
		_playQueue = new Queue(bufferCount);
		for (int i = 0; i < bufferCount; i++)
		{
			_fillQueue.Enqueue(new byte[bufferSize]);
		}
		_bufferSize = bufferSize;
		_bufferCount = bufferCount;
		_zeroValue = (byte)((bitsPerSample == 8) ? 128 : 0);
		_device = new Device();
		_device.SetCooperativeLevel(mainForm, CooperativeLevel.Priority);
		WaveFormat wfx = new()
		{
			FormatTag = WaveFormatTag.Pcm,
			SamplesPerSecond = samplesPerSecond,
			BitsPerSample = bitsPerSample,
			Channels = channels
		};
		wfx.BlockAlign = (short)(wfx.Channels * (wfx.BitsPerSample / 8));
		wfx.AverageBytesPerSecond = wfx.SamplesPerSecond * wfx.BlockAlign;
		_soundBuffer = new SecondaryBuffer(new BufferDescription(wfx)
		{
			BufferBytes = _bufferSize * _bufferCount,
			ControlPositionNotify = true,
			GlobalFocus = true
		}, _device);
		_notify = new Notify(_soundBuffer);
		BufferPositionNotify[] array = new BufferPositionNotify[_bufferCount];
		for (int j = 0; j < array.Length; j++)
		{
			ref BufferPositionNotify reference = ref array[j];
			reference = new BufferPositionNotify();
			array[j].Offset = j * _bufferSize;
			array[j].EventNotifyHandle = _fillEvent.SafeWaitHandle.DangerousGetHandle();
		}
		_notify.SetNotificationPositions(array);
		_waveFillThread = new Thread(WaveFillThreadProc)
		{
			IsBackground = true,
			Name = "Wave fill thread",
			Priority = ThreadPriority.Highest
		};
		_waveFillThread.Start();
	}

	public void Dispose()
	{
		if (_waveFillThread == null)
		{
			return;
		}
		try
		{
			_isFinished = true;
			if (_soundBuffer != null && _soundBuffer.Status.Playing)
			{
				_soundBuffer.Stop();
			}
			_fillEvent.Set();
			_waveFillThread.Join();
			if (_soundBuffer != null)
			{
				_soundBuffer.Dispose();
			}
			if (_notify != null)
			{
				_notify.Dispose();
			}
			if (_device != null)
			{
				_device.Dispose();
			}
		}
		finally
		{
			_waveFillThread = null;
			_soundBuffer = null;
			_notify = null;
			_device = null;
		}
	}

	private unsafe void WaveFillThreadProc()
	{
		int num = -1;
		byte[] array = new byte[_bufferSize];
		fixed (byte* ptr = array)
		{
			try
			{
				_soundBuffer.Play(0, BufferPlayFlags.Looping);
				while (!_isFinished)
				{
					_fillEvent.WaitOne();
					int num2;
					for (num2 = (num + 1) % _bufferCount; num2 != _soundBuffer.PlayPosition / _bufferSize; num2 = ++num2 % _bufferCount)
					{
						OnBufferFill((IntPtr)ptr, array.Length);
						_soundBuffer.Write(_bufferSize * num2, array, LockFlag.None);
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
		lock (_playQueue.SyncRoot)
		{
			if (_playQueue.Count > 0)
			{
				array = _playQueue.Dequeue() as byte[];
			}
		}
		if (array != null)
		{
			uint* ptr = (uint*)(void*)buffer;
			fixed (byte* ptr2 = array)
			{
				uint* ptr3 = (uint*)ptr2;
				for (int i = 0; i < length / 4; i++)
				{
					ptr[i] = ptr3[i];
				}
				lastSample = ptr[length / 4 - 1];
			}
			lock (_fillQueue.SyncRoot)
			{
				_fillQueue.Enqueue(array);
				return;
			}
		}
		uint* ptr4 = (uint*)(void*)buffer;
		for (int j = 0; j < length / 4; j++)
		{
			ptr4[j] = lastSample;
		}
	}

	public byte[] LockBuffer()
	{
		byte[] result = null;
		lock (_fillQueue.SyncRoot)
		{
			if (_fillQueue.Count > 0)
			{
				return _fillQueue.Dequeue() as byte[];
			}
			return result;
		}
	}

	public void UnlockBuffer(byte[] sndbuf)
	{
		lock (_playQueue.SyncRoot)
		{
			_playQueue.Enqueue(sndbuf);
		}
	}
}
