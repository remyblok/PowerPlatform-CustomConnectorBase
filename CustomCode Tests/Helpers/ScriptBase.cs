// Copy of official documentation: https://learn.microsoft.com/en-us/connectors/custom-connectors/write-code#definition-of-supporting-classes-and-interfaces
// Added implemenation details to ScriptBase

public abstract class ScriptBase
{
	// Context object
	public IScriptContext Context { get; set; } = null!;

	// CancellationToken for the execution
	public CancellationToken CancellationToken { get; set; }

	// Helper: Creates a StringContent object from the serialized JSON
	public static StringContent CreateJsonContent(string serializedJson)
	{
		return new StringContent(serializedJson, Encoding.UTF8, "application/json");
	}

	// Abstract method for your code
	public abstract Task<HttpResponseMessage> ExecuteAsync();
}

public interface IScriptContext
{
	// Correlation Id
	string CorrelationId { get; }

	// Connector Operation Id
	string OperationId { get; }

	// Incoming request
	HttpRequestMessage Request { get; }

	// Logger instance
	ILogger Logger { get; }

	// Used to send an HTTP request
	// Use this method to send requests instead of HttpClient.SendAsync
	Task<HttpResponseMessage> SendAsync(
		HttpRequestMessage request,
		CancellationToken cancellationToken);
}
