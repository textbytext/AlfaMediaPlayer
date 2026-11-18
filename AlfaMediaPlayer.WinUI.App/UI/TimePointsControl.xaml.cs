using AlfaMediaPlayer.Domain;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace AlfaMediaPlayer.WinUI.App.UI
{
	public sealed partial class TimePointsControl : UserControl
	{
		ObservableCollection<TimePointUI> _points = new ObservableCollection<TimePointUI>();
		MediaFile _mediaFile = new MediaFile() { FilePath = ""};
		TimeSpan _currentPosition = TimeSpan.Zero;

		public TimePointsControl()
		{
			InitializeComponent();

			TimeMarks.ItemsSource = _points;
		}

		public void OnKeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
		{
			switch (args.KeyboardAccelerator.Key)
			{
				case Windows.System.VirtualKey.P:
					break;
			}
		}

		internal void Init(MediaFile mediaFile, TimeSpan currentPosition)
		{
			_currentPosition = currentPosition;
			_mediaFile = mediaFile;

			_points.Clear();

			foreach (var point in mediaFile.TimePoints.Select(tp => new TimePointUI(tp)))
			{
				AddTimePoint(point);
			}
		}

		private void AddButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
		{
			var point = new TimePointUI
			{
				Name = $"New time point at {_currentPosition}",
				HH = (uint) _currentPosition.Hours,
				mm = (uint)_currentPosition.Minutes,
				ss = (uint)_currentPosition.Seconds,
			};
			AddTimePoint(point);
		}

		private void AddTimePoint(TimePointUI timePoint)
		{
			if (_points.Count == 0 || timePoint.TimeValue >= _points.Last().TimeValue)
			{
				_points.Add(timePoint);
				return;
			}

			if (timePoint.TimeValue <= _points.First().TimeValue)
			{
				_points.Insert(0, timePoint);
				return;
			}

			var index = _points.Count / 2;
			while (true)
			{
				if (timePoint.TimeValue == _points[index].TimeValue)
				{
					_points.Insert(index, timePoint);
					break;
				}

				var newIndex = timePoint.TimeValue < _points[index].TimeValue
					? index / 2
					: (index + _points.Count) / 2;

				if (Math.Abs(newIndex - index) == 1)
				{
					if (timePoint.TimeValue < _points[newIndex].TimeValue)
					{
						_points.Insert(newIndex, timePoint);
					}
					else
					{
						_points.Insert(newIndex + 1, timePoint);
					}
					break;
				}
				else
				{
					index = newIndex;
				}
			}
		}

		private static void Sort(ref List<TimePointUI> points)
		{
			points.Sort((TimePointUI l, TimePointUI r) =>
			{
				if (l is null && r is null) return 0;
				if (l is not null && r is null) return 1;
				if (l is null && r is not null) return -1;

				Debug.Assert(l is not null);
				Debug.Assert(r is not null);

				return l.TimeValue > r.TimeValue
					? 1
					: r.TimeValue > l.TimeValue
					? -1
					: 0;
			});
		}

		private void SaveButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
		{
			;
		}

		private void DeletePointButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
		{
			var point = (sender as AlfaButton)!.DataContext as TimePointUI;
			var index = -1;
			for (var i = 0; i < _points.Count; i++)
			{
				if (_points[i].Id == point!.Id)
				{
					index = i;
					break;
				}
			}

			if (index >= 0)
			{
				_points.RemoveAt(index);
			}
		}
	}

	internal class TimePointUI
	{
		public TimePointUI() { }

		public TimePointUI(TimePoint tp)
		{
			this.Name = tp.Name;
			this.HH = (uint) tp.Time.Hours;
			this.mm = (uint)tp.Time.Minutes;
			this.ss = (uint)tp.Time.Seconds;
		}

		public Guid Id { get; } = Guid.NewGuid();
		public string Name { get; set; } = default!;
		public uint HH { get; set; }
		public uint mm { get; set; }
		public uint ss { get; set; }

		public ulong TimeValue => HH * 60 * 60 + mm * 60 + ss;
	}
}