using System;
using System.Collections;
using System.Text.RegularExpressions;

namespace ZipLib.Core
{
	public class NameFilter : IScanFilter
	{
		public NameFilter(string filter)
		{
			this.filter_ = filter;
			this.inclusions_ = new ArrayList();
			this.exclusions_ = new ArrayList();
			this.Compile();
		}

		public static bool IsValidExpression(string expression)
		{
			bool result = true;
			try
			{
				new Regex(expression, RegexOptions.IgnoreCase | RegexOptions.Singleline);
			}
			catch
			{
				result = false;
			}
			return result;
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
				string[] array = toTest.Split(new char[]
				{
					';'
				});
				for (int i = 0; i < array.Length; i++)
				{
					if (array[i] != null && array[i].Length > 0)
					{
						string pattern;
						if (array[i][0] == '+')
						{
							pattern = array[i].Substring(1, array[i].Length - 1);
						}
						else if (array[i][0] == '-')
						{
							pattern = array[i].Substring(1, array[i].Length - 1);
						}
						else
						{
							pattern = array[i];
						}
						new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
					}
				}
			}
			catch (Exception)
			{
				result = false;
			}
			return result;
		}

		public override string ToString()
		{
			return this.filter_;
		}

		public bool IsIncluded(string name)
		{
			bool result = false;
			if (this.inclusions_.Count == 0)
			{
				result = true;
			}
			else
			{
				foreach (object obj in this.inclusions_)
				{
					Regex regex = (Regex)obj;
					if (regex.IsMatch(name))
					{
						result = true;
						break;
					}
				}
			}
			return result;
		}

		public bool IsExcluded(string name)
		{
			bool result = false;
			foreach (object obj in this.exclusions_)
			{
				Regex regex = (Regex)obj;
				if (regex.IsMatch(name))
				{
					result = true;
					break;
				}
			}
			return result;
		}

		public bool IsMatch(string name)
		{
			return this.IsIncluded(name) && !this.IsExcluded(name);
		}

		private void Compile()
		{
			if (this.filter_ == null)
			{
				return;
			}
			string[] array = this.filter_.Split(new char[]
			{
				';'
			});
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] != null && array[i].Length > 0)
				{
					bool flag = array[i][0] != '-';
					string pattern;
					if (array[i][0] == '+')
					{
						pattern = array[i].Substring(1, array[i].Length - 1);
					}
					else if (array[i][0] == '-')
					{
						pattern = array[i].Substring(1, array[i].Length - 1);
					}
					else
					{
						pattern = array[i];
					}
					if (flag)
					{
						this.inclusions_.Add(new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline));
					}
					else
					{
						this.exclusions_.Add(new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline));
					}
				}
			}
		}

		private string filter_;

		private ArrayList inclusions_;

		private ArrayList exclusions_;
	}
}
