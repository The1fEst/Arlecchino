using System.Collections.Concurrent;

namespace Arlecchino.Input;

/// <summary>
/// What the terminal has said since the last frame. Reading and drawing are two loops, and the frame loop
/// drains this queue just before it draws, so one thread touches every view, widget and atom.
/// </summary>
internal sealed class PendingInput
{
    private readonly ConcurrentQueue<Event> _events = new();

    public void Add(KeyPress key) => _events.Enqueue(Event.OfKey(key));

    public void Add(MouseEvent mouse) => _events.Enqueue(Event.OfMouse(mouse));

    public void AddPaste(string text) => _events.Enqueue(Event.OfPaste(text));

    public void Drain(InputRouter router)
    {
        while (_events.TryDequeue(out var pending))
        {
            switch (pending.Kind)
            {
                case Kind.Key:
                    router.ProcessKey(pending.Key);
                    break;
                case Kind.Mouse:
                    router.ProcessMouse(pending.Mouse);
                    break;
                case Kind.Paste:
                    router.ProcessPaste(pending.Text ?? "");
                    break;
            }
        }
    }

    private enum Kind : byte
    {
        Key,
        Mouse,
        Paste,
    }

    private readonly record struct Event(Kind Kind, KeyPress Key, MouseEvent Mouse, string? Text)
    {
        public static Event OfKey(KeyPress key) => new(Kind.Key, key, default, null);

        public static Event OfMouse(MouseEvent mouse) => new(Kind.Mouse, default, mouse, null);

        public static Event OfPaste(string text) => new(Kind.Paste, default, default, text);
    }
}
