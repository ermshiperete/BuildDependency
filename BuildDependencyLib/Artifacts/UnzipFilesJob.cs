// Copyright (c) 2014 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;
using System.IO;
using System.Runtime.Serialization;
using ICSharpCode.SharpZipLib.Zip;
using BuildDependency.Tools;
using ICSharpCode.SharpZipLib.Core;
using System.Threading.Tasks;

namespace BuildDependency.Artifacts
{
	public class UnzipFilesJob: IJob
	{
		public UnzipFilesJob(string serializedJob)
		{
			var parts = serializedJob.Split('\t');
			if (parts.Length < 5 || parts[0] != "U")
				throw new SerializationException("Unexpected line for UnzipFilesJob: " + serializedJob);

			Conditions = (Conditions)Convert.ToInt32(parts[1]);
			ZipFile = parts[2];
			SourcePath = parts[3];
			DestinationPath = parts[4];
		}

		public UnzipFilesJob(Conditions condition, string zipFile, string sourcePath, string destinationPath)
		{
			Conditions = condition;
			ZipFile = zipFile;
			SourcePath = sourcePath;
			DestinationPath = destinationPath;
		}

		public string ZipFile { get; private set; }

		public string SourcePath { get; private set; }

		public string DestinationPath { get; private set; }

		#region IJob implementation

		public Conditions Conditions { get; private set; }

		public async Task<bool> Execute(ILog log, string workDir)
		{
			if (!Conditions.AreTrue())
				return true;

			var fileFilter = WildcardToRegex.FileFilter(SourcePath);
			var directoryFilter = WildcardToRegex.DirectoryFilter(SourcePath);
			var events = new FastZipEvents();
			events.ProcessFile += (sender, e) => log.LogMessage("Extracting file {0}", e.Name);

			var fastZip = new FastZip(events) { CreateEmptyDirectories = true };
			fastZip.ExtractZip(Path.Combine(workDir, ZipFile), Path.Combine(workDir, DestinationPath), FastZip.Overwrite.Always,
				fn => true, fileFilter, directoryFilter, true);
			return true;
		}

		public override string ToString()
		{
			return $"U\t{(int) Conditions}\t{ZipFile}\t{SourcePath}\t{DestinationPath}";
		}

		#endregion
	}
}

