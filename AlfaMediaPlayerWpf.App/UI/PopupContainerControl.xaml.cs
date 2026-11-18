using System.Windows;
using System.Windows.Controls;

namespace AlfaMediaPlayerWpf.App.UI
{
	/// <summary>
	/// Interaction logic for PopupContainerControl.xaml
	/// </summary>
	public partial class PopupContainerControl : UserControl
	{
		public static readonly DependencyProperty TitleProperty =
		DependencyProperty.Register(
			nameof(Title),
			typeof(string),
			typeof(PopupContainerControl),
			new PropertyMetadata(string.Empty));

		public string Title
		{
			get => (string)GetValue(TitleProperty);
			set => SetValue(TitleProperty, value);
		}

		public EventHandler<object>? OnClose;

		public PopupContainerControl()
		{
			InitializeComponent();
		}

		private void CloseButton_Click(object sender, RoutedEventArgs e)
		{
			OnClose?.Invoke(this, e);
			this.Hide();
		}

		private void DockPanel_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			OnClose?.Invoke(this, default);
			this.Hide();
		}
	}
}
