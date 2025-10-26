using System.Buffers;
using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using SmtpServer;
using SmtpServer.Authentication;
using SmtpServer.Protocol;
using SmtpServer.Storage;
using MimeKit;

namespace Gehtsoft.FourCDesigner.UITests.Infrastructure;

/// <summary>
/// Test SMTP server with authentication support for UI integration tests.
/// Uses SmtpServer package to run a real SMTP server on localhost.
/// </summary>
public class TestSmtpServer : IDisposable
{
    private readonly SmtpServer.SmtpServer mSmtpServer;
    private readonly ConcurrentBag<ReceivedEmail> mReceivedEmails;
    private readonly int mPort;
    private readonly string mUsername;
    private readonly string mPassword;
    private bool mIsRunning;
    private readonly object mLock = new object();
    private Task? mServerTask;
    private CancellationTokenSource? mCancellationTokenSource;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestSmtpServer"/> class.
    /// </summary>
    /// <param name="port">The port to listen on (default: 25025).</param>
    /// <param name="username">The username for authentication (default: "testuser").</param>
    /// <param name="password">The password for authentication (default: "testpass").</param>
    public TestSmtpServer(int port = 25025, string username = "testuser", string password = "testpass")
    {
        mPort = port;
        mUsername = username;
        mPassword = password;
        mReceivedEmails = new ConcurrentBag<ReceivedEmail>();

        var services = new ServiceCollection();
        services.AddSingleton<IMessageStore>(provider => new TestMessageStore(mReceivedEmails));
        services.AddSingleton<IUserAuthenticator>(provider => new TestUserAuthenticator(username, password));
        var serviceProvider = services.BuildServiceProvider();

        var options = new SmtpServerOptionsBuilder()
            .ServerName("localhost")
            .Endpoint(builder => builder
                .Port(port, false)
                .AllowUnsecureAuthentication(true))
            .Build();

        mSmtpServer = new SmtpServer.SmtpServer(options, serviceProvider);
    }

    /// <summary>
    /// Starts the SMTP server.
    /// </summary>
    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        lock (mLock)
        {
            if (mIsRunning)
                return Task.CompletedTask;
            mIsRunning = true;
        }

        // Create cancellation token source for the server
        mCancellationTokenSource = new CancellationTokenSource();

        // Start server task - SmtpServer.StartAsync runs until cancellation
        mServerTask = mSmtpServer.StartAsync(mCancellationTokenSource.Token);

        // Give server a moment to start listening
        return Task.Delay(500, cancellationToken);
    }

    /// <summary>
    /// Ensures the SMTP server is running. Starts it if not already started.
    /// Safe to call multiple times.
    /// </summary>
    public Task EnsureServerRunningAsync(CancellationToken cancellationToken = default)
    {
        lock (mLock)
        {
            if (mIsRunning)
                return Task.CompletedTask;
        }
        return StartAsync(cancellationToken);
    }

    /// <summary>
    /// Stops the SMTP server.
    /// </summary>
    public void Stop()
    {
        lock (mLock)
        {
            if (!mIsRunning)
                return;
            mIsRunning = false;
        }

        // Signal cancellation to shut down the server gracefully
        mCancellationTokenSource?.Cancel();

        // Wait for server to finish
        try
        {
            mServerTask?.Wait(TimeSpan.FromSeconds(5));
        }
        catch (AggregateException)
        {
            // Ignore cancellation exceptions
        }

        mCancellationTokenSource?.Dispose();
        mCancellationTokenSource = null;
    }

    /// <summary>
    /// Gets all received emails.
    /// </summary>
    public ReceivedEmail[] GetReceivedEmails() => mReceivedEmails.ToArray();

    /// <summary>
    /// Gets the most recent email sent to a specific recipient.
    /// </summary>
    public ReceivedEmail? GetMostRecentEmailFor(string recipientEmail)
    {
        return mReceivedEmails
            .Where(e => e.Message.To.Mailboxes.Any(mb =>
                mb.Address.Equals(recipientEmail, StringComparison.OrdinalIgnoreCase)))
            .OrderByDescending(e => e.ReceivedAt)
            .FirstOrDefault();
    }

    /// <summary>
    /// Clears all received emails.
    /// </summary>
    public void ClearEmails() => mReceivedEmails.Clear();

    /// <summary>
    /// Gets the port the server is listening on.
    /// </summary>
    public int Port => mPort;

    /// <summary>
    /// Gets the username for authentication.
    /// </summary>
    public string Username => mUsername;

    /// <summary>
    /// Gets the password for authentication.
    /// </summary>
    public string Password => mPassword;

    public void Dispose()
    {
        Stop();
    }
}

/// <summary>
/// Represents a received email with metadata.
/// </summary>
public class ReceivedEmail
{
    public MimeMessage Message { get; set; } = null!;
    public DateTime ReceivedAt { get; set; }
    public string From { get; set; } = string.Empty;
    public List<string> To { get; set; } = new();
}

/// <summary>
/// Test message store that captures emails in memory.
/// </summary>
internal class TestMessageStore : MessageStore
{
    private readonly ConcurrentBag<ReceivedEmail> mReceivedEmails;

    public TestMessageStore(ConcurrentBag<ReceivedEmail> receivedEmails)
    {
        mReceivedEmails = receivedEmails;
    }

    public override async Task<SmtpResponse> SaveAsync(
        ISessionContext context,
        IMessageTransaction transaction,
        ReadOnlySequence<byte> buffer,
        CancellationToken cancellationToken)
    {
        try
        {
            // Convert ReadOnlySequence to MemoryStream
            using var stream = new MemoryStream();
            foreach (var segment in buffer)
            {
                await stream.WriteAsync(segment, cancellationToken);
            }
            stream.Position = 0;

            // Parse as MIME message
            var message = await MimeMessage.LoadAsync(stream, cancellationToken);

            // Store the received email
            mReceivedEmails.Add(new ReceivedEmail
            {
                Message = message,
                ReceivedAt = DateTime.UtcNow,
                From = transaction.From?.ToString() ?? string.Empty,
                To = transaction.To.Select(t => t.ToString()!).ToList()
            });

            return SmtpResponse.Ok;
        }
        catch (Exception ex)
        {
            return new SmtpResponse(SmtpReplyCode.TransactionFailed, $"Failed to save message: {ex.Message}");
        }
    }
}

/// <summary>
/// Test user authenticator for SMTP authentication.
/// </summary>
internal class TestUserAuthenticator : IUserAuthenticator
{
    private readonly string mUsername;
    private readonly string mPassword;

    public TestUserAuthenticator(string username, string password)
    {
        mUsername = username;
        mPassword = password;
    }

    public Task<bool> AuthenticateAsync(
        ISessionContext context,
        string user,
        string password,
        CancellationToken cancellationToken)
    {
        // Simple authentication check
        bool isAuthenticated = user == mUsername && password == mPassword;
        return Task.FromResult(isAuthenticated);
    }
}
