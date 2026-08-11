namespace Arlecchino.Hosting;

/// <summary>When the framework draws its own box of keys in the corner.</summary>
public enum HintsShown
{
    /// <summary>On every frame, listing whatever can be pressed where the cursor is.</summary>
    Always,

    /// <summary>Only while a chord is half typed, listing the keys that would finish it.</summary>
    WhileWaiting,

    /// <summary>Never. The application draws the keys itself, chords included.</summary>
    Never,
}
