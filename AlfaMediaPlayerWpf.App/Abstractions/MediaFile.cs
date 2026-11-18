namespace AlfaMediaPlayerWpf.Abstractions;

public class MediaFile
{
	public required string FilePath { get; set; }
	public TimeSpan Position { get; set; } = TimeSpan.Zero;
	public DateTime TimeUpdated { get; set; } = DateTime.Now;
	public bool Repeat { get; set; } = false;
	public List<TimePoint> TimePoints { get; set; } = new List<TimePoint>();
}

public record TimePoint(string Name, TimeSpan Time);