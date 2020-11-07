using System;
using System.IO;

namespace ZipLib.Zip
{
	public abstract class BaseArchiveStorage : IArchiveStorage
	{
		public BaseArchiveStorage(FileUpdateMode updateMode)
		{
			this.updateMode_ = updateMode;
		}

		public abstract Stream GetTemporaryOutput();

		public abstract Stream ConvertTemporaryToFinal();

		public abstract Stream MakeTemporaryCopy(Stream stream);

		public abstract Stream OpenForDirectUpdate(Stream stream);

		public abstract void Dispose();

		public FileUpdateMode UpdateMode
		{
			get
			{
				return this.updateMode_;
			}
		}

		private FileUpdateMode updateMode_;
	}
}
