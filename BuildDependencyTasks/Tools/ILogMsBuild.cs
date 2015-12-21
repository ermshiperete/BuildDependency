// Copyright (c) 2015 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;
using Microsoft.Build.Framework;

namespace BuildDependency.Tasks.Tools
{
	public interface ILogMsBuild
	{
		void LogMessage(MessageImportance importance, string message, params object[] messageArgs);
	}
}

