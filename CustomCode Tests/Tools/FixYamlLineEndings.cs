using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CustomCode.Tests.Tools
{
	[TestClass]
	public class FixYamlLineEndings
	{

		[TestMethod]
		[Ignore]
		public void FixLineEndingsForSwagger()
		{
			const string swaggerFile = "swagger.yaml";

			var lines = File.ReadAllLines(swaggerFile);
			var newLines = new List<string>();

			for (int i = 0; i < lines.Length; i++)
			{
				var line = lines[i];
				if (line.EndsWith(">-"))
				{
					string fullLine = line.Replace(">-", "");
					line = lines[++i];
					string indent = new string(' ', line.Length - line.TrimStart().Length);

					while (line.StartsWith(indent))
					{
						fullLine += line.Trim() + " ";
						line = lines[++i];
					}

					newLines.Add(fullLine);
				}

				newLines.Add(line);
			}

			File.WriteAllLines(swaggerFile, newLines);
		}
	}
}
