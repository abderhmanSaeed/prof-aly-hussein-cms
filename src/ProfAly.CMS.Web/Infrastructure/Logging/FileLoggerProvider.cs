using System.Collections.Concurrent;
using System.Globalization;
using System.Text;

namespace ProfAly.CMS.Web.Infrastructure.Logging;

/// <summary>
/// Options for <see cref="FileLoggerProvider"/>.
/// </summary>
public sealed class FileLoggerOptions
{
    /// <summary>Directory (relative to the content root) that log files are written to.</summary>
    public string Directory { get; set; } = "Logs";

    /// <summary>Minimum level a message must meet to be written to file.</summary>
    public LogLevel MinLevel { get; set; } = LogLevel.Information;

    /// <summary>
    /// When true, roll a new file each day (<c>yyyy-MM-dd.log</c>); otherwise monthly
    /// (<c>yyyy-MM.log</c>). Defaults to monthly per the production spec.
    /// </summary>
    public bool DailyRolling { get; set; }
}

/// <summary>
/// A minimal, dependency-free file logging provider for production. Writes structured,
/// human-readable lines to a monthly (or daily) rolling file under <c>Logs/</c>. Writes
/// are serialized behind a lock and flushed per entry so records survive a crash.
/// </summary>
/// <remarks>
/// Secrets are never logged by this provider: it only persists the message the
/// application already chose to emit. The application code is written to log identifiers
/// (emails on lockout, request ids) — never passwords, tokens, or connection secrets.
/// </remarks>
[ProviderAlias("File")]
public sealed class FileLoggerProvider : ILoggerProvider
{
    private readonly FileLoggerOptions _options;
    private readonly string _directory;
    private readonly object _sync = new();
    private readonly ConcurrentDictionary<string, FileLogger> _loggers = new();

    public FileLoggerProvider(string contentRoot, FileLoggerOptions options)
    {
        _options = options;
        _directory = Path.IsPathRooted(options.Directory)
            ? options.Directory
            : Path.Combine(contentRoot, options.Directory);
        System.IO.Directory.CreateDirectory(_directory);
    }

    public ILogger CreateLogger(string categoryName) =>
        _loggers.GetOrAdd(categoryName, name => new FileLogger(name, this));

    public void Dispose() => _loggers.Clear();

    internal bool IsEnabled(LogLevel level) => level >= _options.MinLevel && level != LogLevel.None;

    internal void Write(string category, LogLevel level, EventId eventId, string message, Exception? exception)
    {
        var now = DateTimeOffset.Now;
        var fileName = _options.DailyRolling
            ? now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
            : now.ToString("yyyy-MM", CultureInfo.InvariantCulture);
        var path = Path.Combine(_directory, $"{fileName}.log");

        var builder = new StringBuilder()
            .Append(now.ToString("yyyy-MM-dd HH:mm:ss.fff zzz", CultureInfo.InvariantCulture))
            .Append(" [").Append(Level(level)).Append("] ")
            .Append(category);

        if (eventId.Id != 0)
        {
            builder.Append(" (").Append(eventId.Id).Append(')');
        }

        builder.Append(" - ").Append(message);

        if (exception is not null)
        {
            builder.Append(Environment.NewLine).Append(exception);
        }

        builder.Append(Environment.NewLine);

        lock (_sync)
        {
            try
            {
                File.AppendAllText(path, builder.ToString(), Encoding.UTF8);
            }
            catch
            {
                // Logging must never throw into the request pipeline.
            }
        }
    }

    private static string Level(LogLevel level) => level switch
    {
        LogLevel.Trace => "TRACE",
        LogLevel.Debug => "DEBUG",
        LogLevel.Information => "INFO ",
        LogLevel.Warning => "WARN ",
        LogLevel.Error => "ERROR",
        LogLevel.Critical => "CRIT ",
        _ => "     ",
    };

    private sealed class FileLogger : ILogger
    {
        private readonly string _category;
        private readonly FileLoggerProvider _provider;

        public FileLogger(string category, FileLoggerProvider provider)
        {
            _category = category;
            _provider = provider;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => _provider.IsEnabled(logLevel);

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            var message = formatter(state, exception);
            if (string.IsNullOrEmpty(message) && exception is null)
            {
                return;
            }

            _provider.Write(_category, logLevel, eventId, message, exception);
        }
    }
}

/// <summary>Registration helpers for <see cref="FileLoggerProvider"/>.</summary>
public static class FileLoggerExtensions
{
    /// <summary>
    /// Adds the rolling file logger, reading optional overrides from the <c>Logging:File</c>
    /// configuration section (<c>Directory</c>, <c>MinLevel</c>, <c>DailyRolling</c>).
    /// </summary>
    public static ILoggingBuilder AddFileLogger(this ILoggingBuilder builder, IWebHostEnvironment env, IConfiguration configuration)
    {
        var options = new FileLoggerOptions();
        configuration.GetSection("Logging:File").Bind(options);
        builder.AddProvider(new FileLoggerProvider(env.ContentRootPath, options));
        return builder;
    }
}
