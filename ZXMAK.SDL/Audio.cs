using System;
using System.Collections;
using SdlDotNet.Audio;

namespace ZXMAK.Platform.SDL
{
	public class Audio
	{
		public Audio(int samplesPerSecond, short bufferSize, int bufferCount)
		{
			this._fillQueue = new Queue(bufferCount);
			this._playQueue = new Queue(bufferCount);
			for (int i = 0; i < bufferCount; i++)
			{
				this._fillQueue.Enqueue(new byte[(int)bufferSize]);
			}
			this._bufferSize = (int)bufferSize;
			this._callback = new AudioCallback(this.Unsigned16LittleStream);
			this._stream = new AudioStream(samplesPerSecond, -32752, 2, bufferSize / 4, this._callback, "zx");
			this._stream.Paused = false;
		}

		private unsafe void Unsigned16LittleStream(IntPtr userData, IntPtr buffer, int length)
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

		private AudioStream _stream;

		private AudioCallback _callback;

		private int _bufferSize;

		private Queue _fillQueue;

		private Queue _playQueue;

		private uint lastSample;
	}
}
