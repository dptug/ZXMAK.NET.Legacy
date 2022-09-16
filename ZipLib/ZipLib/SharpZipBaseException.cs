using System;
using System.Runtime.Serialization;

namespace ZipLib;

[Serializable]
public class SharpZipBaseException : ApplicationException
{
	protected SharpZipBaseException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public SharpZipBaseException()
	{
	}

	public SharpZipBaseException(string msg)
		: base(msg)
	{
	}

	public SharpZipBaseException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
