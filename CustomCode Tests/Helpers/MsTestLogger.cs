#nullable enable
using System.Threading.Channels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CustomCode.Tests.Helpers
{

	// Copied from https://www.jvandertil.nl/posts/2022-07-07_testingloggingmstest/
	[DebuggerStepThrough]
	internal class MsTestLogger : ILogger
	{
		private readonly ChannelWriter<string> _logs;
		private readonly string _categoryName;

		public MsTestLogger(ChannelWriter<string> logs, string categoryName)
		{
			_logs = logs;
			_categoryName = categoryName;
		}

		public IDisposable BeginScope<TState>(TState state)
			where TState : notnull
		{
			return NoopDisposable.Instance;
		}

		public bool IsEnabled(LogLevel logLevel)
			=> true;

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
		{
			WriteLog($"{logLevel}: {_categoryName} [{eventId}] {formatter(state, exception)}");

			if (exception is not null)
			{
				WriteLog(exception.ToString());
			}
		}

		private void WriteLog(string message)
		{
			bool writtenWithinTimeout = SpinWait.SpinUntil(() => _logs.TryWrite(message), TimeSpan.FromSeconds(1));

			if (!writtenWithinTimeout)
			{
				// Since we created an unbounded channel we don't expect this to fail, but if it does we want to know.
				throw new TimeoutException("Timed out while writing to log channel.");
			}
		}

		/// <summary>
		/// Creates a new logger for the given test context.
		/// </summary>
		public static DisposableMsTestLogger<T> Create<T>(TestContext context)
		{
			var logs = Channel.CreateUnbounded<string>();

			// Workaround for the AsyncLocal issue. Assumption being invocation of this constructor captures
			// the right execution context that is unaffected by code under test where we cannot assume that
			// execution context gets preserved when Log() gets invoked.
			Task.Run(async () =>
			{
				while (await logs.Reader.WaitToReadAsync())
				{
					while (logs.Reader.TryRead(out string? message))
					{
						context.WriteLine(message);
					}
				}
			});

			return new DisposableMsTestLogger<T>(logs.Writer);
		}

		private class NoopDisposable : IDisposable
		{
			public static readonly NoopDisposable Instance = new NoopDisposable();

			public void Dispose()
			{
			}
		}
	}

	[DebuggerStepThrough]
	internal class MsTestLogger<T> : MsTestLogger, ILogger<T>
	{
		public MsTestLogger(ChannelWriter<string> logs)
			: base(logs, typeof(T).Name)
		{
		}
	}

	[DebuggerStepThrough]
	internal class DisposableMsTestLogger<T> : MsTestLogger<T>, IDisposable
	{
		private readonly ChannelWriter<string> _logs;

		public DisposableMsTestLogger(ChannelWriter<string> logs)
			: base(logs)
		{
			_logs = logs;
		}

		public void Dispose()
		{
			_logs.TryComplete();
		}
	}

	[DebuggerStepThrough]
	internal class MsTestLoggerFactory : ILoggerFactory
	{
		private readonly Channel<string> _logs;
		private readonly TestContext _context;

		public MsTestLoggerFactory(TestContext context)
		{
			_context = context;

			_logs = Channel.CreateUnbounded<string>();

			// Workaround for the AsyncLocal issue. Assumption being invocation of this constructor captures
			// the right execution context that is unaffected by code under test where we cannot assume that
			// execution context gets preserved when Log() gets invoked.
			Task.Run(async () =>
			{
				while (await _logs.Reader.WaitToReadAsync())
				{
					while (_logs.Reader.TryRead(out string? message))
					{
						_context.WriteLine(message);
					}
				}
			});
		}

		public void AddProvider(ILoggerProvider provider)
		{
			throw new NotImplementedException();
		}

		public ILogger CreateLogger(string categoryName)
		{
			return new MsTestLogger(_logs.Writer, categoryName);
		}

		public void Dispose()
		{
			_logs.Writer.TryComplete();
		}
	}

}
