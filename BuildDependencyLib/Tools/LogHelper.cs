// Copyright (c) 2014-2017 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)

using System;
using BuildDependency.Artifacts;

namespace BuildDependency.Tools
{
	public class LogHelper : ILog
	{
		private readonly object _syncObj = new object();

		public void LogError(string message, params object[] messageArgs)
		{
			lock (_syncObj)
			{
				Console.Write("ERROR: ");
				Console.WriteLine(message, messageArgs);
			}
		}

		public void LogMessage(string message, params object[] messageArgs)
		{
			lock (_syncObj)
			{
				Console.WriteLine(message, messageArgs);
			}
		}
	}
}