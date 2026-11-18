using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace AlfaMediaPlayerWpf.Abstractions;

public sealed class AlfaMediaPlayerLibraryDto
{
	public List<MediaFile> MediaFiles { get; set; } = new List<MediaFile>();

	public MediaFile? GetLastActiveFileOrNull() => MediaFiles?.OrderByDescending(f => f.TimeUpdated).FirstOrDefault();

	public void Update(string filePath, TimeSpan position)
	{
		MediaFiles ??= [];
		var mediaFile = GetMediaFile(filePath);
		mediaFile.Position = position;
		mediaFile.TimeUpdated = DateTime.Now;

		SaveOnDisk();
	}

	public static AlfaMediaPlayerLibraryDto LoadFromDisk()
	{
		if (File.Exists(MediaLibraryPath))
		{
			var json = File.ReadAllText(MediaLibraryPath)!;
			var files = JsonSerializer.Deserialize<List<MediaFile>>(json)!;
			return new AlfaMediaPlayerLibraryDto { MediaFiles = files };
		}
		else
		{
			return new AlfaMediaPlayerLibraryDto();
		}
	}

	private void SaveOnDisk()
	{
		var files = this.MediaFiles.OrderByDescending(f => f.TimeUpdated).ToList();

		var json = JsonSerializer.Serialize(files, new JsonSerializerOptions
		{
			Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
			WriteIndented = true,
		});

		File.WriteAllText(MediaLibraryPath, json, Encoding.UTF8);
	}

	public MediaFile GetMediaFile(string filePath)
	{
		var mediaFile = MediaFiles.FirstOrDefault(f => f.FilePath == filePath);
		if (mediaFile is null)
		{
			mediaFile = new MediaFile
			{
				FilePath = filePath
			};
			MediaFiles.Add(mediaFile);
		}
		return mediaFile;
	}

	internal void Repeat(string filePath, bool? repeat)
	{
		var mf = GetMediaFile(filePath);
		mf.Repeat = repeat == true;
		SaveOnDisk();
	}

	internal void SaveTimeMarks(MediaFile mediaFile, TimePoint[] timePoints)
	{
		var mf = GetMediaFile(mediaFile.FilePath);
		mf.TimePoints ??= new List<TimePoint>();
		mf.TimePoints.Clear();
		mf.TimePoints.AddRange(timePoints);
		SaveOnDisk();
	}

	private static string MediaLibraryPath
	{
		get
		{
			var fi = new FileInfo(System.Diagnostics.Process.GetCurrentProcess().MainModule!.FileName);

			var folder = Path.Combine(fi.DirectoryName!, "AlfaMediaPlayer");
			if (!Directory.Exists(folder))
			{
				Directory.CreateDirectory(folder);
			}

			return Path.Combine(folder, "Library.json");
		}
	}
}
