using System;

namespace ZXMAK.Engine.Disk;

[Flags]
public enum WD_STATUS
{
	WDS_BUSY = 1,
	WDS_INDEX = 2,
	WDS_DRQ = 2,
	WDS_TRK00 = 4,
	WDS_LOST = 4,
	WDS_CRCERR = 8,
	WDS_NOTFOUND = 0x10,
	WDS_SEEKERR = 0x10,
	WDS_RECORDT = 0x20,
	WDS_HEADL = 0x20,
	WDS_WRFAULT = 0x20,
	WDS_WRITEP = 0x40,
	WDS_NOTRDY = 0x80
}
