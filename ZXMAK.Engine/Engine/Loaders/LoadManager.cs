using System;
using System.Diagnostics;
using System.IO;
using ZipLib.Zip;
using ZXMAK.Engine.Loaders.DiskSerializers;
using ZXMAK.Engine.Loaders.SnapshotSerializers;
using ZXMAK.Engine.Loaders.TapeSerializers;
using ZXMAK.Platform;

namespace ZXMAK.Engine.Loaders
{
	public class LoadManager : SerializeManager
	{
		public LoadManager(Spectrum spec)
		{
			this._spec = spec;
			base.AddSerializer(new Z80Serializer(spec));
			base.AddSerializer(new SnaSerializer(spec));
			base.AddSerializer(new SitSerializer(spec));
			base.AddSerializer(new ZxSerializer(spec));
			base.AddSerializer(new ScrSerializer(spec));
			ITapeDevice tapeDevice = spec as ITapeDevice;
			if (tapeDevice != null)
			{
				base.AddSerializer(new TapSerializer(tapeDevice.Tape));
				base.AddSerializer(new TzxSerializer(tapeDevice.Tape));
				base.AddSerializer(new CswSerializer(tapeDevice.Tape));
			}
			IBetaDiskDevice betaDiskDevice = spec as IBetaDiskDevice;
			if (betaDiskDevice != null)
			{
				base.AddSerializer(new UdiSerializer(betaDiskDevice.BetaDisk.FDD[0]));
				base.AddSerializer(new FdiSerializer(betaDiskDevice.BetaDisk.FDD[0]));
				base.AddSerializer(new Td0Serializer(betaDiskDevice.BetaDisk.FDD[0]));
				base.AddSerializer(new TrdSerializer(betaDiskDevice.BetaDisk.FDD[0]));
				base.AddSerializer(new SclSerializer(betaDiskDevice.BetaDisk.FDD[0]));
				base.AddSerializer(new HobetaSerializer(betaDiskDevice.BetaDisk.FDD[0]));
				this._diskLoaders = new DiskLoadManager[betaDiskDevice.BetaDisk.FDD.Length];
				for (int i = 0; i < this._diskLoaders.Length; i++)
				{
					this._diskLoaders[i] = new DiskLoadManager(betaDiskDevice.BetaDisk.FDD[i]);
				}
			}
		}

		public DiskLoadManager[] DiskLoaders
		{
			get
			{
				return this._diskLoaders;
			}
		}

		public void LoadROMS()
		{
			try
			{
				string name = Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName), "roms.zip");
				using (ZipFile zipFile = new ZipFile(name))
				{
					foreach (object obj in zipFile)
					{
						ZipEntry zipEntry = (ZipEntry)obj;
						if (zipEntry.IsFile && zipEntry.CanDecompress)
						{
							string value = Path.GetFileNameWithoutExtension(zipEntry.Name).ToLower();
							foreach (object obj2 in Enum.GetValues(typeof(RomName)))
							{
								RomName romName = (RomName)obj2;
								if (romName.ToString().Equals(value, StringComparison.InvariantCultureIgnoreCase))
								{
									using (Stream inputStream = zipFile.GetInputStream(zipEntry))
									{
										int num = (int)zipEntry.Size;
										if (num >= 16384)
										{
											num = 16384;
										}
										byte[] array = new byte[num];
										inputStream.Read(array, 0, num);
										this._spec.SetRomImage(romName, array, 0, num);
										break;
									}
								}
							}
						}
					}
				}
			}
			catch (FileNotFoundException)
			{
				PlatformFactory.Platform.ShowWarning("Load ROM images failed!\nFile not found: roms.zip", "Error");
			}
		}

		private Spectrum _spec;

		private DiskLoadManager[] _diskLoaders = new DiskLoadManager[0];
	}
}
