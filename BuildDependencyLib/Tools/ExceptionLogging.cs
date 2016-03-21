// Copyright (c) 2016 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using Bugsnag;
using Bugsnag.Clients;

namespace BuildDependency.Tools
{
	public class ExceptionLogging: BaseClient
	{
		private ExceptionLogging(string apiKey, string callerFilePath)
			: base(apiKey)
		{
			Setup(callerFilePath);
		}

		private void Setup(string callerFilePath)
		{
			var solutionPath = Path.GetFullPath(Path.Combine(callerFilePath, "../../"));
			Config.FilePrefixes = new[] { solutionPath };
			Config.UserId = UserId;
			Config.BeforeNotify(OnBeforeNotify);

			Config.Metadata.AddToTab("App", "runtime", Platform.IsMono ? "Mono" : ".NET");
			Config.Metadata.AddToTab("Device", "desktop", Platform.DesktopEnvironment);
			if (!string.IsNullOrEmpty(Platform.DesktopEnvironmentInfoString))
				Config.Metadata.AddToTab("Device", "shell", Platform.DesktopEnvironmentInfoString);
			AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
		}

		private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			Notify(e.ExceptionObject as Exception);
		}

		private static string UserId
		{
			get
			{
				// NOTE: we use the hashcode to anonymize the real username
				var hashcode = (uint)(Platform.IsWindows ?
					WindowsIdentity.GetCurrent().User.Value.GetHashCode() :
					Environment.MachineName.GetHashCode() ^ Environment.UserName.GetHashCode());
				return hashcode.ToString();
			}
		}

		private string RemoveFileNamePrefix(string fileName)
		{
			var result = fileName;
			if (!string.IsNullOrEmpty(result))
			{
				foreach (string prefix in Config.FilePrefixes)
				{
					result = result.Replace(prefix, string.Empty);
				}
			}
			return result;
		}

		private bool OnBeforeNotify(Event error)
		{
			var stackTrace = new StackTrace(error.Exception, true);
			if (stackTrace.FrameCount > 0)
			{
				var frame = stackTrace.GetFrame(0);
				// During development the line number probably changes frequently, but we want
				// to treat all errors with the same exception in the same method as being the
				// same, even when the line numbers differ, so we set it to 0. For releases
				// we can assume the line number to be constant for a released build.
				var linenumber = Config.ReleaseStage == "development" ? 0 : frame.GetFileLineNumber();
				error.GroupingHash = string.Format("{0} {1} {2} {3}", error.Exception.GetType().Name,
					RemoveFileNamePrefix(frame.GetFileName()), frame.GetMethod().Name, linenumber);
			}

			return true;
		}

		public static ExceptionLogging Initialize(string apiKey, [CallerFilePathAttribute] string filename = null)
		{
			Client = new ExceptionLogging(apiKey, filename);
			return Client;
		}

		public static ExceptionLogging Client { get; private set; }
	}
}

