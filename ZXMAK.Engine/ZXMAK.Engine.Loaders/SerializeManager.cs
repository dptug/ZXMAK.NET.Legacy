using System.Collections.Generic;
using System.IO;
using ZipLib.Zip;
using ZXMAK.Platform;

namespace ZXMAK.Engine.Loaders;

public abstract class SerializeManager
{
	private Dictionary<string, FormatSerializer> _formats = new Dictionary<string, FormatSerializer>();

	private string[] OpenFileExtensionList
	{
		get
		{
			List<string> list = new List<string>();
			foreach (FormatSerializer value in _formats.Values)
			{
				if (value.CanDeserialize)
				{
					if (value.FormatExtension == "$")
					{
						list.Add(".!*");
						list.Add(".$*");
					}
					else
					{
						list.Add("." + value.FormatExtension.ToUpper());
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
			foreach (FormatSerializer value in _formats.Values)
			{
				if (value.CanSerialize)
				{
					if (value.FormatExtension == "$")
					{
						list.Add(".!*");
						list.Add(".$*");
					}
					else
					{
						list.Add("." + value.FormatExtension.ToUpper());
					}
				}
			}
			return list.ToArray();
		}
	}

	public string GetOpenExtFilter()
	{
		string text = string.Empty;
		List<string> list = new List<string>();
		foreach (FormatSerializer value in _formats.Values)
		{
			if (!list.Contains(value.FormatGroup))
			{
				list.Add(value.FormatGroup);
			}
		}
		string text2 = string.Empty;
		foreach (string item in list)
		{
			string text3 = string.Empty;
			foreach (FormatSerializer value2 in _formats.Values)
			{
				if (value2.FormatGroup == item && value2.CanDeserialize)
				{
					if (text3.Length > 0)
					{
						text3 += ";";
					}
					if (text2.Length > 0)
					{
						text2 += ";";
					}
					if (value2.FormatExtension == "$")
					{
						text3 += "*.!*;.$*";
						text2 += "*.!*;.$*";
					}
					else
					{
						text3 = text3 + "*." + value2.FormatExtension.ToLower();
						text2 = text2 + "*." + value2.FormatExtension.ToLower();
					}
				}
			}
			if (text3.Length > 0)
			{
				text3 += ";*.zip";
				string text4 = text;
				text = text4 + "|" + item + " (" + text3 + ")|" + text3;
			}
		}
		if (text2.Length > 0)
		{
			text2 += ";*.zip";
		}
		return "All supported files|" + text2 + text;
	}

	public string GetSaveExtFilter()
	{
		string text = string.Empty;
		List<string> list = new List<string>();
		foreach (FormatSerializer value in _formats.Values)
		{
			if (!list.Contains(value.FormatGroup))
			{
				list.Add(value.FormatGroup);
			}
		}
		foreach (string item in list)
		{
			foreach (FormatSerializer value2 in _formats.Values)
			{
				if (!(value2.FormatGroup == item) || !value2.CanSerialize)
				{
					continue;
				}
				string empty = string.Empty;
				empty = ((!(value2.FormatExtension == "$")) ? (empty + "*." + value2.FormatExtension.ToLower()) : (empty + "*.!*;.$*"));
				if (empty.Length > 0)
				{
					if (text.Length > 0)
					{
						text += "|";
					}
					string text2 = text;
					text = text2 + value2.FormatName + " (" + empty + ")|" + empty;
				}
			}
		}
		return text;
	}

	public string SaveFileName(string fileName)
	{
		using (FileStream stream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read))
		{
			saveStream(stream, Path.GetExtension(fileName).ToUpper());
		}
		return Path.GetFileName(fileName);
	}

	public string OpenFileName(string fileName, bool wp, bool x)
	{
		string text = Path.GetExtension(fileName).ToUpper();
		if (text != ".ZIP")
		{
			using (FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				openStream(stream, text);
			}
			return Path.GetFileName(fileName);
		}
		using (ZipFile zipFile = new ZipFile(fileName))
		{
			foreach (ZipEntry item in zipFile)
			{
				if (!item.IsFile || !item.CanDecompress || !(Path.GetExtension(item.Name).ToUpper() != ".ZIP") || !CheckCanOpenFileName(item.Name))
				{
					continue;
				}
				using Stream stream2 = zipFile.GetInputStream(item);
				byte[] array = new byte[item.Size];
				stream2.Read(array, 0, array.Length);
				using MemoryStream stream3 = new MemoryStream(array);
				if (intCheckCanOpenFileName(item.Name))
				{
					openStream(stream3, Path.GetExtension(item.Name).ToUpper());
					return Path.Combine(Path.GetFileName(fileName), item.Name);
				}
				PlatformFactory.Platform.ShowWarning("Can't open " + fileName + "\\" + item.Name + "!\n\nFile not supported!", "Error");
				return string.Empty;
			}
		}
		PlatformFactory.Platform.ShowWarning("Can't open " + fileName + "!\n\nSupported file not found!", "Error");
		return string.Empty;
	}

	public bool CheckCanOpenFileName(string fileName)
	{
		if (Path.GetExtension(fileName).ToUpper() != ".ZIP")
		{
			return intCheckCanOpenFileName(fileName);
		}
		using (ZipFile zipFile = new ZipFile(fileName))
		{
			foreach (ZipEntry item in zipFile)
			{
				if (item.IsFile && item.CanDecompress && intCheckCanOpenFileName(item.Name))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool CheckCanSaveFileName(string fileName)
	{
		return intCheckCanSaveFileName(fileName);
	}

	public string GetDefaultExtension()
	{
		foreach (FormatSerializer value in _formats.Values)
		{
			if (value.CanSerialize && value.FormatExtension != "$")
			{
				return "." + value.FormatExtension;
			}
		}
		return string.Empty;
	}

	private void saveStream(Stream stream, string ext)
	{
		FormatSerializer serializer = GetSerializer(ext);
		if (serializer == null)
		{
			PlatformFactory.Platform.ShowWarning("Save " + ext + " file format not implemented!", "Warning");
		}
		else
		{
			serializer.Serialize(stream);
		}
	}

	private void openStream(Stream stream, string ext)
	{
		FormatSerializer serializer = GetSerializer(ext);
		if (serializer == null)
		{
			PlatformFactory.Platform.ShowWarning("Open " + ext + " file format not implemented!", "Warning");
		}
		else
		{
			serializer.Deserialize(stream);
		}
	}

	private bool intCheckCanOpenFileName(string fileName)
	{
		string text = Path.GetExtension(fileName).ToUpper();
		string[] openFileExtensionList = OpenFileExtensionList;
		foreach (string text2 in openFileExtensionList)
		{
			if (text == text2)
			{
				return true;
			}
			if (text2.IndexOf('*') >= 0 && text.Length >= 2 && (text.Substring(0, 2) == ".!" || text.Substring(0, 2) == ".$"))
			{
				return true;
			}
		}
		return false;
	}

	private bool intCheckCanSaveFileName(string fileName)
	{
		string text = Path.GetExtension(fileName).ToUpper();
		string[] saveFileExtensionList = SaveFileExtensionList;
		foreach (string text2 in saveFileExtensionList)
		{
			if (text == text2)
			{
				return true;
			}
			if (text2.IndexOf('*') >= 0 && text.Length >= 2 && (text.Substring(0, 2) == ".!" || text.Substring(0, 2) == ".$"))
			{
				return true;
			}
		}
		return false;
	}

	protected void AddSerializer(FormatSerializer serializer)
	{
		if (!_formats.ContainsKey(serializer.FormatExtension))
		{
			_formats.Add(serializer.FormatExtension, serializer);
		}
	}

	protected FormatSerializer GetSerializer(string ext)
	{
		FormatSerializer result = null;
		if ((ext.Length >= 2 && ext.Substring(0, 2) == ".!") || ext.Substring(0, 2) == ".$")
		{
			result = _formats["$"];
		}
		else
		{
			if (ext.StartsWith("."))
			{
				ext = ext.Substring(1, ext.Length - 1);
			}
			if (_formats.ContainsKey(ext))
			{
				result = _formats[ext];
			}
		}
		return result;
	}
}
