using ZXMAK.Engine.AY;

namespace ZXMAK.Engine;

public interface IAyDevice
{
	AY8910 Sound { get; }
}
