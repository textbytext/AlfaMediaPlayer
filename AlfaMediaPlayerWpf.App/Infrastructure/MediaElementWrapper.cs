using AlfaMediaPlayer.Domain;
using System.IO;
using System.Reflection;
using System.Windows.Controls;

namespace AlfaMediaPlayerWpf.Infrastructure;

public class MediaElementWrapper : IMediaPlayerController
{
	MediaElement _player { get; }

	public MediaElementWrapper(MediaElement player)
	{
		_player = player;
	}

	public void OpenFile(string filePath)
	{
		_player.Source = new Uri(filePath);
	}

	public void Play()
	{
		if (IsEnd)
		{
			SetPosition(TimeSpan.Zero);
		}

		_player.Play();
	}

	public void Stop()
	{
		_player.Stop();
	}

	public TimeSpan Position => _player.Position;

	public TimeSpan Duration => _player.NaturalDuration.HasTimeSpan 
		? _player.NaturalDuration.TimeSpan
		: TimeSpan.FromSeconds(0);

	public string? GetCurrentFilePath()
	{
		var path = _player.Source?.LocalPath;
		return File.Exists(path) ? path : null;
	}

	public void SetPosition(TimeSpan position)
	{
		_player.Position = position;
	}

	public void Pause()
	{
		_player.Pause();
	}

	public bool IsPlaying => GetMediaState() == MediaState.Play;

	public bool IsPaused => GetMediaState() == MediaState.Pause;

	public bool IsStopped => GetMediaState() == MediaState.Stop;

	public bool IsEnd => Position == Duration;

	private MediaState? GetMediaState()
	{
		FieldInfo? hlp = _player.GetType().GetField("_helper", BindingFlags.NonPublic |BindingFlags.Instance);
		if (hlp is null)
		{
			return null;
		}
		object? helperObject = hlp.GetValue(_player);
		FieldInfo? stateField = helperObject?.GetType().GetField("_currentState", BindingFlags.NonPublic | BindingFlags.Instance);

		if (stateField is null)
		{
			return null;
		}

		return (MediaState?)stateField.GetValue(helperObject);
	}
}

