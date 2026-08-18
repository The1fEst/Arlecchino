namespace Arlecchino;

/// <summary>One program that takes a clipboard's worth of text on its standard input.</summary>
/// <param name="FileName">The program, looked up on the path.</param>
/// <param name="Arguments">What to hand it, one argument to an element.</param>
internal sealed record ClipboardProgram(string FileName, params string[] Arguments);
