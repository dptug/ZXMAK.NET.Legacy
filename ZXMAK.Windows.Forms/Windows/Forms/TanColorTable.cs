using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Microsoft.Win32;

namespace ZXMAK.Windows.Forms
{
	public class TanColorTable : ProfessionalColorTable
	{
		internal Color FromKnownColor(TanColorTable.KnownColors color)
		{
			return this.ColorTable[color];
		}

		internal static void InitTanLunaColors(ref Dictionary<TanColorTable.KnownColors, Color> rgbTable)
		{
			rgbTable[TanColorTable.KnownColors.GripDark] = Color.FromArgb(193, 190, 179);
			rgbTable[TanColorTable.KnownColors.SeparatorDark] = Color.FromArgb(197, 194, 184);
			rgbTable[TanColorTable.KnownColors.MenuItemSelected] = Color.FromArgb(193, 210, 238);
			rgbTable[TanColorTable.KnownColors.ButtonPressedBorder] = Color.FromArgb(49, 106, 197);
			rgbTable[TanColorTable.KnownColors.CheckBackground] = Color.FromArgb(225, 230, 232);
			rgbTable[TanColorTable.KnownColors.MenuItemBorder] = Color.FromArgb(49, 106, 197);
			rgbTable[TanColorTable.KnownColors.CheckBackgroundMouseOver] = Color.FromArgb(49, 106, 197);
			rgbTable[TanColorTable.KnownColors.MenuItemBorderMouseOver] = Color.FromArgb(75, 75, 111);
			rgbTable[TanColorTable.KnownColors.ToolStripDropDownBackground] = Color.FromArgb(252, 252, 249);
			rgbTable[TanColorTable.KnownColors.MenuBorder] = Color.FromArgb(138, 134, 122);
			rgbTable[TanColorTable.KnownColors.SeparatorLight] = Color.FromArgb(255, 255, 255);
			rgbTable[TanColorTable.KnownColors.ToolStripBorder] = Color.FromArgb(163, 163, 124);
			rgbTable[TanColorTable.KnownColors.MenuStripGradientBegin] = Color.FromArgb(229, 229, 215);
			rgbTable[TanColorTable.KnownColors.MenuStripGradientEnd] = Color.FromArgb(244, 242, 232);
			rgbTable[TanColorTable.KnownColors.ImageMarginGradientBegin] = Color.FromArgb(254, 254, 251);
			rgbTable[TanColorTable.KnownColors.ImageMarginGradientMiddle] = Color.FromArgb(236, 231, 224);
			rgbTable[TanColorTable.KnownColors.ImageMarginGradientEnd] = Color.FromArgb(189, 189, 163);
			rgbTable[TanColorTable.KnownColors.OverflowButtonGradientBegin] = Color.FromArgb(243, 242, 240);
			rgbTable[TanColorTable.KnownColors.OverflowButtonGradientMiddle] = Color.FromArgb(226, 225, 219);
			rgbTable[TanColorTable.KnownColors.OverflowButtonGradientEnd] = Color.FromArgb(146, 146, 118);
			rgbTable[TanColorTable.KnownColors.MenuItemPressedGradientBegin] = Color.FromArgb(252, 252, 249);
			rgbTable[TanColorTable.KnownColors.MenuItemPressedGradientEnd] = Color.FromArgb(246, 244, 236);
			rgbTable[TanColorTable.KnownColors.ImageMarginRevealedGradientBegin] = Color.FromArgb(247, 246, 239);
			rgbTable[TanColorTable.KnownColors.ImageMarginRevealedGradientMiddle] = Color.FromArgb(242, 240, 228);
			rgbTable[TanColorTable.KnownColors.ImageMarginRevealedGradientEnd] = Color.FromArgb(230, 227, 210);
			rgbTable[TanColorTable.KnownColors.ButtonCheckedGradientBegin] = Color.FromArgb(225, 230, 232);
			rgbTable[TanColorTable.KnownColors.ButtonCheckedGradientMiddle] = Color.FromArgb(225, 230, 232);
			rgbTable[TanColorTable.KnownColors.ButtonCheckedGradientEnd] = Color.FromArgb(225, 230, 232);
			rgbTable[TanColorTable.KnownColors.ButtonSelectedGradientBegin] = Color.FromArgb(193, 210, 238);
			rgbTable[TanColorTable.KnownColors.ButtonSelectedGradientMiddle] = Color.FromArgb(193, 210, 238);
			rgbTable[TanColorTable.KnownColors.ButtonSelectedGradientEnd] = Color.FromArgb(193, 210, 238);
			rgbTable[TanColorTable.KnownColors.ButtonPressedGradientBegin] = Color.FromArgb(152, 181, 226);
			rgbTable[TanColorTable.KnownColors.ButtonPressedGradientMiddle] = Color.FromArgb(152, 181, 226);
			rgbTable[TanColorTable.KnownColors.ButtonPressedGradientEnd] = Color.FromArgb(152, 181, 226);
			rgbTable[TanColorTable.KnownColors.GripLight] = Color.FromArgb(255, 255, 255);
		}

