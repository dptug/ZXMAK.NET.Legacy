using System;
using System.IO;
using ZXMAK.Engine.Disk;
using ZXMAK.Engine.Loaders.DiskSerializers;
using ZXMAK.Platform;

namespace ZXMAK.Engine.Loaders
{
	public class DiskLoadManager : SerializeManager
	{
		public DiskLoadManager(DiskImage diskImage)
		{
			base.AddSerializer(new UdiSerializer(diskImage));
			base.AddSerializer(new FdiSerializer(diskImage));
			base.AddSerializer(new Td0Serializer(diskImage));
			base.AddSerializer(new TrdSerializer(diskImage));
			base.AddSerializer(new SclSerializer(diskImage));
			base.AddSerializer(new HobetaSerializer(diskImage));
			diskImage.SaveDisk += this.saveDisk;
		}

		private void saveDisk(DiskImage sender)
		{
			if (PlatformFactory.Platform.QueryDialog("Disk changed!\nDisk file: " + sender.FileName + "\n\nSave changes?", "Attention!", QueryButtons.YesNo) == QueryResult.Yes)
			{
				if (sender.FileName == string.Empty)
				{
					for (int i = 0; i < 10001; i++)
					{
						sender.FileName = "zxmak_image" + i.ToString() + base.GetDefaultExtension();
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
				base.SaveFileName(sender.FileName);
				PlatformFactory.Platform.ShowNotification("Disk image successfuly saved to file:\n" + sender.FileName, "Notification");
			}
		}
	}
}
