namespace Arlecchino.State;

/// <summary>
/// A store that lives as long as the screen that asked for it: navigating away disposes the scope and
/// with it the store, and navigating back builds a fresh one. Registered by
/// <c>AddGeneratedStores()</c> exactly as an <see cref="IArlecchinoStore"/> is, only scoped.
/// </summary>
public interface IArlecchinoScopedStore : IArlecchinoStore;