		public override Color ButtonCheckedGradientBegin
		{
			get
			{
				if (!this.UseBaseColorTable)
				{
					return this.FromKnownColor(TanColorTable.KnownColors.ButtonCheckedGradientBegin);
				}
				return base.ButtonCheckedGradientBegin;
			}
		}

		public override Color ButtonCheckedGradientEnd
		{
			get
			{
				if (!this.UseBaseColorTable)
				{
					return this.FromKnownColor(TanColorTable.KnownColors.ButtonCheckedGradientEnd);
				}
				return base.ButtonCheckedGradientEnd;
			}
		}

		public override Color ButtonCheckedGradientMiddle
		{
			get
			{
				if (!this.UseBaseColorTable)
				{
					return this.FromKnownColor(TanColorTable.KnownColors.ButtonCheckedGradientMiddle);
				}
				return base.ButtonCheckedGradientMiddle;
			}
		}

		public override Color ButtonPressedBorder
		{
			get
			{
				if (!this.UseBaseColorTable)
				{
					return this.FromKnownColor(TanColorTable.KnownColors.ButtonPressedBorder);
				}
				return base.ButtonPressedBorder;
			}
		}

		public override Color ButtonPressedGradientBegin
		{
			get
			{
				if (!this.UseBaseColorTable)
				{
					return this.FromKnownColor(TanColorTable.KnownColors.ButtonPressedGradientBegin);
				}
				return base.ButtonPressedGradientBegin;
			}
		}

		public override Color ButtonPressedGradientEnd
		{
			get
			{
				if (!this.UseBaseColorTable)
				{
					return this.FromKnownColor(TanColorTable.KnownColors.ButtonPressedGradientEnd);
				}
				return base.ButtonPressedGradientEnd;
			}
		}

		public override Color ButtonPressedGradientMiddle
		{
			get
			{
				if (!this.UseBaseColorTable)
				{
					return this.FromKnownColor(TanColorTable.KnownColors.ButtonPressedGradientMiddle);
				}
				return base.ButtonPressedGradientMiddle;
			}
		}

		public override Color ButtonSelectedBorder
		{
			get
			{
				if (!this.UseBaseColorTable)
				{
					return this.FromKnownColor(TanColorTable.KnownColors.ButtonPressedBorder);
				}
				return base.ButtonSelectedBorder;
			}
		}

		public override Color ButtonSelectedGradientBegin
		{
			get
			{
				if (!this.UseBaseColorTable)
				{
					return this.FromKnownColor(TanColorTable.KnownColors.ButtonSelectedGradientBegin);
				}
				return base.ButtonSelectedGradientBegin;
			}
		}

		public override Color ButtonSelectedGradientEnd
		{
			get
			{
				if (!this.UseBaseColorTable)
				{
					return this.FromKnownColor(TanColorTable.KnownColors.ButtonSelectedGradientEnd);
				}
				return base.ButtonSelectedGradientEnd;
			}
		}

		public override Color ButtonSelectedGradientMiddle
		{
			get
			{
				if (!this.UseBaseColorTable)
				{
					return this.FromKnownColor(TanColorTable.KnownColors.ButtonSelectedGradientMiddle);
				}
				return base.ButtonSelectedGradientMiddle;
			}
		}

		public override Color CheckBackground
		{
			get
			{
				if (!this.UseBaseColorTable)
				{
					return this.FromKnownColor(TanColorTable.KnownColors.CheckBackground);
				}
				return base.CheckBackground;
			}
		}

		public override Color CheckPressedBackground
		{
			get
			{
				if (!this.UseBaseColorTable)
				{
					return this.FromKnownColor(TanColorTable.KnownColors.CheckBackgroundMouseOver);
				}
				return base.CheckPressedBackground;
			}
		}

