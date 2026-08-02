namespace Arlecchino.Rendering;

/// <summary>
/// A terminal that holds what it was told to draw against the frame that was composed. Every frame but
/// the first is written as the difference from the one before, so a cell the difference failed to send
/// stays on screen as whatever used to be there; only the frame itself says what should have been.
///
/// The testing package is the one thing that implements this. A terminal that draws for real has no
/// screen to read back and nothing to compare, so it never does — a surface writing to one costs a null
/// check for the whole arrangement.
/// </summary>
internal interface IChecksFrames
{
    /// <summary>Called once a frame has been composed and whatever it came to has been written.</summary>
    /// <param name="surface">The surface holding the cells the frame was composed into.</param>
    void FrameBuilt(Surface surface);
}
