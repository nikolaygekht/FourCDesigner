namespace Gehtsoft.FourCDesigner.UITests;

/// <summary>
/// Collection definition for UI tests to ensure they run sequentially.
/// All UI tests share the same named in-memory database, so they cannot run in parallel.
/// </summary>
[CollectionDefinition("UI Tests", DisableParallelization = true)]
public class UiTestCollection
{
}
