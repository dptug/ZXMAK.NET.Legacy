using System;
using System.IO;
using ZipLib.Core;

namespace ZipLib.Zip;

public class ZipNameTransform : INameTransform
{
	private string trimPrefix_;

	private static readonly char[] InvalidEntryChars;

	private static readonly char[] InvalidEntryCharsRelaxed;

	public string TrimPrefix
	{
		get
		{
			return trimPrefix_;
		}
		set
		{
			trimPrefix_ = value;
		}
	}

	public ZipNameTransform()
	{
	}

	public ZipNameTransform(string trimPrefix)
	{
		if (trimPrefix != null)
		{
			trimPrefix_ = trimPrefix.ToLower();
		}
	}

	static ZipNameTransform()
	{
		char[] invalidPathChars = Path.GetInvalidPathChars();
		int num = invalidPathChars.Length + 2;
		InvalidEntryCharsRelaxed = new char[num];
		Array.Copy(invalidPathChars, 0, InvalidEntryCharsRelaxed, 0, invalidPathChars.Length);
		InvalidEntryCharsRelaxed[num - 1] = '*';
		InvalidEntryCharsRelaxed[num - 2] = '?';
		num = invalidPathChars.Length + 4;
		InvalidEntryChars = new char[num];
		Array.Copy(invalidPathChars, 0, InvalidEntryChars, 0, invalidPathChars.Length);
		InvalidEntryChars[num - 1] = ':';
		InvalidEntryChars[num - 2] = '\\';
		InvalidEntryChars[num - 3] = '*';
		InvalidEntryChars[num - 4] = '?';
	}

	public string TransformDirectory(string name)
	{
		name = TransformFile(name);
		if (name.Length > 0)
		{
			if (!name.EndsWith("/"))
			{
				name += "/";
			}
			return name;
		}
		throw new ZipException("Cannot have an empty directory name");
	}

	public string TransformFile(string name)
	{
		if (name != null)
		{
			string text = name.ToLower();
			if (trimPrefix_ != null && text.IndexOf(trimPrefix_) == 0)
			{
				name = name.Substring(trimPrefix_.Length);
			}
			if (Path.IsPathRooted(name))
			{
				name = name.Substring(Path.GetPathRoot(name).Length);
			}
			name = name.Replace("\\", "/");
			while (name.Length > 0 && name[0] == '/')
			{
				name = name.Remove(0, 1);
			}
		}
		else
		{
			name = string.Empty;
		}
		return name;
	}

	public static bool IsValidName(string name, bool relaxed)
	{
		bool flag = name != null;
		if (flag)
		{
			flag = ((!relaxed) ? (name.IndexOfAny(InvalidEntryChars) < 0 && name.IndexOf('/') != 0) : (name.IndexOfAny(InvalidEntryCharsRelaxed) < 0));
		}
		return flag;
	}

	public static bool IsValidName(string name)
	{
		return name != null && name.IndexOfAny(InvalidEntryChars) < 0 && name.IndexOf('/') != 0;
	}
}
