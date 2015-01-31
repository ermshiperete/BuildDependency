// Copyright (c) 2014 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using BuildDependency;
using BuildDependency.Tools;

namespace BuildDependency.Artifacts
{
	public class ArtifactRule
	{
		private string _repoUrl;

		public ArtifactRule(Conditions conditions, string repoUrl, string rule)
		{
			_repoUrl = repoUrl ?? string.Empty;
			if (!_repoUrl.EndsWith("/"))
				_repoUrl += "/";
			Conditions = conditions;
			Include = true;
			ArchivePath = string.Empty;
			DestinationPath = string.Empty;
			ParseRule(rule);
		}

		public Conditions Conditions { get; private set; }

		public bool Include { get; private set; }

		public string SourcePath { get; private set; }

		public string ArchivePath { get; private set; }

		public string DestinationPath { get; private set; }

		public bool IsMatch(string filename)
		{
			return Include && FileHelper.FilenameMatch(SourcePath, filename, false);
		}

		public string GetTarget(string filename)
		{
			if (!IsMatch(filename))
				return null;

			var wildcard = SourcePath.IndexOfAny(new[] { '*', '?' });
			if (wildcard > -1 && filename.Length > wildcard)
			{
				return Path.Combine(DestinationPath, SourcePath.Substring(0, wildcard) + filename.Substring(wildcard));
			}
			return Path.Combine(DestinationPath, filename);
		}

		private string GetSourceUrl(string fileName)
		{
			return _repoUrl + fileName;
		}

		public List<IJob> GetJobs(string filename)
		{
			var list = new List<IJob>();
			if (!IsMatch(filename))
				return null;
			if (string.IsNullOrEmpty(ArchivePath))
				list.Add(new DownloadFileJob(Conditions, GetSourceUrl(filename), GetTarget(filename)));
			else
			{
				var zipFile = Path.Combine(/*TODO*/ "Downloads", filename);
				list.Add(new DownloadZipJob(Conditions, GetSourceUrl(filename), zipFile));
				list.Add(new UnzipFilesJob(Conditions, zipFile, ArchivePath, DestinationPath));
			}

			return list;
		}

		private void ParseRule(string rule)
		{
			var regex = new Regex(@"^(?<ignore>(\+|-):)?(?<source>[^!=\n]+)(?<archive>![^=]+)?(?<dest>=>.+)?$", RegexOptions.Compiled);
			var match = regex.Match(rule);
			var include = match.Groups["ignore"].Value;
			if (!string.IsNullOrEmpty(include) && include[0] == '-')
				Include = false;
			SourcePath = match.Groups["source"].Value.Trim();
			var archive = match.Groups["archive"].Value;
			ArchivePath = archive.Trim().TrimStart('!');
			var dest = match.Groups["dest"].Value;
			if (!string.IsNullOrEmpty(dest))
			{
				var index = dest.IndexOf("=>", StringComparison.InvariantCulture);
				if (index > -1)
					DestinationPath = dest.Substring(index + 2).Trim();
			}
		}
	}
}

