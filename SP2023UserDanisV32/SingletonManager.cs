using SP2023UserDanisV32.Utils;
using System;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media;

namespace SP2023UserDanisV32
{
	public static class SingletonManager
	{
		public static Frame MainFrame { get; set; }

		public static void Navigate(Page newPage)
		{
			MainFrame?.Navigate(newPage);
		}

		public static string PathToMedia = "../../../Media";

		public static ImageSource AltImage = UniversalUtils.SourceFromURI(new Uri("Resources/picture.png", UriKind.Relative));
	}
}
