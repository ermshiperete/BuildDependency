// Copyright (c) 2014 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;

namespace BuildDependency.Artifacts
{
	public class DownloadZipJob: DownloadFileJob
	{
		public DownloadZipJob(string serializedJob)
			: base(serializedJob)
		{
		}

		public DownloadZipJob(Conditions condition, string sourceFile, string url, string downloadDir)
			: base(condition, sourceFile, url, downloadDir)
		{
		}

		protected override string JobTypeMarker => "D";
	}
}

