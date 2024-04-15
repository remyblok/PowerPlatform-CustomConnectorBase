
// Add a public constructor to Script, so that it can be created from the Unit Tests
public partial class Script
{
	public Script(IScriptContext context, CancellationToken token)
	{
		Context = context;
		CancellationToken = token;
	}
}