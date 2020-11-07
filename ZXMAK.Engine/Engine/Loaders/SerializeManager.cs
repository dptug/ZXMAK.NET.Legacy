using System;
using System.Collections.Generic;
using System.IO;
using ZipLib.Zip;
using ZXMAK.Platform;

namespace ZXMAK.Engine.Loaders
{
	public abstract class SerializeManager
	{
		public string GetOpenExtFilter()
		{
			string text = string.Empty;
			List<string> list = new List<string>();
			foreach (FormatSerializer formatSerializer in this._formats.Values)
			{
				if (!list.Contains(formatSerializer.FormatGroup))
				{
					list.Add(formatSerializer.FormatGroup);
				}
			}
			string text2 = string.Empty;
			foreach (string text3 in list)
			{
				string text4 = string.Empty;
				foreach (FormatSerializer formatSerializer2 in this._formats.Values)
				{
					if (formatSerializer2.FormatGroup == text3 && formatSerializer2.CanDeserialize)
					{
						if (text4.Length > 0)
						{
							text4 += ";";
						}
						if (text2.Length > 0)
						{
							text2 += ";";
						}
						if (formatSerializer2.FormatExtension == "$")
						{
							text4 += "*.!*;.$*";
							text2 += "*.!*;.$*";
						}
						else
						{
							text4 = text4 + "*." + formatSerializer2.FormatExtension.ToLower();
							text2 = text2 + "*." + formatSerializer2.FormatExtension.ToLower();
						}
					}
				}
				if (text4.Length > 0)
				{
					text4 += ";*.zip";
					string text5 = text;
					text = string.Concat(new string[]
					{
						text5,
						"|",
						text3,
						" (",
						text4,
						")|",
						text4
					});
				}
			}
			if (text2.Length > 0)
			{
				text2 += ";*.zip";
			}
			text = "All supported files|" + text2 + text;
			return text;
		}

		public string GetSaveExtFilter()
		{
			string text = string.Empty;
			List<string> list = new List<string>();
			foreach (FormatSerializer formatSerializer in this._formats.Values)
			{
				if (!list.Contains(formatSerializer.FormatGroup))
				{
					list.Add(formatSerializer.FormatGroup);
				}
			}
			foreach (string b in list)
			{
				foreach (FormatSerializer formatSerializer2 in this._formats.Values)
				{
					if (formatSerializer2.FormatGroup == b && formatSerializer2.CanSerialize)
					{
						string text2 = string.Empty;
						if (formatSerializer2.FormatExtension == "$")
						{
							text2 += "*.!*;.$*";
						}
						else
						{
							text2 = text2 + "*." + formatSerializer2.FormatExtension.ToLower();
						}
						if (text2.Length > 0)
						{
							if (text.Length > 0)
							{
								text += "|";
							}
							string text3 = text;
							text = string.Concat(new string[]
							{
								text3,
								formatSerializer2.FormatName,
								" (",
								text2,
								")|",
								text2
							});
						}
					}
				}
			}
			return text;
		}

		public string SaveFileName(string fileName)
		{
			using (FileStream fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read))
			{
				this.saveStream(fileStream, Path.GetExtension(fileName).ToUpper());
			}
			return Path.GetFileName(fileName);
		}

		public string OpenFileName(string fileName, bool wp, bool x)
		{
			string text = Path.GetExtension(fileName).ToUpper();
			if (text != ".ZIP")
			{
				using (FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					this.openStream(fileStream, text);
				}
				return Path.GetFileName(fileName);
			}
			using (ZipFile zipFile = new ZipFile(fileName))
			{
				foreach (object obj in zipFile)
				{
					ZipEntry zipEntry = (ZipEntry)obj;
					if (zipEntry.IsFile && zipEntry.CanDecompress && Path.GetExtension(zipEntry.Name).ToUpper() != ".ZIP" && this.CheckCanOpenFileName(zipEntry.Name))
					{
						using (Stream inputStream = zipFile.GetInputStream(zipEntry))
						{
							byte[] array = new byte[zipEntry.Size];
							inputStream.Read(array, 0, array.Length);
							using (MemoryStream memoryStream = new MemoryStream(array))
							{
								if (this.intCheckCanOpenFileName(zipEntry.Name))
								{
									this.openStream(memoryStream, Path.GetExtension(zipEntry.Name).ToUpper());
									return Path.Combine(Path.GetFileName(fileName), zipEntry.Name);
								}
								PlatformFactory.Platform.ShowWarning(string.Concat(new string[]
								{
									"Can't open ",
									fileName,
									"\\",
									zipEntry.Name,
									"!\n\nFile not supported!"
								}), "Error");
								return string.Empty;
							}
						}
					}
				}
			}
			PlatformFactory.Platform.ShowWarning("Can't open " + fileName + "!\n\nSupported file not found!", "Error");
			return string.Empty;
		}

