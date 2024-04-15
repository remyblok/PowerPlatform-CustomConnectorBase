#nullable enable

/// <summary>
/// Main script called from the Custom Connector
/// </summary>
public partial class Script : ScriptBase
{
	/// <summary>
	/// Handle the different Operations in the Connector. Each operation has is't own processor
	/// </summary>
	/// <returns>The resulting HTTP response send back from the Custom connctor to the client</returns>
	public override async Task<HttpResponseMessage> ExecuteAsync()
	{
		// Fix doing requests from the Custom Connector test pane
		var authHeader = Context.Request.Headers.Authorization;
		Context.Request.Headers.Clear();
		Context.Request.Headers.Authorization = authHeader;

		try
		{
			switch (Context.OperationId)
			{
				case "OperationId":
					var odataUrlEncodingProcessor = new OperationProcessor(Context);
					return await odataUrlEncodingProcessor.Process(CancellationToken).ConfigureAwait(false);

				default:
					return await Context.SendAsync(Context.Request, CancellationToken).ConfigureAwait(false);
			}
		}
		catch (Exception ex)
		{
			Context.Logger.Log(LogLevel.Critical, ex, "Error while processing Operation ID '{operationId}'", Context.OperationId);
			var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
			response.Content = new StringContent(ex.ToString());
			return response;
		}
	}

	public class OperationProcessor
	{
		private readonly IScriptContext _context;

		public OperationProcessor(IScriptContext context)
		{
			_context = context;
		}

		public virtual async Task<HttpResponseMessage> Process(CancellationToken cancellationToken)
		{
			//pre-process HTTP Request message in _context.Request
			_context.Request.RequestUri = new Uri("https://example.org/test");
			//Send request
			HttpResponseMessage response = await _context.SendAsync(_context.Request, cancellationToken).ConfigureAwait(false);
			//post-process HTTP Response message
			response.Content = ScriptBase.CreateJsonContent($@"{{""message"": ""{await response.Content.ReadAsStringAsync()}""}}");
			return response;
		}
	}
}
