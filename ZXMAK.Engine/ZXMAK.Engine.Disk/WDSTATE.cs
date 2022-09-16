namespace ZXMAK.Engine.Disk;

public enum WDSTATE : byte
{
	S_IDLE,
	S_WAIT,
	S_DELAY_BEFORE_CMD,
	S_CMD_RW,
	S_FOUND_NEXT_ID,
	S_READ,
	S_WRSEC,
	S_WRITE,
	S_WRTRACK,
	S_WR_TRACK_DATA,
	S_TYPE1_CMD,
	S_STEP,
	S_SEEKSTART,
	S_SEEK,
	S_VERIFY,
	S_RESET
}
