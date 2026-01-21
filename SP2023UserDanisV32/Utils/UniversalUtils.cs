using System;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SP2023UserDanisV32.Utils
{
	public static class UniversalUtils
	{
		public static Uri UriForMedia(string path) {
			return new Uri(PathToMedia(path), UriKind.Relative);
		}

		public static ImageSource SourceFromURI(Uri uri)
		{
			try
			{
				BitmapImage img = new BitmapImage();
				img.BeginInit();
				img.CacheOption = BitmapCacheOption.OnLoad;
				img.UriSource = uri;
				img.EndInit();
				img.Freeze();
				return img;
			}
			catch (FileNotFoundException)
			{
				return null;
			}
		}

		internal static string PathToMedia(string v)
		{
			return SingletonManager.PathToMedia + v;
		}
	}
}
