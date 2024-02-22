using System.Collections;
using System.Collections.Generic;
using System.Text;
using AngeliA;

namespace AngeliaEngine;

public static class EngineUtil {


	private static readonly StringBuilder CacheBuilder = new();


	public static void BuildProject (string projectFolderPath, string sdkPath, string outputFolderPath) {

		CacheBuilder.Clear();

		// SDK
		CacheBuilder.AppendWithDoubleQuotes(sdkPath);

		// Build
		CacheBuilder.Append(" build");

		// Version
		CacheBuilder.Append(" -p:Version=");
		CacheBuilder.Append(AngeliaVersionAttribute.GetVersionString(prefixV: false, lifeCycle: false));

		// Output
		CacheBuilder.Append(" -o ");
		CacheBuilder.AppendWithDoubleQuotes(outputFolderPath);

		// Release
		CacheBuilder.Append(" -c release");

		// Execute
		Util.ExecuteCommand(projectFolderPath, CacheBuilder.ToString());
		CacheBuilder.Clear();

	}


}