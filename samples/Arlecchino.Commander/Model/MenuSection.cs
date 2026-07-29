using System.Collections.Generic;

namespace Arlecchino.Commander.Model;

public sealed record MenuSection(string Title, IReadOnlyList<string> Items);
