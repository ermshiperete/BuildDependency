// Copyright (c) 2014 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BuildDependency.TeamCity;
using BuildDependency.Artifacts;

namespace BuildDependency
{
	public static class JobsFile
	{
		public async static void WriteJobsFile(string fileName, List<ArtifactTemplate> artifactTemplates)
		{
			await WriteJobsFileAsync(fileName, artifactTemplates);
		}

		public async static Task WriteJobsFileAsync(string fileName, List<ArtifactTemplate> artifactTemplates)
		{
			using (var file = new StreamWriter(fileName))
			{
				foreach (var artifact in artifactTemplates)
				{
					var server = artifact.Server as TeamCityApi;
					if (server == null)
						continue;

					foreach (var job in await artifact.GetJobs())
					{
						file.WriteLine(job);
					}
				}
			}
		}

		private static IJob ProcessLine(string line)
		{
			switch (line[0])
			{
				case 'F':
					return new DownloadFileJob(line);
				case 'D':
					return new DownloadZipJob(line);
				case 'U':
					return new UnzipFilesJob(line);
			}
			return null;
		}

		public static List<IJob> ReadJobsFile(string fileName)
		{
			var jobs = new List<IJob>();
			using (var file = new StreamReader(fileName))
			{
				string line;
				for (line = file.ReadLine(); !file.EndOfStream; line = file.ReadLine())
				{
					var job = ProcessLine(line);
					if (job != null)
						jobs.Add(job);
				}
				if (!string.IsNullOrEmpty(line))
				{
					var job = ProcessLine(line);
					if (job != null)
						jobs.Add(job);
				}
			}

			return jobs;
		}
	}
}

