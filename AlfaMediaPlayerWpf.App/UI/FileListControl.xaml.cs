using AlfaMediaPlayer.Domain;
using System.IO;
using System.Windows.Controls;

namespace AlfaMediaPlayerWpf.App.UI;

/// <summary>
/// Interaction logic for FileListControl.xaml
/// </summary>
public partial class FileListControl : UserControl
{
	IMegaBusSender _megaBus => MegaBus.Instance;

	public FileListControl()
	{
		InitializeComponent();
	}

	internal void Init(AlfaMediaPlayerLibraryDto mediaPlayerLibrary)
	{
		FileList.ItemsSource = mediaPlayerLibrary.MediaFiles.Where(mf => File.Exists(mf.FilePath));
	}

	private void TextBlock_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
	{
		if (sender is TextBlock item && item.DataContext is MediaFile mediaFile)
		{
			_megaBus!.Send(new HistoryFileSelected(mediaFile));
		}
	}
}