		public bool CheckCanOpenFileName(string fileName)
		{
			if (Path.GetExtension(fileName).ToUpper() != ".ZIP")
			{
				return this.intCheckCanOpenFileName(fileName);
			}
			using (ZipFile zipFile = new ZipFile(fileName))
			{
				foreach (object obj in zipFile)
				{
					ZipEntry zipEntry = (ZipEntry)obj;
					if (zipEntry.IsFile && zipEntry.CanDecompress && this.intCheckCanOpenFileName(zipEntry.Name))
					{
						return true;
					}
				}
			}
			return false;
		}

		public bool CheckCanSaveFileName(string fileName)
		{
			return this.intCheckCanSaveFileName(fileName);
		}

		public string GetDefaultExtension()
		{
			foreach (FormatSerializer formatSerializer in this._formats.Values)
			{
				if (formatSerializer.CanSerialize && formatSerializer.FormatExtension != "$")
				{
					return "." + formatSerializer.FormatExtension;
				}
			}
			return string.Empty;
		}

		private void saveStream(Stream stream, string ext)
		{
			FormatSerializer serializer = this.GetSerializer(ext);
			if (serializer == null)
			{
				PlatformFactory.Platform.ShowWarning("Save " + ext + " file format not implemented!", "Warning");
				return;
			}
			serializer.Serialize(stream);
		}

		private void openStream(Stream stream, string ext)
		{
			FormatSerializer serializer = this.GetSerializer(ext);
			if (serializer == null)
			{
				PlatformFactory.Platform.ShowWarning("Open " + ext + " file format not implemented!", "Warning");
				return;
			}
			serializer.Deserialize(stream);
		}

		private bool intCheckCanOpenFileName(string fileName)
		{
			string text = Path.GetExtension(fileName).ToUpper();
			string[] openFileExtensionList = this.OpenFileExtensionList;
			int i = 0;
			while (i < openFileExtensionList.Length)
			{
				string text2 = openFileExtensionList[i];
				bool result;
				if (text == text2)
				{
					result = true;
				}
				else
				{
					if (text2.IndexOf('*') < 0 || text.Length < 2 || (!(text.Substring(0, 2) == ".!") && !(text.Substring(0, 2) == ".$")))
					{
						i++;
						continue;
					}
					result = true;
				}
				return result;
			}
			return false;
		}

		private bool intCheckCanSaveFileName(string fileName)
		{
			string text = Path.GetExtension(fileName).ToUpper();
			string[] saveFileExtensionList = this.SaveFileExtensionList;
			int i = 0;
			while (i < saveFileExtensionList.Length)
			{
				string text2 = saveFileExtensionList[i];
				bool result;
				if (text == text2)
				{
					result = true;
				}
				else
				{
					if (text2.IndexOf('*') < 0 || text.Length < 2 || (!(text.Substring(0, 2) == ".!") && !(text.Substring(0, 2) == ".$")))
					{
						i++;
						continue;
					}
					result = true;
				}
				return result;
			}
			return false;
		}

		private string[] OpenFileExtensionList
		{
			get
			{
				List<string> list = new List<string>();
				foreach (FormatSerializer formatSerializer in this._formats.Values)
				{
					if (formatSerializer.CanDeserialize)
					{
						if (formatSerializer.FormatExtension == "$")
						{
							list.Add(".!*");
							list.Add(".$*");
						}
						else
						{
							list.Add("." + formatSerializer.FormatExtension.ToUpper());
						}
					}
				}
				return list.ToArray();
			}
		}

		private string[] SaveFileExtensionList
		{
			get
			{
				List<string> list = new List<string>();
				foreach (FormatSerializer formatSerializer in this._formats.Values)
				{
					if (formatSerializer.CanSerialize)
					{
						if (formatSerializer.FormatExtension == "$")
						{
							list.Add(".!*");
							list.Add(".$*");
						}
						else
						{
							list.Add("." + formatSerializer.FormatExtension.ToUpper());
						}
					}
				}
				return list.ToArray();
			}
		}

		protected void AddSerializer(FormatSerializer serializer)
		{
			if (!this._formats.ContainsKey(serializer.FormatExtension))
			{
				this._formats.Add(serializer.FormatExtension, serializer);
			}
		}

		protected FormatSerializer GetSerializer(string ext)
		{
			FormatSerializer result = null;
			if ((ext.Length >= 2 && ext.Substring(0, 2) == ".!") || ext.Substring(0, 2) == ".$")
			{
				result = this._formats["$"];
			}
			else
			{
				if (ext.StartsWith("."))
				{
					ext = ext.Substring(1, ext.Length - 1);
				}
				if (this._formats.ContainsKey(ext))
				{
					result = this._formats[ext];
				}
			}
			return result;
		}

		private Dictionary<string, FormatSerializer> _formats = new Dictionary<string, FormatSerializer>();
	}
}
