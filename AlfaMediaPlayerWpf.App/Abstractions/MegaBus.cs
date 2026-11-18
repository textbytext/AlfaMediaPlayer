using System.Windows.Threading;

namespace AlfaMediaPlayerWpf.App.Abstractions;

public interface IMegaBusSender
{
	void Send<TMessage>(TMessage message) where TMessage : MegaBusMessageBase;
}

public delegate Task MegaBusMessageHandler(MegaBusMessageBase message);

public interface IMegaBus : IMegaBusSender
{
	void Subscribe(Dispatcher dispatcher, MegaBusMessageHandler handler, IEnumerable<Type> messageTypes);
}

public closed record MegaBusMessageBase();

public class MegaBus : IMegaBus
{
	public static IMegaBus Instance { get; } = new MegaBus();

	private MegaBus() { }

	List<(Dispatcher dispatcher, MegaBusMessageHandler, HashSet<Type> messages)> _handlers = new List<(Dispatcher, MegaBusMessageHandler, HashSet<Type>)>();

	public void Send<TMessage>(TMessage message) where TMessage : MegaBusMessageBase
	{
		var mt = message.GetType();

		foreach (var (dispatcher, handler, messages) in _handlers)
		{
			if (messages.Contains(mt))
			{
				try
				{
					Task.Factory.StartNew(() => dispatcher.Invoke(() => handler?.Invoke(message)));
				}
				catch
				{
					;
				}
			}
		}
	}

	public void Subscribe(Dispatcher dispatcher, MegaBusMessageHandler handler, IEnumerable<Type> messages)
	{
		_handlers.Add((dispatcher, handler, messages.ToHashSet()));
	}
}
