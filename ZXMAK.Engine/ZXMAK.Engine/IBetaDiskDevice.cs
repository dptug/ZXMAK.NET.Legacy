using ZXMAK.Engine.Disk;

namespace ZXMAK.Engine;

public interface IBetaDiskDevice
{
	WD1793 BetaDisk { get; }

	bool SEL_TRDOS { get; set; }
}
