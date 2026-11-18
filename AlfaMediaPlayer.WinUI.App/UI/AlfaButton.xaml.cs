// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;

namespace AlfaMediaPlayer.WinUI.App.UI
{
	public sealed partial class AlfaButton : UserControl
	{
		TextBlock _rich = new TextBlock();
		RoutedEventHandler? _click;

		public event RoutedEventHandler Click
		{
			add => _click += value;
			remove => _click -= value;
		}

		public string? ImagePath
		{
			get => field;
			set => field = value;
		}

		public int ImageHeight { get; set; } = 20;
		public string? Title { get; set; }

		public static readonly DependencyProperty TextInlineProperty =
			DependencyProperty.Register(
				nameof(TextInline),
				typeof(InlineCollection),
				typeof(AlfaButton),
				new PropertyMetadata(null));

		public InlineCollection TextInline
		{
			get => (GetValue(TextInlineProperty) as InlineCollection) ?? _rich.Inlines;
			set => SetValue(TextInlineProperty, value);
		}

		public AlfaButton()
		{
			InitializeComponent();
			ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Hand);
			button.Loaded += Button_Loaded;
		}

		private void Button_Loaded(object sender, RoutedEventArgs e)
		{
			if (TextInline.Count > 0)
			{
				text.Inlines.Clear();
				foreach (var b in TextInline)
				{
					text.Inlines.Add(b.CloneInline());
				}
				return;
			}

			if (Title?.Length > 0)
			{
				text.Text = Title;
				return;
			}
		}		

		private void button_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e) => _click?.Invoke(this, e);
	}
}
