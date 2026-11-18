namespace AlfaMediaPlayer.Domain;

public interface IMegaBusSender
{
	void Send<TMessage>(TMessage message) where TMessage : MegaBusMessageBase;
}

public delegate Task MegaBusMessageHandler(MegaBusMessageBase message);

public interface IMegaBus : IMegaBusSender
{
	void Subscribe(MegaBusMessageHandler handler, IEnumerable<Type> messageTypes);
}

public abstract record MegaBusMessageBase();

public class MegaBus : IMegaBus
{
	public static IMegaBus Instance { get; } = new MegaBus();

	private MegaBus() { }

	List<(MegaBusMessageHandler, HashSet<Type> messages)> _handlers = new List<(MegaBusMessageHandler, HashSet<Type>)>();

	public void Send<TMessage>(TMessage message) where TMessage : MegaBusMessageBase
	{
		var mt = message.GetType();

		foreach (var (handler, messages) in _handlers)
		{
			if (messages.Contains(mt))
			{
				try
				{
					Task.Factory.StartNew(() => handler?.Invoke(message));
				}
				catch
				{
					;
				}
			}
		}
	}

	public void Subscribe(MegaBusMessageHandler handler, IEnumerable<Type> messages)
	{
		_handlers.Add((handler, messages.ToHashSet()));
	}
}
