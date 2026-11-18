using AlfaMediaPlayer.Domain;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace AlfaMediaPlayerWpf.App.UI;

/// <summary>
/// Interaction logic for TimePointsControl.xaml
/// </summary>
public partial class TimePointsControl : UserControl
{
	IMegaBusSender _megaBus = MegaBus.Instance;
	ObservableCollection<TimePointUI> _points = new ObservableCollection<TimePointUI>();
	MediaFile _mediaFile = new MediaFile() { FilePath = "" };
	TimeSpan _currentPosition = TimeSpan.Zero;

	public TimePointsControl()
	{
		InitializeComponent();

		TimeMarks.ItemsSource = _points;
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

	private void AddButton_Click(object sender, System.Windows.RoutedEventArgs e)
	{
		var point = new TimePointUI
		{
			Name = $"New time point at {_currentPosition}",
			HH = (int) _currentPosition.TotalHours,
			mm = _currentPosition.Minutes,
			ss = _currentPosition.Seconds,
		};
		AddTimePoint(point);
	}

	private void SaveButton_Click(object sender, System.Windows.RoutedEventArgs e)
	{
		var points = _points.Select(p => new TimePoint(
			Name: p.Name,
			Time: new TimeSpan(p.HH, p.mm, p.ss))).ToArray();

		_megaBus.Send(new TimeMarksSave(_mediaFile, points));
	}

	private void PlayButton_Click(object sender, System.Windows.RoutedEventArgs e)
	{
		var point = (sender as Button)!.DataContext as TimePointUI;
		_megaBus.Send(new PlayerSetPosition(point!.Position));
	}

	private void DeleteButton_Click(object sender, System.Windows.RoutedEventArgs e)
	{
		var point = (sender as Button)!.DataContext as TimePointUI;
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
		this.HH = (int)tp.Time.TotalHours;
		this.mm = tp.Time.Minutes;
		this.ss = tp.Time.Seconds;
	}

	public Guid Id { get; } = Guid.NewGuid();
	public string Name { get; set; } = default!;
	public int HH { get; set; }
	public int mm { get; set; }
	public int ss { get; set; }

	public TimeSpan Position => new TimeSpan(HH, mm, ss);

	public string HHs
	{
		get => $"{HH}";
		set => HH = int.TryParse(value, out var v) ? v : HH;
	}

	public string mms
	{
		get => $"{mm}";
		set => mm = int.TryParse(value, out var v) ? v : mm;
	}

	public string sss
	{
		get => $"{ss}";
		set => ss = int.TryParse(value, out var v) ? v : ss;
	}

	public long TimeValue => HH * 60 * 60 + mm * 60 + ss;
}
