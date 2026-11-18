using Microsoft.UI.Dispatching;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;

namespace AlfaMediaPlayer.WinUI.App;

public static class UIExtensions
{
	extension(FrameworkElement uie)
	{
		public void Hide()
		{
			uie.Visibility = Visibility.Collapsed;
		}

		public void Show()
		{
			uie.Visibility = Visibility.Visible;
		}

		public bool IsVisible => uie.Visibility == Visibility.Visible;
	}

	extension(UIElement element)
	{
		public void SetCursor(InputSystemCursorShape cursor = InputSystemCursorShape.Hand)
		{
			var p = element.GetType().GetProperty("ProtectedCursor",
				System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
			if (p is not null)
			{
				p.SetValue(element, InputSystemCursor.Create(cursor));
			}
		}
	}

	extension(DependencyObject dependencyObject)
	{
		public void DispatcherInvoke(Action action)
		{
			dependencyObject.DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.High, () =>
			{
				action();
			});
		}

		public void DispatcherInvoke(Func<Task> action)
		{
			dependencyObject.DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.High, async () =>
			{
				await action();
			});
		}

		public T? FindDescendant<T>(string name) where T : FrameworkElement
		{			
			int count = VisualTreeHelper.GetChildrenCount(dependencyObject);
			for (int i = 0; i < count; i++)
			{
				var child = VisualTreeHelper.GetChild(dependencyObject, i);

				if (child is T typedChild && typedChild.Name == name)
				{
					return typedChild;
				}

				var result = child.FindDescendant<T>(name);
				if (result != null)
				{
					return result;
				}
			}
			return null;
		}
	}

	extension(Inline source)
	{
		public Inline CloneInline()
		{
			if (source == null) return null;

			Inline clone = source switch
			{
				Run run => new Run
				{
					Text = run.Text,
					FlowDirection = run.FlowDirection
				},
				LineBreak => new LineBreak(),
				Hyperlink hyperlink => CloneSpanChildren(hyperlink, new Hyperlink
				{
					NavigateUri = hyperlink.NavigateUri,
					UnderlineStyle = hyperlink.UnderlineStyle
				}),
				Bold bold => CloneSpanChildren(bold, new Bold()),
				Italic italic => CloneSpanChildren(italic, new Italic()),
				Underline underline => CloneSpanChildren(underline, new Underline()),
				Span span => CloneSpanChildren(span, new Span()),
				_ => throw new NotSupportedException($"Cloning {source.GetType().Name} is not supported.")
			};

			// Copy base TextElement formatting properties
			clone.FontFamily = source.FontFamily;
			clone.FontSize = source.FontSize;
			clone.FontStyle = source.FontStyle;
			clone.FontWeight = source.FontWeight;
			clone.FontStretch = source.FontStretch;
			clone.Foreground = source.Foreground;
			clone.CharacterSpacing = source.CharacterSpacing;
			clone.Language = source.Language;

			return clone;
		}
	}

	private static T CloneSpanChildren<T>(Span source, T target) where T : Span
	{
		foreach (Inline child in source.Inlines)
		{
			target.Inlines.Add(child.CloneInline());
		}
		return target;
	}
}