using System;
using System.IO;

namespace ZXMAK.Engine.Loaders.SnapshotSerializers
{
	public class ScrSerializer : FormatSerializer
	{
		public ScrSerializer(Spectrum spec)
		{
			this._spec = spec;
		}

		public override string FormatGroup
		{
			get
			{
				return "Snapshots";
			}
		}

		public override string FormatName
		{
			get
			{
				return "SCR snapshot";
			}
		}

		public override string FormatExtension
		{
			get
			{
				return "SCR";
			}
		}

		public override bool CanDeserialize
		{
			get
			{
				return true;
			}
		}

		public override bool CanSerialize
		{
			get
			{
				return true;
			}
		}

		public override void Deserialize(Stream stream)
		{
			ISpectrum spectrum = this._spec as ISpectrum;
			if (spectrum == null)
			{
				return;
			}
			stream.Read(spectrum.UlaBuffer, 0, spectrum.UlaBufferSize);
		}

		public override void Serialize(Stream stream)
		{
			ISpectrum spectrum = this._spec as ISpectrum;
			if (spectrum == null)
			{
				return;
			}
			stream.Write(spectrum.UlaBuffer, 0, spectrum.UlaBufferSize);
		}

		protected Spectrum _spec;
	}
}
