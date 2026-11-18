using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace AlfaMediaPlayer.WinUI.App.UI
{
	[ContentProperty(Name = nameof(InnerContent))]
	public sealed partial class PopupContainerControl : UserControl
	{
		public static readonly DependencyProperty InnerContentProperty =
			DependencyProperty.Register(
				nameof(InnerContent),
				typeof(object),
				typeof(PopupContainerControl),
				new PropertyMetadata(null));

		public object InnerContent
		{
			get => GetValue(InnerContentProperty);
			set => SetValue(InnerContentProperty, value);
		}

		RoutedEventHandler? _OnClose;

		public event RoutedEventHandler OnClose
		{
			add => _OnClose += value;
			remove => _OnClose -= value;
		}

		public string? Title { get; set; }

		public PopupContainerControl()
		{
			InitializeComponent();
			CloseButton.Click += CloseButton_Click;
			CloseButton.SetCursor();
		}

		private void CloseButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
		{
			_OnClose?.Invoke(this, e);
			this.Hide();
		}
	}
}
