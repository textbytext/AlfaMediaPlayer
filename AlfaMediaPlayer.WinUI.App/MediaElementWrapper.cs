using AlfaMediaPlayer.Domain;
using Microsoft.UI.Xaml.Controls;
using Windows.Media.Core;
using Windows.Media.Playback;

namespace AlfaMediaPlayer.WinUI.App;

public class MediaElementWrapper : IMediaPlayerController
{
	MediaPlayerElement _media { get; }
	MediaPlayer _player => _media.MediaPlayer;

	public MediaElementWrapper(MediaPlayerElement media)
	{
		_media = media;
	}

	public void OpenFile(string filePath)
	{
		_player.Source = MediaSource.CreateFromUri(new Uri(filePath));
	}

	public void Play()
	{
		if (!HasFile) return;

		_player.Play();
	}

	public void Stop()
	{
		if (!HasFile) return;

		_player.Pause();
	}

	public bool IsPlaying => _player.CurrentState == MediaPlayerState.Playing;
	public bool IsStopped => _player.CurrentState == MediaPlayerState.Stopped;
	public bool IsPaused => _player.CurrentState == MediaPlayerState.Paused;

	public TimeSpan Position => _player.Position;

	public TimeSpan Duration => _player.NaturalDuration;

	public string? GetCurrentFilePath()
	{
		var source = _player.Source as MediaSource;

		if (source is not null
			&& File.Exists(source.Uri.LocalPath))
		{
			return source.Uri.LocalPath;
		}

		return null;
	}

	public void SetPosition(TimeSpan position)
	{
		_player.Position = position;
	}

	public void Pause()
	{
		if (!HasFile) return;

		_player.Pause();
	}

	private bool HasFile => _player.Source as MediaSource is not null;

	public bool IsEnd => Position == Duration;
}

