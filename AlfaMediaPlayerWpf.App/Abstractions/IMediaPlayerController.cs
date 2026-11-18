namespace AlfaMediaPlayerWpf.Abstractions;

public interface IMediaPlayerController
{
	void OpenFile(string filePath);
	void Play();
	void Stop();
	TimeSpan GetCurrentPosition();
	string? GetCurrentFilePath();
	void SetPosition(TimeSpan position);
	TimeSpan GetFileDuration();
	void Pause();
	bool IsPlaying { get; }
	bool IsPause { get; }
	bool IsStop { get; }
}

