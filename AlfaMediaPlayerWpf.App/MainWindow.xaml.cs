using AlfaMediaPlayer.Domain;
using AlfaMediaPlayerWpf.App.UI;
using AlfaMediaPlayerWpf.Infrastructure;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace AlfaMediaPlayerWpf.App
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		FileListControl FileList => (FileListControl)FileListPopup.Content;
		TimePointsControl TimePoints => (TimePointsControl)TimePointsPopup.Content;
		IMegaBus _megaBus => MegaBus.Instance;
		IMediaPlayerController _player;
		AlfaMediaPlayerLibraryDto _mediaPlayerLibrary;
		OpenFileDialog _openFileDialog;
		Timer _timer;

		public MainWindow()
		{
			InitializeComponent();

			ControlPanel.Hide();
			TheEndPanel.Hide();

			_openFileDialog = new OpenFileDialog
			{
				Multiselect = false,
				//_openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
				InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
			};

			_timer = new System.Threading.Timer(mediaTimer_Tick, null, 0, (int)TimeSpan.FromSeconds(1).TotalMilliseconds);


			_player = new MediaElementWrapper(Media);
			_mediaPlayerLibrary = AlfaMediaPlayerLibraryDto.LoadFromDisk();

			OpenFileButton.Click += openFileToolStripMenuItem_Click;
			OpenFileListButton.Click += OpenFileListButton_Click;
			TimePointsButton.Click += TimePointsButton_Click;

			MediaPositionSlider.ValueChanged += MediaPositionSlider_ValueChanged;
			PlayButton.Click += (_, _) => _megaBus.Send(new PlayerPlay());
			PauseButton.Click += (_, _) => _megaBus.Send(new PlayerPause());
			RepeatButton.Click += (_, _) => _megaBus.Send(new PlayerSetRepeat(_player.GetCurrentFilePath()!, RepeatButton.IsChecked == true));
			Add5SecButton.Click += (_, _) => _megaBus.Send(new PlayerIncPosition(TimeSpan.FromSeconds(5)));
			Dec5SecButton.Click += (_, _) => _megaBus.Send(new PlayerIncPosition(TimeSpan.FromSeconds(-5)));

			_megaBus.Subscribe(MegaBusMessageHandler,
				[typeof(TimeMarksSave), typeof(HistoryFileSelected), typeof(PlayerIncPosition), typeof(PlayerSetRepeat), typeof(PlayerPlay), typeof(PlayerPause), typeof(PlayerSetPosition)]);

			this.SizeChanged += MainWindow_SizeChanged;

#if RELEASE
			PlayFile(_mediaPlayerLibrary.GetLastActiveFileOrNull()?.FilePath);
#endif
		}

		private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			TheEndText.FontSize = (int)this.ActualHeight / 2;
		}

		private void TimePointsButton_Click(object sender, RoutedEventArgs e)
		{
			var mf = GetCurrentMediaFile();
			if (mf is null)
			{
				return;
			}

			TimePoints.Init(mf, _player.GetCurrentPosition());
			TimePointsPopup.Show();
		}

		private void OpenFileListButton_Click(object sender, RoutedEventArgs e)
		{
			FileList.Init(_mediaPlayerLibrary);
			FileListPopup.Show();
		}

		private void MediaPositionSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (MediaPositionSlider.IsEnabled is false)
			{
				return;
			}

			var max = MediaPositionSlider.Maximum;
			var position = e.NewValue;
			var dur = _player.GetFileDuration();
			var start = dur * position / max;

			_megaBus.Send(new PlayerSetPosition(start));
		}

		private void openFileToolStripMenuItem_Click(object sender, RoutedEventArgs e) => PlayFile();
		private void mediaTimer_Tick(object? sender) => Dispatcher.Invoke(OnTimer);

		private void PlayFile(string? filePath = null)
		{
			if (filePath is null)
			{
				// https://wpf-tutorial.com/dialogs/the-openfiledialog/
				if (_openFileDialog.ShowDialog() is true)
				{
					filePath = _openFileDialog.FileName;
				}
			}

			if (filePath is not null)
			{
				var mediaFile = _mediaPlayerLibrary.GetMediaFile(filePath);
				PlayMediaFile(mediaFile);
			}
		}

		private void OnTimer()
		{
			SavePlayerState();
			ShowPlayerFileInfo();
		}

		private MediaFile GetCurrentMediaFile()
		{
			var filePath = _player.GetCurrentFilePath()!;
			return _mediaPlayerLibrary.GetMediaFile(filePath);
		}

		private void ShowPlayerFileInfo()
		{
			var filePath = _player.GetCurrentFilePath();
			if (!File.Exists(filePath))
			{
				return;
			}

			FileNameLable.Text = Path.GetFileNameWithoutExtension(filePath);

			var pos = _player.GetCurrentPosition().To_hh_mm_ss();
			var dur = _player.GetFileDuration().To_hh_mm_ss();
			FilePositionTime.Content = $"{pos} of {dur}";

			if (_player.GetFileDuration().TotalSeconds > 0)
			{
				TheEndPanel.Hide();

				MediaPositionSlider.IsEnabled = false;
				MediaPositionSlider.Value = MediaPositionSlider.Maximum * _player.GetCurrentPosition().TotalSeconds / _player.GetFileDuration().TotalSeconds;
				MediaPositionSlider.IsEnabled = true;
			}
			if (pos == dur)
			{
				var mf = _mediaPlayerLibrary.GetMediaFile(filePath);
				if (mf.Repeat)
				{
					_player.SetPosition(TimeSpan.Zero);
					_player.Play();
				}
				else
				{
					TheEndPanel.Show();
				}
			}
		}

		DateTime _nextSaveState = DateTime.Now.AddSeconds(5);
		private void SavePlayerState()
		{
			if (DateTime.Now < _nextSaveState)
			{
				return;
			}

			var position = _player.GetCurrentPosition();
			var filePath = _player.GetCurrentFilePath();
			if (File.Exists(filePath))
			{
				_mediaPlayerLibrary.Update(filePath, position);
			}

			_nextSaveState = DateTime.Now.AddSeconds(5);
		}

		async Task MegaBusMessageHandler(MegaBusMessageBase message)
		{
			await this.Dispatcher.InvokeAsync(() => MegaBusMessageHandlerImpl(message));
		}

		async Task MegaBusMessageHandlerImpl(MegaBusMessageBase message)
		{
			switch (message)
			{
				case HistoryFileSelected hfs:
					PlayMediaFile(hfs.mediaFile);
					FileListPopup.Hide();
					break;

				case PlayerIncPosition pip:
					_player.SetPosition(_player.GetCurrentPosition().Add(pip.duration));
					break;

				case PlayerSetRepeat psr:
					if (File.Exists(psr.filePath))
					{
						_mediaPlayerLibrary.Repeat(psr.filePath, psr.repeat);
						RepeatButton.IsChecked = psr.repeat;
					}
					break;

				case PlayerPlay ppl:
					_player.Play();
					break;

				case PlayerPause pps:
					_player.Pause();
					break;

				case PlayerSetPosition psp:
					_player.SetPosition(psp.position);
					TimePointsPopup.Hide();
					break;

				case TimeMarksSave tms:
					_mediaPlayerLibrary.SaveTimeMarks(tms.mediaFile, tms.timePoints);
					break;
			}
		}

		private void PlayMediaFile(MediaFile mediaFile)
		{
			ControlPanel.Hide();

			if (mediaFile is null)
			{
				return;
			}

			_player.OpenFile(mediaFile.FilePath);

			var start = mediaFile.Position - TimeSpan.FromSeconds(5.0);
			if (start.TotalSeconds < 0.0)
			{
				start = mediaFile.Position;
			}
			_player.SetPosition(start);
			_player.Play();
			RepeatButton.IsChecked = mediaFile.Repeat;

			SavePlayerState();

			ControlPanel.Show();
		}

		private void Grid_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if (FileListPopup.IsVisible)
			{
				//FileList.OnKeyboardAccelerator_Invoked(sender, args);
				return;
			}
			else if (TimePoints.IsVisible)
			{
				//TimePoints.OnKeyboardAccelerator_Invoked(sender, args);
				return;
			}

			switch (e.Key)
			{
				case System.Windows.Input.Key.P:
				case System.Windows.Input.Key.Space:
					if (_player.IsPlaying)
					{
						_player.Pause();
					}
					else
					{
						_player.Play();
					}
					break;

				case System.Windows.Input.Key.Left:
					_megaBus.Send(new PlayerIncPosition(TimeSpan.FromSeconds(-5)));
					break;

				case System.Windows.Input.Key.Right:
					_megaBus.Send(new PlayerIncPosition(TimeSpan.FromSeconds(5)));
					break;

				case System.Windows.Input.Key.H:
					OpenFileListButton_Click(default, default);
					break;

				case System.Windows.Input.Key.O:
					this.Dispatcher.Invoke((Delegate)(() => PlayFile(null)));
					break;

				case System.Windows.Input.Key.R:
					var mf = GetCurrentMediaFile();
					_megaBus.Send(new PlayerSetRepeat(mf.FilePath, !mf.Repeat));
					break;

				case System.Windows.Input.Key.M:
					TimePointsButton_Click(default, default);
					break;
			}
		}
	}
}