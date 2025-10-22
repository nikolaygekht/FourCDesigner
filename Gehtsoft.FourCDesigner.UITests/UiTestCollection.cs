namespace Gehtsoft.FourCDesigner.UITests;

/// <summary>
/// Collection definition for UI tests to ensure they run sequentially.
/// All UI tests share the same server instance and in-memory database, so they cannot run in parallel.
/// </summary>
[CollectionDefinition("UI Tests", DisableParallelization = true)]
public class UiTestCollection : ICollectionFixture<UiTestServerFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}
