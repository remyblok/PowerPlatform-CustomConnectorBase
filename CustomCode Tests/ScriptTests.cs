#nullable enable
using CustomCode.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CustomCode.Tests
{
	[TestClass]
	public class ScriptTests
	{
		private static TestContext _testContext = null!;
		private static MsTestLoggerFactory _loggerFactory = null!;

		[ClassInitialize]
		public static void ClassInit(TestContext testContext)
		{
			_testContext = testContext;
			_loggerFactory = new MsTestLoggerFactory(testContext);
		}


		[ClassCleanup]
		public static void ClassCleanup()
		{
			_loggerFactory?.Dispose();
		}

		[TestMethod]
		public async Task TestOperationProcessorAsync()
		{
			//arrange
			HttpResponseMessage sendAsyncResult = new HttpResponseMessage(HttpStatusCode.OK);
			sendAsyncResult.Content = new StringContent("OK", Encoding.UTF8, "text/plain");

			IScriptContext context = new UnitTestContext(_loggerFactory, "ListOfEntities", sendAsyncResult);

			var sut = new Script.OperationProcessor(context);

			//act
			var response = await sut.Process(_testContext.CancellationTokenSource.Token).ConfigureAwait(false);

			//assert
			Assert.AreEqual(context.Request.RequestUri.ToString(), "https://example.org/test", true);

			var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

			JObject result = JObject.Parse(responseBody);
			Assert.IsTrue(result.ContainsKey("message"));
			Assert.AreEqual("OK", result["message"]!.Value<string>());
		}

	}
}
