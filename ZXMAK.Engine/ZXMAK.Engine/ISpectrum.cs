namespace ZXMAK.Engine;

public interface ISpectrum
{
	byte PortFE { get; set; }

	byte[] UlaBuffer { get; }

	int UlaBufferSize { get; }
}
