using AlfaMediaPlayer.Domain;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace AlfaMediaPlayer.WinUI.App.UI
{
	public sealed partial class FileListControl : UserControl
	{
		IMegaBusSender _megaBus => MegaBus.Instance;

		public FileListControl()
		{
			InitializeComponent();
			FileList.SelectionChanged += FileList_SelectionChanged;
		}

		private void FileList_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (FileList.SelectedItem is MediaFile mediaFile)
			{
				_megaBus!.Send(new HistoryFileSelected(mediaFile));
			}
		}

		internal void Init(AlfaMediaPlayerLibraryDto mediaPlayerLibrary)
		{
			var items = mediaPlayerLibrary.MediaFiles.Where(mf => File.Exists(mf.FilePath)).ToArray();
			FileList.ItemsSource = items;			
		}

		public void OnKeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
		{
			switch (args.KeyboardAccelerator.Key)
			{
				case Windows.System.VirtualKey.P:
					break;
			}
		}
	}
}
