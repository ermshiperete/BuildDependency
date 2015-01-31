// Copyright (c) 2014 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using BuildDependency.Artifacts;
using BuildDependency.Tools;
using System.Threading.Tasks;
using System.IO;

namespace BuildDependency
{
	public class Dependencies: Microsoft.Build.Utilities.Task
	{
		private class LogHelper: ILog
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

		public Dependencies()
		{
			WorkingDir = string.Empty;
			RunAsync = true;
		}

		public string DependencyFile
		{ get; set; }

		public string JobsFile
		{ get; set; }

		[Required]
		public bool UseDependencyFile
		{ get; set; }

		public string WorkingDir
		{ get; set; }

		public bool RunAsync
		{ get; set; }

		/// <summary>
		/// Only relevant when <see cref="UseDependencyFile"/> is <c>true</c>.
		/// If <c>true</c> don't delete the generated JobsFile after finishing (and re-use
		/// on the next run); otherwise delete the generated JobsFile at the end of the task
		/// (and re-generate on the next build).
		/// </summary>
		/// <value><c>true</c> if keep jobs file; otherwise, <c>false</c>.</value>
		public bool KeepJobsFile
		{ get; set; }

		public override bool Execute()
		{
			ILog logHelper;
			if (Log != null)
				logHelper = new LogWrapper(Log);
			else
				logHelper = new LogHelper();

			if (UseDependencyFile && string.IsNullOrEmpty(DependencyFile))
			{
				logHelper.LogError("DependencyFile is not specified, but I was told to use it.");
				return false;
			}
			if (UseDependencyFile && !File.Exists(DependencyFile))
			{
				logHelper.LogError("Can't find DependencyFile {0}", DependencyFile);
				return false;
			}
			if (!UseDependencyFile && string.IsNullOrEmpty(JobsFile))
			{
				logHelper.LogError("JobsFile is not specified, but I was told to use it.");
				return false;
			}
			if (!UseDependencyFile && !File.Exists(JobsFile))
			{
				logHelper.LogError("Can't find JobsFile {0}", JobsFile);
				return false;
			}

			if (string.IsNullOrEmpty(JobsFile))
				JobsFile = Path.ChangeExtension(DependencyFile, ".files");

			if ((UseDependencyFile && !(KeepJobsFile && File.Exists(JobsFile))) ||
				!File.Exists(JobsFile))
			{
				var artifactTemplates = BuildDependency.DependencyFile.LoadFile(DependencyFile);
				BuildDependency.JobsFile.WriteJobsFile(JobsFile, artifactTemplates);
			}

			var jobs = BuildDependency.JobsFile.ReadJobsFile(JobsFile);

			if (RunAsync)
			{
				Parallel.ForEach(jobs.OfType<DownloadFileJob>(), job =>
					{
						job.Execute(logHelper, WorkingDir);
					});
				Parallel.ForEach(jobs.OfType<UnzipFilesJob>(), job =>
					{
						job.Execute(logHelper, WorkingDir);
					});
			}
			else
			{
				foreach (var job in jobs.OfType<DownloadFileJob>())
				{
					job.Execute(logHelper, WorkingDir);
				}
				foreach (var job in jobs.OfType<UnzipFilesJob>())
				{
					job.Execute(logHelper, WorkingDir);
				}
			}

			if (UseDependencyFile && !KeepJobsFile)
				File.Delete(JobsFile);

			return true;
		}
	}
}

