namespace Gehtsoft.FourCDesigner.Tests.Logic.Email;

/// <summary>
/// Collection definition for SMTP server tests.
/// Ensures tests don't run in parallel (to avoid port conflicts).
/// </summary>
[CollectionDefinition("SmtpServer")]
public class SmtpServerCollection : ICollectionFixture<SmtpServerFixture>
{
}

/// <summary>
/// Shared fixture for SMTP server tests.
/// One instance per test collection (not per test).
/// Server is created here but started in each test to avoid async/sync deadlock.
/// </summary>
public class SmtpServerFixture : IDisposable
{
    public TestSmtpServer SmtpServer { get; }

    public SmtpServerFixture()
    {
        // Create SMTP server on unique port for this collection
        // Note: Server is NOT started here - tests will start it themselves
        SmtpServer = new TestSmtpServer(
            port: 25025,
            username: "testuser",
            password: "testpass"
        );
    }

    public void Dispose()
    {
        SmtpServer?.Dispose();
    }
}
