using System;
using System.Diagnostics;
using System.IO;
using ZipLib.Zip;
using ZXMAK.Engine.Loaders.DiskSerializers;
using ZXMAK.Engine.Loaders.SnapshotSerializers;
using ZXMAK.Engine.Loaders.TapeSerializers;
using ZXMAK.Platform;

namespace ZXMAK.Engine.Loaders;

public class LoadManager : SerializeManager
{
	private Spectrum _spec;

	private DiskLoadManager[] _diskLoaders = new DiskLoadManager[0];

	public DiskLoadManager[] DiskLoaders => _diskLoaders;

	public LoadManager(Spectrum spec)
	{
		_spec = spec;
		AddSerializer(new Z80Serializer(spec));
		AddSerializer(new SnaSerializer(spec));
		AddSerializer(new SitSerializer(spec));
		AddSerializer(new ZxSerializer(spec));
		AddSerializer(new ScrSerializer(spec));
		if (spec is ITapeDevice tapeDevice)
		{
			AddSerializer(new TapSerializer(tapeDevice.Tape));
			AddSerializer(new TzxSerializer(tapeDevice.Tape));
			AddSerializer(new CswSerializer(tapeDevice.Tape));
		}
		if (spec is IBetaDiskDevice betaDiskDevice)
		{
			AddSerializer(new UdiSerializer(betaDiskDevice.BetaDisk.FDD[0]));
			AddSerializer(new FdiSerializer(betaDiskDevice.BetaDisk.FDD[0]));
			AddSerializer(new Td0Serializer(betaDiskDevice.BetaDisk.FDD[0]));
			AddSerializer(new TrdSerializer(betaDiskDevice.BetaDisk.FDD[0]));
			AddSerializer(new SclSerializer(betaDiskDevice.BetaDisk.FDD[0]));
			AddSerializer(new HobetaSerializer(betaDiskDevice.BetaDisk.FDD[0]));
			_diskLoaders = new DiskLoadManager[betaDiskDevice.BetaDisk.FDD.Length];
			for (int i = 0; i < _diskLoaders.Length; i++)
			{
				_diskLoaders[i] = new DiskLoadManager(betaDiskDevice.BetaDisk.FDD[i]);
			}
		}
	}

	public void LoadROMS()
	{
		try
		{
			string name = Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName), "roms.zip");
			using ZipFile zipFile = new ZipFile(name);
			foreach (ZipEntry item in zipFile)
			{
				if (!item.IsFile || !item.CanDecompress)
				{
					continue;
				}
				string value = Path.GetFileNameWithoutExtension(item.Name).ToLower();
				foreach (RomName value2 in Enum.GetValues(typeof(RomName)))
				{
					if (!value2.ToString().Equals(value, StringComparison.InvariantCultureIgnoreCase))
					{
						continue;
					}
					using (Stream stream = zipFile.GetInputStream(item))
					{
						int num = (int)item.Size;
						if (num >= 16384)
						{
							num = 16384;
						}
						byte[] array = new byte[num];
						stream.Read(array, 0, num);
						_spec.SetRomImage(value2, array, 0, num);
					}
					break;
				}
			}
		}
		catch (FileNotFoundException)
		{
			PlatformFactory.Platform.ShowWarning("Load ROM images failed!\nFile not found: roms.zip", "Error");
		}
	}
}
