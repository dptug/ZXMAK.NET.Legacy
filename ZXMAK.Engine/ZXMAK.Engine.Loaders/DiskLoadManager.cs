using System.IO;
using ZXMAK.Engine.Disk;
using ZXMAK.Engine.Loaders.DiskSerializers;
using ZXMAK.Platform;

namespace ZXMAK.Engine.Loaders;

public class DiskLoadManager : SerializeManager
{
	public DiskLoadManager(DiskImage diskImage)
	{
		AddSerializer(new UdiSerializer(diskImage));
		AddSerializer(new FdiSerializer(diskImage));
		AddSerializer(new Td0Serializer(diskImage));
		AddSerializer(new TrdSerializer(diskImage));
		AddSerializer(new SclSerializer(diskImage));
		AddSerializer(new HobetaSerializer(diskImage));
		diskImage.SaveDisk += saveDisk;
	}

	private void saveDisk(DiskImage sender)
	{
		if (PlatformFactory.Platform.QueryDialog("Disk changed!\nDisk file: " + sender.FileName + "\n\nSave changes?", "Attention!", QueryButtons.YesNo) != 0)
		{
			return;
		}
		if (sender.FileName == string.Empty)
		{
			for (int i = 0; i < 10001; i++)
			{
				sender.FileName = "zxmak_image" + i + GetDefaultExtension();
				if (!File.Exists(sender.FileName))
				{
					break;
				}
				sender.FileName = string.Empty;
			}
		}
		if (sender.FileName == string.Empty)
		{
			PlatformFactory.Platform.ShowWarning("Can't save disk image!\nNo space on HDD!", "Warning");
			return;
		}
		SaveFileName(sender.FileName);
		PlatformFactory.Platform.ShowNotification("Disk image successfuly saved to file:\n" + sender.FileName, "Notification");
	}
}
