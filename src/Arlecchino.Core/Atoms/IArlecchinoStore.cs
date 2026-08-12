namespace Arlecchino.Atoms;

/// <summary>
/// A holder of application state: a class of atoms that outlive the screens reading them. Marking it is all
/// the registration there is, and <see cref="IArlecchinoScopedStore"/> is the one for a single screen.
/// </summary>
public interface IArlecchinoStore;
