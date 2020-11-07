using System;
using System.IO;

namespace ZXMAK.Engine.Loaders
{
	public abstract class FormatSerializer
	{
		public abstract string FormatGroup { get; }

		public abstract string FormatName { get; }

		public abstract string FormatExtension { get; }

		public virtual bool CanDeserialize
		{
			get
			{
				return false;
			}
		}

		public virtual bool CanSerialize
		{
			get
			{
				return false;
			}
		}

		public virtual void Deserialize(Stream stream)
		{
			throw new NotImplementedException(base.GetType().ToString() + ".Deserialize is not implemented.");
		}

		public virtual void Serialize(Stream stream)
		{
			throw new NotImplementedException(base.GetType().ToString() + ".Serialize is not implemented.");
		}

		protected static void setUint16(byte[] buf, int offsetIndex, ushort value)
		{
			buf[offsetIndex] = (byte)value;
			buf[offsetIndex + 1] = (byte)(value >> 8);
		}

		protected static ushort getUInt16(byte[] buf, int offsetIndex)
		{
			return (ushort)((int)buf[offsetIndex] | (int)buf[offsetIndex + 1] << 8);
		}

		protected static int getInt32(byte[] buf, int offsetIndex)
		{
			return (int)buf[offsetIndex] | (int)buf[offsetIndex + 1] << 8 | (int)buf[offsetIndex + 2] << 16 | (int)buf[offsetIndex + 3] << 24;
		}

		protected static byte[] getBytes(int value)
		{
			return new byte[]
			{
				(byte)value,
				(byte)(value >> 8),
				(byte)(value >> 16),
				(byte)(value >> 24)
			};
		}
	}
}