		public override Color CheckSelectedBackground
		{
			get
			{
				if (!this.UseBaseColorTable)
				{
					return this.FromKnownColor(TanColorTable.KnownColors.CheckBackgroundMouseOver);
				}
				return base.CheckSelectedBackground;
			}
		}

		internal static string ColorScheme
		{
			get
			{
				return TanColorTable.DisplayInformation.ColorScheme;
			}
		}

		private Dictionary<TanColorTable.KnownColors, Color> ColorTable
		{
			get
			{
				if (this.tanRGB == null)
				{
					this.tanRGB = new Dictionary<TanColorTable.KnownColors, Color>(34);
					TanColorTable.InitTanLunaColors(ref this.tanRGB);
				}
				return this.tanRGB;
			}
		}

		public override Color GripDark
		{
			get
			{
				if (!this.UseBaseColorTable)
				{
					return this.FromKnownColor(TanColorTable.KnownColors.GripDark);
				}
				return base.GripDark;
			}
		}

		public override Color GripLight
		{
			get
			{
				if (!this.UseBaseColorTable)
				{
					return this.FromKnownColor(TanColorTable.KnownColors.GripLight);
				}
				return base.GripLight;
			}
		}

		public override Color ImageMarginGradientBegin
		{
			get
			{
				if (!this.UseBaseColorTable)
				{
					return this.FromKnownColor(TanColorTable.KnownColors.ImageMarginGradientBegin);
				}
				return base.ImageMarginGradientBegin;
			}
		}

		public override Color ImageMarginGradientEnd
		{
			get
			{
				if (!this.UseBaseColorTable)
				{
					return this.FromKnownColor(TanColorTable.KnownColors.ImageMarginGradientEnd);
				}
				return base.ImageMarginGradientEnd;
			}
		}

		public override Color ImageMarginGradientMiddle
		{
			get
			{
				if (!this.UseBaseColorTable)
				{
					return this.FromKnownColor(TanColorTable.KnownColors.ImageMarginGradientMiddle);
				}
				return base.ImageMarginGradientMiddle;
			}
		}

		public override Color ImageMarginRevealedGradientBegin
		{
			get
			{
				if (!this.UseBaseColorTable)
				{
					return this.FromKnownColor(TanColorTable.KnownColors.ImageMarginRevealedGradientBegin);
				}
				return base.ImageMarginRevealedGradientBegin;
			}
		}

		public override Color ImageMarginRevealedGradientEnd
		{
			get
			{
				if (!this.UseBaseColorTable)
				{
					return this.FromKnownColor(TanColorTable.KnownColors.ImageMarginRevealedGradientEnd);
				}
				return base.ImageMarginRevealedGradientEnd;
			}
		}

		public override Color ImageMarginRevealedGradientMiddle
		{
			get
			{
				if (!this.UseBaseColorTable)
				{
					return this.FromKnownColor(TanColorTable.KnownColors.ImageMarginRevealedGradientMiddle);
				}
				return base.ImageMarginRevealedGradientMiddle;
			}
		}

		public override Color MenuBorder
		{
			get
			{
				if (!this.UseBaseColorTable)
				{
					return this.FromKnownColor(TanColorTable.KnownColors.MenuBorder);
				}
				return base.MenuItemBorder;
			}
		}

		public override Color MenuItemBorder
		{
			get
			{
				if (!this.UseBaseColorTable)
				{
					return this.FromKnownColor(TanColorTable.KnownColors.MenuItemBorder);
				}
				return base.MenuItemBorder;
			}
		}

		public override Color MenuItemPressedGradientBegin
		{
			get
			{
				if (!this.UseBaseColorTable)
				{
					return this.FromKnownColor(TanColorTable.KnownColors.MenuItemPressedGradientBegin);
				}
				return base.MenuItemPressedGradientBegin;
			}
		}

		public override Color MenuItemPressedGradientEnd
		{
			get
			{
				if (!this.UseBaseColorTable)
				{
					return this.FromKnownColor(TanColorTable.KnownColors.MenuItemPressedGradientEnd);
				}
				return base.MenuItemPressedGradientEnd;
			}
		}

		public override Color MenuItemPressedGradientMiddle
		{
			get
			{
				if (!this.UseBaseColorTable)
				{
					return this.FromKnownColor(TanColorTable.KnownColors.ImageMarginRevealedGradientMiddle);
				}
				return base.MenuItemPressedGradientMiddle;
			}
		}

