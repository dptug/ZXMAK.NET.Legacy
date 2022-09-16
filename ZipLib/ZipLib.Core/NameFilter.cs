using System;
using System.Collections;
using System.Text.RegularExpressions;

namespace ZipLib.Core;

public class NameFilter : IScanFilter
{
	private string filter_;

	private ArrayList inclusions_;

	private ArrayList exclusions_;

	public NameFilter(string filter)
	{
		filter_ = filter;
		inclusions_ = new ArrayList();
		exclusions_ = new ArrayList();
		Compile();
	}

	public static bool IsValidExpression(string expression)
	{
		bool result = true;
		try
		{
			new Regex(expression, RegexOptions.IgnoreCase | RegexOptions.Singleline);
			return result;
		}
		catch
		{
			return false;
		}
	}

	public static bool IsValidFilterExpression(string toTest)
	{
		if (toTest == null)
		{
			throw new ArgumentNullException("toTest");
		}
		bool result = true;
		try
		{
			string[] array = toTest.Split(';');
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] != null && array[i].Length > 0)
				{
					string pattern = ((array[i][0] == '+') ? array[i].Substring(1, array[i].Length - 1) : ((array[i][0] != '-') ? array[i] : array[i].Substring(1, array[i].Length - 1)));
					new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
				}
			}
			return result;
		}
		catch (Exception)
		{
			return false;
		}
	}

	public override string ToString()
	{
		return filter_;
	}

	public bool IsIncluded(string name)
	{
		bool result = false;
		if (inclusions_.Count == 0)
		{
			return true;
		}
		foreach (Regex item in inclusions_)
		{
			if (item.IsMatch(name))
			{
				return true;
			}
		}
		return result;
	}

	public bool IsExcluded(string name)
	{
		bool result = false;
		foreach (Regex item in exclusions_)
		{
			if (item.IsMatch(name))
			{
				return true;
			}
		}
		return result;
	}

	public bool IsMatch(string name)
	{
		if (IsIncluded(name))
		{
			return !IsExcluded(name);
		}
		return false;
	}

	private void Compile()
	{
		if (filter_ == null)
		{
			return;
		}
		string[] array = filter_.Split(';');
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] != null && array[i].Length > 0)
			{
				bool flag = array[i][0] != '-';
				string pattern = ((array[i][0] == '+') ? array[i].Substring(1, array[i].Length - 1) : ((array[i][0] != '-') ? array[i] : array[i].Substring(1, array[i].Length - 1)));
				if (flag)
				{
					inclusions_.Add(new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline));
				}
				else
				{
					exclusions_.Add(new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline));
				}
			}
		}
	}
}
