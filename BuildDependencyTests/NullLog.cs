// Copyright (c) 2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using BuildDependency.Artifacts;

namespace BuildDependencyTests
{
	public class NullLog: ILog
	{
		public void LogError(string message, params object[] messageArgs)
		{
			Console.WriteLine(message, messageArgs);
		}

		public void LogMessage(string message, params object[] messageArgs)
		{
			Console.WriteLine(message, messageArgs);
		}

		public void LogMessage(LogMessageImportance importance, string message, params object[] messageArgs)
		{
			Console.WriteLine(message, messageArgs);
		}
	}
}