		public override Color MenuItemSelected
		{
			get
			{
				if (!this.UseBaseColorTable)
				{
					return this.FromKnownColor(TanColorTable.KnownColors.MenuItemSelected);
				}
				return base.MenuItemSelected;
			}
		}

		public override Color MenuItemSelectedGradientBegin
		{
			get
			{
				if (!this.UseBaseColorTable)
				{
					return this.FromKnownColor(TanColorTable.KnownColors.ButtonSelectedGradientBegin);
				}
				return base.MenuItemSelectedGradientBegin;
			}
		}

		public override Color MenuItemSelectedGradientEnd
		{
			get
			{
				if (!this.UseBaseColorTable)
				{
					return this.FromKnownColor(TanColorTable.KnownColors.ButtonSelectedGradientEnd);
				}
				return base.MenuItemSelectedGradientEnd;
			}
		}

		public override Color MenuStripGradientBegin
		{
			get
			{
				if (!this.UseBaseColorTable)
				{
					return this.FromKnownColor(TanColorTable.KnownColors.MenuStripGradientBegin);
				}
				return base.MenuStripGradientBegin;
			}
		}

		public override Color MenuStripGradientEnd
		{
			get
			{
				if (!this.UseBaseColorTable)
				{
					return this.FromKnownColor(TanColorTable.KnownColors.MenuStripGradientEnd);
				}
				return base.MenuStripGradientEnd;
			}
		}

		public override Color OverflowButtonGradientBegin
		{
			get
			{
				if (!this.UseBaseColorTable)
				{
					return this.FromKnownColor(TanColorTable.KnownColors.OverflowButtonGradientBegin);
				}
				return base.OverflowButtonGradientBegin;
			}
		}

		public override Color OverflowButtonGradientEnd
		{
			get
			{
				if (!this.UseBaseColorTable)
				{
					return this.FromKnownColor(TanColorTable.KnownColors.OverflowButtonGradientEnd);
				}
				return base.OverflowButtonGradientEnd;
			}
		}

		public override Color OverflowButtonGradientMiddle
		{
			get
			{
				if (!this.UseBaseColorTable)
				{
					return this.FromKnownColor(TanColorTable.KnownColors.OverflowButtonGradientMiddle);
				}
				return base.OverflowButtonGradientMiddle;
			}
		}

		public override Color RaftingContainerGradientBegin
		{
			get
			{
				if (!this.UseBaseColorTable)
				{
					return this.FromKnownColor(TanColorTable.KnownColors.MenuStripGradientBegin);
				}
				return base.RaftingContainerGradientBegin;
			}
		}

		public override Color RaftingContainerGradientEnd
		{
			get
			{
				if (!this.UseBaseColorTable)
				{
					return this.FromKnownColor(TanColorTable.KnownColors.MenuStripGradientEnd);
				}
				return base.RaftingContainerGradientEnd;
			}
		}

		public override Color SeparatorDark
		{
			get
			{
				if (!this.UseBaseColorTable)
				{
					return this.FromKnownColor(TanColorTable.KnownColors.SeparatorDark);
				}
				return base.SeparatorDark;
			}
		}

		public override Color SeparatorLight
		{
			get
			{
				if (!this.UseBaseColorTable)
				{
					return this.FromKnownColor(TanColorTable.KnownColors.SeparatorLight);
				}
				return base.SeparatorLight;
			}
		}

		public override Color ToolStripBorder
		{
			get
			{
				if (!this.UseBaseColorTable)
				{
					return this.FromKnownColor(TanColorTable.KnownColors.ToolStripBorder);
				}
				return base.ToolStripBorder;
			}
		}

		public override Color ToolStripDropDownBackground
		{
			get
			{
				if (!this.UseBaseColorTable)
				{
					return this.FromKnownColor(TanColorTable.KnownColors.ToolStripDropDownBackground);
				}
				return base.ToolStripDropDownBackground;
			}
		}

		public override Color ToolStripGradientBegin
		{
			get
			{
				if (!this.UseBaseColorTable)
				{
					return this.FromKnownColor(TanColorTable.KnownColors.ImageMarginGradientBegin);
				}
				return base.ToolStripGradientBegin;
			}
		}

		public override Color ToolStripGradientEnd
		{
			get
			{
				if (!this.UseBaseColorTable)
				{
					return this.FromKnownColor(TanColorTable.KnownColors.ImageMarginGradientEnd);
				}
				return base.ToolStripGradientEnd;
			}
		}

