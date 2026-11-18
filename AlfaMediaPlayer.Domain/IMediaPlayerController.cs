namespace AlfaMediaPlayer.Domain;

public interface IMediaPlayerController
{
	void OpenFile(string filePath);
	void Play();
	void Stop();
	TimeSpan Position { get; }
	string? GetCurrentFilePath();
	void SetPosition(TimeSpan position);
	TimeSpan Duration { get; }	
	void Pause();
	bool IsPlaying { get; }
	bool IsStopped { get; }
	bool IsPaused { get; }
	bool IsEnd { get; }
}

