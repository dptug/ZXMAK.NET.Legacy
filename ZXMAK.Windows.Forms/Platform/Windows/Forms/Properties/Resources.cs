using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace ZXMAK.Platform.Windows.Forms.Properties
{
	[DebuggerNonUserCode]
	[CompilerGenerated]
	[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "2.0.0.0")]
	internal class Resources
	{
		internal Resources()
		{
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static ResourceManager ResourceManager
		{
			get
			{
				if (object.ReferenceEquals(Resources.resourceMan, null))
				{
					ResourceManager resourceManager = new ResourceManager("ZXMAK.Platform.Windows.Forms.Properties.Resources", typeof(Resources).Assembly);
					Resources.resourceMan = resourceManager;
				}
				return Resources.resourceMan;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static CultureInfo Culture
		{
			get
			{
				return Resources.resourceCulture;
			}
			set
			{
				Resources.resourceCulture = value;
			}
		}

		internal static Bitmap NextIcon
		{
			get
			{
				object @object = Resources.ResourceManager.GetObject("NextIcon", Resources.resourceCulture);
				return (Bitmap)@object;
			}
		}

		internal static Bitmap PlayIcon
		{
			get
			{
				object @object = Resources.ResourceManager.GetObject("PlayIcon", Resources.resourceCulture);
				return (Bitmap)@object;
			}
		}

		internal static Bitmap PrevIcon
		{
			get
			{
				object @object = Resources.ResourceManager.GetObject("PrevIcon", Resources.resourceCulture);
				return (Bitmap)@object;
			}
		}

		internal static Bitmap RecordIcon
		{
			get
			{
				object @object = Resources.ResourceManager.GetObject("RecordIcon", Resources.resourceCulture);
				return (Bitmap)@object;
			}
		}

		internal static Bitmap RewindIcon
		{
			get
			{
				object @object = Resources.ResourceManager.GetObject("RewindIcon", Resources.resourceCulture);
				return (Bitmap)@object;
			}
		}

		internal static Bitmap StopIcon
		{
			get
			{
				object @object = Resources.ResourceManager.GetObject("StopIcon", Resources.resourceCulture);
				return (Bitmap)@object;
			}
		}

		internal static Bitmap zxmak
		{
			get
			{
				object @object = Resources.ResourceManager.GetObject("zxmak", Resources.resourceCulture);
				return (Bitmap)@object;
			}
		}

		private static ResourceManager resourceMan;

		private static CultureInfo resourceCulture;
	}
}
