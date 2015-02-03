// Copyright (c) 2015 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;
using BuildDependency;
using Microsoft.Build.Utilities;
using System.IO;

namespace BuildDependencyTestApp
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			var task = new Dependencies();
			task.DependencyFile = "/tmp/test.dep";
			task.JobsFile = "/tmp/test.files";
			task.UseDependencyFile = true;
			task.KeepJobsFile = true;
			task.WorkingDir = "/tmp/bla";
			Directory.CreateDirectory("/tmp/bla");
			task.RunAsync = true;
			task.Execute();
		}
	}
}
