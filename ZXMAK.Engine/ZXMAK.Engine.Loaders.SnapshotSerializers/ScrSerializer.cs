using System.IO;

namespace ZXMAK.Engine.Loaders.SnapshotSerializers;

public class ScrSerializer : FormatSerializer
{
	protected Spectrum _spec;

	public override string FormatGroup => "Snapshots";

	public override string FormatName => "SCR snapshot";

	public override string FormatExtension => "SCR";

	public override bool CanDeserialize => true;

	public override bool CanSerialize => true;

	public ScrSerializer(Spectrum spec)
	{
		_spec = spec;
	}

	public override void Deserialize(Stream stream)
	{
		if (_spec is ISpectrum spectrum)
		{
			stream.Read(spectrum.UlaBuffer, 0, spectrum.UlaBufferSize);
		}
	}

	public override void Serialize(Stream stream)
	{
		if (_spec is ISpectrum spectrum)
		{
			stream.Write(spectrum.UlaBuffer, 0, spectrum.UlaBufferSize);
		}
	}
}
