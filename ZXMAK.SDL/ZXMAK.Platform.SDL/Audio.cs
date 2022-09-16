using System;
using System.Collections;
using SdlDotNet.Audio;

namespace ZXMAK.Platform.SDL;

public class Audio
{
	private AudioStream _stream;

	private AudioCallback _callback;

	private int _bufferSize;

	private Queue _fillQueue;

	private Queue _playQueue;

	private uint lastSample;

	public Audio(int samplesPerSecond, short bufferSize, int bufferCount)
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Expected O, but got Unknown
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Expected O, but got Unknown
		_fillQueue = new Queue(bufferCount);
		_playQueue = new Queue(bufferCount);
		for (int i = 0; i < bufferCount; i++)
		{
			_fillQueue.Enqueue(new byte[bufferSize]);
		}
		_bufferSize = bufferSize;
		_callback = new AudioCallback(Unsigned16LittleStream);
		_stream = new AudioStream(samplesPerSecond, (AudioFormat)(-32752), (SoundChannel)2, (short)(bufferSize / 4), _callback, (object)"zx");
		_stream.Paused = false;
	}

	private unsafe void Unsigned16LittleStream(IntPtr userData, IntPtr buffer, int length)
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
