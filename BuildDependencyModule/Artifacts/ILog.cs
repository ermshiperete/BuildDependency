// Copyright (c) 2015 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;
using Microsoft.Build.Framework;

namespace BuildDependency.Artifacts
{
	public interface ILog
	{
		void LogError(string message, params object[] messageArgs);
		void LogMessage(string message, params object[] messageArgs);
		void LogMessage(MessageImportance importance, string message, params object[] messageArgs);
	}
}

