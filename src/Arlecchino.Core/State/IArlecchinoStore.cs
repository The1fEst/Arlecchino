namespace Arlecchino.State;

/// <summary>
/// A holder of application state — a class of atoms that outlive the screens reading them. Marking it
/// is all the registration there is: the generator finds every store in the project and
/// <c>AddGeneratedStores()</c> puts them in the container as singletons, built from their public
/// constructor with the most parameters. Implement <see cref="IArlecchinoScopedStore"/> instead for state that
/// belongs to one screen.
/// </summary>
public interface IArlecchinoStore;
