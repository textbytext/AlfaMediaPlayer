using System.Windows;
using System;

namespace AlfaMediaPlayerWpf.App.UI;

public static class UIExtensions
{
	extension (UIElement uie)
	{
		public void Hide()
		{ 
			uie.Visibility = Visibility.Collapsed;
		}

		public void Show()
		{
			uie.Visibility = Visibility.Visible;
		}
	}

	extension(TimeSpan timeSpan)
	{
		public string To_hh_mm_ss()
		{
			return $"{(timeSpan.Days * 24 + timeSpan.Hours).ToString().PadLeft(2, '0')}:{timeSpan.Minutes.ToString().PadLeft(2, '0')}:{timeSpan.Seconds.ToString().PadLeft(2, '0')}";
		}
	}
}