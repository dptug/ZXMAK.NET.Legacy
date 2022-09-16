using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace ZXMAK.Platform.Windows.Forms.Properties;

[DebuggerNonUserCode]
[CompilerGenerated]
[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "2.0.0.0")]
internal class Resources
{
	private static ResourceManager resourceMan;

	private static CultureInfo resourceCulture;

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static ResourceManager ResourceManager
	{
		get
		{
			if (object.ReferenceEquals(resourceMan, null))
			{
				ResourceManager resourceManager = (resourceMan = new ResourceManager("ZXMAK.Platform.Windows.Forms.Properties.Resources", typeof(Resources).Assembly));
			}
			return resourceMan;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static CultureInfo Culture
	{
		get
		{
			return resourceCulture;
		}
		set
		{
			resourceCulture = value;
		}
	}

	internal static Bitmap NextIcon
	{
		get
		{
			object @object = ResourceManager.GetObject("NextIcon", resourceCulture);
			return (Bitmap)@object;
		}
	}

	internal static Bitmap PlayIcon
	{
		get
		{
			object @object = ResourceManager.GetObject("PlayIcon", resourceCulture);
			return (Bitmap)@object;
		}
	}

	internal static Bitmap PrevIcon
	{
		get
		{
			object @object = ResourceManager.GetObject("PrevIcon", resourceCulture);
			return (Bitmap)@object;
		}
	}

	internal static Bitmap RecordIcon
	{
		get
		{
			object @object = ResourceManager.GetObject("RecordIcon", resourceCulture);
			return (Bitmap)@object;
		}
	}

	internal static Bitmap RewindIcon
	{
		get
		{
			object @object = ResourceManager.GetObject("RewindIcon", resourceCulture);
			return (Bitmap)@object;
		}
	}

	internal static Bitmap StopIcon
	{
		get
		{
			object @object = ResourceManager.GetObject("StopIcon", resourceCulture);
			return (Bitmap)@object;
		}
	}

	internal static Bitmap zxmak
	{
		get
		{
			object @object = ResourceManager.GetObject("zxmak", resourceCulture);
			return (Bitmap)@object;
		}
	}

	internal Resources()
	{
	}
}
