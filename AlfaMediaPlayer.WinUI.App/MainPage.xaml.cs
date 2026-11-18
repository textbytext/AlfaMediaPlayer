using AlfaMediaPlayer.Domain;
using AlfaMediaPlayer.WinUI.App.UI;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.Windows.Storage.Pickers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace AlfaMediaPlayer.WinUI.App
{
	/// <summary>
	/// The main content page displayed inside the application window.
	/// Add your UI logic, event handlers, and data binding here.
	/// </summary>
	public sealed partial class MainPage : Page
	{
		IMegaBus _megaBus => MegaBus.Instance;
		AlfaMediaPlayerLibraryDto _mediaPlayerLibrary;
		DateTime _nextSaveState = DateTime.Now.AddSeconds(5);
		IMediaPlayerController _player;
		FileListControl FileList => (FileListControl)FileListPopup.InnerContent;
		TimePointsControl TimePoints => (TimePointsControl)TimeMarksPopup.InnerContent;
		readonly System.Threading.Timer _timer;

		public MainPage()
		{
			InitializeComponent();

			// background services
			_megaBus.Subscribe(
				handler: MegaBusMessageHandler,
				messageTypes: new Type[] { typeof(HistoryFileSelected), typeof(PlayerIncPosition), typeof(PlayerSetRepeat), typeof(PlayerPlay), typeof(PlayerPause), typeof(PlayerSetPosition) }
				);
			_timer = new System.Threading.Timer((_) => this.DispatcherInvoke(() => OnTimer()), null, 0, (int)TimeSpan.FromSeconds(1).TotalMilliseconds);

			_player = new MediaElementWrapper(Media);
			_mediaPlayerLibrary = AlfaMediaPlayerLibraryDto.LoadFromDisk();

			// ui top menu
			OpenFileButton.Click += async (_, _) => await PlayFile();
			OpenFileListButton.Click += OpenFileListButton_Click;

			// ui player file menu
			PlayButton.Click += (_, _) => _megaBus.Send(new PlayerPlay());
			PauseButton.Click += (_, _) => _megaBus.Send(new PlayerPause());
			Add5SecButton.Click += (_, _) => _megaBus.Send(new PlayerIncPosition(TimeSpan.FromSeconds(5)));
			MediaPositionSlider.ValueChanged += MediaPositionSlider_ValueChanged1;
			Dec5SecButton.Click += (_, _) => _megaBus.Send(new PlayerIncPosition(TimeSpan.FromSeconds(-5)));
			RepeatButton.Click += (_, _) => _megaBus.Send(new PlayerSetRepeat(_player.GetCurrentFilePath()!, RepeatButton.IsChecked == true));
			TimePointsButton.Click += TimePointsButton_Click;

			ControlPanel.Hide();

#if RELEASE
			PlayMediaFile(_mediaPlayerLibrary.GetLastActiveFileOrNull());
#endif
		}

		private void KeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
		{
			if (FileListPopup.IsVisible)
			{
				FileList.OnKeyboardAccelerator_Invoked(sender, args);
				return;
			}
			else if (TimeMarksPopup.IsVisible)
			{
				TimePoints.OnKeyboardAccelerator_Invoked(sender, args);
				return;
			}

			switch (args.KeyboardAccelerator.Key)
			{
				case Windows.System.VirtualKey.P:
				case Windows.System.VirtualKey.Space:
					if (_player.IsPlaying)
					{
						_player.Pause();
					}
					else
					{
						_player.Play();
					}
					break;

				case Windows.System.VirtualKey.Left:
					_megaBus.Send(new PlayerIncPosition(TimeSpan.FromSeconds(-5)));
					break;

				case Windows.System.VirtualKey.Right:
					_megaBus.Send(new PlayerIncPosition(TimeSpan.FromSeconds(5)));
					break;

				case Windows.System.VirtualKey.H:
					OpenFileListButton_Click(default, default);
					break;

				case Windows.System.VirtualKey.O:
					this.DispatcherInvoke(() => PlayFile(null));
					break;

				case Windows.System.VirtualKey.R:
					var mf = GetCurrentMediaFile();
					_megaBus.Send(new PlayerSetRepeat(mf.FilePath, !mf.Repeat));
					break;

				case Windows.System.VirtualKey.M:
					TimePointsButton_Click(default, default);
					break;
			}
		}

		private void TimePointsButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
		{
			TimePoints.Init(GetCurrentMediaFile(), _player.GetCurrentPosition());
			TimeMarksPopup.Show();
		}

		private void OpenFileListButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
		{
			FileList.Init(_mediaPlayerLibrary);
			FileListPopup.Show();
		}

		private void MediaPositionSlider_ValueChanged1(object sender, RangeBaseValueChangedEventArgs e)
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

		private async Task PlayFile(string? filePath = null)
		{
			if (filePath is null)
			{
				var picker = new FileOpenPicker(MainWindow.WindowId);
				picker.SuggestedFolder = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments);

				var result = await picker.PickSingleFileAsync();

				if (result != null)
				{
					filePath = result.Path;
				}
			}

			if (filePath is not null)
			{
				var mediaFile = _mediaPlayerLibrary.GetMediaFile(filePath);
				PlayMediaFile(mediaFile);
			}
		}

		private void PlayMediaFile(MediaFile? mediaFile)
		{
			ControlPanel.Hide();

			if (mediaFile is null)
			{
				return;
			}

			if (mediaFile is not null)
			{
				var start = mediaFile.Position - TimeSpan.FromSeconds(5.0);
				if (start.TotalSeconds < 0.0)
				{
					start = mediaFile.Position;
				}
				_player.OpenFile(mediaFile.FilePath);
				_player.SetPosition(start);
				RepeatButton.IsChecked = mediaFile.Repeat;
			}

			SavePlayerState();

			ControlPanel.Show();
		}

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

		private void ShowPlayerFileInfo()
		{
			var filePath = _player.GetCurrentFilePath();
			if (!File.Exists(filePath))
			{
				return;
			}

			FileNameLable.Text = Path.GetFileNameWithoutExtension(filePath);

			var pos = _player.GetCurrentPosition().ToString("hh':'mm':'ss");
			var dur = _player.GetFileDuration().ToString("hh':'mm':'ss");
			FilePositionTime.Text = $"{pos} of {dur}";

			if (_player.GetFileDuration().TotalSeconds > 0)
			{
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
			}
		}

		async Task MegaBusMessageHandler(MegaBusMessageBase message)
		{
			this.DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.High, async () =>
			{
				await MegaBusMessageHandlerImpl(message);
			});
		}

		private MediaFile GetCurrentMediaFile()
		{
			var filePath = _player.GetCurrentFilePath()!;
			return _mediaPlayerLibrary.GetMediaFile(filePath);
		}

		private void OnTimer()
		{
			SavePlayerState();
			ShowPlayerFileInfo();
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
					break;
			}
		}

		
	}
}
