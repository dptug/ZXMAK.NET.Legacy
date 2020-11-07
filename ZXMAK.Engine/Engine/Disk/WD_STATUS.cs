using System;

namespace ZXMAK.Engine.Disk
{
	[Flags]
	public enum WD_STATUS
	{
		WDS_BUSY = 1,
		WDS_INDEX = 2,
		WDS_DRQ = 2,
		WDS_TRK00 = 4,
		WDS_LOST = 4,
		WDS_CRCERR = 8,
		WDS_NOTFOUND = 16,
		WDS_SEEKERR = 16,
		WDS_RECORDT = 32,
		WDS_HEADL = 32,
		WDS_WRFAULT = 32,
		WDS_WRITEP = 64,
		WDS_NOTRDY = 128
	}
}
