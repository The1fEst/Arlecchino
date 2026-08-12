namespace Arlecchino.Rendering.Terminals;

/// <summary>
/// A terminal that holds what it was told to draw against the frame that was composed, which the testing
/// package alone implements. A terminal drawing for real has no screen to read back.
/// </summary>
internal interface IChecksFrames
{
    /// <summary>Called once a frame has been composed and whatever it came to has been written.</summary>
    /// <param name="surface">The surface holding the cells the frame was composed into.</param>
    void FrameBuilt(Surface surface);
}
