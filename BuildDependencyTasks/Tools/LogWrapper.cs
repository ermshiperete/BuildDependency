// Copyright (c) 2015 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;
using BuildDependency.Artifacts;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;

namespace BuildDependency.Tasks.Tools
{
	public class LogWrapper: ILog, ILogMsBuild
	{
		private readonly TaskLoggingHelper _taskLoggingHelper;

		public LogWrapper(TaskLoggingHelper helper)
		{
			_taskLoggingHelper = helper;
		}

		#region ILog implementation

		public void LogError(string message, params object[] messageArgs)
		{
			lock (_taskLoggingHelper)
			{
				_taskLoggingHelper.LogError(message, messageArgs);
			}
		}

		public void LogMessage(string message, params object[] messageArgs)
		{
			lock (_taskLoggingHelper)
			{
				_taskLoggingHelper.LogMessage(message, messageArgs);
			}
		}


		public void LogMessage(MessageImportance importance, string message, params object[] messageArgs)
		{
			lock (_taskLoggingHelper)
			{
				_taskLoggingHelper.LogMessage(importance, message, messageArgs);
			}
		}
		#endregion
	}
}

