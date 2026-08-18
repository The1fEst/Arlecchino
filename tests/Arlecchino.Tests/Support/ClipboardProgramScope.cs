using System;
using System.Collections.Generic;

namespace Arlecchino.Tests.Support;

internal sealed class ClipboardProgramScope : IDisposable
{
    private readonly IReadOnlyList<ClipboardProgram> _previous = ClipboardPrograms.Programs;

    public ClipboardProgramScope(params ClipboardProgram[] programs)
    {
        ClipboardPrograms.Programs = programs;
    }

    public void Dispose() => ClipboardPrograms.Programs = _previous;
}
