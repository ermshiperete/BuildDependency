// Copyright (c) 2014 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;
using BuildDependency.Artifacts;
using BuildDependency.TeamCity;
using BuildDependency.TeamCity.RestClasses;
using System.IO;
using System.Threading.Tasks;

namespace BuildDependency.Artifacts
{
	public class ArtifactTemplate: ArtifactProperties
	{
		public ArtifactTemplate(Server server, Project project, string buildTypeId)
		{
			Server = server;
			Project = project;
			SourceBuildTypeId = buildTypeId;
			Condition = Conditions.All;
		}

		public ArtifactTemplate(Server server, ArtifactProperties artifact, BuildType buildType)
		{
			Server = server;
			PathRules = artifact.PathRules;
			RevisionName = artifact.RevisionName;
			RevisionValue = artifact.RevisionValue;
			SourceBuildTypeId = artifact.SourceBuildTypeId ?? buildType.Id;
			CleanDestinationDirectory = artifact.CleanDestinationDirectory;
			Condition = Conditions.All;
			var tcServer = server as TeamCityApi;
			if (tcServer == null || !tcServer.Projects.ContainsKey(Config.ProjectId))
				return;
			Project = tcServer.Projects[Config.ProjectId];
		}

		public string ConfigName
		{
			get
			{
				return Config.Name;
			}
		}

		public BuildType Config
		{
			get { return ((TeamCityApi)Server).BuildTypes[SourceBuildTypeId]; }
		}

		public Conditions Condition { get; set; }

		public Project Project { get; set; }

		public Server Server { get; set; }

		public string RepoUrl
		{
			get
			{
				return string.Format("{0}/download/{1}/{2}", ((TeamCityApi)Server).BaseRepoUrl,
					Config.IdForArtifacts, RevisionValue);
			}
		}

		public string Source
		{
			get
			{
				var source = $"{Server.Name}::{ConfigName}\n({TagLabel})";
				if ((Condition & Conditions.All) != Conditions.All && Condition != Conditions.None)
					source = $"{source}\nCondition: {Condition}";
				return source;
			}
		}

		public async Task<List<IJob>> GetJobs()
		{
			var rules = new List<ArtifactRule>();
			foreach (var ruleString in PathRules.Split('\n'))
			{
				if (string.IsNullOrEmpty(ruleString.Trim()))
					continue;
				rules.Add(new ArtifactRule(Condition, RepoUrl, ruleString));
			}

			var jobs = new List<IJob>();
			var artifacts = await ((TeamCityApi)Server).GetArtifactsAsync(this);
			foreach (var file in artifacts)
			{
				foreach (var rule in rules)
				{
					var jobsForThisRule = rule.GetJobs(file.ToString());
					if (jobsForThisRule != null)
						jobs.AddRange(jobsForThisRule);
				}
			}
			return jobs;
		}
	}
}