		public override Color ToolStripGradientMiddle
		{
			get
			{
				if (!this.UseBaseColorTable)
				{
					return this.FromKnownColor(TanColorTable.KnownColors.ImageMarginGradientMiddle);
				}
				return base.ToolStripGradientMiddle;
			}
		}

		private bool UseBaseColorTable
		{
			get
			{
				bool flag = !TanColorTable.DisplayInformation.IsLunaTheme || (TanColorTable.ColorScheme != "HomeStead" && TanColorTable.ColorScheme != "NormalColor");
				if (flag && this.tanRGB != null)
				{
					this.tanRGB.Clear();
					this.tanRGB = null;
				}
				return flag;
			}
		}

		private const string blueColorScheme = "NormalColor";

		private const string oliveColorScheme = "HomeStead";

		private const string silverColorScheme = "Metallic";

		private Dictionary<TanColorTable.KnownColors, Color> tanRGB;

		private static class DisplayInformation
		{
			static DisplayInformation()
			{
				SystemEvents.UserPreferenceChanged += TanColorTable.DisplayInformation.OnUserPreferenceChanged;
				TanColorTable.DisplayInformation.SetScheme();
			}

			private static void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
			{
				TanColorTable.DisplayInformation.SetScheme();
			}

			private static void SetScheme()
			{
				TanColorTable.DisplayInformation.isLunaTheme = false;
				if (!VisualStyleRenderer.IsSupported)
				{
					TanColorTable.DisplayInformation.colorScheme = null;
					return;
				}
				TanColorTable.DisplayInformation.colorScheme = VisualStyleInformation.ColorScheme;
				if (!VisualStyleInformation.IsEnabledByUser)
				{
					return;
				}
				StringBuilder stringBuilder = new StringBuilder(512);
				TanColorTable.DisplayInformation.GetCurrentThemeName(stringBuilder, stringBuilder.Capacity, null, 0, null, 0);
				string path = stringBuilder.ToString();
				TanColorTable.DisplayInformation.isLunaTheme = string.Equals("luna.msstyles", Path.GetFileName(path), StringComparison.InvariantCultureIgnoreCase);
			}

			public static string ColorScheme
			{
				get
				{
					return TanColorTable.DisplayInformation.colorScheme;
				}
			}

			internal static bool IsLunaTheme
			{
				get
				{
					return TanColorTable.DisplayInformation.isLunaTheme;
				}
			}

			[DllImport("uxtheme.dll", CharSet = CharSet.Auto)]
			public static extern int GetCurrentThemeName(StringBuilder pszThemeFileName, int dwMaxNameChars, StringBuilder pszColorBuff, int dwMaxColorChars, StringBuilder pszSizeBuff, int cchMaxSizeChars);

			private const string lunaFileName = "luna.msstyles";

			[ThreadStatic]
			private static string colorScheme;

			[ThreadStatic]
			private static bool isLunaTheme;
		}

		internal enum KnownColors
		{
			ButtonPressedBorder,
			MenuItemBorder,
			MenuItemBorderMouseOver,
			MenuItemSelected,
			CheckBackground,
			CheckBackgroundMouseOver,
			GripDark,
			GripLight,
			MenuStripGradientBegin,
			MenuStripGradientEnd,
			ImageMarginRevealedGradientBegin,
			ImageMarginRevealedGradientEnd,
			ImageMarginRevealedGradientMiddle,
			MenuItemPressedGradientBegin,
			MenuItemPressedGradientEnd,
			ButtonPressedGradientBegin,
			ButtonPressedGradientEnd,
			ButtonPressedGradientMiddle,
			ButtonSelectedGradientBegin,
			ButtonSelectedGradientEnd,
			ButtonSelectedGradientMiddle,
			OverflowButtonGradientBegin,
			OverflowButtonGradientEnd,
			OverflowButtonGradientMiddle,
			ButtonCheckedGradientBegin,
			ButtonCheckedGradientEnd,
			ButtonCheckedGradientMiddle,
			ImageMarginGradientBegin,
			ImageMarginGradientEnd,
			ImageMarginGradientMiddle,
			MenuBorder,
			ToolStripDropDownBackground,
			ToolStripBorder,
			SeparatorDark,
			SeparatorLight,
			LastKnownColor = 34
		}
	}
}
