// Copyright (c) 2015 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)

using System;
using System.Collections;
using System.IO;
using System.Runtime.ConstrainedExecution;
using BuildDependency.Tasks;
using Microsoft.Build.Framework;

namespace BuildDependencyTestApp
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			var task = new Dependencies
			{
				BuildEngine = new TmpBuildEngine(MessageImportance.Normal),
				DependencyFile = Path.Combine(Path.GetTempPath(), "test.dep"),
				JobsFile = Path.Combine(Path.GetTempPath(), "test.files"),
				WorkingDir = Path.Combine(Path.GetTempPath(), "bla"),
				UseDependencyFile = true,
				KeepJobsFile = true,
				RunAsync = true,
			};
			Directory.CreateDirectory(task.WorkingDir);
			task.Execute();
		}

		class TmpBuildEngine : IBuildEngine
		{
			private MessageImportance _importance;

			public TmpBuildEngine(MessageImportance importance)
			{
				_importance = importance;
			}

			public bool BuildProjectFile(string projectFileName, string[] targetNames, IDictionary globalProperties,
				IDictionary targetOutputs)
			{
				return true;
			}

			public void LogCustomEvent(CustomBuildEventArgs e)
			{
				Console.WriteLine($"Log custom event: {e.Message}");
			}

			public void LogErrorEvent(BuildErrorEventArgs e)
			{
				Console.WriteLine($"Log error: {e.Message}");
			}

			public void LogMessageEvent(BuildMessageEventArgs e)
			{
				if ((int)e.Importance <= (int)_importance)
					Console.WriteLine($"Log message: {e.Message}");
			}

			public void LogWarningEvent(BuildWarningEventArgs e)
			{
				Console.WriteLine($"Log warning: {e.Message}");
			}

			public int ColumnNumberOfTaskNode { get; }
			public bool ContinueOnError { get; }
			public int LineNumberOfTaskNode { get; }
			public string ProjectFileOfTaskNode { get; }
		}
	}
}
