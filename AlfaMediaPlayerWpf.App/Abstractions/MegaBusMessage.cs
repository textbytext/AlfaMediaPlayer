using AlfaMediaPlayerWpf.Abstractions;

namespace AlfaMediaPlayerWpf.App.Abstractions;

public sealed record HistoryFileSelected(MediaFile mediaFile) : MegaBusMessageBase;

public sealed record PlayerIncPosition(TimeSpan duration) : MegaBusMessageBase;

public sealed record PlayerSetRepeat(string filePath, bool repeat) : MegaBusMessageBase;
public sealed record PlayerPlay() : MegaBusMessageBase;
public sealed record PlayerPause() : MegaBusMessageBase;
public sealed record PlayerSetPosition(TimeSpan position) : MegaBusMessageBase;
public sealed record TimeMarksSave(MediaFile mediaFile, TimePoint[] timePoints) : MegaBusMessageBase;
